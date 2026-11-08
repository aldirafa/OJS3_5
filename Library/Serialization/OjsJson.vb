Imports System.Text.Json
Imports System.Text.Json.Nodes
Imports System.Text.Json.Serialization
Imports System.Text.Json.Serialization.Metadata

Namespace Serialization

    ''' <summary>
    ''' JSON (de)serialization used by the client. Exposed so that callers can (de)serialize OJS payloads
    ''' themselves (for example data captured from webhooks, fixtures or logs) with the same rules.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' OJS is written in PHP and its JSON has quirks that System.Text.Json rejects by default. Because VB.NET
    ''' cannot implement <c>JsonConverter</c> (it requires the ref struct <c>Utf8JsonReader</c>), payloads are
    ''' first parsed into a <see cref="JsonNode"/> tree and then normalized against the target type's
    ''' serialization contract before being bound. See <see cref="JsonNormalizer"/> for the full list of fixes.
    ''' </para>
    ''' <para>
    ''' When writing, <see cref="DateTime"/> values are emitted in the OJS format <c>yyyy-MM-dd HH:mm:ss</c> and
    ''' <see cref="DateOnly"/> values as <c>yyyy-MM-dd</c>; <c>Nothing</c> properties are omitted so that
    ''' <c>PUT</c> requests only change what you set.
    ''' </para>
    ''' </remarks>
    ''' <example>
    ''' <code>
    ''' Dim submission = OjsJson.Deserialize(Of Submission)(File.ReadAllText("submission.json"))
    ''' Dim json = OjsJson.Serialize(New PublicationEdit With {.Title = LocalizedString.Create("en", "Hello")})
    ''' </code>
    ''' </example>
    Public NotInheritable Class OjsJson

        Private Sub New()
        End Sub

        ''' <summary>The serializer options used for all requests and responses (read-only).</summary>
        Public Shared ReadOnly Property Options As JsonSerializerOptions = CreateOptions()

        Private Shared Function CreateOptions() As JsonSerializerOptions
            Dim o As New JsonSerializerOptions(JsonSerializerDefaults.Web) With {
                .PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                .PropertyNameCaseInsensitive = True,
                .DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                .NumberHandling = JsonNumberHandling.AllowReadingFromString,
                .ReadCommentHandling = JsonCommentHandling.Skip,
                .AllowTrailingCommas = True,
                .TypeInfoResolver = New DefaultJsonTypeInfoResolver()
            }
            o.MakeReadOnly()
            Return o
        End Function

        ''' <summary>Deserializes OJS JSON into <typeparamref name="T"/>, tolerating PHP quirks.</summary>
        ''' <typeparam name="T">The type to bind the response to.</typeparam>
        ''' <param name="json">The JSON text.</param>
        ''' <returns>The value, or <c>Nothing</c>/default when the JSON is empty or <c>null</c>.</returns>
        Public Shared Function Deserialize(Of T)(json As String) As T
            Return DirectCast(Deserialize(json, GetType(T)), T)
        End Function

        ''' <summary>Deserializes OJS JSON into <paramref name="returnType"/>, tolerating PHP quirks.</summary>
        ''' <param name="json">The JSON text.</param>
        ''' <param name="returnType">The type to bind to.</param>
        Public Shared Function Deserialize(json As String, returnType As Type) As Object
            If String.IsNullOrWhiteSpace(json) Then
                Return If(returnType.IsValueType, Activator.CreateInstance(returnType), Nothing)
            End If
            Dim node = JsonNode.Parse(json, documentOptions:=New JsonDocumentOptions With {
                .AllowTrailingCommas = True, .CommentHandling = JsonCommentHandling.Skip})
            Return FromNode(node, returnType)
        End Function

        ''' <summary>Binds an already parsed <see cref="JsonNode"/> to <paramref name="returnType"/>, tolerating PHP quirks.</summary>
        ''' <param name="node">The JSON tree.</param>
        ''' <param name="returnType">The type to bind to.</param>
        Public Shared Function FromNode(node As JsonNode, returnType As Type) As Object
            If returnType Is GetType(JsonElement) Then
                Return If(node Is Nothing, JsonDocument.Parse("null").RootElement.Clone(), JsonSerializer.SerializeToElement(node, Options))
            End If
            If returnType Is GetType(JsonNode) Then Return node
            Dim normalized = JsonNormalizer.NormalizeForRead(node, returnType)
            If normalized Is Nothing Then
                Return If(returnType.IsValueType AndAlso Nullable.GetUnderlyingType(returnType) Is Nothing,
                          Activator.CreateInstance(returnType), Nothing)
            End If
            Return normalized.Deserialize(returnType, Options)
        End Function

        ''' <summary>Serializes <paramref name="value"/> into OJS-compatible JSON.</summary>
        ''' <param name="value">The object to serialize.</param>
        Public Shared Function Serialize(value As Object) As String
            Dim node = SerializeToNode(value)
            Return If(node Is Nothing, "null", node.ToJsonString(Options))
        End Function

        ''' <summary>Serializes <paramref name="value"/> into an OJS-compatible <see cref="JsonNode"/>.</summary>
        ''' <param name="value">The object to serialize.</param>
        Public Shared Function SerializeToNode(value As Object) As JsonNode
            If value Is Nothing Then Return Nothing
            Dim node = JsonSerializer.SerializeToNode(value, value.GetType(), Options)
            Return JsonNormalizer.NormalizeForWrite(node, value.GetType())
        End Function

    End Class

End Namespace
