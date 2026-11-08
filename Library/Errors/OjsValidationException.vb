Imports System.Net

Namespace Errors

    ''' <summary>
    ''' Thrown when OJS rejects a request because of invalid parameters (HTTP 400 with an
    ''' <c>InvalidParametersResponse</c> body such as <c>{"title": ["You must provide a title."]}</c>).
    ''' </summary>
    ''' <example>
    ''' <code>
    ''' Try
    '''     Await ojs.Announcements.CreateAsync(New AnnouncementEdit())
    ''' Catch ex As OjsValidationException
    '''     For Each kv In ex.Errors
    '''         Console.WriteLine($"{kv.Key}: {String.Join("; ", kv.Value)}")
    '''     Next
    ''' End Try
    ''' </code>
    ''' </example>
    Public Class OjsValidationException
        Inherits OjsApiException

        ''' <summary>
        ''' Validation messages keyed by property name. Multilingual properties may use keys such as
        ''' <c>title.en</c> depending on the endpoint.
        ''' </summary>
        Public ReadOnly Property Errors As IReadOnlyDictionary(Of String, String())

        ''' <summary>Creates a new <see cref="OjsValidationException"/>.</summary>
        Public Sub New(errors As IReadOnlyDictionary(Of String, String()), responseBody As String,
                       Optional requestMethod As String = Nothing, Optional requestUri As Uri = Nothing)
            MyBase.New(HttpStatusCode.BadRequest, "api.400.invalidParameters", Summarize(errors), responseBody,
                       requestMethod, requestUri,
                       $"OJS rejected the request parameters: {Summarize(errors)}")
            Me.Errors = errors
        End Sub

        Private Shared Function Summarize(errors As IReadOnlyDictionary(Of String, String())) As String
            If errors Is Nothing OrElse errors.Count = 0 Then Return "Invalid parameters."
            Return String.Join(" | ", errors.Select(Function(kv) $"{kv.Key}: {String.Join("; ", kv.Value)}"))
        End Function

    End Class

End Namespace
