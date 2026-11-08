Imports OJS3_5.Library.Http
Imports OJS3_5.Library.Models

Namespace Queries

    ''' <summary>Filters for <c>GET /dois</c>.</summary>
    ''' <example>
    ''' <code>
    ''' Dim failed = Await ojs.Dois.ListAllAsync(New DoiQuery With {.Status = {DoiStatus.Error, DoiStatus.Stale}})
    ''' </code>
    ''' </example>
    Public Class DoiQuery
        Inherits ListQuery

        ''' <summary>Only DOIs with one of these statuses.</summary>
        Public Property Status As IEnumerable(Of DoiStatus)

        ''' <inheritdoc/>
        Protected Friend Overrides Sub AppendTo(query As QueryBuilder)
            MyBase.AppendTo(query)
            query.AddEnums("status", Status)
        End Sub

    End Class

    ''' <summary>Filters for <c>GET /invitations/{type}</c>.</summary>
    ''' <example>
    ''' <code>
    ''' Dim mine = Await ojs.Invitations.ListAsync(InvitationTypes.UserRoleAssignment, New InvitationQuery With {.Inviter = 1, .Status = InvitationStatus.Pending})
    ''' </code>
    ''' </example>
    Public Class InvitationQuery
        Inherits ListQuery

        ''' <summary>Only invitations with this status.</summary>
        Public Property Status As InvitationStatus?

        ''' <summary>Only invitations for this email address or user ID.</summary>
        Public Property Invitee As String

        ''' <summary>Only invitations sent by this user.</summary>
        Public Property Inviter As Integer?

        ''' <inheritdoc/>
        Protected Friend Overrides Sub AppendTo(query As QueryBuilder)
            MyBase.AppendTo(query)
            query.Add("status", If(Status.HasValue, Status.Value.ToString().ToLowerInvariant(), Nothing)).
                  Add("invitee", Invitee).Add("inviter", Inviter)
        End Sub

    End Class

    ''' <summary>The resolution of a usage timeline.</summary>
    Public Enum TimelineResolution
        ''' <summary>One point per month (OJS default).</summary>
        Month
        ''' <summary>One point per day.</summary>
        Day
    End Enum

    ''' <summary>Shared filters of the usage statistics endpoints.</summary>
    ''' <remarks>
    ''' OJS defaults to the last 30 days (ending yesterday) when no dates are given. Usage statistics are compiled by
    ''' a daily scheduled task, so today's visits are not included yet.
    ''' </remarks>
    ''' <example>
    ''' <code>
    ''' ' All usage statistics queries share these filters.
    ''' Dim q As New PublicationStatsQuery With {.DateStart = New DateOnly(2026, 1, 1), .DateEnd = New DateOnly(2026, 3, 31), .OrderDirection = SortDirection.Ascending}
    ''' </code>
    ''' </example>
    Public MustInherit Class StatsQuery
        Inherits ListQuery

        ''' <summary>The first day to include.</summary>
        Public Property DateStart As DateOnly?

        ''' <summary>The last day to include.</summary>
        Public Property DateEnd As DateOnly?

        ''' <summary>Matches against titles (and other text, depending on the endpoint).</summary>
        Public Property SearchPhrase As String

        ''' <summary>Order by views ascending or descending (OJS default descending).</summary>
        Public Property OrderDirection As SortDirection?

        ''' <summary>Writes the filters shared by list and timeline requests (no paging or ordering).</summary>
        ''' <param name="query">The query string builder to write to.</param>
        Protected Friend Overridable Sub AppendFilters(query As QueryBuilder)
            query.Add("dateStart", DateStart).Add("dateEnd", DateEnd).Add("searchPhrase", SearchPhrase)
        End Sub

        ''' <inheritdoc/>
        Protected Friend Overrides Sub AppendTo(query As QueryBuilder)
            MyBase.AppendTo(query)
            AppendFilters(query)
            query.Add("orderDirection", QueryText.Direction(OrderDirection))
        End Sub

        ''' <summary>Builds the query of a timeline request.</summary>
        ''' <param name="resolution">Per day or per month; <c>Nothing</c> for the OJS default.</param>
        ''' <param name="filesOnly"><c>True</c> to count only file views and downloads.</param>
        Friend Function ToTimelineQuery(resolution As TimelineResolution?, filesOnly As Boolean) As QueryBuilder
            Dim q As New QueryBuilder()
            AppendFilters(q)
            q.Add("timelineInterval", If(resolution.HasValue, resolution.Value.ToString().ToLowerInvariant(), Nothing))
            If filesOnly Then q.Add("type", "files")
            Return q
        End Function

    End Class

    ''' <summary>Filters for <c>GET /stats/contexts</c> (site-wide journal homepage views).</summary>
    ''' <example>
    ''' <code>
    ''' Dim top = Await ojs.Stats.Contexts.ListAsync(New ContextStatsQuery With {.DateStart = New DateOnly(2026, 1, 1)})
    ''' </code>
    ''' </example>
    Public Class ContextStatsQuery
        Inherits StatsQuery

        ''' <summary>Only these journals.</summary>
        Public Property ContextIds As IEnumerable(Of Integer)

        ''' <inheritdoc/>
        Protected Friend Overrides Sub AppendFilters(query As QueryBuilder)
            MyBase.AppendFilters(query)
            query.Add("contextIds", ContextIds)
        End Sub

    End Class

    ''' <summary>Filters for <c>GET /stats/issues</c>.</summary>
    ''' <example>
    ''' <code>
    ''' Dim views = Await ojs.Stats.Issues.ListAsync(New IssueStatsQuery With {.IssueIds = {4, 5}})
    ''' </code>
    ''' </example>
    Public Class IssueStatsQuery
        Inherits StatsQuery

        ''' <summary>Only these issues.</summary>
        Public Property IssueIds As IEnumerable(Of Integer)

        ''' <inheritdoc/>
        Protected Friend Overrides Sub AppendFilters(query As QueryBuilder)
            MyBase.AppendFilters(query)
            query.Add("issueIds", IssueIds)
        End Sub

    End Class

    ''' <summary>Filters for the <c>GET /stats/publications</c> endpoints (articles, files and geography).</summary>
    ''' <example>
    ''' <code>
    ''' Dim q As New PublicationStatsQuery With {
    '''     .DateStart = New DateOnly(2026, 1, 1), .DateEnd = New DateOnly(2026, 6, 30), .SectionIds = {1}}
    ''' Dim countries = Await ojs.Stats.Publications.ListCountriesAsync(q)
    ''' </code>
    ''' </example>
    Public Class PublicationStatsQuery
        Inherits StatsQuery

        ''' <summary>Only articles in these issues.</summary>
        Public Property IssueIds As IEnumerable(Of Integer)

        ''' <summary>Only articles in these sections.</summary>
        Public Property SectionIds As IEnumerable(Of Integer)

        ''' <summary>Only these submissions.</summary>
        Public Property SubmissionIds As IEnumerable(Of Integer)

        ''' <inheritdoc/>
        Protected Friend Overrides Sub AppendFilters(query As QueryBuilder)
            MyBase.AppendFilters(query)
            query.Add("issueIds", IssueIds).Add("sectionIds", SectionIds).Add("submissionIds", SubmissionIds)
        End Sub

    End Class

End Namespace
