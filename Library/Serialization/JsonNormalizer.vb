Imports System.Globalization
Imports System.Text.Json
Imports System.Text.Json.Nodes
Imports System.Text.Json.Serialization.Metadata

Namespace Serialization

    ''' <summary>
    ''' Rewrites a <see cref="JsonNode"/> tree so that it matches what System.Text.Json expects for a target type.
    ''' The target type's serialization contract (<see cref="JsonTypeInfo"/>) drives the walk, so only values whose
    ''' destination type is known are touched; unknown properties (captured by <c>JsonExtensionData</c>) are left as-is.
    ''' </summary>
    ''' <remarks>
    ''' Fixes applied when reading:
    ''' <list type="bullet">
    ''' <item>Dates <c>"yyyy-MM-dd HH:mm:ss"</c> → ISO 8601; <c>""</c> and <c>"0000-00-00"</c> → <c>null</c>.</item>
    ''' <item>Booleans sent as <c>0</c>/<c>1</c>/<c>"0"</c>/<c>"1"</c>/<c>"true"</c>/<c>""</c>.</item>
    ''' <item>Numbers and enums sent as strings (including <c>""</c> → <c>null</c>).</item>
    ''' <item>Strings sent as numbers/booleans.</item>
    ''' <item>Empty objects/dictionaries sent as <c>[]</c>, and lists sent as objects with numeric keys.</item>
    ''' <item>A plain string where a <see cref="LocalizedString"/> is expected (stored under key <c>""</c>).</item>
    ''' </list>
    ''' Fixes applied when writing: <see cref="DateTime"/>/<see cref="DateTimeOffset"/> → <c>"yyyy-MM-dd HH:mm:ss"</c>.
    ''' </remarks>
    ''' <example>
    ''' <code>
    ''' ' Used internally by OjsJson; PHP-style JSON binds to the models as-is:
    ''' Dim s = OjsJson.Deserialize(Of Submission)("{""id"": ""7"", ""dateSubmitted"": ""2026-06-01 10:00:00"", ""locale"": []}")
    ''' </code>
    ''' </example>
    Public NotInheritable Class JsonNormalizer

        Private Const MaxDepth As Integer = 64
        Private Const OjsDateTimeFormat As String = "yyyy-MM-dd HH:mm:ss"

        Private Sub New()
        End Sub

        ''' <summary>Normalizes a response tree for binding to <paramref name="targetType"/>.</summary>
        ''' <param name="node">The JSON tree.</param>
        ''' <param name="targetType">The type the JSON will be bound to.</param>
        ''' <returns>The (possibly replaced) root node.</returns>
        Public Shared Function NormalizeForRead(node As JsonNode, targetType As Type) As JsonNode
            Return Visit(node, targetType, forWrite:=False, depth:=0)
        End Function

        ''' <summary>Normalizes a serialized request tree produced from <paramref name="sourceType"/>.</summary>
        ''' <param name="node">The JSON tree.</param>
        ''' <param name="sourceType">The type the JSON was serialized from.</param>
        Public Shared Function NormalizeForWrite(node As JsonNode, sourceType As Type) As JsonNode
            Return Visit(node, sourceType, forWrite:=True, depth:=0)
        End Function

        Private Shared Function Visit(node As JsonNode, type As Type, forWrite As Boolean, depth As Integer) As JsonNode
            If node Is Nothing OrElse type Is Nothing OrElse depth > MaxDepth Then Return node

            Dim t = If(Nullable.GetUnderlyingType(type), type)
            If t Is GetType(Object) OrElse t Is GetType(JsonElement) OrElse t Is GetType(JsonDocument) OrElse
               GetType(JsonNode).IsAssignableFrom(t) Then
                Return node
            End If

            If forWrite Then Return VisitWrite(node, t, depth)
            Return VisitRead(node, t, depth)
        End Function

