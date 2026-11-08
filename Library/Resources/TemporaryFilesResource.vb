Imports System.IO
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Threading
Imports Library.Http
Imports Library.Models

Namespace Resources

    ''' <summary>
    ''' <c>/temporaryFiles</c> — upload files to temporary storage and get an ID to use in other requests.
    ''' Temporary files can only be used by the user who uploaded them.
    ''' </summary>
    ''' <example>
    ''' <code>
    ''' Using fs = IO.File.OpenRead("logo.png")
    '''     Dim tmp = Await ojs.TemporaryFiles.UploadAsync(fs, "logo.png", "image/png")
    '''     ' then e.g. PUT /contexts/{id} with pageHeaderLogoImage = {temporaryFileId = tmp.Id, altText = "Logo"}
    ''' End Using
    ''' </code>
    ''' </example>
    Public NotInheritable Class TemporaryFilesResource
        Inherits ResourceBase

        Friend Sub New(http As OjsHttp)
            MyBase.New(http)
        End Sub

        ''' <summary>Uploads a file and returns its temporary file record.</summary>
        ''' <param name="content">The file content.</param>
        ''' <param name="fileName">The file name to report to OJS (used to detect the file type).</param>
        ''' <param name="contentType">Optional MIME type; defaults to <c>application/octet-stream</c>.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Using image = File.OpenRead("cover.jpg")
        '''     Dim tmp = Await ojs.TemporaryFiles.UploadAsync(image, "cover.jpg", "image/jpeg")
        '''     Console.WriteLine(tmp.Id)
        ''' End Using
        ''' </code>
        ''' </example>
        Public Function UploadAsync(content As Stream, fileName As String, Optional contentType As String = Nothing,
                                    Optional cancellationToken As CancellationToken = Nothing) As Task(Of TemporaryFile)
            ArgumentNullException.ThrowIfNull(content)
            ArgumentException.ThrowIfNullOrEmpty(fileName)
            Dim form As New MultipartFormDataContent()
            form.Add(CreateFilePart(content, contentType), "file", fileName)
            Return Http.PostMultipartAsync(Of TemporaryFile)("temporaryFiles", form, cancellationToken)
        End Function

        ''' <summary>Uploads a file from disk and returns its temporary file record.</summary>
        ''' <param name="path">The path of the file to upload.</param>
        ''' <param name="contentType">The MIME type; guessed from the file name when <c>Nothing</c>.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim tmp = Await ojs.TemporaryFiles.UploadAsync("C:\images\cover.jpg")
        ''' Await ojs.Publications.UpdateAsync(12, 15, New PublicationEdit With {
        '''     .CoverImage = New Dictionary(Of String, CoverImage) From {{"en", New CoverImage With {.TemporaryFileId = tmp.Id, .AltText = "Cover"}}}})
        ''' </code>
        ''' </example>
        Public Async Function UploadAsync(path As String, Optional contentType As String = Nothing,
                                          Optional cancellationToken As CancellationToken = Nothing) As Task(Of TemporaryFile)
            Using fs = File.OpenRead(path)
                Return Await UploadAsync(fs, IO.Path.GetFileName(path), contentType, cancellationToken).ConfigureAwait(False)
            End Using
        End Function

        Friend Shared Function CreateFilePart(content As Stream, contentType As String) As StreamContent
            Dim part As New StreamContent(content)
            part.Headers.ContentType = New MediaTypeHeaderValue(If(contentType, "application/octet-stream"))
            Return part
        End Function

    End Class

End Namespace
