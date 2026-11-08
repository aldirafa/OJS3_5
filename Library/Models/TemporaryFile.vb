Namespace Models

    ''' <summary>
    ''' A file uploaded to the temporary file storage area. Pass its <see cref="Id"/> as <c>temporaryFileId</c>
    ''' in later requests (for example a context logo, a submission file or a user avatar).
    ''' </summary>
    ''' <example>
    ''' <code>
    ''' Dim tmp = Await ojs.TemporaryFiles.UploadAsync("C:\files\logo.png")
    ''' Console.WriteLine($"{tmp.Id} {tmp.Name} {tmp.Mimetype}")
    ''' </code>
    ''' </example>
    Public Class TemporaryFile

        ''' <summary>The temporary file ID.</summary>
        Public Property Id As Integer

        ''' <summary>The original file name.</summary>
        Public Property Name As String

        ''' <summary>The detected MIME type.</summary>
        Public Property Mimetype As String

        ''' <summary>The detected document type, e.g. <c>image</c>, <c>pdf</c>, <c>word</c>.</summary>
        Public Property DocumentType As String

    End Class

    ''' <summary>
    ''' A reference to an uploaded temporary file, used by image/file settings such as
    ''' <c>pageHeaderLogoImage</c>: <c>{"temporaryFileId": 123, "altText": "Logo"}</c>.
    ''' </summary>
    ''' <example>
    ''' <code>
    ''' Dim tmp = Await ojs.TemporaryFiles.UploadAsync("figure.png")
    ''' Dim ref As New TemporaryFileReference With {.TemporaryFileId = tmp.Id, .AltText = "Figure 1"}
    ''' </code>
    ''' </example>
    Public Class TemporaryFileReference

        ''' <summary>The ID returned by <c>POST /temporaryFiles</c>.</summary>
        Public Property TemporaryFileId As Integer?

        ''' <summary>Alternative text for images.</summary>
        Public Property AltText As String

    End Class

End Namespace