#Region "Read"

        Private Shared Function VisitRead(node As JsonNode, t As Type, depth As Integer) As JsonNode
            Dim kind = node.GetValueKind()

            ' ---- scalars -------------------------------------------------------------------------------
            If t Is GetType(String) Then
                Select Case kind
                    Case JsonValueKind.Number, JsonValueKind.True, JsonValueKind.False
                        Return JsonValue.Create(ScalarText(node))
                    Case JsonValueKind.Object, JsonValueKind.Array
                        Return JsonValue.Create(node.ToJsonString())
                    Case Else
                        Return node
                End Select
            End If

            If t Is GetType(Boolean) Then Return ReadBoolean(node, kind)

            If t Is GetType(DateTime) OrElse t Is GetType(DateTimeOffset) Then Return ReadDateTime(node, kind)

            If t Is GetType(DateOnly) Then Return ReadDateOnly(node, kind)

            If t.IsEnum OrElse IsNumeric(t) Then Return ReadNumber(node, kind)

            If t Is GetType(Guid) OrElse t Is GetType(TimeSpan) OrElse t Is GetType(Uri) Then
                If kind = JsonValueKind.String AndAlso node.GetValue(Of String)().Length = 0 Then Return Nothing
                Return node
            End If

            ' ---- shape tolerance -----------------------------------------------------------------------
            ' Some list endpoints return a bare array although others (and the docs) use {itemsMax, items}.
            If t.IsGenericType AndAlso t.GetGenericTypeDefinition() Is GetType(Paging.PagedResult(Of )) AndAlso
               kind = JsonValueKind.Array Then
                Dim arr = node.AsArray()
                node = New JsonObject From {{"itemsMax", arr.Count}, {"items", arr.DeepClone()}}
                kind = JsonValueKind.Object
            End If

            ' OJS < 3.5 sent vocabulary terms as plain strings.
            If t Is GetType(VocabEntry) AndAlso kind = JsonValueKind.String Then
                Return New JsonObject From {{"name", node.GetValue(Of String)()}}
            End If

            ' ---- complex types driven by the contract ------------------------------------------------
            Dim info As JsonTypeInfo = Nothing
            Try
                info = OjsJson.Options.GetTypeInfo(t)
            Catch ex As NotSupportedException
                Return node
            End Try

            Select Case info.Kind
                Case JsonTypeInfoKind.Dictionary
                    Return ReadDictionary(node, kind, t, info, depth)
                Case JsonTypeInfoKind.Enumerable
                    Return ReadEnumerable(node, kind, info, depth)
                Case JsonTypeInfoKind.Object
                    Return ReadObject(node, kind, info, depth)
                Case Else
                    Return node
            End Select
        End Function

        Private Shared Function ReadBoolean(node As JsonNode, kind As JsonValueKind) As JsonNode
            Select Case kind
                Case JsonValueKind.Number
                    Return JsonValue.Create(node.GetValue(Of Double)() <> 0)
                Case JsonValueKind.String
                    Dim s = node.GetValue(Of String)().Trim().ToLowerInvariant()
                    Select Case s
                        Case "1", "true", "yes", "on" : Return JsonValue.Create(True)
                        Case "0", "false", "no", "off" : Return JsonValue.Create(False)
                        Case "" : Return Nothing
                        Case Else : Return node
                    End Select
                Case Else
                    Return node
            End Select
        End Function

        Private Shared Function ReadDateTime(node As JsonNode, kind As JsonValueKind) As JsonNode
            If kind <> JsonValueKind.String Then Return If(kind = JsonValueKind.Null, Nothing, node)
            Dim s = node.GetValue(Of String)().Trim()
            If s.Length = 0 OrElse s.StartsWith("0000-00-00", StringComparison.Ordinal) Then Return Nothing
            If s.Length = 10 Then Return JsonValue.Create(s & "T00:00:00")
            If s.Length >= 19 AndAlso s(10) = " "c Then Return JsonValue.Create(s.Substring(0, 10) & "T" & s.Substring(11))
            Return JsonValue.Create(s)
        End Function

        Private Shared Function ReadDateOnly(node As JsonNode, kind As JsonValueKind) As JsonNode
            If kind <> JsonValueKind.String Then Return If(kind = JsonValueKind.Null, Nothing, node)
            Dim s = node.GetValue(Of String)().Trim()
            If s.Length = 0 OrElse s.StartsWith("0000-00-00", StringComparison.Ordinal) Then Return Nothing
            If s.Length > 10 Then s = s.Substring(0, 10)
            Return JsonValue.Create(s)
        End Function

        Private Shared Function ReadNumber(node As JsonNode, kind As JsonValueKind) As JsonNode
            Select Case kind
                Case JsonValueKind.String
                    Dim s = node.GetValue(Of String)().Trim()
                    If s.Length = 0 Then Return Nothing
                    Dim l As Long
                    If Long.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, l) Then Return JsonValue.Create(l)
                    Dim d As Decimal
                    If Decimal.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, d) Then Return JsonValue.Create(d)
                    Return node ' enum names etc.
                Case JsonValueKind.True
                    Return JsonValue.Create(1)
                Case JsonValueKind.False
                    Return JsonValue.Create(0)
                Case Else
                    Return node
            End Select
        End Function

        Private Shared Function ReadDictionary(node As JsonNode, kind As JsonValueKind, t As Type, info As JsonTypeInfo, depth As Integer) As JsonNode
            Dim obj As JsonObject
            Select Case kind
                Case JsonValueKind.Object
                    obj = node.AsObject()
                Case JsonValueKind.Array
                    ' PHP: an empty associative array is [], a list where a map was expected has numeric keys.
                    obj = New JsonObject()
                    Dim arr = node.AsArray()
                    For i = 0 To arr.Count - 1
                        obj(i.ToString(CultureInfo.InvariantCulture)) = arr(i)?.DeepClone()
                    Next
                Case JsonValueKind.String
                    If GetType(LocalizedString).IsAssignableFrom(t) Then
                        Return New JsonObject From {{String.Empty, node.GetValue(Of String)()}}
                    End If
                    Return node
                Case Else
                    Return node
            End Select

            Dim valueType = info.ElementType
            For Each key In obj.Select(Function(kv) kv.Key).ToList()
                Dim current = obj(key)
                Dim replaced = Visit(current, valueType, forWrite:=False, depth:=depth + 1)
                If Not ReferenceEquals(current, replaced) Then obj(key) = Detach(replaced)
            Next
            Return obj
        End Function

        Private Shared Function ReadEnumerable(node As JsonNode, kind As JsonValueKind, info As JsonTypeInfo, depth As Integer) As JsonNode
            Dim arr As JsonArray
            Select Case kind
                Case JsonValueKind.Array
                    arr = node.AsArray()
                Case JsonValueKind.Object
                    Dim obj = node.AsObject()
                    Dim items As JsonNode = Nothing
                    If obj.TryGetPropertyValue("items", items) AndAlso items IsNot Nothing AndAlso
                       items.GetValueKind() = JsonValueKind.Array Then
                        ' A paged envelope {itemsMax, items} where a plain list was expected.
                        arr = items.AsArray()
                    Else
                        ' PHP list with non-sequential keys: {"0": "a", "3": "b"}
                        arr = New JsonArray()
                        For Each kv In obj
                            arr.Add(kv.Value?.DeepClone())
                        Next
                    End If
                Case JsonValueKind.String
                    If node.GetValue(Of String)().Length = 0 Then Return Nothing
                    Return node
                Case Else
                    Return node
            End Select

            Dim elementType = info.ElementType
            For i = 0 To arr.Count - 1
                Dim current = arr(i)
                Dim replaced = Visit(current, elementType, forWrite:=False, depth:=depth + 1)
                If Not ReferenceEquals(current, replaced) Then arr(i) = Detach(replaced)
            Next
            Return arr
        End Function

        Private Shared Function ReadObject(node As JsonNode, kind As JsonValueKind, info As JsonTypeInfo, depth As Integer) As JsonNode
            Select Case kind
                Case JsonValueKind.Object
                    ' handled below
                Case JsonValueKind.Array
                    ' PHP: an empty object is serialized as []
                    If node.AsArray().Count = 0 Then Return New JsonObject()
                    Return node
                Case JsonValueKind.String, JsonValueKind.False
                    ' PHP sometimes returns "" or false instead of null for a missing object
                    If kind = JsonValueKind.False OrElse node.GetValue(Of String)().Length = 0 Then Return Nothing
                    Return node
                Case Else
                    Return node
            End Select

            Dim obj = node.AsObject()
            Dim props = GetPropertyMap(info)
            For Each key In obj.Select(Function(kv) kv.Key).ToList()
                Dim prop As JsonPropertyInfo = Nothing
                If Not props.TryGetValue(key, prop) Then Continue For
                Dim current = obj(key)
                Dim replaced = Visit(current, prop.PropertyType, forWrite:=False, depth:=depth + 1)
                If replaced Is Nothing AndAlso prop.PropertyType.IsValueType AndAlso
                   Nullable.GetUnderlyingType(prop.PropertyType) Is Nothing Then
                    ' null for a non-nullable value type: drop it so the property keeps its default
                    obj.Remove(key)
                    Continue For
                End If
                If Not ReferenceEquals(current, replaced) Then obj(key) = Detach(replaced)
            Next
            Return obj
        End Function

