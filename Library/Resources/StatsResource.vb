Imports System.Threading
Imports OJS3_5.Library.Http
Imports OJS3_5.Library.Models
Imports OJS3_5.Library.Paging
Imports OJS3_5.Library.Queries

Namespace Resources

    ''' <summary><c>/stats</c> — editorial, user and usage statistics.</summary>
    ''' <remarks>
    ''' Usage statistics (views and downloads) are compiled by OJS's daily scheduled task, so they lag one day behind.
    ''' Most usage endpoints can also return CSV; use the <c>...CsvAsync</c> methods for that.
    ''' </remarks>
    ''' <example>
    ''' <code>
    ''' Dim received = (Await ojs.Stats.GetEditorialAsync()).First(Function(s) s.Key = "submissionsReceived")
    ''' Console.WriteLine($"{received.Name}: {received.Value}")
    '''
    ''' Dim csv = Await ojs.Stats.Publications.ListCsvAsync(New PublicationStatsQuery With {.DateStart = New DateOnly(2026, 1, 1)})
    ''' Await File.WriteAllTextAsync("articles.csv", csv)
    ''' </code>
    ''' </example>
    Public NotInheritable Class StatsResource
        Inherits ResourceBase

        Friend Sub New(http As OjsHttp)
            MyBase.New(http)
            Contexts = New ContextStatsResource(http)
            Issues = New IssueStatsResource(http)
            Publications = New PublicationStatsResource(http)
        End Sub

        ''' <summary>Journal homepage views (<c>/stats/contexts</c>).</summary>
        Public ReadOnly Property Contexts As ContextStatsResource

        ''' <summary>Issue views (<c>/stats/issues</c>).</summary>
        Public ReadOnly Property Issues As IssueStatsResource

        ''' <summary>Article views, file downloads and visitor geography (<c>/stats/publications</c>).</summary>
        Public ReadOnly Property Publications As PublicationStatsResource

        ''' <summary>Gets editorial activity figures (submissions received, accepted, days to decision, ...).</summary>
        ''' <param name="dateStart">Only activity on or after this day.</param>
        ''' <param name="dateEnd">Only activity on or before this day.</param>
        ''' <param name="sectionIds">Only submissions in these sections.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim stats = Await ojs.Stats.GetEditorialAsync(New DateOnly(2026, 1, 1), New DateOnly(2026, 12, 31))
        ''' For Each s In stats : Console.WriteLine($"{s.Name}: {s.Value}") : Next
        ''' </code>
        ''' </example>
        Public Function GetEditorialAsync(Optional dateStart As DateOnly? = Nothing, Optional dateEnd As DateOnly? = Nothing,
                                          Optional sectionIds As IEnumerable(Of Integer) = Nothing,
                                          Optional cancellationToken As CancellationToken = Nothing) As Task(Of List(Of EditorialStat))
            Dim q = New QueryBuilder().Add("dateStart", dateStart).Add("dateEnd", dateEnd).Add("sectionIds", sectionIds)
            Return Http.GetAsync(Of List(Of EditorialStat))("stats/editorial", q, cancellationToken)
        End Function

        ''' <summary>Gets yearly averages of the editorial figures, keyed by <see cref="EditorialStat.Key"/>.</summary>
        ''' <param name="sectionIds">Only submissions in these sections.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <remarks>A value of <c>-1</c> means there is not a full calendar year of activity to average.</remarks>
        ''' <example>
        ''' <code>
        ''' Dim averages = Await ojs.Stats.GetEditorialAveragesAsync()
        ''' Console.WriteLine($"Submissions per year: {averages("submissionsReceived")}")
        ''' </code>
        ''' </example>
        Public Function GetEditorialAveragesAsync(Optional sectionIds As IEnumerable(Of Integer) = Nothing,
                                                  Optional cancellationToken As CancellationToken = Nothing) As Task(Of Dictionary(Of String, Double))
            Return Http.GetAsync(Of Dictionary(Of String, Double))("stats/editorial/averages", New QueryBuilder().Add("sectionIds", sectionIds), cancellationToken)
        End Function

        ''' <summary>Gets the number of users per role, plus a <c>total</c> row.</summary>
        ''' <param name="registeredAfter">Only users registered on or after this day.</param>
        ''' <param name="registeredBefore">Only users registered on or before this day.</param>
        ''' <param name="status">Active, disabled or all accounts.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim users = Await ojs.Stats.GetUsersAsync(status:=UserStatusFilter.Active)
        ''' For Each u In users.Where(Function(x) Not x.IsTotal) : Console.WriteLine($"{u.Name}: {u.Value}") : Next
        ''' </code>
        ''' </example>
        Public Function GetUsersAsync(Optional registeredAfter As DateOnly? = Nothing, Optional registeredBefore As DateOnly? = Nothing,
                                      Optional status As UserStatusFilter? = Nothing,
                                      Optional cancellationToken As CancellationToken = Nothing) As Task(Of List(Of UserStat))
            Dim q = New QueryBuilder().Add("registeredAfter", registeredAfter).Add("registeredBefore", registeredBefore).
                Add("status", QueryText.Status(status))
            Return Http.GetAsync(Of List(Of UserStat))("stats/users", q, cancellationToken)
        End Function

    End Class

    ''' <summary>Shared plumbing of the usage statistics groups.</summary>
    ''' <example>
    ''' <code>
    ''' Dim groups As StatsGroupBase() = {ojs.Stats.Contexts, ojs.Stats.Issues, ojs.Stats.Publications}
    ''' </code>
    ''' </example>
    Public MustInherit Class StatsGroupBase
        Inherits ResourceBase

        Friend Const Csv As String = "text/csv"

        Friend Sub New(http As OjsHttp)
            MyBase.New(http)
        End Sub

        Friend Function ListCoreAsync(Of T)(path As String, query As StatsQuery, ct As CancellationToken) As Task(Of PagedResult(Of T))
            Return Http.GetAsync(Of PagedResult(Of T))(path, query.ToQueryBuilder(), ct)
        End Function

        Friend Function CsvCoreAsync(path As String, query As QueryBuilder, ct As CancellationToken) As Task(Of String)
            Return Http.GetStringAsync(path, query, Csv, ct)
        End Function

        Friend Function TimelineCoreAsync(path As String, query As StatsQuery, resolution As TimelineResolution?,
                                          filesOnly As Boolean, ct As CancellationToken) As Task(Of List(Of TimelineInterval))
            Return Http.GetAsync(Of List(Of TimelineInterval))(path, query.ToTimelineQuery(resolution, filesOnly), ct)
        End Function

        Friend Shared Function Range(dateStart As DateOnly?, dateEnd As DateOnly?) As QueryBuilder
            Return New QueryBuilder().Add("dateStart", dateStart).Add("dateEnd", dateEnd)
        End Function

        Friend Shared Function RangeTimeline(dateStart As DateOnly?, dateEnd As DateOnly?, resolution As TimelineResolution?, filesOnly As Boolean) As QueryBuilder
            Dim q = Range(dateStart, dateEnd).Add("timelineInterval", If(resolution.HasValue, resolution.Value.ToString().ToLowerInvariant(), Nothing))
            If filesOnly Then q.Add("type", "files")
            Return q
        End Function

    End Class

    ''' <summary><c>/stats/contexts</c> — journal homepage views. Listing all journals needs a site administrator token.</summary>
    ''' <example>
    ''' <code>
    ''' Dim timeline = Await ojs.Stats.Contexts.GetTimelineAsync(1, TimelineResolution.Month)
    ''' </code>
    ''' </example>
    Public NotInheritable Class ContextStatsResource
        Inherits StatsGroupBase

        Friend Sub New(http As OjsHttp)
            MyBase.New(http)
        End Sub

        ''' <summary>Gets journals ordered by homepage views.</summary>
        ''' <param name="query">Filters, ordering and paging.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim page = Await ojs.Stats.Contexts.ListAsync(New ContextStatsQuery With {.DateStart = New DateOnly(2026, 1, 1), .DateEnd = New DateOnly(2026, 6, 30)})
        ''' For Each item In page.Items : Console.WriteLine($"{item.Context.UrlPath}: {item.Total}") : Next
        ''' </code>
        ''' </example>
        Public Function ListAsync(Optional query As ContextStatsQuery = Nothing, Optional cancellationToken As CancellationToken = Nothing) As Task(Of PagedResult(Of ContextStats))
            Return ListCoreAsync(Of ContextStats)("stats/contexts", If(query, New ContextStatsQuery()), cancellationToken)
        End Function

        ''' <summary>Like <see cref="ListAsync"/>, as CSV.</summary>
        ''' <param name="query">Filters, ordering and paging.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim csv = Await ojs.Stats.Contexts.ListCsvAsync(New ContextStatsQuery With {.DateStart = New DateOnly(2026, 1, 1), .DateEnd = New DateOnly(2026, 6, 30)})
        ''' Await File.WriteAllTextAsync("stats.csv", csv)
        ''' </code>
        ''' </example>
        Public Function ListCsvAsync(Optional query As ContextStatsQuery = Nothing, Optional cancellationToken As CancellationToken = Nothing) As Task(Of String)
            Return CsvCoreAsync("stats/contexts", If(query, New ContextStatsQuery()).ToQueryBuilder(), cancellationToken)
        End Function

        ''' <summary>Gets homepage views of the matching journals per day or month.</summary>
        ''' <param name="query">Filters (paging and ordering are ignored).</param>
        ''' <param name="resolution">Per day or per month (OJS default month).</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim points = Await ojs.Stats.Contexts.GetTimelineAsync(New ContextStatsQuery With {.DateStart = New DateOnly(2026, 1, 1), .DateEnd = New DateOnly(2026, 6, 30)}, TimelineResolution.Day)
        ''' For Each p In points : Console.WriteLine($"{p.Date}: {p.Value}") : Next
        ''' </code>
        ''' </example>
        Public Function GetTimelineAsync(Optional query As ContextStatsQuery = Nothing, Optional resolution As TimelineResolution? = Nothing,
                                         Optional cancellationToken As CancellationToken = Nothing) As Task(Of List(Of TimelineInterval))
            Return TimelineCoreAsync("stats/contexts/timeline", If(query, New ContextStatsQuery()), resolution, False, cancellationToken)
        End Function

        ''' <summary>Like <see cref="GetTimelineAsync"/>, as CSV.</summary>
        ''' <param name="query">Filters.</param>
        ''' <param name="resolution">Per day or per month.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim csv = Await ojs.Stats.Contexts.GetTimelineCsvAsync(New ContextStatsQuery With {.DateStart = New DateOnly(2026, 1, 1), .DateEnd = New DateOnly(2026, 6, 30)}, TimelineResolution.Month)
        ''' Await File.WriteAllTextAsync("timeline.csv", csv)
        ''' </code>
        ''' </example>
        Public Function GetTimelineCsvAsync(Optional query As ContextStatsQuery = Nothing, Optional resolution As TimelineResolution? = Nothing,
                                            Optional cancellationToken As CancellationToken = Nothing) As Task(Of String)
            Return CsvCoreAsync("stats/contexts/timeline", If(query, New ContextStatsQuery()).ToTimelineQuery(resolution, False), cancellationToken)
        End Function

        ''' <summary>Gets one journal's homepage views.</summary>
        ''' <param name="contextId">The journal ID.</param>
        ''' <param name="dateStart">The first day to include.</param>
        ''' <param name="dateEnd">The last day to include.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim stats = Await ojs.Stats.Contexts.GetAsync(1, New DateOnly(2026, 1, 1), New DateOnly(2026, 6, 30))
        ''' Console.WriteLine($"Homepage views: {stats.Total}")
        ''' </code>
        ''' </example>
        Public Function GetAsync(contextId As Integer, Optional dateStart As DateOnly? = Nothing, Optional dateEnd As DateOnly? = Nothing,
                                 Optional cancellationToken As CancellationToken = Nothing) As Task(Of ContextStats)
            Return Http.GetAsync(Of ContextStats)($"stats/contexts/{contextId}", Range(dateStart, dateEnd), cancellationToken)
        End Function

        ''' <summary>Like <see cref="GetAsync"/>, as CSV.</summary>
        ''' <param name="contextId">The journal ID.</param>
        ''' <param name="dateStart">The first day to include.</param>
        ''' <param name="dateEnd">The last day to include.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim csv = Await ojs.Stats.Contexts.GetCsvAsync(1, New DateOnly(2026, 1, 1), New DateOnly(2026, 6, 30))
        ''' </code>
        ''' </example>
        Public Function GetCsvAsync(contextId As Integer, Optional dateStart As DateOnly? = Nothing, Optional dateEnd As DateOnly? = Nothing,
                                    Optional cancellationToken As CancellationToken = Nothing) As Task(Of String)
            Return CsvCoreAsync($"stats/contexts/{contextId}", Range(dateStart, dateEnd), cancellationToken)
        End Function

        ''' <summary>Gets one journal's homepage views per day or month.</summary>
        ''' <param name="contextId">The journal ID.</param>
        ''' <param name="resolution">Per day or per month.</param>
        ''' <param name="dateStart">The first day to include.</param>
        ''' <param name="dateEnd">The last day to include.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim points = Await ojs.Stats.Contexts.GetTimelineAsync(1, TimelineResolution.Month)
        ''' For Each p In points : Console.WriteLine($"{p.Label}: {p.Value}") : Next
        ''' </code>
        ''' </example>
        Public Function GetTimelineAsync(contextId As Integer, Optional resolution As TimelineResolution? = Nothing,
                                         Optional dateStart As DateOnly? = Nothing, Optional dateEnd As DateOnly? = Nothing,
                                         Optional cancellationToken As CancellationToken = Nothing) As Task(Of List(Of TimelineInterval))
            Return Http.GetAsync(Of List(Of TimelineInterval))($"stats/contexts/{contextId}/timeline", RangeTimeline(dateStart, dateEnd, resolution, False), cancellationToken)
        End Function

        ''' <summary>Like <see cref="GetTimelineAsync(Integer, TimelineResolution?, DateOnly?, DateOnly?, CancellationToken)"/>, as CSV.</summary>
        ''' <param name="contextId">The journal ID.</param>
        ''' <param name="resolution">Per day or per month.</param>
        ''' <param name="dateStart">The first day to include.</param>
        ''' <param name="dateEnd">The last day to include.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim csv = Await ojs.Stats.Contexts.GetTimelineCsvAsync(1, TimelineResolution.Day)
        ''' </code>
        ''' </example>
        Public Function GetTimelineCsvAsync(contextId As Integer, Optional resolution As TimelineResolution? = Nothing,
                                            Optional dateStart As DateOnly? = Nothing, Optional dateEnd As DateOnly? = Nothing,
                                            Optional cancellationToken As CancellationToken = Nothing) As Task(Of String)
            Return CsvCoreAsync($"stats/contexts/{contextId}/timeline", RangeTimeline(dateStart, dateEnd, resolution, False), cancellationToken)
        End Function

    End Class

    ''' <summary><c>/stats/issues</c> — issue table of contents and issue galley views.</summary>
    ''' <example>
    ''' <code>
    ''' Dim top = Await ojs.Stats.Issues.ListAsync(New IssueStatsQuery With {.Count = 5})
    ''' </code>
    ''' </example>
    Public NotInheritable Class IssueStatsResource
        Inherits StatsGroupBase

        Friend Sub New(http As OjsHttp)
            MyBase.New(http)
        End Sub

        ''' <summary>Gets issues ordered by views.</summary>
        ''' <param name="query">Filters, ordering and paging.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim page = Await ojs.Stats.Issues.ListAsync(New IssueStatsQuery With {.DateStart = New DateOnly(2026, 1, 1), .DateEnd = New DateOnly(2026, 6, 30)})
        ''' For Each item In page.Items : Console.WriteLine($"{item.Issue.Identification}: {item.TotalViews}") : Next
        ''' </code>
        ''' </example>
        Public Function ListAsync(Optional query As IssueStatsQuery = Nothing, Optional cancellationToken As CancellationToken = Nothing) As Task(Of PagedResult(Of IssueStats))
            Return ListCoreAsync(Of IssueStats)("stats/issues", If(query, New IssueStatsQuery()), cancellationToken)
        End Function

        ''' <summary>Like <see cref="ListAsync"/>, as CSV.</summary>
        ''' <param name="query">Filters, ordering and paging.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim csv = Await ojs.Stats.Issues.ListCsvAsync(New IssueStatsQuery With {.DateStart = New DateOnly(2026, 1, 1), .DateEnd = New DateOnly(2026, 6, 30)})
        ''' Await File.WriteAllTextAsync("stats.csv", csv)
        ''' </code>
        ''' </example>
        Public Function ListCsvAsync(Optional query As IssueStatsQuery = Nothing, Optional cancellationToken As CancellationToken = Nothing) As Task(Of String)
            Return CsvCoreAsync("stats/issues", If(query, New IssueStatsQuery()).ToQueryBuilder(), cancellationToken)
        End Function

        ''' <summary>Gets views of the matching issues per day or month.</summary>
        ''' <param name="query">Filters (paging and ordering are ignored).</param>
        ''' <param name="resolution">Per day or per month.</param>
        ''' <param name="galleysOnly"><c>True</c> to count only issue galley views, not table of contents views.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim points = Await ojs.Stats.Issues.GetTimelineAsync(New IssueStatsQuery With {.DateStart = New DateOnly(2026, 1, 1), .DateEnd = New DateOnly(2026, 6, 30)}, TimelineResolution.Day, galleysOnly:=True)
        ''' For Each p In points : Console.WriteLine($"{p.Date}: {p.Value}") : Next
        ''' </code>
        ''' </example>
        Public Function GetTimelineAsync(Optional query As IssueStatsQuery = Nothing, Optional resolution As TimelineResolution? = Nothing,
                                         Optional galleysOnly As Boolean = False, Optional cancellationToken As CancellationToken = Nothing) As Task(Of List(Of TimelineInterval))
            Return TimelineCoreAsync("stats/issues/timeline", If(query, New IssueStatsQuery()), resolution, galleysOnly, cancellationToken)
        End Function

        ''' <summary>Like <see cref="GetTimelineAsync(IssueStatsQuery, TimelineResolution?, Boolean, CancellationToken)"/>, as CSV.</summary>
        ''' <param name="query">Filters.</param>
        ''' <param name="resolution">Per day or per month.</param>
        ''' <param name="galleysOnly"><c>True</c> to count only issue galley views.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim csv = Await ojs.Stats.Issues.GetTimelineCsvAsync(New IssueStatsQuery With {.DateStart = New DateOnly(2026, 1, 1), .DateEnd = New DateOnly(2026, 6, 30)}, TimelineResolution.Month)
        ''' Await File.WriteAllTextAsync("timeline.csv", csv)
        ''' </code>
        ''' </example>
        Public Function GetTimelineCsvAsync(Optional query As IssueStatsQuery = Nothing, Optional resolution As TimelineResolution? = Nothing,
                                            Optional galleysOnly As Boolean = False, Optional cancellationToken As CancellationToken = Nothing) As Task(Of String)
            Return CsvCoreAsync("stats/issues/timeline", If(query, New IssueStatsQuery()).ToTimelineQuery(resolution, galleysOnly), cancellationToken)
        End Function

        ''' <summary>Gets one issue's views.</summary>
        ''' <param name="issueId">The issue ID.</param>
        ''' <param name="dateStart">The first day to include.</param>
        ''' <param name="dateEnd">The last day to include.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim stats = Await ojs.Stats.Issues.GetAsync(4, New DateOnly(2026, 1, 1), New DateOnly(2026, 6, 30))
        ''' Console.WriteLine($"{stats.TocViews} TOC views, {stats.IssueGalleyViews} galley views")
        ''' </code>
        ''' </example>
        Public Function GetAsync(issueId As Integer, Optional dateStart As DateOnly? = Nothing, Optional dateEnd As DateOnly? = Nothing,
                                 Optional cancellationToken As CancellationToken = Nothing) As Task(Of IssueStats)
            Return Http.GetAsync(Of IssueStats)($"stats/issues/{issueId}", Range(dateStart, dateEnd), cancellationToken)
        End Function

        ''' <summary>Like <see cref="GetAsync"/>, as CSV.</summary>
        ''' <param name="issueId">The issue ID.</param>
        ''' <param name="dateStart">The first day to include.</param>
        ''' <param name="dateEnd">The last day to include.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim csv = Await ojs.Stats.Issues.GetCsvAsync(4, New DateOnly(2026, 1, 1), New DateOnly(2026, 6, 30))
        ''' </code>
        ''' </example>
        Public Function GetCsvAsync(issueId As Integer, Optional dateStart As DateOnly? = Nothing, Optional dateEnd As DateOnly? = Nothing,
                                    Optional cancellationToken As CancellationToken = Nothing) As Task(Of String)
            Return CsvCoreAsync($"stats/issues/{issueId}", Range(dateStart, dateEnd), cancellationToken)
        End Function

        ''' <summary>Gets one issue's views per day or month.</summary>
        ''' <param name="issueId">The issue ID.</param>
        ''' <param name="resolution">Per day or per month.</param>
        ''' <param name="galleysOnly"><c>True</c> to count only issue galley views.</param>
        ''' <param name="dateStart">The first day to include.</param>
        ''' <param name="dateEnd">The last day to include.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim points = Await ojs.Stats.Issues.GetTimelineAsync(4, TimelineResolution.Month)
        ''' For Each p In points : Console.WriteLine($"{p.Label}: {p.Value}") : Next
        ''' </code>
        ''' </example>
        Public Function GetTimelineAsync(issueId As Integer, Optional resolution As TimelineResolution? = Nothing, Optional galleysOnly As Boolean = False,
                                         Optional dateStart As DateOnly? = Nothing, Optional dateEnd As DateOnly? = Nothing,
                                         Optional cancellationToken As CancellationToken = Nothing) As Task(Of List(Of TimelineInterval))
            Return Http.GetAsync(Of List(Of TimelineInterval))($"stats/issues/{issueId}/timeline", RangeTimeline(dateStart, dateEnd, resolution, galleysOnly), cancellationToken)
        End Function

        ''' <summary>Like <see cref="GetTimelineAsync(Integer, TimelineResolution?, Boolean, DateOnly?, DateOnly?, CancellationToken)"/>, as CSV.</summary>
        ''' <param name="issueId">The issue ID.</param>
        ''' <param name="resolution">Per day or per month.</param>
        ''' <param name="galleysOnly"><c>True</c> to count only issue galley views.</param>
        ''' <param name="dateStart">The first day to include.</param>
        ''' <param name="dateEnd">The last day to include.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim csv = Await ojs.Stats.Issues.GetTimelineCsvAsync(4, TimelineResolution.Day)
        ''' </code>
        ''' </example>
        Public Function GetTimelineCsvAsync(issueId As Integer, Optional resolution As TimelineResolution? = Nothing, Optional galleysOnly As Boolean = False,
                                            Optional dateStart As DateOnly? = Nothing, Optional dateEnd As DateOnly? = Nothing,
                                            Optional cancellationToken As CancellationToken = Nothing) As Task(Of String)
            Return CsvCoreAsync($"stats/issues/{issueId}/timeline", RangeTimeline(dateStart, dateEnd, resolution, galleysOnly), cancellationToken)
        End Function

    End Class

    ''' <summary><c>/stats/publications</c> — article views, file downloads and visitor geography.</summary>
    ''' <example>
    ''' <code>
    ''' Dim countries = Await ojs.Stats.Publications.ListCountriesAsync()
    ''' For Each c In countries.Items : Console.WriteLine($"{c.Country}: {c.Total}") : Next
    ''' </code>
    ''' </example>
    Public NotInheritable Class PublicationStatsResource
        Inherits StatsGroupBase

        Friend Sub New(http As OjsHttp)
            MyBase.New(http)
        End Sub

        Private Shared Function Q(query As PublicationStatsQuery) As PublicationStatsQuery
            Return If(query, New PublicationStatsQuery())
        End Function

        ''' <summary>Gets articles ordered by views.</summary>
        ''' <param name="query">Filters, ordering and paging.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim page = Await ojs.Stats.Publications.ListAsync(New PublicationStatsQuery With {.DateStart = New DateOnly(2026, 1, 1), .DateEnd = New DateOnly(2026, 6, 30)})
        ''' For Each item In page.Items : Console.WriteLine($"{item.Publication.FullTitle.GetValue("en")}: {item.AbstractViews} / {item.GalleyViews}") : Next
        ''' </code>
        ''' </example>
        Public Function ListAsync(Optional query As PublicationStatsQuery = Nothing, Optional cancellationToken As CancellationToken = Nothing) As Task(Of PagedResult(Of PublicationStats))
            Return ListCoreAsync(Of PublicationStats)("stats/publications", Q(query), cancellationToken)
        End Function

        ''' <summary>Like <see cref="ListAsync"/>, as CSV.</summary>
        ''' <param name="query">Filters, ordering and paging.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim csv = Await ojs.Stats.Publications.ListCsvAsync(New PublicationStatsQuery With {.DateStart = New DateOnly(2026, 1, 1), .DateEnd = New DateOnly(2026, 6, 30)})
        ''' Await File.WriteAllTextAsync("stats.csv", csv)
        ''' </code>
        ''' </example>
        Public Function ListCsvAsync(Optional query As PublicationStatsQuery = Nothing, Optional cancellationToken As CancellationToken = Nothing) As Task(Of String)
            Return CsvCoreAsync("stats/publications", Q(query).ToQueryBuilder(), cancellationToken)
        End Function

        ''' <summary>Gets views of the matching articles per day or month.</summary>
        ''' <param name="query">Filters (paging and ordering are ignored).</param>
        ''' <param name="resolution">Per day or per month.</param>
        ''' <param name="filesOnly"><c>True</c> to count only file views and downloads, not abstract views.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim points = Await ojs.Stats.Publications.GetTimelineAsync(New PublicationStatsQuery With {.DateStart = New DateOnly(2026, 1, 1), .DateEnd = New DateOnly(2026, 6, 30)}, TimelineResolution.Day, filesOnly:=True)
        ''' For Each p In points : Console.WriteLine($"{p.Date}: {p.Value}") : Next
        ''' </code>
        ''' </example>
        Public Function GetTimelineAsync(Optional query As PublicationStatsQuery = Nothing, Optional resolution As TimelineResolution? = Nothing,
                                         Optional filesOnly As Boolean = False, Optional cancellationToken As CancellationToken = Nothing) As Task(Of List(Of TimelineInterval))
            Return TimelineCoreAsync("stats/publications/timeline", Q(query), resolution, filesOnly, cancellationToken)
        End Function

        ''' <summary>Like <see cref="GetTimelineAsync(PublicationStatsQuery, TimelineResolution?, Boolean, CancellationToken)"/>, as CSV.</summary>
        ''' <param name="query">Filters.</param>
        ''' <param name="resolution">Per day or per month.</param>
        ''' <param name="filesOnly"><c>True</c> to count only file views and downloads.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim csv = Await ojs.Stats.Publications.GetTimelineCsvAsync(New PublicationStatsQuery With {.DateStart = New DateOnly(2026, 1, 1), .DateEnd = New DateOnly(2026, 6, 30)}, TimelineResolution.Month)
        ''' Await File.WriteAllTextAsync("timeline.csv", csv)
        ''' </code>
        ''' </example>
        Public Function GetTimelineCsvAsync(Optional query As PublicationStatsQuery = Nothing, Optional resolution As TimelineResolution? = Nothing,
                                            Optional filesOnly As Boolean = False, Optional cancellationToken As CancellationToken = Nothing) As Task(Of String)
            Return CsvCoreAsync("stats/publications/timeline", Q(query).ToTimelineQuery(resolution, filesOnly), cancellationToken)
        End Function

        ''' <summary>Gets one article's views.</summary>
        ''' <param name="submissionId">The submission ID.</param>
        ''' <param name="dateStart">The first day to include.</param>
        ''' <param name="dateEnd">The last day to include.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim stats = Await ojs.Stats.Publications.GetAsync(12, New DateOnly(2026, 1, 1), New DateOnly(2026, 6, 30))
        ''' Console.WriteLine($"{stats.AbstractViews} abstract views, {stats.PdfViews} PDF views")
        ''' </code>
        ''' </example>
        Public Function GetAsync(submissionId As Integer, Optional dateStart As DateOnly? = Nothing, Optional dateEnd As DateOnly? = Nothing,
                                 Optional cancellationToken As CancellationToken = Nothing) As Task(Of PublicationStats)
            Return Http.GetAsync(Of PublicationStats)($"stats/publications/{submissionId}", Range(dateStart, dateEnd), cancellationToken)
        End Function

        ''' <summary>Gets one article's views per day or month.</summary>
        ''' <param name="submissionId">The submission ID.</param>
        ''' <param name="resolution">Per day or per month.</param>
        ''' <param name="filesOnly"><c>True</c> to count only file views and downloads.</param>
        ''' <param name="dateStart">The first day to include.</param>
        ''' <param name="dateEnd">The last day to include.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim points = Await ojs.Stats.Publications.GetTimelineAsync(12, TimelineResolution.Month)
        ''' For Each p In points : Console.WriteLine($"{p.Label}: {p.Value}") : Next
        ''' </code>
        ''' </example>
        Public Function GetTimelineAsync(submissionId As Integer, Optional resolution As TimelineResolution? = Nothing, Optional filesOnly As Boolean = False,
                                         Optional dateStart As DateOnly? = Nothing, Optional dateEnd As DateOnly? = Nothing,
                                         Optional cancellationToken As CancellationToken = Nothing) As Task(Of List(Of TimelineInterval))
            Return Http.GetAsync(Of List(Of TimelineInterval))($"stats/publications/{submissionId}/timeline", RangeTimeline(dateStart, dateEnd, resolution, filesOnly), cancellationToken)
        End Function

        ''' <summary>Gets published files ordered by downloads.</summary>
        ''' <param name="query">Filters, ordering and paging.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim top = Await ojs.Stats.Publications.ListFilesAsync(New PublicationStatsQuery With {.DateStart = New DateOnly(2026, 1, 1), .Count = 10})
        ''' For Each f In top.Items : Console.WriteLine($"{f.FileName}: {f.Downloads}") : Next
        ''' </code>
        ''' </example>
        Public Function ListFilesAsync(Optional query As PublicationStatsQuery = Nothing, Optional cancellationToken As CancellationToken = Nothing) As Task(Of PagedResult(Of PublicationFileStats))
            Return ListCoreAsync(Of PublicationFileStats)("stats/publications/files", Q(query), cancellationToken)
        End Function

        ''' <summary>Like <see cref="ListFilesAsync"/>, as CSV.</summary>
        ''' <param name="query">Filters, ordering and paging.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim csv = Await ojs.Stats.Publications.ListFilesCsvAsync(New PublicationStatsQuery With {.DateStart = New DateOnly(2026, 1, 1), .Count = 10})
        ''' </code>
        ''' </example>
        Public Function ListFilesCsvAsync(Optional query As PublicationStatsQuery = Nothing, Optional cancellationToken As CancellationToken = Nothing) As Task(Of String)
            Return CsvCoreAsync("stats/publications/files", Q(query).ToQueryBuilder(), cancellationToken)
        End Function

        ''' <summary>Gets countries ordered by article views. Requires geographical statistics to be enabled.</summary>
        ''' <param name="query">Filters, ordering and paging.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim page = Await ojs.Stats.Publications.ListCountriesAsync(New PublicationStatsQuery With {.DateStart = New DateOnly(2026, 1, 1), .Count = 10})
        ''' For Each g In page.Items : Console.WriteLine($"{g.Country}: {g.Total} ({g.Unique} unique)") : Next
        ''' </code>
        ''' </example>
        Public Function ListCountriesAsync(Optional query As PublicationStatsQuery = Nothing, Optional cancellationToken As CancellationToken = Nothing) As Task(Of PagedResult(Of GeoStats))
            Return ListCoreAsync(Of GeoStats)("stats/publications/countries", Q(query), cancellationToken)
        End Function

        ''' <summary>Like <see cref="ListCountriesAsync"/>, as CSV.</summary>
        ''' <param name="query">Filters, ordering and paging.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim csv = Await ojs.Stats.Publications.ListCountriesCsvAsync(New PublicationStatsQuery With {.DateStart = New DateOnly(2026, 1, 1), .Count = 10})
        ''' Await File.WriteAllTextAsync("countries.csv", csv)
        ''' </code>
        ''' </example>
        Public Function ListCountriesCsvAsync(Optional query As PublicationStatsQuery = Nothing, Optional cancellationToken As CancellationToken = Nothing) As Task(Of String)
            Return CsvCoreAsync("stats/publications/countries", Q(query).ToQueryBuilder(), cancellationToken)
        End Function

        ''' <summary>Gets regions ordered by article views. Requires regional statistics to be enabled.</summary>
        ''' <param name="query">Filters, ordering and paging.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim page = Await ojs.Stats.Publications.ListRegionsAsync(New PublicationStatsQuery With {.DateStart = New DateOnly(2026, 1, 1), .Count = 10})
        ''' For Each g In page.Items : Console.WriteLine($"{g.Region}: {g.Total} ({g.Unique} unique)") : Next
        ''' </code>
        ''' </example>
        Public Function ListRegionsAsync(Optional query As PublicationStatsQuery = Nothing, Optional cancellationToken As CancellationToken = Nothing) As Task(Of PagedResult(Of GeoStats))
            Return ListCoreAsync(Of GeoStats)("stats/publications/regions", Q(query), cancellationToken)
        End Function

        ''' <summary>Like <see cref="ListRegionsAsync"/>, as CSV.</summary>
        ''' <param name="query">Filters, ordering and paging.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim csv = Await ojs.Stats.Publications.ListRegionsCsvAsync(New PublicationStatsQuery With {.DateStart = New DateOnly(2026, 1, 1), .Count = 10})
        ''' Await File.WriteAllTextAsync("regions.csv", csv)
        ''' </code>
        ''' </example>
        Public Function ListRegionsCsvAsync(Optional query As PublicationStatsQuery = Nothing, Optional cancellationToken As CancellationToken = Nothing) As Task(Of String)
            Return CsvCoreAsync("stats/publications/regions", Q(query).ToQueryBuilder(), cancellationToken)
        End Function

        ''' <summary>Gets cities ordered by article views. Requires city statistics to be enabled.</summary>
        ''' <param name="query">Filters, ordering and paging.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim page = Await ojs.Stats.Publications.ListCitiesAsync(New PublicationStatsQuery With {.DateStart = New DateOnly(2026, 1, 1), .Count = 10})
        ''' For Each g In page.Items : Console.WriteLine($"{g.City}: {g.Total} ({g.Unique} unique)") : Next
        ''' </code>
        ''' </example>
        Public Function ListCitiesAsync(Optional query As PublicationStatsQuery = Nothing, Optional cancellationToken As CancellationToken = Nothing) As Task(Of PagedResult(Of GeoStats))
            Return ListCoreAsync(Of GeoStats)("stats/publications/cities", Q(query), cancellationToken)
        End Function

        ''' <summary>Like <see cref="ListCitiesAsync"/>, as CSV.</summary>
        ''' <param name="query">Filters, ordering and paging.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim csv = Await ojs.Stats.Publications.ListCitiesCsvAsync(New PublicationStatsQuery With {.DateStart = New DateOnly(2026, 1, 1), .Count = 10})
        ''' Await File.WriteAllTextAsync("cities.csv", csv)
        ''' </code>
        ''' </example>
        Public Function ListCitiesCsvAsync(Optional query As PublicationStatsQuery = Nothing, Optional cancellationToken As CancellationToken = Nothing) As Task(Of String)
            Return CsvCoreAsync("stats/publications/cities", Q(query).ToQueryBuilder(), cancellationToken)
        End Function

    End Class

End Namespace
