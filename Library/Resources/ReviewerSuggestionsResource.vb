Imports System.Text.Json
Imports System.Threading
Imports OJS3_5.Library.Http
Imports OJS3_5.Library.Models
Imports OJS3_5.Library.Paging

Namespace Resources

    ''' <summary>
    ''' <c>/submissions/{submissionId}/reviewers/suggestions</c> — reviewers suggested by authors (OJS 3.5+).
    ''' </summary>
    ''' <example>
    ''' <code>
    ''' Dim page = Await ojs.ReviewerSuggestions.ListAsync(12)
    ''' For Each s In page.Items : Console.WriteLine($"{s.FullName}: {s.SuggestionReason.GetValue("en")}") : Next
    ''' </code>
    ''' </example>
    Public NotInheritable Class ReviewerSuggestionsResource
        Inherits ResourceBase

        Friend Sub New(http As OjsHttp)
            MyBase.New(http)
        End Sub

        Private Shared Function PathOf(submissionId As Integer, Optional suggestionId As Integer? = Nothing) As String
            Dim p = $"submissions/{submissionId}/reviewers/suggestions"
            If suggestionId.HasValue Then p &= $"/{suggestionId.Value}"
            Return p
        End Function

        ''' <summary>Gets the reviewer suggestions of a submission.</summary>
        ''' <param name="submissionId">The submission ID.</param>
        ''' <param name="approved">Only approved (<c>True</c>) or not yet approved (<c>False</c>) suggestions.</param>
        ''' <param name="includeReviewerData">Include the matching reviewer account, when one exists.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim page = Await ojs.ReviewerSuggestions.ListAsync(12, approved:=False)
        ''' For Each s In page.Items : Console.WriteLine(s.Email) : Next
        ''' </code>
        ''' </example>
        Public Function ListAsync(submissionId As Integer, Optional approved As Boolean? = Nothing,
                                  Optional includeReviewerData As Boolean? = Nothing,
                                  Optional cancellationToken As CancellationToken = Nothing) As Task(Of PagedResult(Of ReviewerSuggestion))
            Dim q = New QueryBuilder().Add("approved", approved).Add("include_reviewer_data", includeReviewerData)
            Return Http.GetAsync(Of PagedResult(Of ReviewerSuggestion))(PathOf(submissionId), q, cancellationToken)
        End Function

        ''' <summary>Gets one reviewer suggestion.</summary>
        ''' <param name="submissionId">The submission ID.</param>
        ''' <param name="suggestionId">The reviewer suggestion ID.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim suggestion = Await ojs.ReviewerSuggestions.GetAsync(12, 3)
        ''' Console.WriteLine(suggestion.SuggestionReason.GetValue("en"))
        ''' </code>
        ''' </example>
        Public Function GetAsync(submissionId As Integer, suggestionId As Integer,
                                 Optional cancellationToken As CancellationToken = Nothing) As Task(Of ReviewerSuggestion)
            Return Http.GetAsync(Of ReviewerSuggestion)(PathOf(submissionId, suggestionId), Nothing, cancellationToken)
        End Function

        ''' <summary>Adds a reviewer suggestion.</summary>
        ''' <param name="submissionId">The submission ID.</param>
        ''' <param name="suggestion">The suggested reviewer.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Await ojs.ReviewerSuggestions.CreateAsync(12, New ReviewerSuggestionEdit With {
        '''     .GivenName = LocalizedString.Create("en", "Grace"), .FamilyName = LocalizedString.Create("en", "Hopper"), .Email = "grace@example.com",
        '''     .Affiliation = LocalizedString.Create("en", "Example University"), .SuggestionReason = LocalizedString.Create("en", "Expert in compilers")})
        ''' </code>
        ''' </example>
        Public Function CreateAsync(submissionId As Integer, suggestion As ReviewerSuggestionEdit,
                                    Optional cancellationToken As CancellationToken = Nothing) As Task(Of ReviewerSuggestion)
            ArgumentNullException.ThrowIfNull(suggestion)
            Return Http.PostAsync(Of ReviewerSuggestion)(PathOf(submissionId), suggestion, cancellationToken)
        End Function

        ''' <summary>Edits a reviewer suggestion. Only properties you set are changed.</summary>
        ''' <param name="submissionId">The submission ID.</param>
        ''' <param name="suggestionId">The reviewer suggestion ID.</param>
        ''' <param name="changes">The properties to change; only the ones you set are sent.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Await ojs.ReviewerSuggestions.UpdateAsync(12, 3, New ReviewerSuggestionEdit With {.OrcidId = "0000-0002-1825-0097"})
        ''' </code>
        ''' </example>
        Public Function UpdateAsync(submissionId As Integer, suggestionId As Integer, changes As ReviewerSuggestionEdit,
                                    Optional cancellationToken As CancellationToken = Nothing) As Task(Of ReviewerSuggestion)
            ArgumentNullException.ThrowIfNull(changes)
            Return Http.PutAsync(Of ReviewerSuggestion)(PathOf(submissionId, suggestionId), changes, cancellationToken)
        End Function

        ''' <summary>Deletes a reviewer suggestion.</summary>
        ''' <param name="submissionId">The submission ID.</param>
        ''' <param name="suggestionId">The reviewer suggestion ID.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Await ojs.ReviewerSuggestions.DeleteAsync(12, 3)
        ''' </code>
        ''' </example>
        Public Function DeleteAsync(submissionId As Integer, suggestionId As Integer,
                                    Optional cancellationToken As CancellationToken = Nothing) As Task
            Return Http.DeleteAsync(Of JsonElement)(PathOf(submissionId, suggestionId), ct:=cancellationToken)
        End Function

    End Class

End Namespace