#End Region

#Region "Write"

        Private Shared Function VisitWrite(node As JsonNode, t As Type, depth As Integer) As JsonNode
            Dim kind = node.GetValueKind()

            If t Is GetType(DateTime) AndAlso kind = JsonValueKind.String Then
                Dim dt As DateTime
                If DateTime.TryParse(node.GetValue(Of String)(), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, dt) Then
                    Return JsonValue.Create(dt.ToString(OjsDateTimeFormat, CultureInfo.InvariantCulture))
                End If
                Return node
            End If

            If t Is GetType(DateTimeOffset) AndAlso kind = JsonValueKind.String Then
                Dim dto As DateTimeOffset
                If DateTimeOffset.TryParse(node.GetValue(Of String)(), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, dto) Then
                    Return JsonValue.Create(dto.ToString(OjsDateTimeFormat, CultureInfo.InvariantCulture))
                End If
                Return node
            End If

            If t.IsPrimitive OrElse t.IsEnum OrElse t Is GetType(String) OrElse t Is GetType(Decimal) OrElse t Is GetType(DateOnly) Then
                Return node
            End If

            Dim info As JsonTypeInfo = Nothing
            Try
                info = OjsJson.Options.GetTypeInfo(t)
            Catch ex As NotSupportedException
                Return node
            End Try

            Select Case info.Kind
                Case JsonTypeInfoKind.Dictionary
                    If kind <> JsonValueKind.Object Then Return node
                    Dim obj = node.AsObject()
                    For Each key In obj.Select(Function(kv) kv.Key).ToList()
                        Dim current = obj(key)
                        Dim replaced = Visit(current, info.ElementType, forWrite:=True, depth:=depth + 1)
                        If Not ReferenceEquals(current, replaced) Then obj(key) = Detach(replaced)
                    Next
                    Return obj
                Case JsonTypeInfoKind.Enumerable
                    If kind <> JsonValueKind.Array Then Return node
                    Dim arr = node.AsArray()
                    For i = 0 To arr.Count - 1
                        Dim current = arr(i)
                        Dim replaced = Visit(current, info.ElementType, forWrite:=True, depth:=depth + 1)
                        If Not ReferenceEquals(current, replaced) Then arr(i) = Detach(replaced)
                    Next
                    Return arr
                Case JsonTypeInfoKind.Object
                    If kind <> JsonValueKind.Object Then Return node
                    Dim obj = node.AsObject()
                    Dim props = GetPropertyMap(info)
                    For Each key In obj.Select(Function(kv) kv.Key).ToList()
                        Dim prop As JsonPropertyInfo = Nothing
                        If Not props.TryGetValue(key, prop) Then Continue For
                        Dim current = obj(key)
                        Dim replaced = Visit(current, prop.PropertyType, forWrite:=True, depth:=depth + 1)
                        If Not ReferenceEquals(current, replaced) Then obj(key) = Detach(replaced)
                    Next
                    Return obj
                Case Else
                    Return node
            End Select
        End Function

