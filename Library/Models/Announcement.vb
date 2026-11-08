Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports Library.Serialization

Namespace Models

    ''' <summary>A news item shown on a journal's or the site's announcements page.</summary>
    ''' <example>
    ''' <code>
    ''' Dim page = Await ojs.Announcements.ListAsync()
    ''' For Each a In page.Items
    '''     Console.WriteLine($"{a.DatePosted:d} {a.Title.GetValue("en")}")
    ''' Next
    ''' </code>
    ''' </example>
    Partial Public Class Announcement

        ''' <summary>The journal, press or preprint server ID. Null for site-level announcements.</summary>
        Public Property AssocId As Integer?

        ''' <summary>The assoc object. This should always be <c>ASSOC_TYPE_JOURNAL</c> (OJS), <c>ASSOC_TYPE_PRESS</c> (OMP) or <c>ASSOC_TYPE_SERVER</c> (OPS).</summary>
        Public Property AssocType As Integer?

        ''' <summary>(Optional) The date that this announcement expires, if one is set. This is typically used to express closing dates for calls for papers.</summary>
        Public Property DateExpire As DateOnly?

        ''' <summary>When the announcement was posted. Read-only.</summary>
        Public Property DatePosted As DateTime?

        ''' <summary>Description.</summary>
        Public Property Description As LocalizedString

        ''' <summary>Description short.</summary>
        Public Property DescriptionShort As LocalizedString

        ''' <summary>The URL to this announcement in the REST API. Read-only.</summary>
        <JsonPropertyName("_href")>
        Public Property Href As String

        ''' <summary>Id. Read-only.</summary>
        Public Property Id As Integer?

        ''' <summary>The image to show with this announcement.</summary>
        Public Property Image As ImageFile

        ''' <summary>Title.</summary>
        Public Property Title As LocalizedString

        ''' <summary>(Optional) One of the announcement type IDs.</summary>
        Public Property TypeId As Integer?

        ''' <summary>The URL to the published announcement. Read-only.</summary>
        Public Property Url As String

        ''' <summary>
        ''' Properties returned by the server that this class does not model, such as those added by plugins
        ''' (<c>crossrefplugin::status</c>, <c>dataciteplugin::registeredDoi</c>, …).
        ''' </summary>
        <JsonExtensionData>
        Public Property AdditionalProperties As Dictionary(Of String, JsonElement)

    End Class

    ''' <summary>The writable properties of an announcement. Only properties you set are sent.</summary>
    ''' <example>
    ''' <code>
    ''' Await ojs.Announcements.CreateAsync(New AnnouncementEdit With {
    '''     .Title = LocalizedString.Create("en", "Call for papers"),
    '''     .DescriptionShort = LocalizedString.Create("en", "Submit by June."),
    '''     .DateExpire = New DateOnly(2027, 6, 30)})
    ''' </code>
    ''' </example>
    Partial Public Class AnnouncementEdit

        ''' <summary>(Optional) The date that this announcement expires, if one is set. This is typically used to express closing dates for calls for papers.</summary>
        Public Property DateExpire As DateOnly?

        ''' <summary>Description.</summary>
        Public Property Description As LocalizedString

        ''' <summary>Description short.</summary>
        Public Property DescriptionShort As LocalizedString

        ''' <summary>The image to show with this announcement. To upload one, set <see cref="ImageFile.TemporaryFileId"/>.</summary>
        Public Property Image As ImageFile

        ''' <summary>Title.</summary>
        Public Property Title As LocalizedString

        ''' <summary>(Optional) One of the announcement type IDs.</summary>
        Public Property TypeId As Integer?

    End Class

End Namespace
