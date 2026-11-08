Imports Library.Http
Imports Library.Models

Namespace Queries

    ''' <summary>How to order submissions.</summary>
    Public Enum SubmissionOrderBy
        ''' <summary><c>dateSubmitted</c> (OJS default)</summary>
        DateSubmitted
        ''' <summary><c>datePublished</c></summary>
        DatePublished
        ''' <summary><c>lastActivity</c></summary>
        LastActivity
        ''' <summary><c>lastModified</c></summary>
        LastModified
        ''' <summary><c>sequence</c></summary>
        Sequence
        ''' <summary><c>title</c></summary>
        Title
    End Enum

    ''' <summary>How to order issues.</summary>
    Public Enum IssueOrderBy
        ''' <summary><c>datePublished</c></summary>
        DatePublished
        ''' <summary><c>lastModified</c></summary>
        LastModified
        ''' <summary><c>seq</c> (the manual order set by editors)</summary>
        Sequence
    End Enum

    ''' <summary>Converts query enums to the strings OJS expects.</summary>
    Friend Module QueryText

        ''' <summary>Converts a sort direction to <c>ASC</c>/<c>DESC</c>.</summary>
        ''' <param name="value">The value to convert; <c>Nothing</c> gives <c>Nothing</c>.</param>
        Public Function Direction(value As SortDirection?) As String
            If Not value.HasValue Then Return Nothing
            Return If(value.Value = SortDirection.Ascending, "ASC", "DESC")
        End Function

        ''' <summary>Converts a submission sort order to its query value.</summary>
        ''' <param name="value">The value to convert; <c>Nothing</c> gives <c>Nothing</c>.</param>
        Public Function OrderBy(value As SubmissionOrderBy?) As String
            If Not value.HasValue Then Return Nothing
            Select Case value.Value
                Case SubmissionOrderBy.DatePublished : Return "datePublished"
                Case SubmissionOrderBy.LastActivity : Return "lastActivity"
                Case SubmissionOrderBy.LastModified : Return "lastModified"
                Case SubmissionOrderBy.Sequence : Return "sequence"
                Case SubmissionOrderBy.Title : Return "title"
                Case Else : Return "dateSubmitted"
            End Select
        End Function

        ''' <summary>Converts an issue sort order to its query value.</summary>
        ''' <param name="value">The value to convert; <c>Nothing</c> gives <c>Nothing</c>.</param>
        Public Function OrderBy(value As IssueOrderBy?) As String
            If Not value.HasValue Then Return Nothing
            Select Case value.Value
                Case IssueOrderBy.LastModified : Return "lastModified"
                Case IssueOrderBy.Sequence : Return "seq"
                Case Else : Return "datePublished"
            End Select
        End Function

    End Module

    ''' <summary>Filters for <c>GET /submissions</c>.</summary>
    ''' <example>
    ''' <code>
    ''' Dim page = Await ojs.Submissions.ListAsync(New SubmissionQuery With {
    '''     .Status = {SubmissionStatus.Published},
    '''     .SearchPhrase = "education",
    '''     .OrderBy = SubmissionOrderBy.DatePublished,
    '''     .Count = 50})
    ''' </code>
    ''' </example>
    Public Class SubmissionQuery
        Inherits ListQuery

        ''' <summary>Only submissions assigned to these user IDs. Non-managers are always limited to their own assignments.</summary>
        Public Property AssignedTo As IEnumerable(Of Integer)

        ''' <summary>Only submissions in one of these categories.</summary>
        Public Property CategoryIds As IEnumerable(Of Integer)

        ''' <summary>Only submissions with no activity in the last X days.</summary>
        Public Property DaysInactive As Integer?

        ''' <summary>Only submissions the author has not finished submitting.</summary>
        Public Property IsIncomplete As Boolean?

        ''' <summary>Only submissions where a reviewer missed a deadline.</summary>
        Public Property IsOverdue As Boolean?

        ''' <summary>Only submissions with a publication assigned to one of these issues.</summary>
        Public Property IssueIds As IEnumerable(Of Integer)

        ''' <summary>The sort order.</summary>
        Public Property OrderBy As SubmissionOrderBy?

        ''' <summary>The sort direction (OJS default descending).</summary>
        Public Property OrderDirection As SortDirection?

        ''' <summary>Matches against titles and author names.</summary>
        Public Property SearchPhrase As String

        ''' <summary>Only submissions with a publication in one of these sections.</summary>
        Public Property SectionIds As IEnumerable(Of Integer)

        ''' <summary>Only submissions currently in one of these stages.</summary>
        Public Property StageIds As IEnumerable(Of WorkflowStage)

        ''' <summary>Only submissions with one of these statuses.</summary>
        Public Property Status As IEnumerable(Of SubmissionStatus)

        ''' <inheritdoc/>
        Protected Friend Overrides Sub AppendTo(query As QueryBuilder)
            MyBase.AppendTo(query)
            query.Add("assignedTo", AssignedTo).Add("categoryIds", CategoryIds).Add("daysInactive", DaysInactive).
                  Add("isIncomplete", IsIncomplete).Add("isOverdue", IsOverdue).Add("issueIds", IssueIds).
                  Add("orderBy", QueryText.OrderBy(OrderBy)).Add("orderDirection", QueryText.Direction(OrderDirection)).
                  Add("searchPhrase", SearchPhrase).Add("sectionIds", SectionIds).
                  AddEnums("stageIds", StageIds).AddEnums("status", Status)
        End Sub

    End Class

    ''' <summary>Filters for <c>GET /issues</c>.</summary>
    ''' <example>
    ''' <code>
    ''' Dim issues = Await ojs.Issues.ListAllAsync(New IssueQuery With {.IsPublished = True, .Years = {2024, 2025}})
    ''' </code>
    ''' </example>
    Public Class IssueQuery
        Inherits ListQuery

        ''' <summary>The sort order.</summary>
        Public Property OrderBy As IssueOrderBy?

        ''' <summary>The sort direction.</summary>
        Public Property OrderDirection As SortDirection?

        ''' <summary>Only published (<c>True</c>) or unpublished (<c>False</c>) issues.</summary>
        Public Property IsPublished As Boolean?

        ''' <summary>Only issues with one of these volume numbers.</summary>
        Public Property Volumes As IEnumerable(Of Integer)

        ''' <summary>Only issues with one of these issue numbers.</summary>
        Public Property Numbers As IEnumerable(Of String)

        ''' <summary>Only issues from one of these years.</summary>
        Public Property Years As IEnumerable(Of Integer)

        ''' <summary>Matches against the issue title and identification.</summary>
        Public Property SearchPhrase As String

        ''' <inheritdoc/>
        Protected Friend Overrides Sub AppendTo(query As QueryBuilder)
            MyBase.AppendTo(query)
            query.Add("orderBy", QueryText.OrderBy(OrderBy)).Add("orderDirection", QueryText.Direction(OrderDirection)).
                  Add("isPublished", IsPublished).Add("volumes", Volumes).Add("numbers", Numbers).
                  Add("years", Years).Add("searchPhrase", SearchPhrase)
        End Sub

    End Class

    ''' <summary>Filters for <c>GET /sections</c>.</summary>
    ''' <example>
    ''' <code>
    ''' Dim active = Await ojs.Sections.ListAllAsync(New SectionQuery With {.IsInactive = False})
    ''' </code>
    ''' </example>
    Public Class SectionQuery
        Inherits ListQuery

        ''' <summary>Matches against the section title and abbreviation.</summary>
        Public Property SearchPhrase As String

        ''' <summary>Only sections with this title.</summary>
        Public Property Title As String

        ''' <summary>Only sections with this abbreviation.</summary>
        Public Property Abbrev As String

        ''' <summary>Only inactive (<c>True</c>) or active (<c>False</c>) sections.</summary>
        Public Property IsInactive As Boolean?

        ''' <summary>Only sections assigned to one of these editors.</summary>
        Public Property EditorIds As IEnumerable(Of Integer)

        ''' <inheritdoc/>
        Protected Friend Overrides Sub AppendTo(query As QueryBuilder)
            MyBase.AppendTo(query)
            query.Add("searchPhrase", SearchPhrase).Add("title", Title).Add("abbrev", Abbrev).
                  Add("isInactive", IsInactive).Add("editorIds", EditorIds)
        End Sub

    End Class

    ''' <summary>Filters for <c>GET /submissions/{id}/decisions</c>.</summary>
    ''' <example>
    ''' <code>
    ''' Dim decisions = Await ojs.Decisions.ListAsync(12, New DecisionQuery With {.StageId = WorkflowStage.ExternalReview})
    ''' </code>
    ''' </example>
    Public Class DecisionQuery

        ''' <summary>Only these decision types (<c>Decision::*</c> constants).</summary>
        Public Property DecisionTypes As IEnumerable(Of Integer)

        ''' <summary>Only decisions made by these editors.</summary>
        Public Property EditorIds As IEnumerable(Of Integer)

        ''' <summary>Only decisions in this review round.</summary>
        Public Property ReviewRoundId As Integer?

        ''' <summary>Only decisions in this stage.</summary>
        Public Property StageId As WorkflowStage?

        Friend Function ToQueryBuilder() As QueryBuilder
            Return New QueryBuilder().Add("decisionTypes", DecisionTypes).Add("editorIds", EditorIds).
                Add("reviewRoundId", ReviewRoundId).AddEnum("stageId", StageId)
        End Function

    End Class

End Namespace
