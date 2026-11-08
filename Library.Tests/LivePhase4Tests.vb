Imports System.Net
Imports System.Text
Imports System.Text.Json
Imports OJS3_5.Library.Errors
Imports OJS3_5.Library.Models
Imports OJS3_5.Library.Queries

''' <summary>
''' Live tests for DOIs, reviews, invitations and statistics. Read tests run with the "Live" settings; tests that
''' change data (category LiveWrite) also need OJS_ALLOW_WRITES=1 and must only target a dummy journal.
''' OJS_AUTHOR_TOKEN (optional) is an API token of a user with only the author role.
''' </summary>
<TestClass>
<DoNotParallelize>
Public Class LivePhase4Tests

    Public Property TestContext As TestContext

    Private Shared Function Env(name As String) As String
        Return Environment.GetEnvironmentVariable(name)
    End Function

    Private Shared Function CreateClient(Optional writes As Boolean = False, Optional token As String = Nothing) As OjsClient
        If String.IsNullOrEmpty(Env("OJS_BASE_URL")) OrElse String.IsNullOrEmpty(Env("OJS_TOKEN")) OrElse String.IsNullOrEmpty(Env("OJS_JOURNAL")) Then
            Assert.Inconclusive("Live tests skipped: set OJS_BASE_URL, OJS_JOURNAL and OJS_TOKEN.")
        End If
        If writes AndAlso Env("OJS_ALLOW_WRITES") <> "1" Then Assert.Inconclusive("Write tests skipped: set OJS_ALLOW_WRITES=1 (dummy journal only).")
        Return New OjsClient(New OjsClientOptions With {
            .BaseUrl = Env("OJS_BASE_URL"), .JournalPath = Env("OJS_JOURNAL"), .ApiToken = If(token, Env("OJS_TOKEN")),
            .UseIndexPhp = Env("OJS_USE_INDEX_PHP") = "1", .Timeout = TimeSpan.FromSeconds(90)})
    End Function

    Private Sub ReportUnmapped(name As String, extra As Dictionary(Of String, JsonElement))
        If extra Is Nothing Then Return
        Dim keys = extra.Keys.Where(Function(k) Not k.Contains("plugin") AndAlso Not k.Contains("::")).ToList()
        If keys.Count > 0 Then TestContext.WriteLine($"{name} unmapped: {String.Join(", ", keys)}")
    End Sub

    ''' <summary>Finds a submission with a completed review: (submissionId, reviewAssignmentId, reviewRoundId).</summary>
    Private Shared Async Function FindCompletedReviewAsync(ojs As OjsClient) As Task(Of (Integer, Integer, Integer))
        For Each s In Await ojs.Submissions.ListAllAsync(New SubmissionQuery With {.StageIds = {WorkflowStage.ExternalReview, WorkflowStage.InternalReview}})
            Dim full = Await ojs.Submissions.GetAsync(s.Id.Value)
            Dim done = full.ReviewAssignments?.FirstOrDefault(Function(r) r.DateCompleted.HasValue)
            If done IsNot Nothing Then Return (full.Id.Value, done.Id.Value, done.RoundId.Value)
        Next
        Assert.Inconclusive("No submission with a completed review.")
        Return Nothing
    End Function

    <TestMethod, TestCategory("Live")>
    Public Async Function Stats_EditorialUsersAndUsage() As Task
        Using ojs = CreateClient()
            Dim editorial = Await ojs.Stats.GetEditorialAsync()
            Assert.IsTrue(editorial.Any(Function(s) s.Key = "submissionsReceived"))
            ReportUnmapped("EditorialStat", editorial(0).AdditionalProperties)
            Dim averages = Await ojs.Stats.GetEditorialAveragesAsync()
            Assert.IsTrue(averages.ContainsKey("submissionsReceived"))
            Dim users = Await ojs.Stats.GetUsersAsync()
            Assert.IsTrue(users.Any(Function(u) u.IsTotal))
            Assert.IsTrue(users.Any(Function(u) u.Role.GetValueOrDefault() = UserRole.Reviewer))

            Dim range As New PublicationStatsQuery With {.DateStart = New DateOnly(2026, 1, 1), .DateEnd = DateOnly.FromDateTime(Date.Today.AddDays(-1))}
            Dim pubs = Await ojs.Stats.Publications.ListAsync(range)
            TestContext.WriteLine($"Publications with views: {pubs.ItemsMax}")
            If pubs.Items.Count > 0 Then
                ReportUnmapped("PublicationStats", pubs.Items(0).AdditionalProperties)
                ReportUnmapped("PublicationStatsItem", pubs.Items(0).Publication.AdditionalProperties)
                Dim one = Await ojs.Stats.Publications.GetAsync(pubs.Items(0).Publication.Id.Value, range.DateStart, range.DateEnd)
                Assert.AreEqual(pubs.Items(0).AbstractViews, one.AbstractViews)
                TestContext.WriteLine($"Top article: {pubs.Items(0).Publication.FullTitle} {pubs.Items(0).AbstractViews} abstract / {pubs.Items(0).GalleyViews} galley views")
            End If
            Dim timeline = Await ojs.Stats.Publications.GetTimelineAsync(range, TimelineResolution.Day)
            Assert.IsNotEmpty(timeline)
            TestContext.WriteLine($"Timeline: {timeline.Count} days, {timeline.Sum(Function(t) t.Value.GetValueOrDefault())} views")
            Dim files = Await ojs.Stats.Publications.ListFilesAsync(range)
            If files.Items.Count > 0 Then ReportUnmapped("PublicationFileStats", files.Items(0).AdditionalProperties)
            Dim countries = Await ojs.Stats.Publications.ListCountriesAsync(range)
            Dim regions = Await ojs.Stats.Publications.ListRegionsAsync(range)
            Dim cities = Await ojs.Stats.Publications.ListCitiesAsync(range)
            TestContext.WriteLine($"Geo: {countries.ItemsMax} countries, {regions.ItemsMax} regions, {cities.ItemsMax} cities")
            If cities.Items.Count > 0 Then ReportUnmapped("GeoStats", cities.Items(0).AdditionalProperties)

            Dim csv = Await ojs.Stats.Publications.ListCsvAsync(range)
            Assert.DoesNotStartWith("{", csv.TrimStart(), "Expected CSV, got JSON.")
            TestContext.WriteLine("CSV header: " & csv.Split(ChrW(10))(0))
            Dim timelineCsv = Await ojs.Stats.Publications.GetTimelineCsvAsync(range)
            Assert.DoesNotStartWith("[", timelineCsv.TrimStart(), "Expected CSV, got JSON.")

            Dim issues = Await ojs.Stats.Issues.ListAsync(New IssueStatsQuery With {.DateStart = range.DateStart, .DateEnd = range.DateEnd})
            If issues.Items.Count > 0 Then
                ReportUnmapped("IssueStats", issues.Items(0).AdditionalProperties)
                Dim one = Await ojs.Stats.Issues.GetAsync(issues.Items(0).Issue.Id.Value, range.DateStart, range.DateEnd)
                Assert.AreEqual(issues.Items(0).TotalViews, one.TotalViews)
            End If
            Await ojs.Stats.Issues.GetTimelineAsync(New IssueStatsQuery With {.DateStart = range.DateStart, .DateEnd = range.DateEnd})

            Dim journal = (Await ojs.Contexts.ListAllAsync()).Single(Function(c) c.UrlPath = ojs.Options.JournalPath)
            Dim home = Await ojs.Stats.Contexts.GetAsync(journal.Id.Value, range.DateStart, range.DateEnd)
            ReportUnmapped("ContextStats", home.AdditionalProperties)
            TestContext.WriteLine($"Journal homepage views: {home.Total}")
            Dim homeTimeline = Await ojs.Stats.Contexts.GetTimelineAsync(journal.Id.Value, TimelineResolution.Month, range.DateStart, range.DateEnd)
            Assert.IsNotNull(homeTimeline)
        End Using
    End Function

    <TestMethod, TestCategory("Live")>
    Public Async Function Reviews_ExportPdfAndXml_And_HistoryNeedsReviewer() As Task
        Using ojs = CreateClient()
            Dim found = Await FindCompletedReviewAsync(ojs)
            Dim submissionId = found.Item1, reviewId = found.Item2, roundId = found.Item3

            Using pdf = Await ojs.Reviews.DownloadPdfAsync(submissionId, reviewId, authorFriendly:=True)
                Dim bytes = Await pdf.ReadAllBytesAsync()
                Assert.AreEqual("%PDF", Encoding.ASCII.GetString(bytes, 0, 4))
                TestContext.WriteLine($"PDF {pdf.FileName} {bytes.Length} bytes ({pdf.ContentType})")
            End Using
            Using xml = Await ojs.Reviews.DownloadXmlAsync(submissionId, reviewId, authorFriendly:=False)
                Dim text = Encoding.UTF8.GetString(Await xml.ReadAllBytesAsync())
                StringAssert.Contains(text, "<")
                TestContext.WriteLine($"XML {xml.FileName} ({xml.ContentType}): {text.Substring(0, Math.Min(120, text.Length)).Replace(vbLf, " ")}")
            End Using

            ' The history endpoint is for the reviewer who wrote the review; an admin gets 404.
            Dim ex = Await Assert.ThrowsAsync(Of OjsApiException)(Function() ojs.Reviews.GetHistoryAsync(submissionId, roundId))
            Assert.AreEqual(HttpStatusCode.NotFound, ex.StatusCode)
        End Using
    End Function

    <TestMethod, TestCategory("Live")>
    Public Async Function AuthorToken_IsForbiddenFromEditorialEndpoints() As Task
        Dim authorToken = Env("OJS_AUTHOR_TOKEN")
        If String.IsNullOrEmpty(authorToken) Then Assert.Inconclusive("Set OJS_AUTHOR_TOKEN to run this test.")
        Using ojs = CreateClient(token:=authorToken)
            ' OJS answers 401 (not 403) when the token is valid but the role is not allowed.
            Dim ex = Await Assert.ThrowsAsync(Of OjsApiException)(Function() ojs.Users.ListAsync())
            Assert.IsTrue(ex.StatusCode = HttpStatusCode.Unauthorized OrElse ex.StatusCode = HttpStatusCode.Forbidden, ex.StatusCode.ToString())
            TestContext.WriteLine($"Users as author: {CInt(ex.StatusCode)} {ex.Error} - {ex.ErrorMessage}")

            Dim ex2 = Await Assert.ThrowsAsync(Of OjsApiException)(Function() ojs.Stats.GetEditorialAsync())
            TestContext.WriteLine($"Editorial stats as author: {CInt(ex2.StatusCode)} {ex2.Error}")

            ' An author can still read their own submissions.
            Dim mine = Await ojs.Submissions.ListAsync()
            TestContext.WriteLine($"Author sees {mine.ItemsMax} submission(s)")
        End Using
    End Function

    <TestMethod, TestCategory("LiveWrite")>
    Public Async Function Dois_EnableAssignExportAndMark() As Task
        Using ojs = CreateClient(writes:=True)
            Dim journal = (Await ojs.Contexts.ListAllAsync()).Single(Function(c) c.UrlPath = ojs.Options.JournalPath)
            Dim settings = Await ojs.Contexts.GetAsync(journal.Id.Value)
            If settings.EnableDois <> True Then
                ' OJS validates the prefix together with the switch, so it has to be sent again.
                settings = Await ojs.Contexts.UpdateAsync(journal.Id.Value, New ContextEdit With {.EnableDois = True, .DoiPrefix = settings.DoiPrefix})
                Assert.IsTrue(settings.EnableDois)
                TestContext.WriteLine("Enabled DOIs for the journal.")
            End If
            TestContext.WriteLine($"DOI prefix {settings.DoiPrefix}, types {String.Join(",", If(settings.EnabledDoiTypes, New List(Of String)))}, agency {settings.RegistrationAgency}")

            Dim published = Await ojs.Submissions.ListAllAsync(New SubmissionQuery With {.Status = {SubmissionStatus.Published}})
            If published.Count = 0 Then Assert.Inconclusive("No published submissions.")
            Dim ids = published.Select(Function(s) s.Id.Value).ToArray()

            Dim assigned = Await ojs.Dois.Submissions.AssignAsync(ids)
            TestContext.WriteLine($"Assign: succeeded={assigned.Succeeded} {String.Join(" | ", If(assigned.FailedDoiActions, New List(Of String)))}")
            Dim pub = (Await ojs.Submissions.GetAsync(ids(0))).CurrentPublication
            Assert.IsNotNull(pub.DoiObject, "The publication should have a DOI after assigning.")
            StringAssert.StartsWith(pub.DoiObject.Doi, settings.DoiPrefix)
            TestContext.WriteLine($"Publication {pub.Id} DOI {pub.DoiObject.Doi} status {pub.DoiObject.Status}")

            Dim doi = Await ojs.Dois.GetAsync(pub.DoiObject.Id.Value)
            ReportUnmapped("Doi", doi.AdditionalProperties)
            Dim list = Await ojs.Dois.ListAsync(New DoiQuery With {.Count = 50})
            Assert.IsTrue(list.Items.Any(Function(d) Nullable.Equals(d.Id, doi.Id)))

            Await ojs.Dois.Submissions.MarkRegisteredAsync({ids(0)})
            Assert.AreEqual(DoiStatus.Registered, (Await ojs.Dois.GetAsync(doi.Id.Value)).Status)
            Await ojs.Dois.Submissions.MarkStaleAsync({ids(0)})
            Assert.AreEqual(DoiStatus.Stale, (Await ojs.Dois.GetAsync(doi.Id.Value)).Status)
            Await ojs.Dois.Submissions.MarkUnregisteredAsync({ids(0)})
            Assert.AreEqual(DoiStatus.Unregistered, (Await ojs.Dois.GetAsync(doi.Id.Value)).Status)

            Try
                Using xml = Await ojs.Dois.Submissions.ExportAndDownloadAsync({ids(0)})
                    Dim text = Encoding.UTF8.GetString(Await xml.ReadAllBytesAsync())
                    StringAssert.Contains(text, "doi_batch")
                    TestContext.WriteLine($"Export {xml.FileName} ({xml.ContentType}) {text.Length} chars")
                End Using
            Catch ex As OjsApiException
                ' Crossref refuses to export articles with incomplete metadata; report rather than fail.
                TestContext.WriteLine($"Export refused: {CInt(ex.StatusCode)} {ex.Error} {ex.ErrorMessage}")
            End Try

            Dim issues = Await ojs.Issues.ListAllAsync(New IssueQuery With {.IsPublished = True})
            If issues.Count > 0 Then
                Dim issueResult = Await ojs.Dois.Issues.AssignAsync({issues(0).Id.Value})
                TestContext.WriteLine($"Issue assign: succeeded={issueResult.Succeeded} {String.Join(" | ", If(issueResult.FailedDoiActions, New List(Of String)))}")
                Dim issue = Await ojs.Issues.GetAsync(issues(0).Id.Value)
                TestContext.WriteLine($"Issue DOI: {issue.DoiObject?.Doi}")
            End If
        End Using
    End Function

    <TestMethod, TestCategory("LiveWrite")>
    Public Async Function Invitations_CreatePopulateInviteCancel() As Task
        Using ojs = CreateClient(writes:=True)
            ' Invite a reviewer to the author role (a group they do not have yet).
            Dim users = Await ojs.Users.ListAllAsync()
            Dim authorGroup = users.SelectMany(Function(u) u.Groups).FirstOrDefault(Function(g) g.RoleId.GetValueOrDefault() = CInt(UserRole.Author))
            Dim invitee = users.FirstOrDefault(Function(u) u.Groups.All(Function(g) g.RoleId.GetValueOrDefault() <> CInt(UserRole.Author) AndAlso g.RoleId.GetValueOrDefault() <> CInt(UserRole.Manager)))
            If authorGroup Is Nothing OrElse invitee Is Nothing Then Assert.Inconclusive("Need an author group and a user without it.")

            Dim id = Await ojs.Invitations.CreateForUserAsync(InvitationTypes.UserRoleAssignment, invitee.Id.Value)
            Assert.IsGreaterThan(0, id)

            ' Sending without roles must fail validation (422).
            Dim invalid = Await Assert.ThrowsAsync(Of OjsValidationException)(Function() ojs.Invitations.InviteAsync(id))
            TestContext.WriteLine("Invite without roles: " & String.Join("; ", invalid.Errors.Select(Function(kv) $"{kv.Key}: {String.Join(" ", kv.Value)}")))

            Dim populated = Await ojs.Invitations.PopulateAsync(id, New UserRoleAssignmentData With {
                .UserGroupsToAdd = New List(Of InvitationUserGroup) From {
                    New InvitationUserGroup With {.UserGroupId = authorGroup.Id, .Masthead = False, .DateStart = DateOnly.FromDateTime(Date.Today)}},
                .EmailComposer = New InvitationEmail With {.EmailSubject = "Library test invitation", .EmailBody = "<p>Test only.</p>"}})
            ReportUnmapped("Invitation", populated.AdditionalProperties)
            Assert.AreEqual(authorGroup.Id, populated.UserGroupsToAdd(0).UserGroupId)

            Assert.AreEqual(invitee.Email, populated.ExistingUser.Email)
            ReportUnmapped("InvitationUser", populated.ExistingUser.AdditionalProperties)

            Dim preview = Await ojs.Invitations.GetMailableAsync(id)
            Assert.IsFalse(String.IsNullOrEmpty(preview.Subject))
            StringAssert.Contains(preview.View, "{$acceptUrl}")
            TestContext.WriteLine($"Invitation email: {preview.Subject} ({preview.ViewData?.Count} variables)")

            Dim sent = Await ojs.Invitations.InviteAsync(id)
            Assert.AreEqual(InvitationStatus.Pending, sent.StatusValue)

            Dim pending = Await ojs.Invitations.ListAsync(InvitationTypes.UserRoleAssignment, New InvitationQuery With {.Status = InvitationStatus.Pending})
            Assert.IsTrue(pending.Items.Any(Function(i) i.Id.GetValueOrDefault() = id))

            Await ojs.Invitations.CancelAsync(id)
            Dim after = Await ojs.Invitations.ListAsync(InvitationTypes.UserRoleAssignment, New InvitationQuery With {.Status = InvitationStatus.Pending})
            Assert.IsFalse(after.Items.Any(Function(i) i.Id.GetValueOrDefault() = id), "Cancelled invitation is still pending.")
        End Using
    End Function

    <TestMethod, TestCategory("Live")>
    Public Async Function Reviews_HistoryAsReviewer() As Task
        Dim reviewerToken = Env("OJS_REVIEWER_TOKEN")
        If String.IsNullOrEmpty(reviewerToken) Then Assert.Inconclusive("Set OJS_REVIEWER_TOKEN (the reviewer of a completed review).")
        Dim found As (Integer, Integer, Integer)
        Using admin = CreateClient()
            found = Await FindCompletedReviewAsync(admin)
        End Using
        Using ojs = CreateClient(token:=reviewerToken)
            Dim history = Await ojs.Reviews.GetHistoryAsync(found.Item1, found.Item3)
            ReportUnmapped("ReviewHistory", history.AdditionalProperties)
            ReportUnmapped("ReviewAssignment", history.ReviewAssignment.AdditionalProperties)
            Assert.AreEqual(found.Item2, history.ReviewAssignment.Id)
            Assert.IsFalse(String.IsNullOrEmpty(history.PublicationTitle.GetValue("en")))
            Assert.IsFalse(String.IsNullOrEmpty(history.Recommendation))
            TestContext.WriteLine($"History: {history.PublicationTitle} | {history.Recommendation} | {history.Comments?.Count} comment(s), {history.Files?.Count} file(s), keywords {String.Join(",", If(history.PublicationKeywords?.GetNames("en"), Array.Empty(Of String)()))}")
        End Using
    End Function

    <TestMethod, TestCategory("LiveWrite")>
    Public Async Function Reviews_ConfirmOnBehalfOfReviewer() As Task
        ' A reviewer's own token is refused; managers accept on the reviewer's behalf.
        Dim reviewerToken = Env("OJS_REVIEWER2_TOKEN")
        Using ojs = CreateClient(writes:=True)
            Dim target As (Integer, Integer)? = Nothing
            For Each s In Await ojs.Submissions.ListAllAsync(New SubmissionQuery With {.StageIds = {WorkflowStage.ExternalReview, WorkflowStage.InternalReview}})
                Dim full = Await ojs.Submissions.GetAsync(s.Id.Value)
                Dim open = full.ReviewAssignments?.FirstOrDefault(Function(r) Not r.DateConfirmed.HasValue AndAlso Not r.DateCancelled.HasValue AndAlso Not r.DateCompleted.HasValue)
                If open IsNot Nothing Then target = (full.Id.Value, open.Id.Value) : Exit For
            Next
            If target Is Nothing Then Assert.Inconclusive("No unanswered review request.")

            If Not String.IsNullOrEmpty(reviewerToken) Then
                Using asReviewer = CreateClient(writes:=True, token:=reviewerToken)
                    Dim denied = Await Assert.ThrowsAsync(Of OjsApiException)(Function() asReviewer.Reviews.ConfirmAsync(target.Value.Item1, target.Value.Item2, accept:=True))
                    Assert.AreEqual(HttpStatusCode.Unauthorized, denied.StatusCode)
                End Using
            End If

            Dim confirmed = Await ojs.Reviews.ConfirmAsync(target.Value.Item1, target.Value.Item2, accept:=True)
            Assert.IsNotNull(confirmed, "Expected the review assignment in the response.")
            ReportUnmapped("ReviewAssignment (confirm)", confirmed.AdditionalProperties)
            Assert.AreEqual(target.Value.Item2, confirmed.Id)

            ' The response carries the state before the change; the submission shows the new one.
            Dim after = Await ojs.Submissions.GetAsync(target.Value.Item1)
            Dim ra = after.ReviewAssignments.Single(Function(r) r.Id.GetValueOrDefault() = target.Value.Item2)
            Assert.IsNotNull(ra.DateConfirmed)
            TestContext.WriteLine($"Accepted review {ra.Id} on submission {target.Value.Item1} on behalf of {ra.ReviewerFullName}: {ra.Status}")
        End Using
    End Function

    <TestMethod, TestCategory("Live")>
    Public Async Function EditorToken_SeesAssignedWork() As Task
        Dim editorToken = Env("OJS_EDITOR_TOKEN")
        If String.IsNullOrEmpty(editorToken) Then Assert.Inconclusive("Set OJS_EDITOR_TOKEN.")
        Using ojs = CreateClient(token:=editorToken)
            Dim page = Await ojs.Submissions.ListAsync()
            TestContext.WriteLine($"Section editor sees {page.ItemsMax} submission(s)")
            Try
                Dim users = Await ojs.Users.ListAsync()
                TestContext.WriteLine($"Section editor can list {users.ItemsMax} user(s)")
            Catch ex As OjsApiException
                TestContext.WriteLine($"Section editor cannot list users: {CInt(ex.StatusCode)} {ex.Error}")
            End Try
        End Using
    End Function

    <TestMethod, TestCategory("LiveWrite")>
    Public Async Function Orcid_RequestVerificationAndDelete() As Task
        Using ojs = CreateClient(writes:=True)
            Dim published = Await ojs.Submissions.ListAsync(New SubmissionQuery With {.Status = {SubmissionStatus.Published}, .Count = 1})
            If published.Items.Count = 0 Then Assert.Inconclusive("No published submissions.")
            Dim full = Await ojs.Submissions.GetAsync(published.Items(0).Id.Value)
            Dim pub = Await ojs.Publications.GetAsync(full.Id.Value, full.CurrentPublicationId.Value)
            Dim author = pub.Authors.FirstOrDefault(Function(a) String.IsNullOrEmpty(a.Orcid))
            If author Is Nothing Then Assert.Inconclusive("No contributor without an ORCID iD.")

            Try
                Await ojs.Orcid.RequestAuthorVerificationAsync(author.Id.Value)
                TestContext.WriteLine($"Verification request sent to contributor {author.Id} ({author.Email})")
            Catch ex As OjsApiException
                TestContext.WriteLine($"Request verification: {CInt(ex.StatusCode)} {ex.Error} {ex.ErrorMessage}")
                Throw
            End Try

            ' Removing an ORCID iD that is not set must be harmless.
            Await ojs.Orcid.DeleteForAuthorAsync(author.Id.Value)
            Dim after = Await ojs.Contributors.GetAsync(full.Id.Value, full.CurrentPublicationId.Value, author.Id.Value)
            Assert.IsTrue(String.IsNullOrEmpty(after.Orcid))
        End Using
    End Function

    <TestMethod, TestCategory("LiveWrite")>
    Public Async Function Reviews_SendToOrcid_ReportsOutcome() As Task
        Using ojs = CreateClient(writes:=True)
            Dim found = Await FindCompletedReviewAsync(ojs)
            Try
                Await ojs.Reviews.SendToOrcidAsync(found.Item1, found.Item2)
                TestContext.WriteLine("sendToOrcid accepted (deposit job queued).")
            Catch ex As OjsApiException
                ' Expected when the reviewer has not linked an ORCID iD or the journal uses the public API.
                TestContext.WriteLine($"sendToOrcid refused: {CInt(ex.StatusCode)} {ex.Error} {ex.ErrorMessage}")
                Assert.IsGreaterThanOrEqualTo(400, CInt(ex.StatusCode))
            End Try
        End Using
    End Function

End Class
