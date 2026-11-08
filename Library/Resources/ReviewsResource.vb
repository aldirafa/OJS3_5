Imports System.Net.Http
Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports System.Threading
Imports Library.Http
Imports Library.Models

Namespace Resources

    ''' <summary><c>/reviews</c> — review assignment actions, exports and history.</summary>
    ''' <example>
    ''' <code>
    ''' ' Save a review as a PDF that can be shared with the author.
    ''' Using pdf = Await ojs.Reviews.DownloadPdfAsync(submissionId, reviewAssignmentId, authorFriendly:=True)
    '''     Await pdf.SaveAsAsync("review.pdf")
    ''' End Using
    ''' </code>
    ''' </example>
    Public NotInheritable Class ReviewsResource
        Inherits ResourceBase

        Friend Sub New(http As OjsHttp)
            MyBase.New(http)
        End Sub

        Private NotInheritable Class DataEnvelope
            ''' <summary>The review assignment.</summary>
            <JsonPropertyName("_data")>
            Public Property Data As ReviewAssignment
        End Class

        ''' <summary>Gets the history of the current user's review in a review round.</summary>
        ''' <param name="submissionId">The submission ID.</param>
        ''' <param name="reviewRoundId">The review round ID.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <remarks>
        ''' OJS looks the review up by the current user, so call it with the reviewer's own token. Anyone else
        ''' (including administrators) gets a 404 error.
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' ' Call with the reviewer's own token.
        ''' Dim history = Await ojs.Reviews.GetHistoryAsync(12, 5)
        ''' Console.WriteLine($"{history.Recommendation}: {String.Join(" ", history.Comments)}")
        ''' </code>
        ''' </example>
        Public Function GetHistoryAsync(submissionId As Integer, reviewRoundId As Integer,
                                        Optional cancellationToken As CancellationToken = Nothing) As Task(Of ReviewHistory)
            Return Http.GetAsync(Of ReviewHistory)($"reviews/history/{submissionId}/{reviewRoundId}", Nothing, cancellationToken)
        End Function

        ''' <summary>Accepts or declines a review request on behalf of the reviewer.</summary>
        ''' <remarks>
        ''' OJS 3.5 only allows site administrators and journal managers to call this (it is the editor's
        ''' "accept on behalf of the reviewer" action); a reviewer's own token gets 401.
        ''' OJS 3.5.0 returns the assignment as it was <em>before</em> the change (e.g. <c>DateConfirmed</c> still
        ''' empty), so read the submission again (<c>Submissions.GetAsync</c>) to see the new state.
        ''' </remarks>
        ''' <param name="submissionId">The submission ID.</param>
        ''' <param name="reviewAssignmentId">The review assignment ID.</param>
        ''' <param name="accept"><c>True</c> to accept the request, <c>False</c> to decline it.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <returns>The review assignment as returned by OJS (see remarks).</returns>
        ''' <example>
        ''' <code>
        ''' ' As a journal manager, accept the request on the reviewer's behalf.
        ''' Await ojs.Reviews.ConfirmAsync(12, 21, accept:=True)
        ''' </code>
        ''' </example>
        Public Async Function ConfirmAsync(submissionId As Integer, reviewAssignmentId As Integer, accept As Boolean,
                                           Optional cancellationToken As CancellationToken = Nothing) As Task(Of ReviewAssignment)
            Dim body = New Dictionary(Of String, String) From {{"decision", If(accept, "accept", "decline")}}
            Dim result = Await Http.PutAsync(Of DataEnvelope)($"reviews/{submissionId}/{reviewAssignmentId}/confirmReview", body, cancellationToken).ConfigureAwait(False)
            Return result?.Data
        End Function

        ''' <summary>Builds a PDF of a review.</summary>
        ''' <param name="submissionId">The submission ID.</param>
        ''' <param name="reviewAssignmentId">The review assignment ID.</param>
        ''' <param name="authorFriendly"><c>True</c> to leave out the reviewer's identity and comments for the editor only.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <returns>The export file ID; download it with <see cref="DownloadExportAsync"/>.</returns>
        ''' <example>
        ''' <code>
        ''' Dim fileId = Await ojs.Reviews.ExportPdfAsync(12, 21, authorFriendly:=True)
        ''' Using pdf = Await ojs.Reviews.DownloadExportAsync(12, fileId)
        '''     Await pdf.SaveAsAsync("review.pdf")
        ''' End Using
        ''' </code>
        ''' </example>
        Public Function ExportPdfAsync(submissionId As Integer, reviewAssignmentId As Integer, authorFriendly As Boolean,
                                       Optional cancellationToken As CancellationToken = Nothing) As Task(Of Integer)
            Return ExportAsync(submissionId, reviewAssignmentId, "export-pdf", authorFriendly, cancellationToken)
        End Function

        ''' <summary>Builds an XML file of a review.</summary>
        ''' <param name="submissionId">The submission ID.</param>
        ''' <param name="reviewAssignmentId">The review assignment ID.</param>
        ''' <param name="authorFriendly"><c>True</c> to leave out the reviewer's identity and comments for the editor only.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <returns>The export file ID; download it with <see cref="DownloadExportAsync"/>.</returns>
        ''' <example>
        ''' <code>
        ''' Dim fileId = Await ojs.Reviews.ExportXmlAsync(12, 21, authorFriendly:=False)
        ''' Using xml = Await ojs.Reviews.DownloadExportAsync(12, fileId)
        '''     Await xml.SaveAsAsync("review.xml")
        ''' End Using
        ''' </code>
        ''' </example>
        Public Function ExportXmlAsync(submissionId As Integer, reviewAssignmentId As Integer, authorFriendly As Boolean,
                                       Optional cancellationToken As CancellationToken = Nothing) As Task(Of Integer)
            Return ExportAsync(submissionId, reviewAssignmentId, "export-xml", authorFriendly, cancellationToken)
        End Function

        Private Async Function ExportAsync(submissionId As Integer, reviewAssignmentId As Integer, action As String,
                                           authorFriendly As Boolean, ct As CancellationToken) As Task(Of Integer)
            Dim q = New QueryBuilder().Add("authorFriendly", If(authorFriendly, "1", "0"))
            Dim result = Await Http.GetAsync(Of TemporaryFileReference)($"reviews/{submissionId}/{reviewAssignmentId}/{action}", q, ct).ConfigureAwait(False)
            If result?.TemporaryFileId Is Nothing Then Throw New InvalidOperationException("OJS did not return an export file ID.")
            Return result.TemporaryFileId.Value
        End Function

        ''' <summary>Downloads a review export file.</summary>
        ''' <param name="submissionId">The submission ID.</param>
        ''' <param name="fileId">The file ID returned by <see cref="ExportPdfAsync"/> or <see cref="ExportXmlAsync"/>.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <returns>The file; dispose it when done.</returns>
        ''' <example>
        ''' <code>
        ''' Using file = Await ojs.Reviews.DownloadExportAsync(12, fileId)
        '''     Await file.SaveAsAsync(file.FileName)
        ''' End Using
        ''' </code>
        ''' </example>
        Public Function DownloadExportAsync(submissionId As Integer, fileId As Integer,
                                            Optional cancellationToken As CancellationToken = Nothing) As Task(Of DownloadedFile)
            Return Http.DownloadAsync(HttpMethod.Get, $"reviews/{submissionId}/exports/{fileId}", Nothing, Nothing, cancellationToken)
        End Function

        ''' <summary>Builds and downloads a PDF of a review in one call.</summary>
        ''' <param name="submissionId">The submission ID.</param>
        ''' <param name="reviewAssignmentId">The review assignment ID.</param>
        ''' <param name="authorFriendly"><c>True</c> to leave out the reviewer's identity and comments for the editor only.</param>
        ''' <param name="cancellationToken">Cancels the requests.</param>
        ''' <example>
        ''' <code>
        ''' Using pdf = Await ojs.Reviews.DownloadPdfAsync(12, 21, authorFriendly:=True)
        '''     Await pdf.SaveAsAsync("review.pdf")
        ''' End Using
        ''' </code>
        ''' </example>
        Public Async Function DownloadPdfAsync(submissionId As Integer, reviewAssignmentId As Integer, authorFriendly As Boolean,
                                               Optional cancellationToken As CancellationToken = Nothing) As Task(Of DownloadedFile)
            Dim fileId = Await ExportPdfAsync(submissionId, reviewAssignmentId, authorFriendly, cancellationToken).ConfigureAwait(False)
            Return Await DownloadExportAsync(submissionId, fileId, cancellationToken).ConfigureAwait(False)
        End Function

        ''' <summary>Builds and downloads an XML file of a review in one call.</summary>
        ''' <param name="submissionId">The submission ID.</param>
        ''' <param name="reviewAssignmentId">The review assignment ID.</param>
        ''' <param name="authorFriendly"><c>True</c> to leave out the reviewer's identity and comments for the editor only.</param>
        ''' <param name="cancellationToken">Cancels the requests.</param>
        ''' <example>
        ''' <code>
        ''' Using xml = Await ojs.Reviews.DownloadXmlAsync(12, 21, authorFriendly:=True)
        '''     Await xml.SaveAsAsync("review.xml")
        ''' End Using
        ''' </code>
        ''' </example>
        Public Async Function DownloadXmlAsync(submissionId As Integer, reviewAssignmentId As Integer, authorFriendly As Boolean,
                                               Optional cancellationToken As CancellationToken = Nothing) As Task(Of DownloadedFile)
            Dim fileId = Await ExportXmlAsync(submissionId, reviewAssignmentId, authorFriendly, cancellationToken).ConfigureAwait(False)
            Return Await DownloadExportAsync(submissionId, fileId, cancellationToken).ConfigureAwait(False)
        End Function

        ''' <summary>Queues a completed review for deposit to the reviewer's ORCID record.</summary>
        ''' <param name="submissionId">The submission ID.</param>
        ''' <param name="reviewAssignmentId">The review assignment ID.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <remarks>Requires the ORCID integration to be configured and the reviewer to have authorised it.</remarks>
        ''' <example>
        ''' <code>
        ''' Await ojs.Reviews.SendToOrcidAsync(12, 21)
        ''' </code>
        ''' </example>
        Public Function SendToOrcidAsync(submissionId As Integer, reviewAssignmentId As Integer,
                                         Optional cancellationToken As CancellationToken = Nothing) As Task
            Return Http.PostAsync(Of JsonElement)($"reviews/{submissionId}/{reviewAssignmentId}/sendToOrcid", Nothing, cancellationToken)
        End Function

    End Class

    ''' <summary><c>/orcid</c> — ORCID actions editors can take for authors.</summary>
    ''' <remarks>Requires the ORCID integration to be enabled and configured in the journal or site.</remarks>
    ''' <example>
    ''' <code>
    ''' Await ojs.Orcid.RequestAuthorVerificationAsync(contributorId)
    ''' </code>
    ''' </example>
    Public NotInheritable Class OrcidResource
        Inherits ResourceBase

        Friend Sub New(http As OjsHttp)
            MyBase.New(http)
        End Sub

        ''' <summary>Emails an author a request to link their ORCID iD to their contribution.</summary>
        ''' <param name="authorId">The contributor ID (see <c>Contributor.Id</c>).</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' ' Emails contributor 31 a link to connect their ORCID iD.
        ''' Await ojs.Orcid.RequestAuthorVerificationAsync(31)
        ''' </code>
        ''' </example>
        Public Function RequestAuthorVerificationAsync(authorId As Integer, Optional cancellationToken As CancellationToken = Nothing) As Task
            Return Http.PostAsync(Of JsonElement)($"orcid/requestAuthorVerification/{authorId}", Nothing, cancellationToken)
        End Function

        ''' <summary>Removes an author's ORCID iD and access tokens from their contribution.</summary>
        ''' <param name="authorId">The contributor ID.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Await ojs.Orcid.DeleteForAuthorAsync(31)
        ''' </code>
        ''' </example>
        Public Function DeleteForAuthorAsync(authorId As Integer, Optional cancellationToken As CancellationToken = Nothing) As Task
            Return Http.PostAsync(Of JsonElement)($"orcid/deleteForAuthor/{authorId}", Nothing, cancellationToken)
        End Function

    End Class

End Namespace
