Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports OJS3_5.Library.Serialization

Namespace Models

    ''' <summary>An institution (library, university, ...) used for institutional usage statistics and subscriptions.</summary>
    Partial Public Class Institution

        ''' <summary>The context which added this institution. Read-only.</summary>
        Public Property ContextId As Integer?

        ''' <summary>The date this institution was deleted, if it has been deleted. Institutions are kept after being deleted because they may be referenced in statistics data. Read-only.</summary>
        Public Property DeletedAt As DateTime?

        ''' <summary>The URL to this institution in the REST API. Read-only.</summary>
        <JsonPropertyName("_href")>
        Public Property Href As String

        ''' <summary>The unique ID of the institution in the database. Read-only.</summary>
        Public Property Id As Integer?

        ''' <summary>A list of IP ranges belonging to this institution. Used to track usage statistics and subscription access by institution.</summary>
        Public Property IpRanges As List(Of String)

        ''' <summary>Name.</summary>
        Public Property Name As LocalizedString

        ''' <summary>The ROR ID of this institution.</summary>
        Public Property Ror As String

        ''' <summary>
        ''' Properties returned by the server that this class does not model, such as those added by plugins
        ''' (<c>crossrefplugin::status</c>, <c>dataciteplugin::registeredDoi</c>, …).
        ''' </summary>
        <JsonExtensionData>
        Public Property AdditionalProperties As Dictionary(Of String, JsonElement)

    End Class

    ''' <summary>The writable properties of an institution. Only properties you set are sent.</summary>
    ''' <example>
    ''' <code>
    ''' Await ojs.Institutions.CreateAsync(New InstitutionEdit With {
    '''     .Name = LocalizedString.Create("en", "Example University"),
    '''     .Ror = "https://ror.org/05ezgz346",
    '''     .IpRanges = New List(Of String) From {"192.0.2.0/24"}})
    ''' </code>
    ''' </example>
    Partial Public Class InstitutionEdit

        ''' <summary>
        ''' The IP ranges belonging to this institution, e.g. <c>192.0.2.0/24</c>, <c>198.51.100.1-198.51.100.9</c>
        ''' or <c>203.0.113.*</c>. Used to track usage statistics and subscription access by institution.
        ''' </summary>
        ''' <remarks>OJS expects one range per line when writing; this library joins the list for you.</remarks>
        <JsonIgnore>
        Public Property IpRanges As List(Of String)

        ''' <summary>The value sent as <c>ipRanges</c>: OJS 3.5 reads it as newline-separated text, not as an array.</summary>
        <JsonInclude>
        <JsonPropertyName("ipRanges")>
        Private ReadOnly Property IpRangesText As String
            Get
                Return If(IpRanges Is Nothing, Nothing, String.Join(vbLf, IpRanges))
            End Get
        End Property

        ''' <summary>The institution's name.</summary>
        Public Property Name As LocalizedString

        ''' <summary>The ROR ID of this institution.</summary>
        Public Property Ror As String

    End Class

End Namespace
