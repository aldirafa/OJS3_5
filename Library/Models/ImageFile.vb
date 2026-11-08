Imports System.Text.Json
Imports System.Text.Json.Serialization

Namespace Models

    ''' <summary>
    ''' An uploaded image or file setting, such as a journal logo, a favicon, an announcement image or a stylesheet.
    ''' </summary>
    ''' <remarks>
    ''' When reading, OJS fills <see cref="Name"/>, <see cref="UploadName"/> and <see cref="DateUploaded"/>.
    ''' To upload a new file, first upload it with <c>ojs.TemporaryFiles.UploadAsync</c>, then send an
    ''' <see cref="ImageFile"/> with <see cref="TemporaryFileId"/> (and <see cref="AltText"/> for images).
    ''' </remarks>
    ''' <example>
    ''' <code>
    ''' Dim tmp = Await ojs.TemporaryFiles.UploadAsync("C:\logo.png")
    ''' Await ojs.Contexts.UpdateAsync(journalId, New ContextEdit With {
    '''     .PageHeaderLogoImage = New Dictionary(Of String, ImageFile) From {
    '''         {"en", New ImageFile With {.TemporaryFileId = tmp.Id, .AltText = "Journal logo"}}}})
    ''' </code>
    ''' </example>
    Partial Public Class ImageFile

        ''' <summary>The original file name.</summary>
        Public Property Name As String

        ''' <summary>The stored file name, relative to the public files directory.</summary>
        Public Property UploadName As String

        ''' <summary>The image width in pixels, when known.</summary>
        Public Property Width As Integer?

        ''' <summary>The image height in pixels, when known.</summary>
        Public Property Height As Integer?

        ''' <summary>When the file was uploaded.</summary>
        Public Property DateUploaded As String

        ''' <summary>Alternative text for the image.</summary>
        Public Property AltText As String

        ''' <summary>When uploading a new file: the ID returned by the temporary files endpoint.</summary>
        Public Property TemporaryFileId As Integer?

        ''' <summary>Other properties returned by the server.</summary>
        <JsonExtensionData>
        Public Property AdditionalProperties As Dictionary(Of String, JsonElement)

    End Class

End Namespace
