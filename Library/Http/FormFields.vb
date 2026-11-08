Imports System.Net.Http
Imports System.Text.Json
Imports System.Text.Json.Nodes
Imports OJS3_5.Library.Serialization

Namespace Http

    ''' <summary>
    ''' Flattens a model into PHP-style form fields for <c>multipart/form-data</c> requests:
    ''' <c>{"name": {"en": "x"}, "genreId": 1}</c> becomes <c>name[en]=x</c> and <c>genreId=1</c>.
    ''' Booleans become <c>1</c>/<c>0</c> (accepted by Laravel's <c>boolean</c> rule).
    ''' </summary>
    Friend Module FormFields

        ''' <summary>Adds the non-null properties of a model as PHP-style form fields (<c>name[en]</c>).</summary>
        ''' <param name="form">The multipart form to add fields to.</param>
        ''' <param name="model">The object whose non-null properties become form fields.</param>
        Public Sub AddModel(form As MultipartFormDataContent, model As Object)
            If model Is Nothing Then Return
            Dim node = OjsJson.SerializeToNode(model)
            For Each kv In Flatten(node, String.Empty)
                form.Add(New StringContent(kv.Value), kv.Key)
            Next
        End Sub

        ''' <summary>Flattens a JSON tree into PHP-style form field names and values.</summary>
        ''' <param name="node">The JSON tree.</param>
        ''' <param name="prefix">The key prefix for nested values, e.g. <c>title</c>.</param>
        Public Function Flatten(node As JsonNode, prefix As String) As List(Of KeyValuePair(Of String, String))
            Dim result As New List(Of KeyValuePair(Of String, String))()
            FlattenInto(node, prefix, result)
            Return result
        End Function

        Private Sub FlattenInto(node As JsonNode, prefix As String, result As List(Of KeyValuePair(Of String, String)))
            If node Is Nothing Then Return
            Select Case node.GetValueKind()
                Case JsonValueKind.Object
                    For Each kv In node.AsObject()
                        FlattenInto(kv.Value, If(prefix.Length = 0, kv.Key, $"{prefix}[{kv.Key}]"), result)
                    Next
                Case JsonValueKind.Array
                    Dim arr = node.AsArray()
                    For i = 0 To arr.Count - 1
                        FlattenInto(arr(i), $"{prefix}[{i}]", result)
                    Next
                Case JsonValueKind.True
                    result.Add(New KeyValuePair(Of String, String)(prefix, "1"))
                Case JsonValueKind.False
                    result.Add(New KeyValuePair(Of String, String)(prefix, "0"))
                Case JsonValueKind.String
                    result.Add(New KeyValuePair(Of String, String)(prefix, node.GetValue(Of String)()))
                Case JsonValueKind.Number
                    result.Add(New KeyValuePair(Of String, String)(prefix, node.ToJsonString()))
            End Select
        End Sub

    End Module

End Namespace
