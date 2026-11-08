Imports System.Net
Imports System.Net.Http
Imports System.Text
Imports System.Threading

''' <summary>An HttpMessageHandler that records requests and replies with canned responses.</summary>
Public Class FakeHandler
    Inherits HttpMessageHandler

    Public Class RecordedRequest
        Public Property Method As HttpMethod
        Public Property Uri As Uri
        Public Property Authorization As String
        Public Property Accept As String
        Public Property ContentType As String
        Public Property Body As String
    End Class

    Private ReadOnly _responses As New Queue(Of Func(Of HttpResponseMessage))()

    Public ReadOnly Property Requests As New List(Of RecordedRequest)()

    Public ReadOnly Property Last As RecordedRequest
        Get
            Return Requests.Last()
        End Get
    End Property

    Public Function RespondJson(json As String, Optional status As HttpStatusCode = HttpStatusCode.OK) As FakeHandler
        _responses.Enqueue(Function() New HttpResponseMessage(status) With {
            .Content = New StringContent(json, Encoding.UTF8, "application/json")})
        Return Me
    End Function

    Public Function Respond(content As HttpContent, Optional status As HttpStatusCode = HttpStatusCode.OK) As FakeHandler
        _responses.Enqueue(Function() New HttpResponseMessage(status) With {.Content = content})
        Return Me
    End Function

    Protected Overrides Async Function SendAsync(request As HttpRequestMessage, cancellationToken As CancellationToken) As Task(Of HttpResponseMessage)
        Dim rec As New RecordedRequest With {
            .Method = request.Method,
            .Uri = request.RequestUri,
            .Authorization = request.Headers.Authorization?.ToString(),
            .Accept = request.Headers.Accept.ToString()
        }
        If request.Content IsNot Nothing Then
            rec.ContentType = request.Content.Headers.ContentType?.MediaType
            rec.Body = Await request.Content.ReadAsStringAsync(cancellationToken)
        End If
        Requests.Add(rec)
        If _responses.Count = 0 Then Return New HttpResponseMessage(HttpStatusCode.OK) With {.Content = New StringContent("{}")}
        Return _responses.Dequeue()()
    End Function

End Class

Public Module TestClient

    Public Function Create(handler As FakeHandler, Optional journal As String = "jpk") As OjsClient
        Return New OjsClient(New HttpClient(handler), New OjsClientOptions With {
            .BaseUrl = "https://ojs.test", .JournalPath = journal, .ApiToken = "TOKEN"})
    End Function

End Module
