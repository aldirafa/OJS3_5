Imports System.IO
Imports System.Net.Http
Imports System.Threading

Namespace Http

    ''' <summary>
    ''' A file streamed from the OJS API (for example a DOI export or a review exported as PDF).
    ''' Dispose it when done to release the underlying connection.
    ''' </summary>
    ''' <example>
    ''' <code>
    ''' Using file = Await ojs.Reviews.DownloadPdfAsync(submissionId, reviewAssignmentId, authorFriendly:=True)
    '''     Using target = IO.File.Create(If(file.FileName, "review.pdf"))
    '''         Await file.Content.CopyToAsync(target)
    '''     End Using
    ''' End Using
    ''' </code>
    ''' </example>
    Public NotInheritable Class DownloadedFile
        Implements IDisposable, IAsyncDisposable

        Private ReadOnly _response As HttpResponseMessage

        Friend Sub New(response As HttpResponseMessage, content As Stream)
            _response = response
            Me.Content = content
            Dim headers = response.Content.Headers
            ContentType = headers.ContentType?.MediaType
            ContentLength = headers.ContentLength
            Dim disposition = headers.ContentDisposition
            If disposition IsNot Nothing Then
                FileName = If(disposition.FileNameStar, disposition.FileName)?.Trim(""""c)
            End If
        End Sub

        ''' <summary>The response body. Read it once; it is not seekable.</summary>
        Public ReadOnly Property Content As Stream

        ''' <summary>The media type reported by the server, e.g. <c>application/pdf</c>.</summary>
        Public ReadOnly Property ContentType As String

        ''' <summary>The file name from the <c>Content-Disposition</c> header, when present.</summary>
        Public ReadOnly Property FileName As String

        ''' <summary>The length in bytes, when reported.</summary>
        Public ReadOnly Property ContentLength As Long?

        ''' <summary>Reads the whole body into memory.</summary>
        ''' <param name="cancellationToken">Cancels the request.</param>
        Public Async Function ReadAllBytesAsync(Optional cancellationToken As CancellationToken = Nothing) As Task(Of Byte())
            Using ms As New MemoryStream()
                Await Content.CopyToAsync(ms, cancellationToken).ConfigureAwait(False)
                Return ms.ToArray()
            End Using
        End Function

        ''' <summary>Copies the body into a file at <paramref name="path"/>, overwriting it.</summary>
        ''' <param name="path">Where to save the file; an existing file is overwritten.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        Public Async Function SaveAsAsync(path As String, Optional cancellationToken As CancellationToken = Nothing) As Task
            Using target = File.Create(path)
                Await Content.CopyToAsync(target, cancellationToken).ConfigureAwait(False)
            End Using
        End Function

        ''' <inheritdoc/>
        Public Sub Dispose() Implements IDisposable.Dispose
            Content.Dispose()
            _response.Dispose()
        End Sub

        ''' <inheritdoc/>
        Public Function DisposeAsync() As ValueTask Implements IAsyncDisposable.DisposeAsync
            Dispose()
            Return ValueTask.CompletedTask
        End Function

    End Class

End Namespace
