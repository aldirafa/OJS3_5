Imports System.Threading
Imports OJS3_5.Library.Http
Imports OJS3_5.Library.Models
Imports OJS3_5.Library.Queries

Namespace Resources

    ''' <summary><c>/submissions/{submissionId}/decisions</c> — editorial decisions.</summary>
    ''' <example>
    ''' <code>
    ''' Dim history = Await ojs.Decisions.ListAsync(submissionId)
    ''' For Each d In history
    '''     Console.WriteLine($"{d.DateDecided:yyyy-MM-dd} {d.Label}")
    ''' Next
    ''' </code>
    ''' </example>
    Public NotInheritable Class DecisionsResource
        Inherits ResourceBase

        Friend Sub New(http As OjsHttp)
            MyBase.New(http)
        End Sub

        ''' <summary>Gets the decisions recorded for a submission.</summary>
        ''' <param name="submissionId">The submission ID.</param>
        ''' <param name="query">Filters; <c>Nothing</c> for all decisions.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim decisions = Await ojs.Decisions.ListAsync(12, New DecisionQuery With {.StageId = WorkflowStage.ExternalReview})
        ''' For Each d In decisions : Console.WriteLine($"{d.DateDecided:d} {d.Decision}") : Next
        ''' </code>
        ''' </example>
        Public Function ListAsync(submissionId As Integer, Optional query As DecisionQuery = Nothing,
                                  Optional cancellationToken As CancellationToken = Nothing) As Task(Of List(Of Decision))
            Return Http.GetAsync(Of List(Of Decision))($"submissions/{submissionId}/decisions",
                                                       If(query, New DecisionQuery()).ToQueryBuilder(), cancellationToken)
        End Function

        ''' <summary>
        ''' Records a decision, such as accepting a submission or sending it to review. The decision must be available
        ''' to the current user in the submission's current stage (see <see cref="Submission.AvailableEditorialDecisions"/>).
        ''' </summary>
        ''' <param name="submissionId">The submission ID.</param>
        ''' <param name="decision">The decision to record.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim submission = Await ojs.Submissions.GetAsync(12)
        ''' Dim accept = submission.AvailableEditorialDecisions.First(Function(o) o.Label.Contains("Accept"))
        ''' Await ojs.Decisions.CreateAsync(12, New DecisionEdit With {.Decision = accept.Id})
        ''' </code>
        ''' </example>
        Public Function CreateAsync(submissionId As Integer, decision As DecisionEdit,
                                    Optional cancellationToken As CancellationToken = Nothing) As Task(Of Decision)
            ArgumentNullException.ThrowIfNull(decision)
            Return Http.PostAsync(Of Decision)($"submissions/{submissionId}/decisions", decision, cancellationToken)
        End Function

    End Class

End Namespace
