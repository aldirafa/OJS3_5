Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports OJS3_5.Library.Serialization

Namespace Models

    ''' <summary>A journal issue.</summary>
    ''' <example>
    ''' <code>
    ''' Dim current = Await ojs.Issues.GetCurrentAsync()
    ''' Console.WriteLine($"{current.Identification}: {current.Articles.Count} articles")
    ''' </code>
    ''' </example>
    Partial Public Class Issue

        ''' <summary>Access status.</summary>
        Public Property AccessStatus As Integer?

        ''' <summary>Articles. Read-only.</summary>
        Public Property Articles As List(Of Submission)

        ''' <summary>Cover image.</summary>
        Public Property CoverImage As LocalizedString

        ''' <summary>Cover image alt text.</summary>
        Public Property CoverImageAltText As LocalizedString

        ''' <summary>Cover image URL.</summary>
        Public Property CoverImageUrl As LocalizedString

        ''' <summary>Date notified.</summary>
        Public Property DateNotified As DateTime?

        ''' <summary>Date published.</summary>
        Public Property DatePublished As DateTime?

        ''' <summary>Description.</summary>
        Public Property Description As LocalizedString

        ''' <summary>DOI object.</summary>
        Public Property DoiObject As Doi

        ''' <summary>Galleys. Read-only.</summary>
        Public Property Galleys As List(Of Galley)

        ''' <summary>Href. Read-only.</summary>
        <JsonPropertyName("_href")>
        Public Property Href As String

        ''' <summary>Id.</summary>
        Public Property Id As Integer?

        ''' <summary>Identification. Read-only.</summary>
        Public Property Identification As String

        ''' <summary>Journal ID.</summary>
        Public Property JournalId As Integer?

        ''' <summary>Last modified.</summary>
        Public Property LastModified As DateTime?

        ''' <summary>Number.</summary>
        Public Property Number As String

        ''' <summary>Open access date.</summary>
        Public Property OpenAccessDate As DateTime?

        ''' <summary>Original style file name.</summary>
        Public Property OriginalStyleFileName As String

        ''' <summary>Published.</summary>
        Public Property Published As Boolean?

        ''' <summary>Published URL.</summary>
        Public Property PublishedUrl As String

        ''' <summary>Sections. Read-only.</summary>
        Public Property Sections As List(Of Section)

        ''' <summary>Show number.</summary>
        Public Property ShowNumber As Boolean?

        ''' <summary>Show title.</summary>
        Public Property ShowTitle As Boolean?

        ''' <summary>Show volume.</summary>
        Public Property ShowVolume As Boolean?

        ''' <summary>Show year.</summary>
        Public Property ShowYear As Boolean?

        ''' <summary>Style file name.</summary>
        Public Property StyleFileName As String

        ''' <summary>Title.</summary>
        Public Property Title As LocalizedString

        ''' <summary>An optional path to use in the URL instead of the ID.</summary>
        Public Property UrlPath As String

        ''' <summary>Volume.</summary>
        Public Property Volume As Integer?

        ''' <summary>Year.</summary>
        Public Property Year As Integer?

        ''' <summary>
        ''' Properties returned by the server that this class does not model, such as those added by plugins
        ''' (<c>crossrefplugin::status</c>, <c>dataciteplugin::registeredDoi</c>, …).
        ''' </summary>
        <JsonExtensionData>
        Public Property AdditionalProperties As Dictionary(Of String, JsonElement)

    End Class

End Namespace
