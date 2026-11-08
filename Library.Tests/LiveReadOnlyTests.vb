Imports System.Text.Json
Imports OJS3_5.Library.Models
Imports OJS3_5.Library.Queries
Imports OJS3_5.Library.Resources

''' <summary>
''' Read-only smoke tests against a real OJS 3.5 journal. They only issue GET requests.
''' Set OJS_BASE_URL, OJS_JOURNAL, OJS_TOKEN (and optionally OJS_USE_INDEX_PHP=1) to run them; otherwise they are skipped.
''' </summary>
<TestClass>
<DoNotParallelize>
<TestCategory("Live")>
Public Class LiveReadOnlyTests

    Public Property TestContext As TestContext

    Private Shared Function CreateLiveClient(Optional requireJournal As Boolean = True) As OjsClient
        Dim baseUrl = Environment.GetEnvironmentVariable("OJS_BASE_URL")
        Dim token = Environment.GetEnvironmentVariable("OJS_TOKEN")
        Dim journal = Environment.GetEnvironmentVariable("OJS_JOURNAL")
        If String.IsNullOrEmpty(baseUrl) OrElse String.IsNullOrEmpty(token) Then
            Assert.Inconclusive("Live tests skipped: set OJS_BASE_URL, OJS_JOURNAL and OJS_TOKEN.")
        End If
        If requireJournal AndAlso String.IsNullOrEmpty(journal) Then
            Assert.Inconclusive("Journal-scoped live test skipped: set OJS_JOURNAL.")
        End If
        Return New OjsClient(New OjsClientOptions With {
            .BaseUrl = baseUrl,
            .JournalPath = If(String.IsNullOrEmpty(journal), OjsClientOptions.SiteWidePath, journal),
            .UseIndexPhp = Environment.GetEnvironmentVariable("OJS_USE_INDEX_PHP") = "1",
            .ApiToken = token,
            .Timeout = TimeSpan.FromSeconds(60)})
    End Function

    ''' <summary>Logs properties that landed in AdditionalProperties (excluding plugin keys) to spot model gaps.</summary>
    Private Sub ReportUnmapped(name As String, extra As Dictionary(Of String, JsonElement))
        If extra Is Nothing Then Return
        Dim keys = extra.Keys.Where(Function(k) Not k.Contains("plugin") AndAlso Not k.Contains("::")).ToList()
        If keys.Count > 0 Then TestContext.WriteLine($"{name} unmapped: {String.Join(", ", keys)}")
    End Sub

    <TestMethod>
    Public Async Function Submissions_ListAndGet() As Task
        Using ojs = CreateLiveClient()
            Dim page = Await ojs.Submissions.ListAsync(New SubmissionQuery With {.Count = 5, .Status = {SubmissionStatus.Published}})
            If page.ItemsMax = 0 Then Assert.Inconclusive("The journal has no published submissions.")
            Assert.IsNotEmpty(page.Items)
            Dim summary = page.Items(0)
            Assert.IsNotNull(summary.Id)
            Assert.AreEqual(SubmissionStatus.Published, summary.Status)

            Dim full = Await ojs.Submissions.GetAsync(summary.Id.Value)
            ReportUnmapped("Submission", full.AdditionalProperties)
            Assert.IsNotNull(full.CurrentPublication)
            Assert.IsNotNull(full.DateSubmitted)
            Assert.IsNotNull(full.Stages)

            Dim pub = Await ojs.Publications.GetAsync(full.Id.Value, full.CurrentPublicationId.Value)
            ReportUnmapped("Publication", pub.AdditionalProperties)
            Assert.IsFalse(String.IsNullOrEmpty(pub.Title.GetValue(pub.Locale)))
            Assert.IsNotNull(pub.DatePublished)
            If pub.Authors.Count > 0 Then ReportUnmapped("Contributor", pub.Authors(0).AdditionalProperties)
            If pub.Galleys.Count > 0 Then ReportUnmapped("Galley", pub.Galleys(0).AdditionalProperties)
            TestContext.WriteLine($"{full.Id}: {pub.Title.GetValue(pub.Locale)} | keywords: {String.Join(", ", If(pub.Keywords?.GetNames(pub.Locale), Array.Empty(Of String)()))}")

            Dim pubs = Await ojs.Publications.ListAsync(full.Id.Value)
            Assert.IsNotEmpty(pubs)

            Dim contributors = Await ojs.Contributors.ListAsync(full.Id.Value, pub.Id.Value)
            Assert.IsNotEmpty(contributors)
            Dim one = Await ojs.Contributors.GetAsync(full.Id.Value, pub.Id.Value, contributors(0).Id.Value)
            Assert.AreEqual(contributors(0).Id, one.Id)

            Dim files = Await ojs.SubmissionFiles.ListAsync(full.Id.Value)
            If files.Count > 0 Then
                ReportUnmapped("SubmissionFile", files(0).AdditionalProperties)
                Assert.IsNotNull(files(0).FileStage)
            End If

            Dim decisions = Await ojs.Decisions.ListAsync(full.Id.Value)
            If decisions.Count > 0 Then
                ReportUnmapped("Decision", decisions(0).AdditionalProperties)
                Assert.IsNotNull(decisions(0).DateDecided)
            End If

            Dim participants = Await ojs.Submissions.GetParticipantsAsync(full.Id.Value)
            Assert.IsNotNull(participants)
            If participants.Count > 0 Then ReportUnmapped("User", participants(0).AdditionalProperties)
            Dim prodParticipants = Await ojs.Submissions.GetParticipantsAsync(full.Id.Value, WorkflowStage.Production)
            Assert.IsNotNull(prodParticipants)

            Dim suggestions = Await ojs.ReviewerSuggestions.ListAsync(full.Id.Value)
            Assert.IsNotNull(suggestions.Items)

            Dim jats = Await ojs.Publications.GetJatsAsync(full.Id.Value, pub.Id.Value)
            Assert.IsFalse(String.IsNullOrEmpty(jats.JatsContent))
        End Using
    End Function

    <TestMethod>
    Public Async Function Submissions_PagingWalksMultiplePages() As Task
        Using ojs = CreateLiveClient()
            Dim first = Await ojs.Submissions.ListAsync(New SubmissionQuery With {.Count = 1})
            If first.ItemsMax < 3 Then Assert.Inconclusive("The journal needs at least 3 submissions for this test.")
            ' Page size 2 forces several requests; take everything up to 25 items.
            Dim expected = Math.Min(first.ItemsMax, 25)
            Dim items = Await ojs.Submissions.ListAllAsync(New SubmissionQuery With {.Count = 2}, maxItems:=25)
            Assert.HasCount(expected, items)
            Assert.AreEqual(expected, items.Select(Function(s) s.Id).Distinct().Count())

            Dim streamed As New List(Of Integer)
            Dim e = ojs.Submissions.Enumerate(New SubmissionQuery With {.Count = 2}).GetAsyncEnumerator()
            Try
                While Await e.MoveNextAsync()
                    streamed.Add(e.Current.Id.Value)
                    If streamed.Count = expected Then Exit While
                End While
            Finally
                e.DisposeAsync().AsTask().GetAwaiter().GetResult()
            End Try
            CollectionAssert.AreEquivalent(items.Select(Function(s) s.Id.Value).ToList(), streamed)
        End Using
    End Function

    <TestMethod>
    Public Async Function Issues_ListCurrentAndGet() As Task
        Using ojs = CreateLiveClient()
            Dim page = Await ojs.Issues.ListAsync(New IssueQuery With {.Count = 3, .IsPublished = True, .OrderBy = IssueOrderBy.DatePublished})
            If page.ItemsMax = 0 Then Assert.Inconclusive("The journal has no published issues.")
            Assert.IsGreaterThanOrEqualTo(page.Items.Count, page.ItemsMax)

            Dim current = Await ojs.Issues.GetCurrentAsync()
            ReportUnmapped("Issue", current.AdditionalProperties)
            Assert.IsNotNull(current.Id)
            Assert.IsNotNull(current.DatePublished)
            TestContext.WriteLine($"Current issue: {current.Identification} ({current.Articles?.Count} articles)")

            Dim byId = Await ojs.Issues.GetAsync(page.Items(0).Id.Value)
            Assert.AreEqual(page.Items(0).Id, byId.Id)
        End Using
    End Function

    <TestMethod>
    Public Async Function Sections_ListAndGet() As Task
        Using ojs = CreateLiveClient()
            Dim all = Await ojs.Sections.ListAllAsync()
            Assert.IsNotEmpty(all)
            ReportUnmapped("Section", all(0).AdditionalProperties)
            Dim one = Await ojs.Sections.GetAsync(all(0).Id.Value)
            Assert.AreEqual(all(0).Id, one.Id)
        End Using
    End Function

    <TestMethod>
    Public Async Function Site_GetAndTheme() As Task
        Using ojs = CreateLiveClient(requireJournal:=False)
            Dim site = Await ojs.Site.GetAsync()
            ReportUnmapped("Site", site.AdditionalProperties)
            Assert.IsNotEmpty(site.SupportedLocales)
            Assert.IsFalse(String.IsNullOrEmpty(site.PrimaryLocale))
            Dim theme = Await ojs.Site.GetThemeAsync()
            Assert.IsFalse(String.IsNullOrEmpty(theme.ThemePluginPath))
            TestContext.WriteLine($"Site theme {theme.ThemePluginPath}, options: {String.Join(", ", If(theme.Options?.Keys, Enumerable.Empty(Of String)()))}")
            Dim news = Await ojs.Announcements.ListAsync()
            Assert.IsNotNull(news.Items)
        End Using
    End Function

    <TestMethod>
    Public Async Function Contexts_ListGetAndTheme() As Task
        Using ojs = CreateLiveClient(requireJournal:=False)
            Dim all = Await ojs.Contexts.ListAllAsync()
            TestContext.WriteLine($"{all.Count} journal(s)")
            If all.Count = 0 Then Return
            ReportUnmapped("Context (summary)", all(0).AdditionalProperties)
            Dim full = Await ojs.Contexts.GetAsync(all(0).Id.Value)
            ReportUnmapped("Context", full.AdditionalProperties)
            Assert.AreEqual(all(0).UrlPath, full.UrlPath)
            Assert.IsNotEmpty(full.SupportedLocales)
            Dim theme = Await ojs.Contexts.GetThemeAsync(full.Id.Value)
            Assert.IsFalse(String.IsNullOrEmpty(theme.ThemePluginPath))
        End Using
    End Function

    <TestMethod>
    Public Async Function Journal_ConfigurationReads() As Task
        Using ojs = CreateLiveClient()
            Dim users = Await ojs.Users.ListAsync(New UserQuery With {.Count = 5, .Status = UserStatusFilter.All})
            Assert.IsGreaterThan(0, users.ItemsMax)
            Dim user = Await ojs.Users.GetAsync(users.Items(0).Id.Value)
            ReportUnmapped("User", user.AdditionalProperties)
            Dim managers = Await ojs.Users.ListAsync(New UserQuery With {.RoleIds = {UserRole.Manager}})
            Assert.IsNotNull(managers.Items)
            Dim reviewers = Await ojs.Users.ListReviewersAsync(New ReviewerQuery With {.Count = 3})
            If reviewers.Items.Count > 0 Then ReportUnmapped("ReviewerSummary", reviewers.Items(0).AdditionalProperties)

            Dim news = Await ojs.Announcements.ListAsync(New AnnouncementQuery With {.Count = 3})
            If news.Items.Count > 0 Then
                Dim one = Await ojs.Announcements.GetAsync(news.Items(0).Id.Value)
                ReportUnmapped("Announcement", one.AdditionalProperties)
            End If

            Dim institutions = Await ojs.Institutions.ListAsync()
            If institutions.Items.Count > 0 Then ReportUnmapped("Institution", institutions.Items(0).AdditionalProperties)

            Dim templates = Await ojs.EmailTemplates.ListAsync(New EmailTemplateQuery With {.Count = 5})
            Assert.IsGreaterThan(0, templates.ItemsMax)
            Dim template = Await ojs.EmailTemplates.GetAsync(templates.Items(0).Key)
            ReportUnmapped("EmailTemplate", template.AdditionalProperties)
            Assert.IsNotNull(template.Body)

            Dim mailables = Await ojs.Mailables.ListAsync()
            Assert.IsNotEmpty(mailables)
            ReportUnmapped("Mailable (summary)", mailables(0).AdditionalProperties)
            Dim mailable = Await ojs.Mailables.GetAsync(mailables(0).EmailTemplateKey)
            ReportUnmapped("Mailable", mailable.AdditionalProperties)
            TestContext.WriteLine($"Mailable {mailable.EmailTemplateKey}: {mailable.Name}, {mailable.EmailTemplates?.Count} template(s)")

            Dim keywords = Await ojs.Vocabs.ListAsync(VocabNames.Keywords)
            Assert.IsNotNull(keywords)

            Dim rors = Await ojs.Rors.ListAsync(New SearchQuery With {.Count = 2})
            If rors.Items.Count > 0 Then
                Dim r = Await ojs.Rors.GetAsync(rors.Items(0).Id.Value)
                ReportUnmapped("Ror", r.AdditionalProperties)
            End If
        End Using
    End Function

End Class
