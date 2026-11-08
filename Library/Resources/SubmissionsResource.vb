Imports System.Text.Json.Nodes
Imports System.Threading
Imports OJS3_5.Library.Http
Imports OJS3_5.Library.Models
Imports OJS3_5.Library.Paging
Imports OJS3_5.Library.Queries

Namespace Resources

    ''' <summary>
    ''' <c>/submissions</c> — list, read, create, edit, submit and delete submissions, and list their participants.
    ''' </summary>
    ''' <example>
    ''' <code>
    ''' ' First page of published submissions
    ''' Dim page = Await ojs.Submissions.ListAsync(New SubmissionQuery With {.Status = {SubmissionStatus.Published}})
    ''' For Each s In page.Items
    '''     Console.WriteLine($"{s.Id}: {s.CurrentPublication?.Title}")
    ''' Next
    '''
    ''' ' Everything in the review stage
    ''' Dim inReview = Await ojs.Submissions.ListAllAsync(New SubmissionQuery With {.StageIds = {WorkflowStage.ExternalReview}})
    ''' </code>
    ''' </example>
    Public NotInheritable Class SubmissionsResource
        Inherits ResourceBase

        Friend Sub New(http As OjsHttp)
            MyBase.New(http)
        End Sub

        ''' <summary>Gets one page of submissions (<c>GET /submissions</c>). Items contain the summary properties only.</summary>
        ''' <param name="query">Filters and paging; <c>Nothing</c> for OJS defaults (20 items).</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim page = Await ojs.Submissions.ListAsync(New SubmissionQuery With {.Status = {SubmissionStatus.Published}, .SearchPhrase = "education"})
        ''' Console.WriteLine($"{page.ItemsMax} in total")
        ''' For Each item In page.Items : Console.WriteLine(item.CurrentPublication?.Title.GetValue("en")) : Next
        ''' </code>
        ''' </example>
        Public Function ListAsync(Optional query As SubmissionQuery = Nothing,
                                  Optional cancellationToken As CancellationToken = Nothing) As Task(Of PagedResult(Of Submission))
            Return Http.GetAsync(Of PagedResult(Of Submission))("submissions", If(query, New SubmissionQuery()).ToQueryBuilder(), cancellationToken)
        End Function

        ''' <summary>Gets every submission matching <paramref name="query"/>, requesting pages of up to 100 items.</summary>
        ''' <param name="query">Filters; <see cref="ListQuery.Count"/> sets the page size.</param>
        ''' <param name="maxItems">Optional cap on the number of items returned.</param>
        ''' <param name="cancellationToken">Cancels the requests.</param>
        ''' <example>
        ''' <code>
        ''' Dim all = Await ojs.Submissions.ListAllAsync(New SubmissionQuery With {.StageIds = {WorkflowStage.ExternalReview}})
        ''' For Each item In all : Console.WriteLine(item.Id) : Next
        ''' </code>
        ''' </example>
        Public Function ListAllAsync(Optional query As SubmissionQuery = Nothing, Optional maxItems As Integer? = Nothing,
                                     Optional cancellationToken As CancellationToken = Nothing) As Task(Of List(Of Submission))
            Return ListAllCoreAsync(Of Submission, SubmissionQuery)(If(query, New SubmissionQuery()), AddressOf ListAsync, maxItems, cancellationToken)
        End Function

        ''' <summary>Streams every submission matching <paramref name="query"/> page by page (for C# <c>await foreach</c>).</summary>
        ''' <param name="query">Filters; <see cref="ListQuery.Count"/> sets the page size.</param>
        ''' <example>
        ''' <code>
        ''' ' C#: await foreach (var item in ojs.Submissions.Enumerate()) Console.WriteLine(item.Id);
        ''' ' VB has no Await For Each; use the enumerator (or ListAllAsync):
        ''' Dim e = ojs.Submissions.Enumerate().GetAsyncEnumerator()
        ''' While Await e.MoveNextAsync()
        '''     Console.WriteLine(e.Current.Id)
        ''' End While
        ''' Await e.DisposeAsync()
        ''' </code>
        ''' </example>
        Public Function Enumerate(Optional query As SubmissionQuery = Nothing) As IAsyncEnumerable(Of Submission)
            Return EnumerateCore(Of Submission, SubmissionQuery)(If(query, New SubmissionQuery()), AddressOf ListAsync)
        End Function

        ''' <summary>Gets a submission with all its properties (<c>GET /submissions/{submissionId}</c>).</summary>
        ''' <param name="submissionId">The submission ID.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim submission = Await ojs.Submissions.GetAsync(12)
        ''' Console.WriteLine($"{submission.StatusLabel}: {submission.CurrentPublication?.Title.GetValue("en")}")
        ''' </code>
        ''' </example>
        Public Function GetAsync(submissionId As Integer, Optional cancellationToken As CancellationToken = Nothing) As Task(Of Submission)
            Return Http.GetAsync(Of Submission)($"submissions/{submissionId}", Nothing, cancellationToken)
        End Function

        ''' <summary>
        ''' Creates a new, incomplete submission (<c>POST /submissions</c>). The first publication is created with it;
        ''' finish it with <see cref="SubmitAsync"/>.
        ''' </summary>
        ''' <param name="submission">The properties of the new submission.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim draft = Await ojs.Submissions.CreateAsync(New SubmissionEdit With {.Locale = "en", .SectionId = 1})
        ''' Console.WriteLine(draft.Id)
        ''' </code>
        ''' </example>
        Public Function CreateAsync(submission As SubmissionEdit, Optional cancellationToken As CancellationToken = Nothing) As Task(Of Submission)
            ArgumentNullException.ThrowIfNull(submission)
            Return Http.PostAsync(Of Submission)("submissions", submission, cancellationToken)
        End Function

        ''' <summary>Edits a submission (<c>PUT /submissions/{submissionId}</c>). Only properties you set are changed.</summary>
        ''' <param name="submissionId">The submission ID.</param>
        ''' <param name="changes">The properties to change; only the ones you set are sent.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Await ojs.Submissions.UpdateAsync(12, New SubmissionEdit With {.CommentsForTheEditors = "Thank you!"})
        ''' </code>
        ''' </example>
        Public Function UpdateAsync(submissionId As Integer, changes As SubmissionEdit,
                                    Optional cancellationToken As CancellationToken = Nothing) As Task(Of Submission)
            ArgumentNullException.ThrowIfNull(changes)
            Return Http.PutAsync(Of Submission)($"submissions/{submissionId}", changes, cancellationToken)
        End Function

        ''' <summary>Deletes a submission and returns it as it was (<c>DELETE /submissions/{submissionId}</c>).</summary>
        ''' <param name="submissionId">The submission ID.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Await ojs.Submissions.DeleteAsync(12)
        ''' </code>
        ''' </example>
        Public Function DeleteAsync(submissionId As Integer, Optional cancellationToken As CancellationToken = Nothing) As Task(Of Submission)
            Return Http.DeleteAsync(Of Submission)($"submissions/{submissionId}", ct:=cancellationToken)
        End Function

        ''' <summary>
        ''' Records the submission wizard step an author stopped at, so they can resume later
        ''' (<c>PUT /submissions/{submissionId}/saveForLater</c>).
        ''' </summary>
        ''' <param name="submissionId">The submission ID.</param>
        ''' <param name="stepName">One of <c>details</c>, <c>files</c>, <c>contributors</c>, <c>editors</c>, <c>review</c>.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Await ojs.Submissions.SaveForLaterAsync(12, "details")
        ''' </code>
        ''' </example>
        Public Function SaveForLaterAsync(submissionId As Integer, stepName As String,
                                          Optional cancellationToken As CancellationToken = Nothing) As Task(Of Submission)
            Return Http.PutAsync(Of Submission)($"submissions/{submissionId}/saveForLater",
                                                 New JsonObject From {{"step", stepName}}, cancellationToken)
        End Function

        ''' <summary>
        ''' Submits an incomplete submission to the editors (<c>PUT /submissions/{submissionId}/submit</c>).
        ''' Throws <see cref="Errors.OjsValidationException"/> listing what is missing when it cannot be submitted.
        ''' </summary>
        ''' <param name="submissionId">The submission ID.</param>
        ''' <param name="confirmCopyright">Whether the author agrees to the copyright statement (required when the journal has one).</param>
        ''' <param name="validateOnly">When <c>True</c>, only checks whether it could be submitted, without submitting.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' ' Check first, then submit.
        ''' Await ojs.Submissions.SubmitAsync(12, confirmCopyright:=True, validateOnly:=True)
        ''' Dim submitted = Await ojs.Submissions.SubmitAsync(12, confirmCopyright:=True)
        ''' </code>
        ''' </example>
        Public Function SubmitAsync(submissionId As Integer, Optional confirmCopyright As Boolean? = Nothing,
                                    Optional validateOnly As Boolean = False,
                                    Optional cancellationToken As CancellationToken = Nothing) As Task(Of Submission)
            Dim body As New JsonObject()
            If confirmCopyright.HasValue Then body("confirmCopyright") = confirmCopyright.Value
            If validateOnly Then body("_validateOnly") = True
            Return Http.PutAsync(Of Submission)($"submissions/{submissionId}/submit", body, cancellationToken)
        End Function

        ''' <summary>
        ''' Gets the users assigned to a submission (<c>GET /submissions/{submissionId}/participants[/{stageId}]</c>).
        ''' </summary>
        ''' <param name="submissionId">The submission ID.</param>
        ''' <param name="stageId">Optionally, only participants of this stage.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim people = Await ojs.Submissions.GetParticipantsAsync(12, WorkflowStage.Production)
        ''' For Each p In people : Console.WriteLine(p.FullName) : Next
        ''' </code>
        ''' </example>
        Public Function GetParticipantsAsync(submissionId As Integer, Optional stageId As WorkflowStage? = Nothing,
                                             Optional cancellationToken As CancellationToken = Nothing) As Task(Of List(Of User))
            Dim path = $"submissions/{submissionId}/participants"
            If stageId.HasValue Then path &= "/" & CInt(stageId.Value).ToString(Globalization.CultureInfo.InvariantCulture)
            Return Http.GetAsync(Of List(Of User))(path, Nothing, cancellationToken)
        End Function

    End Class

End Namespace
