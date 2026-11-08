Imports System.Net.Http
Imports System.Text.Json
Imports System.Threading
Imports Library.Http
Imports Library.Resources

''' <summary>akun
''' Entry point for the OJS 3.5 REST API. Each property groups the endpoints of one API area.
''' </summary>
''' <remarks>
''' <para>
''' A client targets one journal (context). For site-wide administration endpoints (listing all journals,
''' editing site settings) create a client with <see cref="OjsClientOptions.JournalPath"/> set to
''' <see cref="OjsClientOptions.SiteWidePath"/> or call <see cref="ForSite"/>.
''' </para>
''' <para>
''' The client is thread-safe. When it creates its own <see cref="HttpClient"/> it owns and disposes it;
''' when you pass one in (e.g. from <c>IHttpClientFactory</c>) you keep ownership.
''' </para>
''' </remarks>
''' <example>
''' <code>
''' Using ojs As New OjsClient(New OjsClientOptions With {
'''     .BaseUrl = "https://journals.example.com",
'''     .JournalPath = "jpk",
'''     .ApiToken = Environment.GetEnvironmentVariable("OJS_TOKEN")})
'''
'''     Dim tmp = Await ojs.TemporaryFiles.UploadAsync("C:\cover.jpg")
'''     Console.WriteLine(tmp.Id)
''' End Using
''' </code>
''' </example>
Public Class OjsClient
    Implements IDisposable

    Private ReadOnly _http As OjsHttp
    Private ReadOnly _ownsClient As Boolean
    Private _disposed As Boolean

    ''' <summary>Creates a client that owns its own <see cref="HttpClient"/>.</summary>
    ''' <param name="options">Connection and authentication settings.</param>
    Public Sub New(options As OjsClientOptions)
        Me.New(CreateHttpClient(options), options, ownsClient:=True)
    End Sub

    ''' <summary>Creates a client that uses an existing <see cref="HttpClient"/> (not disposed by this client).</summary>
    ''' <param name="httpClient">The HTTP client to send requests with.</param>
    ''' <param name="options">Connection and authentication settings.</param>
    Public Sub New(httpClient As HttpClient, options As OjsClientOptions)
        Me.New(httpClient, options, ownsClient:=False)
    End Sub

    Private Sub New(httpClient As HttpClient, options As OjsClientOptions, ownsClient As Boolean)
        ArgumentNullException.ThrowIfNull(httpClient)
        ArgumentNullException.ThrowIfNull(options)
        Me.Options = options.Clone()
        _http = New OjsHttp(httpClient, Me.Options)
        _ownsClient = ownsClient
        InitializeResources()
    End Sub

    Private Shared Function CreateHttpClient(options As OjsClientOptions) As HttpClient
        ArgumentNullException.ThrowIfNull(options)
        Dim client As New HttpClient()
        If options.Timeout.HasValue Then client.Timeout = options.Timeout.Value
        Return client
    End Function

    ''' <summary>A copy of the options this client was created with.</summary>
    Public ReadOnly Property Options As OjsClientOptions

    ''' <summary>The API root all requests are sent to, e.g. <c>https://example.com/jpk/api/v1/</c>.</summary>
    Public ReadOnly Property ApiRoot As Uri
        Get
            Return _http.ApiRoot
        End Get
    End Property

    Friend ReadOnly Property Http As OjsHttp
        Get
            Return _http
        End Get
    End Property

    ''' <summary>Returns a client for another journal that shares this client's <see cref="HttpClient"/>.</summary>
    ''' <param name="journalPath">The journal URL path, or <see cref="OjsClientOptions.SiteWidePath"/>.</param>
    ''' <remarks>The returned client does not own the HTTP client; keep this client alive while using it.</remarks>
    Public Function ForJournal(journalPath As String) As OjsClient
        Dim o = Options.Clone()
        o.JournalPath = journalPath
        Return New OjsClient(_http.Client, o, ownsClient:=False)
    End Function

    ''' <summary>Returns a client for the site-wide endpoints (<c>/_/api/v1/...</c>) sharing this client's <see cref="HttpClient"/>.</summary>
    Public Function ForSite() As OjsClient
        Return ForJournal(OjsClientOptions.SiteWidePath)
    End Function

#Region "Resources"

    ''' <summary><c>/temporaryFiles</c> — upload files for use in other requests.</summary>
    Public ReadOnly Property TemporaryFiles As TemporaryFilesResource

    ''' <summary><c>/submissions</c> — submissions and their participants.</summary>
    Public ReadOnly Property Submissions As SubmissionsResource

    ''' <summary><c>/submissions/{id}/publications</c> — publication versions, publishing and JATS XML.</summary>
    Public ReadOnly Property Publications As PublicationsResource

    ''' <summary><c>/submissions/{id}/publications/{id}/contributors</c> — authors and other contributors.</summary>
    Public ReadOnly Property Contributors As ContributorsResource

    ''' <summary><c>/submissions/{id}/files</c> — submission files.</summary>
    Public ReadOnly Property SubmissionFiles As SubmissionFilesResource

    ''' <summary><c>/submissions/{id}/decisions</c> — editorial decisions.</summary>
    Public ReadOnly Property Decisions As DecisionsResource

    ''' <summary><c>/submissions/{id}/reviewers/suggestions</c> — reviewers suggested by authors.</summary>
    Public ReadOnly Property ReviewerSuggestions As ReviewerSuggestionsResource

    ''' <summary><c>/issues</c> — journal issues.</summary>
    Public ReadOnly Property Issues As IssuesResource

    ''' <summary><c>/sections</c> — journal sections.</summary>
    Public ReadOnly Property Sections As SectionsResource

    ''' <summary><c>/users</c> — user accounts, reviewers and role changes.</summary>
    Public ReadOnly Property Users As UsersResource

    ''' <summary><c>/contexts</c> — the journals hosted on the installation (always site-wide).</summary>
    Public ReadOnly Property Contexts As ContextsResource

    ''' <summary><c>/site</c> — site-wide settings and theme (always site-wide).</summary>
    Public ReadOnly Property Site As SiteResource

    ''' <summary><c>/announcements</c> — news items.</summary>
    Public ReadOnly Property Announcements As AnnouncementsResource

    ''' <summary><c>/institutions</c> — institutions for usage statistics and subscriptions.</summary>
    Public ReadOnly Property Institutions As InstitutionsResource

    ''' <summary><c>/emailTemplates</c> — email templates.</summary>
    Public ReadOnly Property EmailTemplates As EmailTemplatesResource

    ''' <summary><c>/mailables</c> — the kinds of email OJS sends.</summary>
    Public ReadOnly Property Mailables As MailablesResource

    ''' <summary><c>/emails</c> — the log of sent emails.</summary>
    Public ReadOnly Property Emails As EmailsResource

    ''' <summary><c>/vocabs</c> — keyword, subject and other vocabulary suggestions.</summary>
    Public ReadOnly Property Vocabs As VocabsResource

    ''' <summary><c>/rors</c> — Research Organization Registry records.</summary>
    Public ReadOnly Property Rors As RorsResource

    ''' <summary><c>/dois</c> — DOIs, bulk DOI actions on articles and issues, and deposit exports.</summary>
    Public ReadOnly Property Dois As DoisResource

    ''' <summary><c>/stats</c> — editorial, user and usage statistics (JSON or CSV).</summary>
    Public ReadOnly Property Stats As StatsResource

    ''' <summary><c>/reviews</c> — review confirmation, history, PDF/XML exports and ORCID deposit.</summary>
    Public ReadOnly Property Reviews As ReviewsResource

    ''' <summary><c>/invitations</c> — invitations to roles.</summary>
    Public ReadOnly Property Invitations As InvitationsResource

    ''' <summary><c>/orcid</c> — ORCID actions for authors.</summary>
    Public ReadOnly Property Orcid As OrcidResource

    Private Sub InitializeResources()
        _Dois = New DoisResource(_http)
        _Stats = New StatsResource(_http)
        _Reviews = New ReviewsResource(_http)
        _Invitations = New InvitationsResource(_http)
        _Orcid = New OrcidResource(_http)
        _Users = New UsersResource(_http)
        _Contexts = New ContextsResource(_http)
        _Site = New SiteResource(_http)
        _Announcements = New AnnouncementsResource(_http)
        _Institutions = New InstitutionsResource(_http)
        _EmailTemplates = New EmailTemplatesResource(_http)
        _Mailables = New MailablesResource(_http)
        _Emails = New EmailsResource(_http)
        _Vocabs = New VocabsResource(_http)
        _Rors = New RorsResource(_http)
        _TemporaryFiles = New TemporaryFilesResource(_http)
        _Submissions = New SubmissionsResource(_http)
        _Publications = New PublicationsResource(_http)
        _Contributors = New ContributorsResource(_http)
        _SubmissionFiles = New SubmissionFilesResource(_http)
        _Decisions = New DecisionsResource(_http)
        _ReviewerSuggestions = New ReviewerSuggestionsResource(_http)
        _Issues = New IssuesResource(_http)
        _Sections = New SectionsResource(_http)
    End Sub

#End Region

#Region "Raw access"

    ''' <summary>
    ''' Sends a request to any endpoint (relative to <see cref="ApiRoot"/>) and returns the JSON response.
    ''' Use it for endpoints added by plugins or not yet covered by this library.
    ''' </summary>
    ''' <param name="method">The HTTP method.</param>
    ''' <param name="path">A path relative to the API root, e.g. <c>"submissions/12"</c>; may include a query string.</param>
    ''' <param name="body">Optional body: a model object, a JSON string, a <c>JsonNode</c> or a <c>JsonElement</c>.</param>
    ''' <param name="cancellationToken">Cancels the request.</param>
    ''' <example>
    ''' <code>
    ''' Dim json = Await ojs.SendAsync(HttpMethod.Get, "submissions?count=1")
    ''' Console.WriteLine(json.GetProperty("itemsMax").GetInt32())
    ''' </code>
    ''' </example>
    Public Function SendAsync(method As HttpMethod, path As String, Optional body As Object = Nothing,
                              Optional cancellationToken As CancellationToken = Nothing) As Task(Of JsonElement)
        Return _http.SendJsonAsync(Of JsonElement)(method, path, Nothing, body, cancellationToken)
    End Function

    ''' <summary>Like <see cref="SendAsync(HttpMethod, String, Object, CancellationToken)"/> but binds the response to <typeparamref name="T"/>.</summary>
    ''' <typeparam name="T">The type to bind the response to.</typeparam>
    ''' <param name="method">The HTTP method.</param>
    ''' <param name="path">A path relative to the API root, e.g. <c>submissions/12</c>; may include a query string.</param>
    ''' <param name="body">Optional body: a model object, a JSON string, a <c>JsonNode</c> or a <c>JsonElement</c>.</param>
    ''' <param name="cancellationToken">Cancels the request.</param>
    Public Function SendAsync(Of T)(method As HttpMethod, path As String, Optional body As Object = Nothing,
                                    Optional cancellationToken As CancellationToken = Nothing) As Task(Of T)
        Return _http.SendJsonAsync(Of T)(method, path, Nothing, body, cancellationToken)
    End Function

#End Region

    ''' <summary>Disposes the underlying <see cref="HttpClient"/> if this client created it.</summary>
    Public Sub Dispose() Implements IDisposable.Dispose
        If _disposed Then Return
        _disposed = True
        If _ownsClient Then _http.Client.Dispose()
        GC.SuppressFinalize(Me)
    End Sub

End Class
