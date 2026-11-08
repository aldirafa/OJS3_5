Imports System.Net
Imports System.Net.Http
Imports System.Text.Json
Imports Library.Errors
Imports Library.Models
Imports Library.Queries
Imports Library.Resources
Imports Library.Serialization

<TestClass>
Public Class Phase3ResourceTests

    <TestMethod>
    Public Async Function Users_List_SendsFilters() As Task
        Dim handler = New FakeHandler().RespondJson("{""itemsMax"": 1, ""items"": [{""id"": 3, ""fullName"": ""A B"", ""groups"": [{""id"": 5, ""roleId"": 16, ""name"": {""en"": ""Journal manager""}}]}]}")
        Using ojs = TestClient.Create(handler)
            Dim page = Await ojs.Users.ListAsync(New UserQuery With {
                .RoleIds = {UserRole.Manager, UserRole.SubEditor}, .Status = UserStatusFilter.All,
                .OrderBy = UserOrderBy.FamilyName, .OrderDirection = SortDirection.Descending,
                .AssignedToSubmissionStage = WorkflowStage.Editing, .SearchPhrase = "ann"})
            Assert.AreEqual("A B", page.Items(0).FullName)
        End Using
        Assert.AreEqual("https://ojs.test/jpk/api/v1/users?status=all&orderBy=familyName&orderDirection=DESC&searchPhrase=ann&roleIds=16,17&assignedToSubmissionStage=4",
                        handler.Last.Uri.AbsoluteUri)
    End Function

    <TestMethod>
    Public Async Function Users_Reviewers_And_EndRole() As Task
        Dim handler = New FakeHandler().
            RespondJson("{""itemsMax"": 1, ""items"": [{""id"": 9, ""reviewsActive"": 2, ""reviewerRating"": 4}]}").
            RespondJson("{""id"": 9}")
        Using ojs = TestClient.Create(handler)
            Dim reviewers = Await ojs.Users.ListReviewersAsync(New ReviewerQuery With {.ReviewerRating = 3, .ReviewsActive = "0-2", .ReviewStage = WorkflowStage.ExternalReview})
            Assert.AreEqual(9, reviewers.Items(0).Id)
            Assert.AreEqual("https://ojs.test/jpk/api/v1/users/reviewers?reviewerRating=3&reviewStage=3&reviewsActive=0-2", handler.Last.Uri.AbsoluteUri)

            Dim u = Await ojs.Users.EndRoleAsync(9, 14)
            Assert.AreEqual(9, u.Id)
            Assert.AreEqual(HttpMethod.Put, handler.Last.Method)
            StringAssert.EndsWith(handler.Last.Uri.AbsolutePath, "/users/9/endRole/14")
        End Using
    End Function

    <TestMethod>
    Public Async Function Contexts_AlwaysUseSiteWidePath() As Task
        Dim handler = New FakeHandler().RespondJson("{""itemsMax"": 1, ""items"": [{""id"": 1, ""urlPath"": ""jpk"", ""name"": {""en"": ""J""}, ""enabled"": true}]}").
            RespondJson("{""id"": 2, ""urlPath"": ""new"", ""defaultReviewMode"": 2, ""publishingMode"": 0, ""journalThumbnail"": {""en"": null}}")
        Using ojs = TestClient.Create(handler)
            Dim page = Await ojs.Contexts.ListAsync(New ContextQuery With {.IsEnabled = True})
            Assert.AreEqual("J", page.Items(0).Name.GetValue("en"))
            Assert.AreEqual("https://ojs.test/_/api/v1/contexts?isEnabled=true", handler.Last.Uri.AbsoluteUri)

            Dim created = Await ojs.Contexts.CreateAsync(New ContextEdit With {
                .Name = LocalizedString.Create("en", "New"), .UrlPath = "new", .PrimaryLocale = "en",
                .SupportedLocales = New List(Of String) From {"en"}, .DefaultReviewMode = ReviewMethod.DoubleAnonymous})
            Assert.AreEqual("https://ojs.test/_/api/v1/contexts", handler.Last.Uri.AbsoluteUri)
            Assert.AreEqual("{""defaultReviewMode"":2,""name"":{""en"":""New""},""primaryLocale"":""en"",""supportedLocales"":[""en""],""urlPath"":""new""}",
                            handler.Last.Body)
            Assert.AreEqual(ReviewMethod.DoubleAnonymous, created.DefaultReviewMode)
            Assert.AreEqual(PublishingMode.Open, created.PublishingMode)
            Assert.IsNull(created.JournalThumbnail("en"))
        End Using
    End Function

    <TestMethod>
    Public Async Function Contexts_Theme_RoundTrip() As Task
        Dim handler = New FakeHandler().
            RespondJson("{""themePluginPath"": ""default"", ""baseColour"": ""#1E6292"", ""showDescriptionInJournalIndex"": null, ""typography"": ""notoSans""}").
            RespondJson("{""id"": 1, ""urlPath"": ""jpk""}").
            RespondJson("{""themePluginPath"": ""default""}")
        Using ojs = TestClient.Create(handler)
            Dim theme = Await ojs.Contexts.GetThemeAsync(1)
            Assert.AreEqual("default", theme.ThemePluginPath)
            Assert.AreEqual("#1E6292", theme.GetString("baseColour"))
            Assert.IsNull(theme.GetString("showDescriptionInJournalIndex"))
            Assert.IsNull(theme.GetString("missing"))

            Await ojs.Contexts.UpdateThemeAsync(1, New ThemeOptions With {.ThemePluginPath = "default"}.SetOption("baseColour", "#000000").SetOption("useHomepageImageAsHeader", True))
            Assert.AreEqual("https://ojs.test/_/api/v1/contexts/1", handler.Requests(1).Uri.AbsoluteUri)
            Assert.AreEqual("https://ojs.test/jpk/api/v1/contexts/1/theme", handler.Last.Uri.AbsoluteUri)
            Assert.AreEqual("{""themePluginPath"":""default"",""baseColour"":""#000000"",""useHomepageImageAsHeader"":true}", handler.Last.Body)
        End Using
    End Function

    <TestMethod>
    Public Async Function Site_UsesSiteWidePath_AndParses() As Task
        Dim handler = New FakeHandler().
            RespondJson("{""title"": {""en"": ""S""}, ""contactEmail"": {""en"": ""a@b.c""}, ""installedLocales"": [""en""], ""minPasswordLength"": 6, ""enableBulkEmails"": null, ""pageHeaderTitleImage"": {""en"": null}, ""sidebar"": null}").
            RespondJson("{}")
        Using ojs = TestClient.Create(handler)
            Dim site = Await ojs.Site.GetAsync()
            Assert.AreEqual("S", site.Title.GetValue("en"))
            Assert.AreEqual(6, site.MinPasswordLength)
            Assert.AreEqual("https://ojs.test/_/api/v1/site", handler.Last.Uri.AbsoluteUri)

            Await ojs.Site.UpdateAsync(New SiteEdit With {.MinPasswordLength = 10})
            Assert.AreEqual(HttpMethod.Put, handler.Last.Method)
            Assert.AreEqual("{""minPasswordLength"":10}", handler.Last.Body)
        End Using
    End Function

    <TestMethod>
    Public Async Function Announcements_Crud() As Task
        Dim handler = New FakeHandler().
            RespondJson("{""itemsMax"": 1, ""items"": [{""id"": 4, ""datePosted"": ""2026-03-01 08:30:00"", ""dateExpire"": ""2026-06-30"", ""title"": {""en"": ""CFP""}, ""image"": null}]}").
            RespondJson("{""id"": 5}").RespondJson("{""id"": 5}").RespondJson("{""id"": 5}")
        Using ojs = TestClient.Create(handler)
            Dim page = Await ojs.Announcements.ListAsync(New AnnouncementQuery With {.TypeIds = {1, 2}})
            Dim a = page.Items(0)
            Assert.AreEqual(New DateTime(2026, 3, 1, 8, 30, 0), a.DatePosted)
            Assert.AreEqual(New DateOnly(2026, 6, 30), a.DateExpire)
            StringAssert.EndsWith(handler.Last.Uri.AbsoluteUri, "/announcements?typeIds=1,2")

            Await ojs.Announcements.CreateAsync(New AnnouncementEdit With {
                .Title = LocalizedString.Create("en", "T"), .DateExpire = New DateOnly(2027, 1, 31),
                .Image = New ImageFile With {.TemporaryFileId = 77, .AltText = "pic"}})
            Assert.AreEqual("{""dateExpire"":""2027-01-31"",""image"":{""altText"":""pic"",""temporaryFileId"":77},""title"":{""en"":""T""}}", handler.Last.Body)

            Await ojs.Announcements.UpdateAsync(5, New AnnouncementEdit With {.TypeId = 2})
            Assert.AreEqual(HttpMethod.Put, handler.Last.Method)
            StringAssert.EndsWith(handler.Last.Uri.AbsolutePath, "/announcements/5")

            Await ojs.Announcements.DeleteAsync(5)
            Assert.AreEqual(HttpMethod.Delete, handler.Last.Method)
        End Using
    End Function

    <TestMethod>
    Public Async Function Institutions_And_Rors() As Task
        Dim handler = New FakeHandler().
            RespondJson("{""id"": 1, ""name"": {""en"": ""U""}, ""ipRanges"": [""192.0.2.0/24""], ""ror"": ""https://ror.org/x"", ""deletedAt"": null}").
            RespondJson("{""itemsMax"": 0, ""items"": []}").
            RespondJson("{""id"": 3, ""ror"": ""https://ror.org/x"", ""isActive"": true}")
        Using ojs = TestClient.Create(handler)
            Dim inst = Await ojs.Institutions.CreateAsync(New InstitutionEdit With {.Name = LocalizedString.Create("en", "U"), .IpRanges = New List(Of String) From {"192.0.2.0/24"}})
            Assert.AreEqual("192.0.2.0/24", inst.IpRanges(0))
            Assert.AreEqual("{""ipRanges"":""192.0.2.0/24"",""name"":{""en"":""U""}}", handler.Last.Body)

            Await ojs.Rors.ListAsync(New SearchQuery With {.SearchPhrase = "uni", .Count = 5})
            Assert.AreEqual("https://ojs.test/jpk/api/v1/rors?count=5&searchPhrase=uni", handler.Last.Uri.AbsoluteUri)

            Dim ror = Await ojs.Rors.SaveAsync(New RorEdit With {.Ror = "https://ror.org/x", .IsActive = True})
            Assert.AreEqual(3, ror.Id)
            Assert.AreEqual(HttpMethod.Post, handler.Last.Method)
        End Using
    End Function

    <TestMethod>
    Public Async Function EmailTemplates_KeyIsEscaped_AndRestoreDefaults() As Task
        Dim handler = New FakeHandler().
            RespondJson("{""key"": ""SUBMISSION_ACK"", ""subject"": {""en"": ""Thanks""}, ""id"": 0}").
            RespondJson("{""key"": ""A B""}").
            RespondJson("[""SUBMISSION_ACK"", ""MY_TEMPLATE""]")
        Using ojs = TestClient.Create(handler)
            Dim t = Await ojs.EmailTemplates.GetAsync("SUBMISSION_ACK")
            Assert.AreEqual("Thanks", t.Subject.GetValue("en"))

            Await ojs.EmailTemplates.UpdateAsync("A B", New EmailTemplateEdit With {.Subject = LocalizedString.Create("en", "S")})
            Assert.AreEqual("https://ojs.test/jpk/api/v1/emailTemplates/A%20B", handler.Last.Uri.AbsoluteUri)

            Dim keys = Await ojs.EmailTemplates.RestoreDefaultsAsync()
            Assert.HasCount(2, keys)
            Assert.AreEqual(HttpMethod.Delete, handler.Last.Method)
            StringAssert.EndsWith(handler.Last.Uri.AbsolutePath, "/emailTemplates/restoreDefaults")
        End Using
    End Function

    <TestMethod>
    Public Async Function EmailTemplates_List_Filters() As Task
        Dim handler = New FakeHandler().RespondJson("{""itemsMax"": 0, ""items"": []}")
        Using ojs = TestClient.Create(handler)
            Await ojs.EmailTemplates.ListAsync(New EmailTemplateQuery With {.AlternateTo = "SUBMISSION_ACK", .IsModified = True})
        End Using
        Assert.AreEqual("https://ojs.test/jpk/api/v1/emailTemplates?alternateTo=SUBMISSION_ACK&isModified=true", handler.Last.Uri.AbsoluteUri)
    End Function

    <TestMethod>
    Public Async Function Mailables_Emails_Vocabs() As Task
        Dim handler = New FakeHandler().
            RespondJson("[{""_href"": ""https://ojs.test/jpk/api/v1/mailables/SUBMISSION_ACK"", ""emailTemplateKey"": ""SUBMISSION_ACK"", ""name"": ""Submission Confirmation"", ""fromRoleIds"": [-1], ""toRoleIds"": [65536], ""supportsTemplates"": false, ""dataDescriptions"": []}]").
            RespondJson("{""emailTemplateKey"": ""SUBMISSION_ACK"", ""dataDescriptions"": {""recipientName"": ""The recipient""}, ""emailTemplates"": [{""key"": ""MY_ACK"", ""alternateTo"": ""SUBMISSION_ACK"", ""name"": {""en"": ""Custom""}}]}").
            RespondJson("{""itemsMax"": 1, ""items"": [{""id"": 2, ""subject"": ""Hi"", ""dateSent"": ""2026-04-01 12:00:00""}]}").
            RespondJson("[""education"", {""name"": ""science"", ""identifier"": ""x""}]")
        Using ojs = TestClient.Create(handler)
            Dim list = Await ojs.Mailables.ListAsync("ack")
            Assert.AreEqual(UserRole.Author, list(0).ToRoleIds(0))
            Assert.AreEqual(-1, CInt(list(0).FromRoleIds(0)))
            Assert.IsEmpty(list(0).DataDescriptions)
            StringAssert.EndsWith(handler.Last.Uri.AbsoluteUri, "/mailables?searchPhrase=ack")

            Dim m = Await ojs.Mailables.GetAsync(list(0).EmailTemplateKey)
            StringAssert.EndsWith(handler.Last.Uri.AbsolutePath, "/mailables/SUBMISSION_ACK")
            Assert.AreEqual("The recipient", m.DataDescriptions("recipientName"))
            Assert.AreEqual("SUBMISSION_ACK", m.EmailTemplates(0).AlternateTo)

            Dim emails = Await ojs.Emails.ListAuthorEmailsAsync(7, eventType:=1)
            Assert.AreEqual(New DateTime(2026, 4, 1, 12, 0, 0), emails(0).DateSent)
            StringAssert.EndsWith(handler.Last.Uri.AbsoluteUri, "/emails/authorEmails?submissionId=7&eventType=1")

            Dim vocab = Await ojs.Vocabs.ListAsync(VocabNames.Keywords, "en")
            Assert.AreEqual("education", vocab(0).Name)
            Assert.AreEqual("x", vocab(1).Identifier)
            StringAssert.EndsWith(handler.Last.Uri.AbsoluteUri, "/vocabs?vocab=submissionKeyword&locale=en")
        End Using
    End Function

    <TestMethod>
    Public Async Function Contexts_ValidationError_IsMapped() As Task
        Dim handler = New FakeHandler().RespondJson("{""urlPath"": [""The path is already in use.""]}", HttpStatusCode.BadRequest)
        Using ojs = TestClient.Create(handler)
            Dim ex = Await Assert.ThrowsExactlyAsync(Of OjsValidationException)(
                Function() ojs.Contexts.CreateAsync(New ContextEdit With {.UrlPath = "jpk"}))
            Assert.AreEqual("The path is already in use.", ex.Errors("urlPath")(0))
        End Using
    End Function

End Class
