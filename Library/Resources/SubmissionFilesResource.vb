Imports System.Globalization
Imports System.IO
Imports System.Net.Http
Imports System.Text.Json.Nodes
Imports System.Threading
Imports Library.Http
Imports Library.Models

Namespace Resources

    ''' <summary>
    ''' <c>/submissions/{submissionId}/files</c> — files attached to a submission at each workflow stage.
    ''' Most operations need the <see cref="WorkflowStage"/> in which the file is being accessed, which OJS uses for
    ''' permission checks.
    ''' </summary>
    ''' <example>
    ''' <code>
    ''' Using fs = IO.File.OpenRead("manuscript.docx")
    '''     Dim file = Await ojs.SubmissionFiles.UploadAsync(submissionId, fs, "manuscript.docx", New SubmissionFileEdit With {
    '''         .FileStage = SubmissionFileStage.Submission,
    '''         .GenreId = 1,
    '''         .Name = LocalizedString.Create("en", "manuscript.docx")})
    ''' End Using
    ''' </code>
    ''' </example>
    Public NotInheritable Class SubmissionFilesResource
        Inherits ResourceBase

        Friend Sub New(http As OjsHttp)
            MyBase.New(http)
        End Sub

        Private Shared Function PathOf(submissionId As Integer, Optional submissionFileId As Integer? = Nothing) As String
            Dim p = $"submissions/{submissionId}/files"
            If submissionFileId.HasValue Then p &= $"/{submissionFileId.Value}"
            Return p
        End Function

        Private Shared Function StageQuery(stageId As WorkflowStage) As QueryBuilder
            Return New QueryBuilder().Add("stageId", CInt(stageId))
        End Function

        ''' <summary>Gets the files of a submission.</summary>
        ''' <param name="submissionId">The submission ID.</param>
        ''' <param name="fileStages">Only files in these file stages.</param>
        ''' <param name="reviewRoundIds">Only files in these review rounds.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim files = Await ojs.SubmissionFiles.ListAsync(12, {SubmissionFileStage.ProductionReady})
        ''' For Each f In files : Console.WriteLine(f.Name.GetValue("en")) : Next
        ''' </code>
        ''' </example>
        Public Function ListAsync(submissionId As Integer, Optional fileStages As IEnumerable(Of SubmissionFileStage) = Nothing,
                                  Optional reviewRoundIds As IEnumerable(Of Integer) = Nothing,
                                  Optional cancellationToken As CancellationToken = Nothing) As Task(Of List(Of SubmissionFile))
            Dim q = New QueryBuilder().AddEnums("fileStages", fileStages).Add("reviewRoundIds", reviewRoundIds)
            Return Http.GetAsync(Of List(Of SubmissionFile))(PathOf(submissionId), q, cancellationToken)
        End Function

        ''' <summary>Gets one submission file.</summary>
        ''' <param name="submissionId">The submission ID.</param>
        ''' <param name="submissionFileId">The submission file ID.</param>
        ''' <param name="stageId">The workflow stage the file is accessed from.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim file = Await ojs.SubmissionFiles.GetAsync(12, 40, WorkflowStage.Production)
        ''' Console.WriteLine(file.Url)
        ''' </code>
        ''' </example>
        Public Function GetAsync(submissionId As Integer, submissionFileId As Integer, stageId As WorkflowStage,
                                 Optional cancellationToken As CancellationToken = Nothing) As Task(Of SubmissionFile)
            Return Http.GetAsync(Of SubmissionFile)(PathOf(submissionId, submissionFileId), StageQuery(stageId), cancellationToken)
        End Function

        ''' <summary>
        ''' Uploads a new submission file. <see cref="SubmissionFileEdit.FileStage"/> is required, and usually
        ''' <see cref="SubmissionFileEdit.GenreId"/> (the file's component type) too.
        ''' </summary>
        ''' <param name="submissionId">The submission ID.</param>
        ''' <param name="content">The file content.</param>
        ''' <param name="fileName">The file name.</param>
        ''' <param name="details">The file metadata (file stage, genre, name, ...).</param>
        ''' <param name="contentType">Optional MIME type.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Using pdf = File.OpenRead("manuscript.pdf")
        '''     Dim uploaded = Await ojs.SubmissionFiles.UploadAsync(12, pdf, "manuscript.pdf", New SubmissionFileEdit With {
        '''         .FileStage = SubmissionFileStage.Submission, .GenreId = 1})
        ''' End Using
        ''' </code>
        ''' </example>
        Public Function UploadAsync(submissionId As Integer, content As Stream, fileName As String, details As SubmissionFileEdit,
                                    Optional contentType As String = Nothing,
                                    Optional cancellationToken As CancellationToken = Nothing) As Task(Of SubmissionFile)
            ArgumentNullException.ThrowIfNull(content)
            ArgumentException.ThrowIfNullOrEmpty(fileName)
            ArgumentNullException.ThrowIfNull(details)
            Dim form As New MultipartFormDataContent()
            FormFields.AddModel(form, details)
            form.Add(TemporaryFilesResource.CreateFilePart(content, contentType), "file", fileName)
            Return Http.PostMultipartAsync(Of SubmissionFile)(PathOf(submissionId), form, cancellationToken)
        End Function

        ''' <summary>Edits a submission file's metadata. Only properties you set are changed.</summary>
        ''' <param name="submissionId">The submission ID.</param>
        ''' <param name="submissionFileId">The submission file ID.</param>
        ''' <param name="stageId">The workflow stage the file is accessed in; OJS checks the user's access to that stage.</param>
        ''' <param name="changes">The properties to change; only the ones you set are sent.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Await ojs.SubmissionFiles.UpdateAsync(12, 40, WorkflowStage.Production, New SubmissionFileEdit With {.Name = LocalizedString.Create("en", "Final PDF")})
        ''' </code>
        ''' </example>
        Public Function UpdateAsync(submissionId As Integer, submissionFileId As Integer, stageId As WorkflowStage, changes As SubmissionFileEdit,
                                    Optional cancellationToken As CancellationToken = Nothing) As Task(Of SubmissionFile)
            ArgumentNullException.ThrowIfNull(changes)
            Return Http.SendJsonAsync(Of SubmissionFile)(HttpMethod.Put, PathOf(submissionId, submissionFileId), StageQuery(stageId), changes, cancellationToken)
        End Function

        ''' <summary>Deletes a submission file and returns it as it was.</summary>
        ''' <param name="submissionId">The submission ID.</param>
        ''' <param name="submissionFileId">The submission file ID.</param>
        ''' <param name="stageId">The workflow stage the file is accessed in; OJS checks the user's access to that stage.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Await ojs.SubmissionFiles.DeleteAsync(12, 40, WorkflowStage.Production)
        ''' </code>
        ''' </example>
        Public Function DeleteAsync(submissionId As Integer, submissionFileId As Integer, stageId As WorkflowStage,
                                    Optional cancellationToken As CancellationToken = Nothing) As Task(Of SubmissionFile)
            Return Http.DeleteAsync(Of SubmissionFile)(PathOf(submissionId, submissionFileId), StageQuery(stageId), ct:=cancellationToken)
        End Function

        ''' <summary>Copies a submission file to another file stage (for example from submission files to review files).</summary>
        ''' <param name="submissionId">The submission ID.</param>
        ''' <param name="submissionFileId">The file to copy.</param>
        ''' <param name="stageId">The workflow stage the file is accessed from.</param>
        ''' <param name="toFileStage">The file stage to copy it to.</param>
        ''' <param name="reviewRoundId">The review round, required when copying to a review file stage.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <returns>The new copy.</returns>
        ''' <example>
        ''' <code>
        ''' ' Copy a copyedited file into the production-ready files.
        ''' Dim copy = Await ojs.SubmissionFiles.CopyAsync(12, 40, WorkflowStage.Editing, SubmissionFileStage.ProductionReady)
        ''' </code>
        ''' </example>
        Public Function CopyAsync(submissionId As Integer, submissionFileId As Integer, stageId As WorkflowStage,
                                  toFileStage As SubmissionFileStage, Optional reviewRoundId As Integer? = Nothing,
                                  Optional cancellationToken As CancellationToken = Nothing) As Task(Of SubmissionFile)
            Dim body As New JsonObject From {{"toFileStage", CInt(toFileStage)}}
            If reviewRoundId.HasValue Then body("reviewRoundId") = reviewRoundId.Value
            Return Http.SendJsonAsync(Of SubmissionFile)(HttpMethod.Put, PathOf(submissionId, submissionFileId) & "/copy",
                                                         StageQuery(stageId), body, cancellationToken)
        End Function

    End Class

End Namespace
