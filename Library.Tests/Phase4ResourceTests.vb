Imports System.Net.Http
Imports System.Text
Imports Library.Models
Imports Library.Queries
Imports Library.Resources

<TestClass>
Public Class Phase4ResourceTests

    <TestMethod>
    Public Async Function Dois_ListUpdateDelete() As Task
        Dim handler = New FakeHandler().
            RespondJson("{""itemsMax"": 1, ""items"": [{""id"": 3, ""doi"": ""10.99999/x"", ""status"": 3, ""crossrefplugin::status"": ""x""}]}").
            RespondJson("{""id"": 3}").RespondJson("{""id"": 3}").RespondJson("")
        Using ojs = TestClient.Create(handler)
            Dim page = Await ojs.Dois.ListAsync(New DoiQuery With {.Status = {DoiStatus.Registered, DoiStatus.Stale}})
            Assert.AreEqual(DoiStatus.Registered, page.Items(0).Status)
            StringAssert.EndsWith(handler.Last.Uri.AbsoluteUri, "/dois?status=3,5")

            Await ojs.Dois.UpdateAsync(3, New DoiEdit With {.Doi = "10.99999/y", .PubObjectType = DoiObjectType.Publication, .PubObjectId = 9})
            Assert.AreEqual("{""doi"":""10.99999/y"",""pubObjectType"":""publication"",""pubObjectId"":9}", handler.Last.Body)

            Await ojs.Dois.DeleteAsync(3, DoiObjectType.Representation, 4)
            Assert.AreEqual(HttpMethod.Delete, handler.Last.Method)
            Assert.AreEqual("{""pubObjectType"":""representation"",""pubObjectId"":4}", handler.Last.Body)

            Await ojs.Dois.DepositAllAsync()
            StringAssert.EndsWith(handler.Last.Uri.AbsolutePath, "/dois/depositAll")
        End Using
    End Function

    <TestMethod>
    Public Async Function Dois_BulkActions_And_Export() As Task
        Dim handler = New FakeHandler().
            RespondJson("{""failedDoiActions"": [""Issue 2: no pattern""]}").
            RespondJson("{""temporaryFileId"": 44}").
            Respond(New StringContent("<doi_batch/>", Encoding.UTF8, "application/xml")).
            RespondJson("[]")
        Using ojs = TestClient.Create(handler)
            Dim result = Await ojs.Dois.Issues.AssignAsync({1, 2})
            Assert.IsFalse(result.Succeeded)
            Assert.AreEqual(HttpMethod.Post, handler.Last.Method)
            StringAssert.EndsWith(handler.Last.Uri.AbsolutePath, "/dois/issues/assignDois")
            Assert.AreEqual("{""ids"":[1,2]}", handler.Last.Body)

            Using file = Await ojs.Dois.Submissions.ExportAndDownloadAsync({7})
                StringAssert.EndsWith(handler.Requests(1).Uri.AbsolutePath, "/dois/submissions/export")
                Assert.AreEqual(HttpMethod.Put, handler.Requests(1).Method)
                StringAssert.EndsWith(handler.Last.Uri.AbsolutePath, "/dois/exports/44")
                Assert.AreEqual("<doi_batch/>", Encoding.UTF8.GetString(Await file.ReadAllBytesAsync()))
            End Using

            Await ojs.Dois.Submissions.MarkStaleAsync({7})
            StringAssert.EndsWith(handler.Last.Uri.AbsolutePath, "/dois/submissions/markStale")
        End Using
    End Function

    <TestMethod>
    Public Async Function Reviews_Confirm_Export_Download() As Task
        Dim handler = New FakeHandler().
            RespondJson("{""_data"": {""id"": 2, ""dateConfirmed"": ""2026-11-08 10:00:00""}}").
            RespondJson("{""temporaryFileId"": 5}").
            Respond(New ByteArrayContent({37, 80, 68, 70})).
            RespondJson("[]")
        Using ojs = TestClient.Create(handler)
            Dim ra = Await ojs.Reviews.ConfirmAsync(3, 2, accept:=False)
            Assert.AreEqual(2, ra.Id)
            Assert.AreEqual("{""decision"":""decline""}", handler.Last.Body)
            StringAssert.EndsWith(handler.Last.Uri.AbsolutePath, "/reviews/3/2/confirmReview")

            Using pdf = Await ojs.Reviews.DownloadPdfAsync(3, 1, authorFriendly:=True)
                StringAssert.EndsWith(handler.Requests(1).Uri.AbsoluteUri, "/reviews/3/1/export-pdf?authorFriendly=1")
                StringAssert.EndsWith(handler.Last.Uri.AbsolutePath, "/reviews/3/exports/5")
                Assert.HasCount(4, Await pdf.ReadAllBytesAsync())
            End Using

            Await ojs.Reviews.SendToOrcidAsync(3, 1)
            Assert.AreEqual(HttpMethod.Post, handler.Last.Method)
        End Using
    End Function

    <TestMethod>
    Public Async Function Invitations_Flow() As Task
        Dim handler = New FakeHandler().
            RespondJson("{""itemsMax"": 1, ""items"": [{""id"": 8, ""status"": ""pending"", ""expiryDate"": ""2026-12-01 00:00:00"", ""userGroupsToAdd"": [{""userGroupId"": 3, ""dateStart"": ""2026-11-08""}]}]}").
            RespondJson("{""invitationId"": 9}").
            RespondJson("{""id"": 9}").RespondJson("{""id"": 9, ""status"": ""pending""}").RespondJson("[]").
            RespondJson("{""id"": 9, ""status"": ""declined""}")
        Using ojs = TestClient.Create(handler)
            Dim page = Await ojs.Invitations.ListAsync(InvitationTypes.UserRoleAssignment, New InvitationQuery With {.Status = InvitationStatus.Pending, .Inviter = 1})
            Dim inv = page.Items(0)
            Assert.AreEqual(InvitationStatus.Pending, inv.StatusValue)
            Assert.AreEqual(New DateOnly(2026, 11, 8), inv.UserGroupsToAdd(0).DateStart)
            StringAssert.EndsWith(handler.Last.Uri.AbsoluteUri, "/invitations/userRoleAssignment?status=pending&inviter=1")

            Dim created = Await ojs.Invitations.CreateForUserAsync(InvitationTypes.UserRoleAssignment, 5)
            Assert.AreEqual(9, created)
            Assert.AreEqual("{""invitationData"":{""userId"":5}}", handler.Last.Body)
            StringAssert.EndsWith(handler.Last.Uri.AbsolutePath, "/invitations/add/userRoleAssignment")

            Await ojs.Invitations.PopulateAsync(9, New UserRoleAssignmentData With {
                .UserGroupsToAdd = New List(Of InvitationUserGroup) From {New InvitationUserGroup With {.UserGroupId = 3, .Masthead = False, .DateStart = New DateOnly(2026, 11, 8)}}})
            StringAssert.EndsWith(handler.Last.Uri.AbsolutePath, "/invitations/9/populate")
            Assert.AreEqual("{""invitationData"":{""userGroupsToAdd"":[{""userGroupId"":3,""masthead"":false,""dateStart"":""2026-11-08""}]}}", handler.Last.Body)
            Await ojs.Invitations.InviteAsync(9)
            StringAssert.EndsWith(handler.Last.Uri.AbsolutePath, "/invitations/9/invite")
            Await ojs.Invitations.CancelAsync(9)
            StringAssert.EndsWith(handler.Last.Uri.AbsolutePath, "/invitations/9/cancel")

            Dim declined = Await ojs.Invitations.DeclineAsync(9, "a/b")
            Assert.AreEqual(InvitationStatus.Declined, declined.StatusValue)
            StringAssert.EndsWith(handler.Last.Uri.AbsoluteUri, "/invitations/9/key/a%2Fb/decline")
        End Using
    End Function

    <TestMethod>
    Public Async Function Invitations_422_IsValidationException() As Task
        Dim handler = New FakeHandler().RespondJson("{""errors"": {""userGroupsToAdd"": [""The user groups field is required.""]}}", Net.HttpStatusCode.UnprocessableEntity)
        Using ojs = TestClient.Create(handler)
            Dim ex = Await Assert.ThrowsExactlyAsync(Of Errors.OjsValidationException)(Function() ojs.Invitations.InviteAsync(9))
            Assert.AreEqual("The user groups field is required.", ex.Errors("userGroupsToAdd")(0))
        End Using
    End Function

    <TestMethod>
    Public Async Function Orcid_Paths() As Task
        Dim handler = New FakeHandler().RespondJson("").RespondJson("")
        Using ojs = TestClient.Create(handler)
            Await ojs.Orcid.RequestAuthorVerificationAsync(12)
            StringAssert.EndsWith(handler.Last.Uri.AbsolutePath, "/orcid/requestAuthorVerification/12")
            Await ojs.Orcid.DeleteForAuthorAsync(12)
            StringAssert.EndsWith(handler.Last.Uri.AbsolutePath, "/orcid/deleteForAuthor/12")
        End Using
    End Function

    <TestMethod>
    Public Async Function Stats_Editorial_Users_Averages() As Task
        Dim handler = New FakeHandler().
            RespondJson("[{""key"": ""submissionsReceived"", ""name"": ""Submissions Received"", ""value"": 5}, {""key"": ""acceptanceRate"", ""name"": ""Rate"", ""value"": 0.25}]").
            RespondJson("{""submissionsReceived"": 32, ""daysToDecision"": -1}").
            RespondJson("[{""id"": ""total"", ""name"": ""All Users"", ""value"": 5}, {""id"": 16, ""name"": ""Journal Manager"", ""value"": 1}]")
        Using ojs = TestClient.Create(handler)
            Dim ed = Await ojs.Stats.GetEditorialAsync(New DateOnly(2026, 1, 1), New DateOnly(2026, 12, 31), {1, 2})
            Assert.AreEqual(5.0, ed(0).Value)
            Assert.AreEqual(0.25, ed(1).Value)
            StringAssert.EndsWith(handler.Last.Uri.AbsoluteUri, "/stats/editorial?dateStart=2026-01-01&dateEnd=2026-12-31&sectionIds=1,2")

            Dim avg = Await ojs.Stats.GetEditorialAveragesAsync()
            Assert.AreEqual(-1.0, avg("daysToDecision"))

            Dim users = Await ojs.Stats.GetUsersAsync(status:=UserStatusFilter.Active)
            Assert.IsTrue(users(0).IsTotal)
            Assert.IsNull(users(0).Role)
            Assert.AreEqual(UserRole.Manager, users(1).Role)
            StringAssert.EndsWith(handler.Last.Uri.AbsoluteUri, "/stats/users?status=active")
        End Using
    End Function

    <TestMethod>
    Public Async Function Stats_Publications_ListTimelineCsvGeo() As Task
        Dim handler = New FakeHandler().
            RespondJson("{""itemsMax"": 1, ""items"": [{""abstractViews"": 10, ""galleyViews"": 4, ""pdfViews"": 4, ""publication"": {""id"": 2, ""fullTitle"": {""en"": ""T""}, ""shortAuthorString"": ""A""}}]}").
            RespondJson("[{""date"": ""2026-11"", ""label"": ""November 2026"", ""value"": 14}]").
            Respond(New StringContent("ID,Title" & vbLf & "2,T", Encoding.UTF8, "text/csv")).
            RespondJson("{""items"": [{""city"": ""Bandung"", ""region"": ""JB"", ""country"": ""ID"", ""total"": 3, ""unique"": 2}], ""itemsMax"": 1}")
        Using ojs = TestClient.Create(handler)
            Dim q As New PublicationStatsQuery With {.DateStart = New DateOnly(2026, 11, 1), .SectionIds = {1}, .Count = 5, .OrderDirection = SortDirection.Ascending}
            Dim page = Await ojs.Stats.Publications.ListAsync(q)
            Assert.AreEqual("T", page.Items(0).Publication.FullTitle.GetValue("en"))
            Assert.AreEqual("https://ojs.test/jpk/api/v1/stats/publications?count=5&dateStart=2026-11-01&sectionIds=1&orderDirection=ASC", handler.Last.Uri.AbsoluteUri)

            Dim timeline = Await ojs.Stats.Publications.GetTimelineAsync(q, TimelineResolution.Day, filesOnly:=True)
            Assert.AreEqual(14, timeline(0).Value)
            Assert.AreEqual("https://ojs.test/jpk/api/v1/stats/publications/timeline?dateStart=2026-11-01&sectionIds=1&timelineInterval=day&type=files", handler.Last.Uri.AbsoluteUri)

            Dim csv = Await ojs.Stats.Publications.ListCsvAsync(q)
            StringAssert.StartsWith(csv, "ID,Title")
            Assert.AreEqual("text/csv", handler.Last.Accept)

            Dim cities = Await ojs.Stats.Publications.ListCitiesAsync()
            Assert.AreEqual("Bandung", cities.Items(0).City)
            StringAssert.EndsWith(handler.Last.Uri.AbsolutePath, "/stats/publications/cities")
        End Using
    End Function

    <TestMethod>
    Public Async Function Stats_Contexts_Issues_Single() As Task
        Dim handler = New FakeHandler().
            RespondJson("{""total"": 7, ""context"": {""id"": 1, ""urlPath"": ""jpk""}}").
            RespondJson("{""totalViews"": 3, ""tocViews"": 2, ""issueGalleyViews"": 1, ""issue"": {""id"": 4, ""identification"": ""Vol 1""}}").
            RespondJson("[]")
        Using ojs = TestClient.Create(handler)
            Dim c = Await ojs.Stats.Contexts.GetAsync(1, dateEnd:=New DateOnly(2026, 11, 7))
            Assert.AreEqual(7, c.Total)
            StringAssert.EndsWith(handler.Last.Uri.AbsoluteUri, "/stats/contexts/1?dateEnd=2026-11-07")

            Dim i = Await ojs.Stats.Issues.GetAsync(4)
            Assert.AreEqual("Vol 1", i.Issue.Identification)

            Await ojs.Stats.Issues.GetTimelineAsync(4, TimelineResolution.Month, galleysOnly:=True)
            StringAssert.EndsWith(handler.Last.Uri.AbsoluteUri, "/stats/issues/4/timeline?timelineInterval=month&type=files")
        End Using
    End Function

End Class
