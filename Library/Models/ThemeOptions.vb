Imports System.Text.Json
Imports System.Text.Json.Serialization

Namespace Models

    ''' <summary>
    ''' The active theme of a journal or the site, and the values of its options.
    ''' </summary>
    ''' <remarks>
    ''' Which options exist depends on the theme, so they are kept in <see cref="Options"/>. When reading, the values
    ''' are <see cref="JsonElement"/>s; when writing, you can put any JSON-serializable value in it.
    ''' <see cref="ThemePluginPath"/> is required when saving, even if the theme does not change.
    ''' </remarks>
    ''' <example>
    ''' <code>
    ''' Dim theme = Await ojs.Contexts.GetThemeAsync(journalId)
    ''' Console.WriteLine($"{theme.ThemePluginPath}: colour {theme.GetString("baseColour")}")
    '''
    ''' Await ojs.Contexts.UpdateThemeAsync(journalId, New ThemeOptions With {.ThemePluginPath = "default"}.
    '''     SetOption("baseColour", "#1E6292").
    '''     SetOption("typography", "notoSans"))
    ''' </code>
    ''' </example>
    Partial Public Class ThemeOptions

        ''' <summary>The path of the theme plugin, e.g. <c>default</c>.</summary>
        Public Property ThemePluginPath As String

        ''' <summary>The theme's option values, keyed by option name.</summary>
        <JsonExtensionData>
        Public Property Options As Dictionary(Of String, Object)

        ''' <summary>Sets an option value and returns this instance, so calls can be chained.</summary>
        ''' <param name="name">The option name, e.g. <c>baseColour</c>.</param>
        ''' <param name="value">The value; <c>Nothing</c> clears the option.</param>
        Public Function SetOption(name As String, value As Object) As ThemeOptions
            ArgumentException.ThrowIfNullOrEmpty(name)
            If Options Is Nothing Then Options = New Dictionary(Of String, Object)()
            Options(name) = value
            Return Me
        End Function

        ''' <summary>Gets an option value as text, or <c>Nothing</c> when it is missing or null.</summary>
        ''' <param name="name">The option name.</param>
        Public Function GetString(name As String) As String
            Dim value As Object = Nothing
            If Options Is Nothing OrElse Not Options.TryGetValue(name, value) OrElse value Is Nothing Then Return Nothing
            If TypeOf value Is JsonElement Then
                Dim e = DirectCast(value, JsonElement)
                Select Case e.ValueKind
                    Case JsonValueKind.Null, JsonValueKind.Undefined : Return Nothing
                    Case JsonValueKind.String : Return e.GetString()
                    Case Else : Return e.GetRawText()
                End Select
            End If
            Return value.ToString()
        End Function

    End Class

End Namespace
