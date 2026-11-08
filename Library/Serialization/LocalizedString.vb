Namespace Serialization

    ''' <summary>
    ''' A multilingual text value (OpenAPI <c>LocaleObject</c>), keyed by locale code:
    ''' <c>{"en": "Title", "fr_CA": "Titre"}</c>. OJS uses this shape for both reading and writing
    ''' multilingual properties, even on single-language installations.
    ''' </summary>
    ''' <remarks>
    ''' PHP quirks are tolerated when reading: an empty value serialized as <c>[]</c>, <c>null</c> entries,
    ''' numeric entries, and a plain string (stored under the key <c>""</c>).
    ''' </remarks>
    ''' <example>
    ''' <code>
    ''' ' Reading: Indonesian title, or English when there is none
    ''' Dim publication = Await ojs.Publications.GetAsync(12, 15)
    ''' Console.WriteLine(publication.Title.GetValue("id", fallbackLocale:="en"))
    '''
    ''' ' Writing
    ''' Dim edit As New PublicationEdit With {
    '''     .Title = New LocalizedString From {{"en", "My title"}, {"id", "Judul saya"}}
    ''' }
    ''' ' or
    ''' edit.Title = LocalizedString.Create("en", "My title")
    ''' </code>
    ''' </example>
    Public Class LocalizedString
        Inherits Dictionary(Of String, String)

        ''' <summary>Creates an empty value.</summary>
        Public Sub New()
            MyBase.New(StringComparer.Ordinal)
        End Sub

        ''' <summary>Creates a value copied from an existing dictionary.</summary>
        Public Sub New(values As IDictionary(Of String, String))
            MyBase.New(values, StringComparer.Ordinal)
        End Sub

        ''' <summary>Creates a value holding a single locale.</summary>
        ''' <param name="locale">A locale code such as <c>en</c> or <c>fr_CA</c>.</param>
        ''' <param name="value">The text in that locale.</param>
        Public Shared Function Create(locale As String, value As String) As LocalizedString
            Return New LocalizedString From {{locale, value}}
        End Function

        ''' <summary>
        ''' Returns the value for <paramref name="locale"/>; when missing or empty, the value for
        ''' <paramref name="fallbackLocale"/>; when that is missing too, the first non-empty value (or <c>Nothing</c>).
        ''' </summary>
        ''' <param name="locale">The locale code, e.g. <c>en</c>.</param>
        ''' <param name="fallbackLocale">A locale to try when <paramref name="locale"/> has no value.</param>
        Public Function GetValue(locale As String, Optional fallbackLocale As String = Nothing) As String
            Dim v As String = Nothing
            If locale IsNot Nothing AndAlso TryGetValue(locale, v) AndAlso Not String.IsNullOrEmpty(v) Then Return v
            If fallbackLocale IsNot Nothing AndAlso TryGetValue(fallbackLocale, v) AndAlso Not String.IsNullOrEmpty(v) Then Return v
            Return Values.FirstOrDefault(Function(x) Not String.IsNullOrEmpty(x))
        End Function

        ''' <summary>Returns the first non-empty value, or an empty string.</summary>
        Public Overrides Function ToString() As String
            Return If(GetValue(Nothing), String.Empty)
        End Function

    End Class

    ''' <summary>
    ''' A multilingual list value, keyed by locale code: <c>{"en": ["keyword 1", "keyword 2"]}</c>.
    ''' OJS 3.5 uses <see cref="LocalizedVocab"/> for keywords and subjects; this type is kept for multilingual
    ''' string lists in plugin data and older OJS versions.
    ''' </summary>
    ''' <example>
    ''' <code>
    ''' Dim tags = LocalizedList.Create("en", "open access", "publishing")
    ''' For Each t In tags.GetValues("en")
    '''     Console.WriteLine(t)
    ''' Next
    ''' </code>
    ''' </example>
    Public Class LocalizedList
        Inherits Dictionary(Of String, List(Of String))

        ''' <summary>Creates an empty value.</summary>
        Public Sub New()
            MyBase.New(StringComparer.Ordinal)
        End Sub

        ''' <summary>Creates a value holding a single locale.</summary>
        ''' <param name="locale">The locale code, e.g. <c>en</c>.</param>
        ''' <param name="values">The values for that locale.</param>
        Public Shared Function Create(locale As String, ParamArray values As String()) As LocalizedList
            Return New LocalizedList From {{locale, New List(Of String)(values)}}
        End Function

        ''' <summary>Returns the list for <paramref name="locale"/>, or an empty list when missing.</summary>
        ''' <param name="locale">The locale code, e.g. <c>en</c>.</param>
        Public Function GetValues(locale As String) As IReadOnlyList(Of String)
            Dim v As List(Of String) = Nothing
            If locale IsNot Nothing AndAlso TryGetValue(locale, v) AndAlso v IsNot Nothing Then Return v
            Return Array.Empty(Of String)()
        End Function

    End Class

End Namespace
