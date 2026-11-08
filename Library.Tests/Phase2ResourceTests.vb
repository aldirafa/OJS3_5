Imports System.Net.Http
Imports System.Text.Json.Nodes
Imports OJS3_5.Library.Models
Imports OJS3_5.Library.Queries
Imports OJS3_5.Library.Serialization

<TestClass>
Public Class Phase2ResourceTests

    ' Synthetic fixture shaped like a real OJS 3.5 response (no personal data).
    Private Const SubmissionJson As String = "{
        ""_href"": ""https://ojs.test/jpk/api/v1/submissions/7"", ""id"": 7, ""contextId"": 1,
        ""currentPublicationId"": 9, ""dateSubmitted"": ""2026-06-01 10:00:00"", ""dateLastActivity"": ""2026-06-02 11:00:00"",
        ""stageId"": 5, ""status"": 3, ""statusLabel"": ""Published"", ""submissionProgress"": """", ""locale"": ""en"",
        ""metadataLocales"": {""en"": ""English""}, ""issueToBePublished"": {""id"": 4, ""label"": ""Vol. 1 No. 1""},
        ""crossrefplugin::status"": null,
        ""stages"": [{""id"": 1, ""label"": ""Submission"", ""isActiveStage"": false, ""currentUserAssignedRoles"": [16, 1], ""uploadedFilesCount"": null}],
        ""reviewAssignments"": [{""id"": 81, ""dateDue"": ""2026-07-26"", ""round"": 1, ""roundId"": 169, ""reviewMethod"": 2}],
        ""participants"": [{""id"": 3, ""fullName"": ""Editor One"", ""canLoginAs"": true}],
        ""publications"": [
          {""id"": 8, ""version"": 1, ""title"": {""en"": ""Old""}},
          {""id"": 9, ""version"": 2, ""status"": 3, ""datePublished"": ""2026-06-30"", ""locale"": ""en"",
           ""title"": {""en"": ""A Title"", ""fr_CA"": """"},
           ""keywords"": {""en"": [{""name"": ""alpha""}, {""name"": ""beta ""}]},
           ""coverImage"": {""en"": null},
           ""authors"": [{""id"": 1, ""givenName"": {""en"": ""Ada""}, ""familyName"": {""en"": ""Lovelace""}, ""userGroupName"": {""en"": ""Author""},
                         ""affiliations"": [{""id"": 2, ""name"": {""en"": ""Uni""}, ""ror"": null}]}],
           ""galleys"": [{""id"": 11, ""label"": ""PDF"", ""file"": {""id"": 5, ""fileStage"": 10, ""createdAt"": ""2026-06-30 19:06:30"", ""revisions"": []}}]}
        ]}"

    <TestMethod>
    Public Sub Submission_Fixture_Deserializes()
        Dim s = OjsJson.Deserialize(Of Submission)(SubmissionJson)
        Assert.AreEqual(7, s.Id)
        Assert.AreEqual(WorkflowStage.Production, s.StageId)
        Assert.AreEqual(SubmissionStatus.Published, s.Status)
        Assert.AreEqual(4, s.IssueToBePublished.Id)
        Assert.AreEqual("English", s.MetadataLocales("en"))
        Assert.AreEqual(New DateOnly(2026, 7, 26), s.ReviewAssignments(0).DateDue)
        Assert.AreEqual(WorkflowStage.Submission, s.Stages(0).Id)
        Assert.IsTrue(s.AdditionalProperties.ContainsKey("crossrefplugin::status"))

        Dim p = s.CurrentPublication
        Assert.AreEqual(9, p.Id)
        Assert.AreEqual("A Title", p.Title.GetValue("fr_CA", "en"))
        CollectionAssert.AreEqual({"alpha", "beta"}, p.Keywords.GetNames("en").ToArray())
        Assert.AreEqual(New DateOnly(2026, 6, 30), p.DatePublished)
        Assert.IsNull(p.CoverImage("en"))
        Assert.AreEqual("Author", p.Authors(0).UserGroupName.GetValue("en"))
        Assert.AreEqual("Uni", p.Authors(0).Affiliations(0).Name.GetValue("en"))
        Assert.AreEqual(SubmissionFileStage.Proof, p.Galleys(0).File.FileStage)
    End Sub

    <TestMethod>
    Public Async Function Submissions_List_SendsFilters() As Task
        Dim handler = New FakeHandler().RespondJson("{""itemsMax"": 1, ""items"": [{""id"": 7}]}")
        Using ojs = TestClient.Create(handler)
            Dim page = Await ojs.Submissions.ListAsync(New SubmissionQuery With {
                .Count = 5, .Offset = 10, .Status = {SubmissionStatus.Published, SubmissionStatus.Scheduled},
                .StageIds = {WorkflowStage.Production}, .OrderBy = SubmissionOrderBy.DatePublished,
                .OrderDirection = SortDirection.Ascending, .SearchPhrase = "x y", .IsOverdue = False})
            Assert.AreEqual(7, page.Items(0).Id)
        End Using
        Assert.AreEqual("https://ojs.test/jpk/api/v1/submissions?count=5&offset=10&isOverdue=false&orderBy=datePublished&orderDirection=ASC&searchPhrase=x%20y&stageIds=5&status=3,5",
                        handler.Last.Uri.AbsoluteUri)
    End Function

    <TestMethod>
    Public Async Function Submissions_Create_Submit_SaveForLater_Delete() As Task
        Dim handler = New FakeHandler().RespondJson("{""id"": 1}").RespondJson("{""id"": 1}").RespondJson("{""id"": 1}").RespondJson("{""id"": 1}")
        Using ojs = TestClient.Create(handler)
            Await ojs.Submissions.CreateAsync(New SubmissionEdit With {.Locale = "en", .SectionId = 2})
            Assert.AreEqual(HttpMethod.Post, handler.Last.Method)
            Assert.AreEqual("{""locale"":""en"",""sectionId"":2}", handler.Last.Body)

            Await ojs.Submissions.SubmitAsync(1, confirmCopyright:=True, validateOnly:=True)
            Assert.AreEqual(HttpMethod.Put, handler.Last.Method)
            StringAssert.EndsWith(handler.Last.Uri.AbsolutePath, "/submissions/1/submit")
            Assert.AreEqual("{""confirmCopyright"":true,""_validateOnly"":true}", handler.Last.Body)

            Await ojs.Submissions.SaveForLaterAsync(1, "files")
            Assert.AreEqual("{""step"":""files""}", handler.Last.Body)

            Await ojs.Submissions.DeleteAsync(1)
            Assert.AreEqual(HttpMethod.Delete, handler.Last.Method)
        End Using
    End Function

    <TestMethod>
    Public Async Function Participants_ByStage_Path() As Task
        Dim handler = New FakeHandler().RespondJson("[{""id"": 3, ""fullName"": ""E""}]")
        Using ojs = TestClient.Create(handler)
            Dim users = Await ojs.Submissions.GetParticipantsAsync(7, WorkflowStage.ExternalReview)
            Assert.AreEqual("E", users(0).FullName)
        End Using
        StringAssert.EndsWith(handler.Last.Uri.AbsolutePath, "/submissions/7/participants/3")
    End Function

    <TestMethod>
    Public Async Function Publications_Update_Publish_ChangeLocale_Version() As Task
        Dim handler = New FakeHandler()
        Using ojs = TestClient.Create(handler)
            Await ojs.Publications.UpdateAsync(7, 9, New PublicationEdit With {
                .Title = LocalizedString.Create("en", "T"), .Keywords = LocalizedVocab.Create("en", "k"),
                .DatePublished = New DateOnly(2026, 1, 2)})
            Assert.AreEqual(HttpMethod.Put, handler.Last.Method)
            Assert.AreEqual("{""datePublished"":""2026-01-02"",""keywords"":{""en"":[{""name"":""k""}]},""title"":{""en"":""T""}}", handler.Last.Body)

            Await ojs.Publications.PublishAsync(7, 9)
            StringAssert.EndsWith(handler.Last.Uri.AbsolutePath, "/submissions/7/publications/9/publish")
            Assert.AreEqual(HttpMethod.Put, handler.Last.Method)

            Await ojs.Publications.ChangeLocaleAsync(7, 9, "id")
            Assert.AreEqual("{""locale"":""id""}", handler.Last.Body)

            Await ojs.Publications.CreateVersionAsync(7, 9)
            Assert.AreEqual(HttpMethod.Post, handler.Last.Method)
            StringAssert.EndsWith(handler.Last.Uri.AbsolutePath, "/publications/9/version")
        End Using
    End Function

    <TestMethod>
    Public Async Function Publications_UploadJats_Multipart() As Task
        Dim handler = New FakeHandler().RespondJson("{""jatsContent"": ""<article/>"", ""isDefaultContent"": false}")
        Using ojs = TestClient.Create(handler)
            Using ms As New IO.MemoryStream(Text.Encoding.UTF8.GetBytes("<article/>"))
                Dim jats = Await ojs.Publications.UploadJatsAsync(7, 9, ms, "a.xml")
                Assert.IsFalse(jats.IsDefaultContent.Value)
            End Using
        End Using
        Assert.AreEqual("multipart/form-data", handler.Last.ContentType)
        StringAssert.Contains(handler.Last.Body, "name=file")
    End Function

    <TestMethod>
    Public Async Function Contributors_PagedOrArray_And_SaveOrder() As Task
        Dim handler = New FakeHandler().
            RespondJson("{""itemsMax"": 2, ""items"": [{""id"": 1}, {""id"": 2}]}").
            RespondJson("[{""id"": 2}, {""id"": 1}]")
        Using ojs = TestClient.Create(handler)
            Dim list = Await ojs.Contributors.ListAsync(7, 9)
            Assert.HasCount(2, list)
            Dim reordered = Await ojs.Contributors.SaveOrderAsync(7, 9, {2, 1})
            Assert.AreEqual(2, reordered(0).Id)
        End Using
        StringAssert.EndsWith(handler.Last.Uri.AbsolutePath, "/contributors/saveOrder")
        Assert.AreEqual("{""sortedAuthors"":[{""id"":2,""seq"":0},{""id"":1,""seq"":1}]}", handler.Last.Body)
    End Function

    <TestMethod>
    Public Async Function Contributors_Create_WithAffiliations() As Task
        Dim handler = New FakeHandler()
        Using ojs = TestClient.Create(handler)
            Await ojs.Contributors.CreateAsync(7, 9, New ContributorEdit With {
                .GivenName = LocalizedString.Create("en", "Ada"), .Email = "a@example.com", .UserGroupId = 14,
                .Affiliations = New List(Of Affiliation) From {New Affiliation With {.Name = LocalizedString.Create("en", "Uni")}}})
        End Using
        Dim body = JsonNode.Parse(handler.Last.Body)
        Assert.AreEqual("Uni", body("affiliations")(0)("name")("en").GetValue(Of String)())
        Assert.AreEqual(14, body("userGroupId").GetValue(Of Integer)())
    End Function

    <TestMethod>
    Public Async Function SubmissionFiles_Upload_FormFields_And_StageQuery() As Task
        Dim handler = New FakeHandler().RespondJson("{""id"": 5, ""fileStage"": 2}").RespondJson("{""id"": 5}").RespondJson("{""id"": 6}").RespondJson("{""itemsMax"": 1, ""items"": [{""id"": 5}]}")
        Using ojs = TestClient.Create(handler)
            Using ms As New IO.MemoryStream(Text.Encoding.UTF8.GetBytes("doc"))
                Dim f = Await ojs.SubmissionFiles.UploadAsync(7, ms, "m.docx", New SubmissionFileEdit With {
                    .FileStage = SubmissionFileStage.Submission, .GenreId = 1, .Name = LocalizedString.Create("en", "m.docx"), .Viewable = True})
                Assert.AreEqual(SubmissionFileStage.Submission, f.FileStage)
            End Using
            Dim body = handler.Last.Body
            StringAssert.Contains(body, "name=fileStage")
            StringAssert.Contains(body, "name=""name[en]""")
            StringAssert.Contains(body, "name=viewable")

            Await ojs.SubmissionFiles.GetAsync(7, 5, WorkflowStage.Submission)
            Assert.AreEqual("?stageId=1", handler.Last.Uri.Query)

            Await ojs.SubmissionFiles.CopyAsync(7, 5, WorkflowStage.ExternalReview, SubmissionFileStage.ReviewFile, reviewRoundId:=3)
            Assert.AreEqual(HttpMethod.Put, handler.Last.Method)
            Assert.AreEqual("?stageId=3", handler.Last.Uri.Query)
            Assert.AreEqual("{""toFileStage"":4,""reviewRoundId"":3}", handler.Last.Body)

            Dim files = Await ojs.SubmissionFiles.ListAsync(7, fileStages:={SubmissionFileStage.Submission, SubmissionFileStage.ReviewFile})
            Assert.HasCount(1, files)
            Assert.AreEqual("?fileStages=2,4", handler.Last.Uri.Query)
        End Using
    End Function

    <TestMethod>
    Public Async Function Decisions_ListAndCreate() As Task
        Dim handler = New FakeHandler().RespondJson("[{""id"": 1, ""decision"": 7, ""stageId"": 4, ""dateDecided"": ""2026-06-29 23:06:12""}]").RespondJson("{""id"": 2}")
        Using ojs = TestClient.Create(handler)
            Dim list = Await ojs.Decisions.ListAsync(7, New DecisionQuery With {.StageId = WorkflowStage.Editing, .DecisionTypes = {7}})
            Assert.AreEqual(WorkflowStage.Editing, list(0).StageId)
            Assert.AreEqual("?decisionTypes=7&stageId=4", handler.Last.Uri.Query)

            Await ojs.Decisions.CreateAsync(7, New DecisionEdit With {.Decision = 7,
                .Actions = New List(Of Object) From {New JsonObject From {{"id", "notifyAuthors"}, {"subject", "S"}}}})
            Assert.AreEqual("{""actions"":[{""id"":""notifyAuthors"",""subject"":""S""}],""decision"":7}", handler.Last.Body)
        End Using
    End Function

    <TestMethod>
    Public Async Function ReviewerSuggestions_And_Issues_Sections() As Task
        Dim handler = New FakeHandler().
            RespondJson("{""items"": [], ""itemsMax"": 0}").
            RespondJson("[]").
            RespondJson("[{""id"": 1}, {""id"": 2}]").
            RespondJson("{""id"": 32, ""datePublished"": ""2026-06-30 00:00:00"", ""articles"": [{""id"": 7}]}").
            RespondJson("{""itemsMax"": 1, ""items"": [{""id"": 1, ""title"": {""en"": ""Articles""}}]}")
        Using ojs = TestClient.Create(handler)
            Dim sugg = Await ojs.ReviewerSuggestions.ListAsync(7, approved:=True, includeReviewerData:=True)
            Assert.AreEqual("?approved=true&include_reviewer_data=true", handler.Last.Uri.Query)
            Assert.IsEmpty(sugg.Items)

            Await ojs.ReviewerSuggestions.DeleteAsync(7, 3)
            Assert.AreEqual(HttpMethod.Delete, handler.Last.Method)

            ' a bare array is accepted where a paged result is expected
            Dim issues = Await ojs.Issues.ListAsync(New IssueQuery With {.IsPublished = True, .Years = {2025, 2026}, .OrderBy = IssueOrderBy.Sequence})
            Assert.AreEqual(2, issues.ItemsMax)
            Assert.AreEqual("?isPublished=true&orderBy=seq&years=2025,2026", SortQuery(handler.Last.Uri.Query))

            Dim current = Await ojs.Issues.GetCurrentAsync()
            Assert.AreEqual(7, current.Articles(0).Id)
            StringAssert.EndsWith(handler.Last.Uri.AbsolutePath, "/issues/current")

            Dim sections = Await ojs.Sections.ListAsync(New SectionQuery With {.IsInactive = False})
            Assert.AreEqual("Articles", sections.Items(0).Title.GetValue("en"))
        End Using
    End Function

    Private Shared Function SortQuery(q As String) As String
        Return "?" & String.Join("&", q.TrimStart("?"c).Split("&"c).OrderBy(Function(x) x, StringComparer.Ordinal))
    End Function

End Class
