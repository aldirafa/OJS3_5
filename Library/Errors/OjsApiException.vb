Imports System.Net

Namespace Errors

    ''' <summary>
    ''' Thrown when the OJS REST API returns a non-success status code.
    ''' OJS error bodies look like <c>{"error": "api.404.resourceNotFound", "errorMessage": "The requested resource was not found."}</c>.
    ''' </summary>
    ''' <example>
    ''' <code>
    ''' Try
    '''     Dim s = Await ojs.Submissions.GetAsync(999999)
    ''' Catch ex As OjsApiException When ex.StatusCode = Net.HttpStatusCode.NotFound
    '''     Console.WriteLine($"Not found: {ex.ErrorMessage}")
    ''' End Try
    ''' </code>
    ''' </example>
    Public Class OjsApiException
        Inherits Exception

        ''' <summary>The HTTP status code returned by the server.</summary>
        Public ReadOnly Property StatusCode As HttpStatusCode

        ''' <summary>The machine-readable locale key from the <c>error</c> property, e.g. <c>api.404.resourceNotFound</c>.</summary>
        Public ReadOnly Property [Error] As String

        ''' <summary>The human-readable message from the <c>errorMessage</c> property.</summary>
        Public ReadOnly Property ErrorMessage As String

        ''' <summary>The raw response body (may be empty or non-JSON, e.g. an HTML error page).</summary>
        Public ReadOnly Property ResponseBody As String

        ''' <summary>The HTTP method of the failed request.</summary>
        Public ReadOnly Property RequestMethod As String

        ''' <summary>The URL of the failed request.</summary>
        Public ReadOnly Property RequestUri As Uri

        ''' <summary>Creates a new <see cref="OjsApiException"/>.</summary>
        Public Sub New(statusCode As HttpStatusCode, [error] As String, errorMessage As String, responseBody As String,
                       Optional requestMethod As String = Nothing, Optional requestUri As Uri = Nothing,
                       Optional message As String = Nothing)
            MyBase.New(If(message, BuildMessage(statusCode, [error], errorMessage, requestMethod, requestUri)))
            Me.StatusCode = statusCode
            Me.Error = [error]
            Me.ErrorMessage = errorMessage
            Me.ResponseBody = responseBody
            Me.RequestMethod = requestMethod
            Me.RequestUri = requestUri
        End Sub

        Private Shared Function BuildMessage(statusCode As HttpStatusCode, [error] As String, errorMessage As String,
                                             method As String, uri As Uri) As String
            Dim detail = If(Not String.IsNullOrEmpty(errorMessage), errorMessage, If([error], "No error details were returned."))
            Dim target = If(uri Is Nothing, String.Empty, $" ({method} {uri})")
            Return $"OJS API request failed with {CInt(statusCode)} {statusCode}{target}: {detail}"
        End Function

    End Class

End Namespace
