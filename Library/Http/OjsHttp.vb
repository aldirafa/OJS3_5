Imports System.IO
Imports System.Net
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Text
Imports System.Text.Json
Imports System.Text.Json.Nodes
Imports System.Threading
Imports OJS3_5.Library.Errors
Imports OJS3_5.Library.Serialization

Namespace Http

    ''' <summary>
    ''' Low-level transport shared by all resources: builds URLs, adds authentication, serializes bodies and
    ''' maps error responses to <see cref="OjsApiException"/>/<see cref="OjsValidationException"/>.
    ''' </summary>
    Friend NotInheritable Class OjsHttp

        Private Shared ReadOnly DefaultUserAgent As String =
            "Ojs.Api.Client/" & If(GetType(OjsHttp).Assembly.GetName().Version?.ToString(), "0")

        ''' <summary>The HTTP client requests are sent with.</summary>
        Public ReadOnly Property Client As HttpClient
        ''' <summary>The connection settings.</summary>
        Public ReadOnly Property Options As OjsClientOptions
        ''' <summary>The API root URL, ending with <c>api/v1/</c>.</summary>
        Public ReadOnly Property ApiRoot As Uri

        Public Sub New(client As HttpClient, options As OjsClientOptions)
            Me.Client = client
            Me.Options = options
            ApiRoot = options.GetApiRoot()
        End Sub

        Private _site As OjsHttp

        ''' <summary>A transport for the site-wide API (<c>/_/api/v1/</c>) sharing this HTTP client.</summary>
        Public Function ForSite() As OjsHttp
            If Options.JournalPath = OjsClientOptions.SiteWidePath Then Return Me
            If _site Is Nothing Then
                Dim o = Options.Clone()
                o.JournalPath = OjsClientOptions.SiteWidePath
                _site = New OjsHttp(Client, o)
            End If
            Return _site
        End Function

        ''' <summary>A transport for another journal sharing this HTTP client.</summary>
        ''' <param name="journalPath">The journal URL path, or <c>_</c> for the site-wide API.</param>
        Public Function ForJournal(journalPath As String) As OjsHttp
            If String.Equals(Options.JournalPath, journalPath, StringComparison.Ordinal) Then Return Me
            Dim o = Options.Clone()
            o.JournalPath = journalPath
            Return New OjsHttp(Client, o)
        End Function

        ''' <summary>Builds the absolute URL of an API path.</summary>
        ''' <param name="path">A path relative to the API root, e.g. <c>submissions/12</c>.</param>
        ''' <param name="query">The query string parameters, or <c>Nothing</c>.</param>
        Public Function BuildUri(path As String, Optional query As QueryBuilder = Nothing) As Uri
            Dim relative = If(path, String.Empty).TrimStart("/"c)
            Return New Uri(ApiRoot, relative & If(query?.ToString(), String.Empty))
        End Function

        ''' <summary>URL-escapes a single path segment.</summary>
        ''' <param name="value">The path segment to escape.</param>
        Public Shared Function Seg(value As String) As String
            Return Uri.EscapeDataString(value)
        End Function

        ''' <summary>Formats a numeric path segment.</summary>
        ''' <param name="value">The path segment to escape.</param>
        Public Shared Function Seg(value As Integer) As String
            Return value.ToString(Globalization.CultureInfo.InvariantCulture)
        End Function

#Region "JSON"

        ''' <summary>Sends a GET request and binds the JSON response.</summary>
        ''' <typeparam name="T">The type to bind the response to.</typeparam>
        ''' <param name="path">A path relative to the API root, e.g. <c>submissions/12</c>.</param>
        ''' <param name="query">The query string parameters, or <c>Nothing</c>.</param>
        ''' <param name="ct">Cancels the request.</param>
        Public Function GetAsync(Of T)(path As String, Optional query As QueryBuilder = Nothing,
                                       Optional ct As CancellationToken = Nothing) As Task(Of T)
            Return SendJsonAsync(Of T)(HttpMethod.Get, path, query, Nothing, ct)
        End Function

        ''' <summary>Sends a POST request with a JSON body and binds the JSON response.</summary>
        ''' <typeparam name="T">The type to bind the response to.</typeparam>
        ''' <param name="path">A path relative to the API root, e.g. <c>submissions/12</c>.</param>
        ''' <param name="body">The request body: a model, a JSON string, a <c>JsonNode</c> or a <c>JsonElement</c>; <c>Nothing</c> for none.</param>
        ''' <param name="ct">Cancels the request.</param>
        Public Function PostAsync(Of T)(path As String, body As Object, Optional ct As CancellationToken = Nothing) As Task(Of T)
            Return SendJsonAsync(Of T)(HttpMethod.Post, path, Nothing, body, ct)
        End Function

        ''' <summary>Sends a PUT request with a JSON body and binds the JSON response.</summary>
        ''' <typeparam name="T">The type to bind the response to.</typeparam>
        ''' <param name="path">A path relative to the API root, e.g. <c>submissions/12</c>.</param>
        ''' <param name="body">The request body: a model, a JSON string, a <c>JsonNode</c> or a <c>JsonElement</c>; <c>Nothing</c> for none.</param>
        ''' <param name="ct">Cancels the request.</param>
        Public Function PutAsync(Of T)(path As String, body As Object, Optional ct As CancellationToken = Nothing) As Task(Of T)
            Return SendJsonAsync(Of T)(HttpMethod.Put, path, Nothing, body, ct)
        End Function

        ''' <summary>Sends a DELETE request (optionally with a JSON body) and binds the JSON response.</summary>
        ''' <typeparam name="T">The type to bind the response to.</typeparam>
        ''' <param name="path">A path relative to the API root, e.g. <c>submissions/12</c>.</param>
        ''' <param name="query">The query string parameters, or <c>Nothing</c>.</param>
        ''' <param name="body">The request body: a model, a JSON string, a <c>JsonNode</c> or a <c>JsonElement</c>; <c>Nothing</c> for none.</param>
        ''' <param name="ct">Cancels the request.</param>
        Public Function DeleteAsync(Of T)(path As String, Optional query As QueryBuilder = Nothing,
                                          Optional body As Object = Nothing, Optional ct As CancellationToken = Nothing) As Task(Of T)
            Return SendJsonAsync(Of T)(HttpMethod.Delete, path, query, body, ct)
        End Function

        ''' <summary>Sends a request with an optional JSON body and binds the JSON response.</summary>
        ''' <typeparam name="T">The type to bind the response to.</typeparam>
        ''' <param name="method">The HTTP method.</param>
        ''' <param name="path">A path relative to the API root, e.g. <c>submissions/12</c>.</param>
        ''' <param name="query">The query string parameters, or <c>Nothing</c>.</param>
        ''' <param name="body">The request body: a model, a JSON string, a <c>JsonNode</c> or a <c>JsonElement</c>; <c>Nothing</c> for none.</param>
        ''' <param name="ct">Cancels the request.</param>
        Public Async Function SendJsonAsync(Of T)(method As HttpMethod, path As String, query As QueryBuilder,
                                                  body As Object, ct As CancellationToken) As Task(Of T)
            Dim content As HttpContent = Nothing
            If body IsNot Nothing Then content = CreateJsonContent(body)
            Using response = Await SendAsync(method, path, query, content, "application/json", ct).ConfigureAwait(False)
                Dim text = Await response.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                Return OjsJson.Deserialize(Of T)(text)
            End Using
        End Function

        ''' <summary>Sends a multipart/form-data POST request and binds the JSON response.</summary>
        ''' <typeparam name="T">The type to bind the response to.</typeparam>
        ''' <param name="path">A path relative to the API root, e.g. <c>submissions/12</c>.</param>
        ''' <param name="content">The request content.</param>
        ''' <param name="ct">Cancels the request.</param>
        Public Async Function PostMultipartAsync(Of T)(path As String, content As MultipartFormDataContent,
                                                       ct As CancellationToken) As Task(Of T)
            Using response = Await SendAsync(HttpMethod.Post, path, Nothing, content, "application/json", ct).ConfigureAwait(False)
                Dim text = Await response.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                Return OjsJson.Deserialize(Of T)(text)
            End Using
        End Function

        ''' <summary>Serializes a request body to JSON content.</summary>
        ''' <param name="body">The request body: a model, a JSON string, a <c>JsonNode</c> or a <c>JsonElement</c>; <c>Nothing</c> for none.</param>
        Public Shared Function CreateJsonContent(body As Object) As HttpContent
            Dim json As String
            If TypeOf body Is String Then
                json = DirectCast(body, String)
            ElseIf TypeOf body Is JsonNode Then
                json = DirectCast(body, JsonNode).ToJsonString()
            ElseIf TypeOf body Is JsonElement Then
                json = DirectCast(body, JsonElement).GetRawText()
            Else
                json = OjsJson.Serialize(body)
            End If
            Return New StringContent(json, Encoding.UTF8, "application/json")
        End Function

