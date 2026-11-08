''' <summary>
''' Configuration for an <see cref="OjsClient"/>: where the OJS installation lives, which journal to talk to,
''' and how to authenticate.
''' </summary>
''' <example>
''' <code>
''' Dim options As New OjsClientOptions With {
'''     .BaseUrl = "https://journals.example.com",
'''     .JournalPath = "jpk",
'''     .ApiToken = Environment.GetEnvironmentVariable("OJS_TOKEN")
''' }
''' ' Requests go to https://journals.example.com/jpk/api/v1/...
''' </code>
''' </example>
Public Class OjsClientOptions

    ''' <summary>
    ''' The special journal path used for site-wide endpoints (for example listing all contexts or editing
    ''' site settings): <c>https://example.com/_/api/v1/contexts</c>.
    ''' </summary>
    Public Const SiteWidePath As String = "_"

    ''' <summary>
    ''' The root URL of the OJS installation, without the journal path or <c>/api/v1</c>,
    ''' for example <c>https://journals.example.com</c> or <c>https://example.com/ojs</c>.
    ''' </summary>
    Public Property BaseUrl As String

    ''' <summary>
    ''' The URL path of the journal (context) to talk to, for example <c>jpk</c>.
    ''' Use <see cref="SiteWidePath"/> (<c>"_"</c>) for site-wide administration endpoints.
    ''' Leave empty when the installation exposes the API directly at <c>{BaseUrl}/api/v1</c>.
    ''' </summary>
    Public Property JournalPath As String

    ''' <summary>
    ''' Set to <c>True</c> when the installation does not use <c>mod_rewrite</c> and every URL must include
    ''' <c>index.php</c>, for example <c>https://example.com/index.php/jpk/api/v1/submissions</c>.
    ''' </summary>
    Public Property UseIndexPhp As Boolean

    ''' <summary>
    ''' The API token found in OJS under <b>User Profile &gt; API Key</b>. It is sent as
    ''' <c>Authorization: Bearer {token}</c> with every request. The installation must have
    ''' <c>api_key_secret</c> set in the <c>[security]</c> section of <c>config.inc.php</c> for tokens to validate.
    ''' </summary>
    Public Property ApiToken As String

    ''' <summary>
    ''' Optional request timeout. Only applied when the client creates its own <see cref="Net.Http.HttpClient"/>.
    ''' </summary>
    Public Property Timeout As TimeSpan?

    ''' <summary>
    ''' The <c>User-Agent</c> header sent with each request. Defaults to <c>Ojs.Api.Client/{version}</c>.
    ''' </summary>
    Public Property UserAgent As String

    ''' <summary>Creates a shallow copy of these options.</summary>
    Public Function Clone() As OjsClientOptions
        Return DirectCast(MemberwiseClone(), OjsClientOptions)
    End Function

    ''' <summary>
    ''' Builds the absolute API root, for example <c>https://example.com/index.php/jpk/api/v1/</c>.
    ''' </summary>
    ''' <exception cref="InvalidOperationException">When <see cref="BaseUrl"/> is missing or not an absolute http(s) URL.</exception>
    Public Function GetApiRoot() As Uri
        If String.IsNullOrWhiteSpace(BaseUrl) Then
            Throw New InvalidOperationException("OjsClientOptions.BaseUrl is required.")
        End If

        Dim root As Uri = Nothing
        If Not Uri.TryCreate(BaseUrl.Trim(), UriKind.Absolute, root) OrElse
           (root.Scheme <> Uri.UriSchemeHttp AndAlso root.Scheme <> Uri.UriSchemeHttps) Then
            Throw New InvalidOperationException($"OjsClientOptions.BaseUrl '{BaseUrl}' is not an absolute http(s) URL.")
        End If

        Dim sb As New Text.StringBuilder(root.GetLeftPart(UriPartial.Path).TrimEnd("/"c))
        sb.Append("/"c)
        If UseIndexPhp Then sb.Append("index.php/")
        Dim journal = If(JournalPath, String.Empty).Trim("/"c, " "c)
        If journal.Length > 0 Then sb.Append(Uri.EscapeDataString(journal)).Append("/"c)
        sb.Append("api/v1/")
        Return New Uri(sb.ToString(), UriKind.Absolute)
    End Function

End Class
