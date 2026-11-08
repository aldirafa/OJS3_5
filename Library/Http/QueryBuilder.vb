Imports System.Globalization
Imports System.Text

Namespace Http

    ''' <summary>
    ''' Builds an OJS query string. Arrays are sent comma-separated (<c>stageIds=1,3</c>, OpenAPI
    ''' <c>style: form, explode: false</c>), booleans as <c>true</c>/<c>false</c>, enums as their numeric value
    ''' and <c>Nothing</c> values are skipped.
    ''' </summary>
    ''' <example>
    ''' <code>
    ''' Dim q As New QueryBuilder()
    ''' q.Add("count", 30).Add("stageIds", {1, 3}).Add("searchPhrase", "climate")
    ''' ' q.ToString() = "?count=30&amp;stageIds=1,3&amp;searchPhrase=climate"
    ''' </code>
    ''' </example>
    Public NotInheritable Class QueryBuilder

        Private ReadOnly _items As New List(Of KeyValuePair(Of String, String))()

        ''' <summary>The parameters added so far, in order.</summary>
        Public ReadOnly Property Items As IReadOnlyList(Of KeyValuePair(Of String, String))
            Get
                Return _items
            End Get
        End Property

        ''' <summary>Adds a raw string parameter (skipped when <c>Nothing</c>).</summary>
        ''' <param name="name">The parameter name.</param>
        ''' <param name="value">The value; <c>Nothing</c> skips the parameter.</param>
        Public Function Add(name As String, value As String) As QueryBuilder
            If value IsNot Nothing Then _items.Add(New KeyValuePair(Of String, String)(name, value))
            Return Me
        End Function

        ''' <summary>Adds an integer parameter (skipped when <c>Nothing</c>).</summary>
        ''' <param name="name">The parameter name.</param>
        ''' <param name="value">The value; <c>Nothing</c> skips the parameter.</param>
        Public Function Add(name As String, value As Integer?) As QueryBuilder
            If value.HasValue Then Add(name, value.Value.ToString(CultureInfo.InvariantCulture))
            Return Me
        End Function

        ''' <summary>Adds a long parameter (skipped when <c>Nothing</c>).</summary>
        ''' <param name="name">The parameter name.</param>
        ''' <param name="value">The value; <c>Nothing</c> skips the parameter.</param>
        Public Function Add(name As String, value As Long?) As QueryBuilder
            If value.HasValue Then Add(name, value.Value.ToString(CultureInfo.InvariantCulture))
            Return Me
        End Function

        ''' <summary>Adds a boolean parameter as <c>true</c>/<c>false</c> (skipped when <c>Nothing</c>).</summary>
        ''' <param name="name">The parameter name.</param>
        ''' <param name="value">The value; <c>Nothing</c> skips the parameter.</param>
        Public Function Add(name As String, value As Boolean?) As QueryBuilder
            If value.HasValue Then Add(name, If(value.Value, "true", "false"))
            Return Me
        End Function

        ''' <summary>Adds a date parameter as <c>yyyy-MM-dd</c> (skipped when <c>Nothing</c>).</summary>
        ''' <param name="name">The parameter name.</param>
        ''' <param name="value">The value; <c>Nothing</c> skips the parameter.</param>
        Public Function Add(name As String, value As DateOnly?) As QueryBuilder
            If value.HasValue Then Add(name, value.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
            Return Me
        End Function

        ''' <summary>Adds a comma-separated list of integers (skipped when <c>Nothing</c> or empty).</summary>
        ''' <param name="name">The parameter name.</param>
        ''' <param name="values">The values, sent comma-separated; <c>Nothing</c> or an empty list skips the parameter.</param>
        Public Function Add(name As String, values As IEnumerable(Of Integer)) As QueryBuilder
            If values IsNot Nothing AndAlso values.Any() Then
                Add(name, String.Join(",", values.Select(Function(v) v.ToString(CultureInfo.InvariantCulture))))
            End If
            Return Me
        End Function

        ''' <summary>Adds a comma-separated list of strings (skipped when <c>Nothing</c> or empty).</summary>
        ''' <param name="name">The parameter name.</param>
        ''' <param name="values">The values, sent comma-separated; <c>Nothing</c> or an empty list skips the parameter.</param>
        Public Function Add(name As String, values As IEnumerable(Of String)) As QueryBuilder
            If values IsNot Nothing AndAlso values.Any() Then Add(name, String.Join(",", values))
            Return Me
        End Function

        ''' <summary>Adds an enum parameter as its numeric value (skipped when <c>Nothing</c>).</summary>
        ''' <typeparam name="TEnum">The enum type.</typeparam>
        ''' <param name="name">The parameter name.</param>
        ''' <param name="value">The value; <c>Nothing</c> skips the parameter.</param>
        Public Function AddEnum(Of TEnum As {Structure, IConvertible})(name As String, value As TEnum?) As QueryBuilder
            If value.HasValue Then Add(name, Convert.ToInt64(value.Value, CultureInfo.InvariantCulture))
            Return Me
        End Function

        ''' <summary>Adds a comma-separated list of enum values as numbers (skipped when <c>Nothing</c> or empty).</summary>
        ''' <typeparam name="TEnum">The enum type.</typeparam>
        ''' <param name="name">The parameter name.</param>
        ''' <param name="values">The values, sent comma-separated; <c>Nothing</c> or an empty list skips the parameter.</param>
        Public Function AddEnums(Of TEnum As {Structure, IConvertible})(name As String, values As IEnumerable(Of TEnum)) As QueryBuilder
            If values IsNot Nothing AndAlso values.Any() Then
                Add(name, String.Join(",", values.Select(Function(v) Convert.ToInt64(v, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture))))
            End If
            Return Me
        End Function

        ''' <summary>Returns the query string including the leading <c>?</c>, or an empty string when there are no parameters.</summary>
        Public Overrides Function ToString() As String
            If _items.Count = 0 Then Return String.Empty
            Dim sb As New StringBuilder("?")
            For i = 0 To _items.Count - 1
                If i > 0 Then sb.Append("&"c)
                sb.Append(Uri.EscapeDataString(_items(i).Key)).Append("="c)
                ' keep commas readable: escape each comma-separated part individually
                sb.Append(String.Join(",", _items(i).Value.Split(","c).Select(Function(p) Uri.EscapeDataString(p))))
            Next
            Return sb.ToString()
        End Function

    End Class

End Namespace