#End Region

#Region "Text / files"

        ''' <summary>Sends a GET request and returns the body as text (e.g. CSV).</summary>
        ''' <param name="path">A path relative to the API root, e.g. <c>submissions/12</c>.</param>
        ''' <param name="query">The query string parameters, or <c>Nothing</c>.</param>
        ''' <param name="accept">The <c>Accept</c> header value.</param>
        ''' <param name="ct">Cancels the request.</param>
        Public Async Function GetStringAsync(path As String, query As QueryBuilder, accept As String,
                                             ct As CancellationToken) As Task(Of String)
            Using response = Await SendAsync(HttpMethod.Get, path, query, Nothing, accept, ct).ConfigureAwait(False)
                Return Await response.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
            End Using
        End Function

        ''' <summary>Sends a request and returns the response body as a stream.</summary>
        ''' <param name="method">The HTTP method.</param>
        ''' <param name="path">A path relative to the API root, e.g. <c>submissions/12</c>.</param>
        ''' <param name="query">The query string parameters, or <c>Nothing</c>.</param>
        ''' <param name="body">The request body: a model, a JSON string, a <c>JsonNode</c> or a <c>JsonElement</c>; <c>Nothing</c> for none.</param>
        ''' <param name="ct">Cancels the request.</param>
        Public Async Function DownloadAsync(method As HttpMethod, path As String, query As QueryBuilder, body As Object,
                                            ct As CancellationToken) As Task(Of DownloadedFile)
            Dim content As HttpContent = Nothing
            If body IsNot Nothing Then content = CreateJsonContent(body)
            Dim response = Await SendAsync(method, path, query, content, "*/*", ct).ConfigureAwait(False)
            Try
                Dim stream = Await response.Content.ReadAsStreamAsync(ct).ConfigureAwait(False)
                Return New DownloadedFile(response, stream)
            Catch
                response.Dispose()
                Throw
            End Try
        End Function

#End Region

#Region "Core"

        ''' <summary>Sends a request and returns the (successful) response. The caller disposes it.</summary>
        ''' <param name="method">The HTTP method.</param>
        ''' <param name="path">A path relative to the API root, e.g. <c>submissions/12</c>.</param>
        ''' <param name="query">The query string parameters, or <c>Nothing</c>.</param>
        ''' <param name="content">The request content.</param>
        ''' <param name="accept">The <c>Accept</c> header value.</param>
        ''' <param name="ct">Cancels the request.</param>
        Public Async Function SendAsync(method As HttpMethod, path As String, query As QueryBuilder, content As HttpContent,
                                        accept As String, ct As CancellationToken) As Task(Of HttpResponseMessage)
            Dim uri = BuildUri(path, query)
            Using request As New HttpRequestMessage(method, uri)
                request.Content = content
                If Not String.IsNullOrEmpty(Options.ApiToken) Then
                    request.Headers.Authorization = New AuthenticationHeaderValue("Bearer", Options.ApiToken)
                End If
                If Not String.IsNullOrEmpty(accept) Then request.Headers.Accept.ParseAdd(accept)
                request.Headers.TryAddWithoutValidation("User-Agent", If(Options.UserAgent, DefaultUserAgent))

                Dim response = Await Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(False)
                If response.IsSuccessStatusCode Then Return response

                Using response
                    Dim body = String.Empty
                    Try
                        body = Await response.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                    Catch ex As HttpRequestException
                    End Try
                    Throw CreateException(response.StatusCode, body, method.Method, uri)
                End Using
            End Using
        End Function

        ''' <summary>Turns an error response into the most specific exception.</summary>
        ''' <param name="status">The response status code.</param>
        ''' <param name="body">The request body: a model, a JSON string, a <c>JsonNode</c> or a <c>JsonElement</c>; <c>Nothing</c> for none.</param>
        ''' <param name="method">The HTTP method.</param>
        ''' <param name="uri">The request URI.</param>
        Public Shared Function CreateException(status As HttpStatusCode, body As String, method As String, uri As Uri) As OjsApiException
            Dim root As JsonNode = Nothing
            Try
                If Not String.IsNullOrWhiteSpace(body) Then root = JsonNode.Parse(body)
            Catch ex As JsonException
                root = Nothing
            End Try

            Dim obj = TryCast(root, JsonObject)
            If obj IsNot Nothing Then
                If obj.ContainsKey("error") OrElse obj.ContainsKey("errorMessage") Then
                    Return New OjsApiException(status, AsText(obj("error")), AsText(obj("errorMessage")), body, method, uri)
                End If

                ' Laravel-style handlers (e.g. invitations) answer 422 with {"errors": {"field": ["msg"]}}.
                If (status = HttpStatusCode.BadRequest OrElse status = HttpStatusCode.UnprocessableEntity) AndAlso
                   obj.Count = 1 AndAlso TypeOf obj("errors") Is JsonObject Then
                    obj = obj("errors").AsObject()
                End If

                If (status = HttpStatusCode.BadRequest OrElse status = HttpStatusCode.UnprocessableEntity) AndAlso obj.Count > 0 Then
                    Dim errors = ReadValidationErrors(obj)
                    If errors IsNot Nothing Then Return New OjsValidationException(errors, body, method, uri)
                End If
            End If

            Return New OjsApiException(status, Nothing, Nothing, body, method, uri)
        End Function

        ''' <summary>
        ''' Parses <c>{"field": ["msg", ...]}</c>, including nested multilingual shapes like
        ''' <c>{"title": {"en": ["msg"]}}</c> (flattened to <c>title.en</c>).
        ''' </summary>
        Private Shared Function ReadValidationErrors(obj As JsonObject) As IReadOnlyDictionary(Of String, String())
            Dim result As New Dictionary(Of String, String())(StringComparer.Ordinal)
            If Not Flatten(obj, String.Empty, result) Then Return Nothing
            Return result
        End Function

        Private Shared Function Flatten(node As JsonNode, prefix As String, result As Dictionary(Of String, String())) As Boolean
            Select Case node?.GetValueKind()
                Case JsonValueKind.Object
                    For Each kv In node.AsObject()
                        Dim key = If(prefix.Length = 0, kv.Key, prefix & "." & kv.Key)
                        If Not Flatten(kv.Value, key, result) Then Return False
                    Next
                    Return True
                Case JsonValueKind.Array
                    Dim messages As New List(Of String)()
                    For Each item In node.AsArray()
                        Select Case item?.GetValueKind()
                            Case JsonValueKind.String
                                messages.Add(item.GetValue(Of String)())
                            Case JsonValueKind.Object, JsonValueKind.Array
                                If Not Flatten(item, prefix, result) Then Return False
                            Case Else
                                Return False
                        End Select
                    Next
                    If messages.Count > 0 Then result(prefix) = messages.ToArray()
                    Return True
                Case JsonValueKind.String
                    result(prefix) = {node.GetValue(Of String)()}
                    Return True
                Case Else
                    Return False
            End Select
        End Function

        Private Shared Function AsText(node As JsonNode) As String
            If node Is Nothing Then Return Nothing
            If node.GetValueKind() = JsonValueKind.String Then Return node.GetValue(Of String)()
            Return node.ToJsonString()
        End Function

#End Region

    End Class

End Namespace
