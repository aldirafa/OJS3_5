Imports System.IO
Imports OJS3_5.Library
Imports OJS3_5.Library.Errors
Imports OJS3_5.Library.Models
Imports OJS3_5.Library.Queries
Imports OJS3_5.Library.Serialization

''' <summary>
''' A small command-line tool showing common tasks with the OJS client.
''' Connection settings come from environment variables (see <see cref="Usage"/>).
''' Commands that change data only run with <c>--yes</c>.
''' </summary>
Module Program

    Private Const Locale As String = "en"

    Function Main(args As String()) As Integer
        ' VB has no Async Main, so run the async entry point and wait for it.
        Return MainAsync(args).GetAwaiter().GetResult()
    End Function

    Private Async Function MainAsync(args As String()) As Task(Of Integer)
        If args.Length = 0 OrElse args(0) = "help" Then
            Usage()
            Return 0
        End If

        Dim options As OjsClientOptions
        Try
            options = ReadOptions()
        Catch ex As InvalidOperationException
            Console.Error.WriteLine(ex.Message)
            Usage()
            Return 2
        End Try

        Using ojs As New OjsClient(options)
            Try
                Select Case args(0).ToLowerInvariant()
                    Case "journals" : Await ListJournalsAsync(ojs)
                    Case "issues" : Await ShowIssuesAsync(ojs)
                    Case "submissions" : Await ListSubmissionsAsync(ojs, If(args.Length > 1, args(1), Nothing))
                    Case "submission" : Await ShowSubmissionAsync(ojs, IntArg(args, 1, "submission ID"))
                    Case "stats" : Await ShowStatsAsync(ojs)
                    Case "export-dois" : Await ExportDoisAsync(ojs, args.Skip(1).Select(Function(a) Integer.Parse(a)).ToArray())
                    Case "review-pdf" : Await SaveReviewPdfAsync(ojs, IntArg(args, 1, "submission ID"), IntArg(args, 2, "review assignment ID"))
                    Case "announce" : Await AnnounceAsync(ojs, args)
                    Case Else
                        Console.Error.WriteLine($"Unknown command '{args(0)}'.")
                        Usage()
                        Return 2
                End Select
                Return 0
            Catch ex As Exception When TypeOf ex Is ArgumentException OrElse TypeOf ex Is FormatException
                Console.Error.WriteLine(ex.Message)
                Return 2
            Catch ex As OjsValidationException
                Console.Error.WriteLine("OJS rejected the request:")
                For Each field In ex.Errors
                    Console.Error.WriteLine($"  {field.Key}: {String.Join(" ", field.Value)}")
                Next
            Catch ex As OjsApiException
                Console.Error.WriteLine($"OJS answered {CInt(ex.StatusCode)} {ex.Error}: {ex.ErrorMessage}")
            Catch ex As Net.Http.HttpRequestException
                Console.Error.WriteLine($"Could not reach {ojs.ApiRoot}: {ex.Message}")
            End Try
            Return 1
        End Using
    End Function

#Region "Commands"

    ''' <summary>journals: every journal on the installation (site-wide endpoint).</summary>
    Private Async Function ListJournalsAsync(ojs As OjsClient) As Task
        For Each j In Await ojs.Contexts.ListAllAsync()
            Console.WriteLine($"{j.Id,4}  {j.UrlPath,-15} {j.Name.GetValue(Locale)}{If(j.Enabled = False, "  (disabled)", "")}")
        Next
    End Function

    ''' <summary>issues: the current issue with its articles, then the archive.</summary>
    Private Async Function ShowIssuesAsync(ojs As OjsClient) As Task
        Try
            Dim current = Await ojs.Issues.GetCurrentAsync()
            Console.WriteLine($"Current issue: {current.Identification} (published {current.DatePublished:d})")
            For Each article In current.Articles
                Dim pub = article.CurrentPublication
                Console.WriteLine($"  - {pub?.Title.GetValue(Locale)} / {pub?.AuthorsStringShort}")
            Next
        Catch ex As OjsApiException When ex.StatusCode = Net.HttpStatusCode.NotFound
            Console.WriteLine("No current issue.")
        End Try

        Console.WriteLine()
        Console.WriteLine("Archive:")
        Dim archive = Await ojs.Issues.ListAllAsync(New IssueQuery With {
            .IsPublished = True, .OrderBy = IssueOrderBy.DatePublished, .OrderDirection = SortDirection.Descending})
        For Each issue In archive
            Console.WriteLine($"  {issue.Id,4}  {issue.Identification}")
        Next
    End Function

    ''' <summary>submissions [published|queued|declined|scheduled]: newest submissions, all pages.</summary>
    Private Async Function ListSubmissionsAsync(ojs As OjsClient, statusName As String) As Task
        Dim query As New SubmissionQuery With {.OrderBy = SubmissionOrderBy.DateSubmitted, .Count = 50}
        If statusName IsNot Nothing Then
            Dim status As SubmissionStatus
            If Not [Enum].TryParse(statusName, ignoreCase:=True, result:=status) Then
                Throw New ArgumentException($"Unknown status '{statusName}'.")
            End If
            query.Status = {status}
        End If

        ' ListAllAsync follows the pages for us; maxItems keeps the output short.
        Dim submissions = Await ojs.Submissions.ListAllAsync(query, maxItems:=100)
        For Each s In submissions
            Console.WriteLine($"{s.Id,5}  {s.StatusLabel,-12} {s.DateSubmitted:yyyy-MM-dd}  {s.CurrentPublication?.Title.GetValue(Locale)}")
        Next
        Console.WriteLine($"({submissions.Count} shown)")
    End Function

    ''' <summary>submission &lt;id&gt;: metadata, authors, files, reviews and decisions of one submission.</summary>
    Private Async Function ShowSubmissionAsync(ojs As OjsClient, submissionId As Integer) As Task
        Dim s = Await ojs.Submissions.GetAsync(submissionId)
        Dim pub = Await ojs.Publications.GetAsync(submissionId, s.CurrentPublicationId.Value)

        Console.WriteLine(pub.FullTitle.GetValue(Locale))
        Console.WriteLine($"Status: {s.StatusLabel}, stage {s.StageId}, submitted {s.DateSubmitted:d}")
        If pub.DoiObject IsNot Nothing Then Console.WriteLine($"DOI: {pub.DoiObject.Doi} ({pub.DoiObject.Status})")
        Console.WriteLine($"Keywords: {String.Join(", ", If(pub.Keywords?.GetNames(Locale), Array.Empty(Of String)()))}")

        Console.WriteLine("Authors:")
        For Each a In pub.Authors
            Console.WriteLine($"  {a.FullName} <{a.Email}>{If(String.IsNullOrEmpty(a.Orcid), "", " " & a.Orcid)}")
        Next

        Console.WriteLine("Files:")
        For Each f In Await ojs.SubmissionFiles.ListAsync(submissionId)
            Console.WriteLine($"  {f.Id,5}  {f.FileStage,-16} {f.Name.GetValue(Locale)}")
        Next

        If s.ReviewAssignments?.Count > 0 Then
            Console.WriteLine("Reviews:")
            For Each r In s.ReviewAssignments
                Console.WriteLine($"  #{r.Id} round {r.Round}: {r.ReviewerFullName} - {r.Status}")
            Next
        End If

        Dim decisions = Await ojs.Decisions.ListAsync(submissionId)
        If decisions.Count > 0 Then
            Console.WriteLine("Decisions:")
            For Each d In decisions
                Console.WriteLine($"  {d.DateDecided:d}  decision {d.Decision} (stage {d.StageId})")
            Next
        End If
    End Function

    ''' <summary>stats: editorial figures and the most viewed articles of the last 12 months.</summary>
    Private Async Function ShowStatsAsync(ojs As OjsClient) As Task
        Console.WriteLine("Editorial activity:")
        For Each stat In Await ojs.Stats.GetEditorialAsync()
            Console.WriteLine($"  {stat.Name,-45} {stat.Value}")
        Next

        Dim yesterday = DateOnly.FromDateTime(Date.Today.AddDays(-1))
        Dim range As New PublicationStatsQuery With {.DateStart = yesterday.AddYears(-1), .DateEnd = yesterday, .Count = 10}

        Console.WriteLine()
        Console.WriteLine("Most viewed articles (last 12 months):")
        Dim top = Await ojs.Stats.Publications.ListAsync(range)
        If top.Items.Count = 0 Then Console.WriteLine("  (no usage recorded yet - statistics are compiled daily)")
        For Each p In top.Items
            Console.WriteLine($"  {p.AbstractViews,6} abstract {p.GalleyViews,6} files  {p.Publication.FullTitle.GetValue(Locale)}")
        Next

        Dim countries = Await ojs.Stats.Publications.ListCountriesAsync(range)
        If countries.Items.Count > 0 Then
            Console.WriteLine()
            Console.WriteLine("Top countries: " & String.Join(", ", countries.Items.Select(Function(c) $"{c.Country} ({c.Total})")))
        End If
    End Function

    ''' <summary>export-dois &lt;submissionId&gt;...: saves the registration agency's deposit XML.</summary>
    Private Async Function ExportDoisAsync(ojs As OjsClient, submissionIds As Integer()) As Task
        If submissionIds.Length = 0 Then Throw New ArgumentException("Give one or more submission IDs.")
        Using xml = Await ojs.Dois.Submissions.ExportAndDownloadAsync(submissionIds)
            Dim target = If(xml.FileName, "doi-export.xml")
            Await xml.SaveAsAsync(target)
            Console.WriteLine($"Saved {Path.GetFullPath(target)}")
        End Using
    End Function

    ''' <summary>review-pdf &lt;submissionId&gt; &lt;reviewAssignmentId&gt;: saves an author-friendly PDF of a review.</summary>
    Private Async Function SaveReviewPdfAsync(ojs As OjsClient, submissionId As Integer, reviewAssignmentId As Integer) As Task
        Using pdf = Await ojs.Reviews.DownloadPdfAsync(submissionId, reviewAssignmentId, authorFriendly:=True)
            Dim target = If(pdf.FileName, $"review-{submissionId}-{reviewAssignmentId}.pdf")
            Await pdf.SaveAsAsync(target)
            Console.WriteLine($"Saved {Path.GetFullPath(target)}")
        End Using
    End Function

    ''' <summary>announce "title" "text" --yes: publishes an announcement (changes data).</summary>
    Private Async Function AnnounceAsync(ojs As OjsClient, args As String()) As Task
        If args.Length < 3 Then Throw New ArgumentException("Usage: announce ""title"" ""text"" --yes")
        If Not args.Contains("--yes") Then
            Console.WriteLine("This creates an announcement on the journal. Add --yes to do it.")
            Return
        End If
        Dim created = Await ojs.Announcements.CreateAsync(New AnnouncementEdit With {
            .Title = LocalizedString.Create(Locale, args(1)),
            .DescriptionShort = LocalizedString.Create(Locale, args(2)),
            .Description = LocalizedString.Create(Locale, $"<p>{Net.WebUtility.HtmlEncode(args(2))}</p>")})
        Console.WriteLine($"Created announcement {created.Id}: {created.Url}")
    End Function

#End Region

#Region "Helpers"

    Private Function ReadOptions() As OjsClientOptions
        Dim baseUrl = Environment.GetEnvironmentVariable("OJS_BASE_URL")
        Dim token = Environment.GetEnvironmentVariable("OJS_TOKEN")
        If String.IsNullOrEmpty(baseUrl) OrElse String.IsNullOrEmpty(token) Then
            Throw New InvalidOperationException("Set OJS_BASE_URL and OJS_TOKEN (and OJS_JOURNAL) first.")
        End If
        Dim journal = Environment.GetEnvironmentVariable("OJS_JOURNAL")
        Return New OjsClientOptions With {
            .BaseUrl = baseUrl,
            .JournalPath = If(String.IsNullOrEmpty(journal), OjsClientOptions.SiteWidePath, journal),
            .UseIndexPhp = Environment.GetEnvironmentVariable("OJS_USE_INDEX_PHP") = "1",
            .ApiToken = token,
            .Timeout = TimeSpan.FromSeconds(60)}
    End Function

    Private Function IntArg(args As String(), index As Integer, name As String) As Integer
        Dim value As Integer
        If args.Length <= index OrElse Not Integer.TryParse(args(index), value) Then
            Throw New ArgumentException($"Missing or invalid {name}.")
        End If
        Return value
    End Function

    Private Sub Usage()
        Console.WriteLine("Library.Sample - examples for the OJS 3.5 .NET client")
        Console.WriteLine()
        Console.WriteLine("Environment: OJS_BASE_URL, OJS_TOKEN, OJS_JOURNAL (journal path), OJS_USE_INDEX_PHP=1 (if URLs contain index.php)")
        Console.WriteLine()
        Console.WriteLine("Commands:")
        Console.WriteLine("  journals                               list the journals on the installation")
        Console.WriteLine("  issues                                 current issue and archive")
        Console.WriteLine("  submissions [published|queued|...]     newest submissions")
        Console.WriteLine("  submission <id>                        details of one submission")
        Console.WriteLine("  stats                                  editorial and usage statistics")
        Console.WriteLine("  export-dois <submissionId>...          save the DOI deposit XML")
        Console.WriteLine("  review-pdf <submissionId> <reviewId>   save a review as PDF")
        Console.WriteLine("  announce ""title"" ""text"" --yes          create an announcement (changes data)")
    End Sub

#End Region

End Module