#End Region

#Region "Helpers"

        Private Shared ReadOnly PropertyMaps As New Concurrent.ConcurrentDictionary(Of Type, Dictionary(Of String, JsonPropertyInfo))()

        Private Shared Function GetPropertyMap(info As JsonTypeInfo) As Dictionary(Of String, JsonPropertyInfo)
            Return PropertyMaps.GetOrAdd(info.Type,
                Function(unused)
                    Dim map As New Dictionary(Of String, JsonPropertyInfo)(StringComparer.OrdinalIgnoreCase)
                    For Each p In info.Properties
                        If p.IsExtensionData Then Continue For
                        map(p.Name) = p
                    Next
                    Return map
                End Function)
        End Function

        Private Shared Function Detach(node As JsonNode) As JsonNode
            If node Is Nothing OrElse node.Parent Is Nothing Then Return node
            Return node.DeepClone()
        End Function

        Private Shared Function ScalarText(node As JsonNode) As String
            Select Case node.GetValueKind()
                Case JsonValueKind.True : Return "true"
                Case JsonValueKind.False : Return "false"
                Case Else : Return node.ToJsonString()
            End Select
        End Function

        Private Shared Function IsNumeric(t As Type) As Boolean
            Select Case Type.GetTypeCode(t)
                Case TypeCode.Byte, TypeCode.SByte, TypeCode.Int16, TypeCode.UInt16, TypeCode.Int32, TypeCode.UInt32,
                     TypeCode.Int64, TypeCode.UInt64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal
                    Return True
                Case Else
                    Return False
            End Select
        End Function

#End Region

    End Class

End Namespace
