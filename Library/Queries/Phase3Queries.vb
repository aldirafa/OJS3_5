Imports Library.Http
Imports Library.Models

Namespace Queries

    ''' <summary>How to order users.</summary>
    Public Enum UserOrderBy
        ''' <summary><c>id</c></summary>
        Id
        ''' <summary><c>familyName</c></summary>
        FamilyName
        ''' <summary><c>givenName</c></summary>
        GivenName
    End Enum

    ''' <summary>Which user accounts to include.</summary>
    Public Enum UserStatusFilter
        ''' <summary>Only active accounts (OJS default).</summary>
        Active
        ''' <summary>Only disabled accounts.</summary>
        Disabled
        ''' <summary>All accounts.</summary>
        All
    End Enum

    Partial Friend Module QueryText

        ''' <summary>Converts a user sort order to its query value.</summary>
        ''' <param name="value">The value to convert; <c>Nothing</c> gives <c>Nothing</c>.</param>
        Public Function OrderBy(value As UserOrderBy?) As String
            If Not value.HasValue Then Return Nothing
            Select Case value.Value
                Case UserOrderBy.FamilyName : Return "familyName"
                Case UserOrderBy.GivenName : Return "givenName"
                Case Else : Return "id"
            End Select
        End Function

        ''' <summary>Converts a user status filter to its query value.</summary>
        ''' <param name="value">The value to convert; <c>Nothing</c> gives <c>Nothing</c>.</param>
        Public Function Status(value As UserStatusFilter?) As String
            If Not value.HasValue Then Return Nothing
            Return value.Value.ToString().ToLowerInvariant()
        End Function

    End Module

    ''' <summary>Shared filters of <c>GET /users</c> and <c>GET /users/reviewers</c>.</summary>
    ''' <example>
    ''' <code>
    ''' Dim q As New UserQuery With {.Status = UserStatusFilter.All, .OrderBy = UserOrderBy.FamilyName, .SearchPhrase = "smith"}
    ''' </code>
    ''' </example>
    Public MustInherit Class UserQueryBase
        Inherits ListQuery

        ''' <summary>Active, disabled or all accounts (OJS default active).</summary>
        Public Property Status As UserStatusFilter?

        ''' <summary>The sort order.</summary>
        Public Property OrderBy As UserOrderBy?

        ''' <summary>The sort direction.</summary>
        Public Property OrderDirection As SortDirection?

        ''' <summary>Matches against names, user names, emails, affiliations and interests.</summary>
        Public Property SearchPhrase As String

        ''' <inheritdoc/>
        Protected Friend Overrides Sub AppendTo(query As QueryBuilder)
            MyBase.AppendTo(query)
            query.Add("status", QueryText.Status(Status)).Add("orderBy", QueryText.OrderBy(OrderBy)).
                  Add("orderDirection", QueryText.Direction(OrderDirection)).Add("searchPhrase", SearchPhrase)
        End Sub

    End Class

    ''' <summary>Filters for <c>GET /users</c>.</summary>
    ''' <example>
    ''' <code>
    ''' Dim editors = Await ojs.Users.ListAllAsync(New UserQuery With {
    '''     .RoleIds = {UserRole.Manager, UserRole.SubEditor},
    '''     .OrderBy = UserOrderBy.FamilyName})
    ''' </code>
    ''' </example>
    Public Class UserQuery
        Inherits UserQueryBase

        ''' <summary>Only users with one of these roles.</summary>
        Public Property RoleIds As IEnumerable(Of UserRole)

        ''' <summary>Only users assigned to this submission.</summary>
        Public Property AssignedToSubmission As Integer?

        ''' <summary>Only users assigned to <see cref="AssignedToSubmission"/> in this stage.</summary>
        Public Property AssignedToSubmissionStage As WorkflowStage?

        ''' <summary>Only users assigned as editors of this section.</summary>
        Public Property AssignedToSection As Integer?

        ''' <inheritdoc/>
        Protected Friend Overrides Sub AppendTo(query As QueryBuilder)
            MyBase.AppendTo(query)
            query.AddEnums("roleIds", RoleIds).Add("assignedToSubmission", AssignedToSubmission).
                  AddEnum("assignedToSubmissionStage", AssignedToSubmissionStage).Add("assignedToSection", AssignedToSection)
        End Sub

    End Class

    ''' <summary>Filters for <c>GET /users/reviewers</c>.</summary>
    ''' <remarks>
    ''' <see cref="ReviewsCompleted"/>, <see cref="ReviewsActive"/> and <see cref="DaysSinceLastAssignment"/> take a
    ''' single number (a minimum) or a range such as <c>"2-5"</c>.
    ''' </remarks>
    ''' <example>
    ''' <code>
    ''' Dim reviewers = Await ojs.Users.ListReviewersAsync(New ReviewerQuery With {
    '''     .ReviewerRating = 4, .ReviewsActive = "0-1", .SearchPhrase = "biology"})
    ''' </code>
    ''' </example>
    Public Class ReviewerQuery
        Inherits UserQueryBase

        ''' <summary>Only reviewers with at least this average rating (1–5).</summary>
        Public Property ReviewerRating As Integer?

        ''' <summary>Only reviewers assigned in this review stage.</summary>
        Public Property ReviewStage As WorkflowStage?

        ''' <summary>Number of completed reviews: a minimum (<c>"3"</c>) or a range (<c>"3-10"</c>).</summary>
        Public Property ReviewsCompleted As String

        ''' <summary>Number of active reviews: a minimum or a range (<c>"0-2"</c>).</summary>
        Public Property ReviewsActive As String

        ''' <summary>Days since the last assignment: a minimum or a range (<c>"30-90"</c>).</summary>
        Public Property DaysSinceLastAssignment As String

        ''' <summary>Only reviewers whose average completion time is at most this many days.</summary>
        Public Property AverageCompletion As Integer?

        ''' <inheritdoc/>
        Protected Friend Overrides Sub AppendTo(query As QueryBuilder)
            MyBase.AppendTo(query)
            query.Add("reviewerRating", ReviewerRating).AddEnum("reviewStage", ReviewStage).
                  Add("reviewsCompleted", ReviewsCompleted).Add("reviewsActive", ReviewsActive).
                  Add("daysSinceLastAssignment", DaysSinceLastAssignment).Add("averageCompletion", AverageCompletion)
        End Sub

    End Class

    ''' <summary>Filters for <c>GET /contexts</c>.</summary>
    ''' <example>
    ''' <code>
    ''' Dim enabled = Await ojs.Contexts.ListAllAsync(New ContextQuery With {.IsEnabled = True})
    ''' </code>
    ''' </example>
    Public Class ContextQuery
        Inherits ListQuery

        ''' <summary>Only enabled (<c>True</c>) or disabled (<c>False</c>) journals.</summary>
        Public Property IsEnabled As Boolean?

        ''' <summary>Matches against the journal name, description and acronym.</summary>
        Public Property SearchPhrase As String

        ''' <inheritdoc/>
        Protected Friend Overrides Sub AppendTo(query As QueryBuilder)
            MyBase.AppendTo(query)
            query.Add("isEnabled", IsEnabled).Add("searchPhrase", SearchPhrase)
        End Sub

    End Class

    ''' <summary>Filters for <c>GET /announcements</c>.</summary>
    ''' <example>
    ''' <code>
    ''' Dim news = Await ojs.Announcements.ListAsync(New AnnouncementQuery With {.SearchPhrase = "call for papers", .Count = 5})
    ''' </code>
    ''' </example>
    Public Class AnnouncementQuery
        Inherits ListQuery

        ''' <summary>Matches against the title and description.</summary>
        Public Property SearchPhrase As String

        ''' <summary>Only announcements of these announcement types.</summary>
        Public Property TypeIds As IEnumerable(Of Integer)

        ''' <inheritdoc/>
        Protected Friend Overrides Sub AppendTo(query As QueryBuilder)
            MyBase.AppendTo(query)
            query.Add("searchPhrase", SearchPhrase).Add("typeIds", TypeIds)
        End Sub

    End Class

    ''' <summary>Filters for list endpoints that only support a search phrase (institutions, RORs).</summary>
    ''' <example>
    ''' <code>
    ''' Dim found = Await ojs.Rors.ListAsync(New SearchQuery With {.SearchPhrase = "Bandung", .Count = 10})
    ''' </code>
    ''' </example>
    Public Class SearchQuery
        Inherits ListQuery

        ''' <summary>Matches against the name (and other text fields, depending on the endpoint).</summary>
        Public Property SearchPhrase As String

        ''' <inheritdoc/>
        Protected Friend Overrides Sub AppendTo(query As QueryBuilder)
            MyBase.AppendTo(query)
            query.Add("searchPhrase", SearchPhrase)
        End Sub

    End Class

    ''' <summary>Filters for <c>GET /emailTemplates</c>.</summary>
    ''' <example>
    ''' <code>
    ''' Dim custom = Await ojs.EmailTemplates.ListAllAsync(New EmailTemplateQuery With {.IsModified = True})
    ''' </code>
    ''' </example>
    Public Class EmailTemplateQuery
        Inherits ListQuery

        ''' <summary>Only templates that are alternates of the template with this key.</summary>
        Public Property AlternateTo As String

        ''' <summary>Only templates changed from (<c>True</c>) or equal to (<c>False</c>) their defaults.</summary>
        Public Property IsModified As Boolean?

        ''' <summary>Matches against the key, name, subject and body.</summary>
        Public Property SearchPhrase As String

        ''' <inheritdoc/>
        Protected Friend Overrides Sub AppendTo(query As QueryBuilder)
            MyBase.AppendTo(query)
            query.Add("alternateTo", AlternateTo).Add("isModified", IsModified).Add("searchPhrase", SearchPhrase)
        End Sub

    End Class

End Namespace
