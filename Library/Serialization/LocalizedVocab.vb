Namespace Serialization

    ''' <summary>
    ''' One entry of a controlled vocabulary (keywords, subjects, disciplines, supporting agencies) as used by OJS 3.5:
    ''' <c>{"name": "open access", "source": "...", "identifier": "..."}</c>.
    ''' </summary>
    ''' <example>
    ''' <code>
    ''' Dim pub = Await ojs.Publications.GetAsync(12, 15)
    ''' For Each k In pub.Keywords.GetEntries("en") : Console.WriteLine($"{k.Name} {k.Identifier}") : Next
    ''' </code>
    ''' </example>
    Public Class VocabEntry

        ''' <summary>Creates an empty entry.</summary>
        Public Sub New()
        End Sub

        ''' <summary>Creates an entry with a name.</summary>
        Public Sub New(name As String)
            Me.Name = name
        End Sub

        ''' <summary>The term itself.</summary>
        Public Property Name As String

        ''' <summary>Optional source of the term (for example an external vocabulary).</summary>
        Public Property Source As String

        ''' <summary>Optional identifier of the term in its source vocabulary.</summary>
        Public Property Identifier As String

        ''' <summary>Returns <see cref="Name"/>.</summary>
        Public Overrides Function ToString() As String
            Return If(Name, String.Empty)
        End Function

    End Class

    ''' <summary>
    ''' A multilingual controlled vocabulary keyed by locale code:
    ''' <c>{"en": [{"name": "ASEAN"}, {"name": "foreign policy"}]}</c>.
    ''' </summary>
    ''' <example>
    ''' <code>
    ''' ' Writing
    ''' Await ojs.Publications.UpdateAsync(12, 15, New PublicationEdit With {
    '''     .Keywords = LocalizedVocab.Create("en", "open access", "publishing")})
    '''
    ''' ' Reading
    ''' Dim publication = Await ojs.Publications.GetAsync(12, 15)
    ''' Dim names = publication.Keywords.GetNames("en")   ' {"open access", "publishing"}
    ''' </code>
    ''' </example>
    Public Class LocalizedVocab
        Inherits Dictionary(Of String, List(Of VocabEntry))

        ''' <summary>Creates an empty value.</summary>
        Public Sub New()
            MyBase.New(StringComparer.Ordinal)
        End Sub

        ''' <summary>Creates a value holding one locale with entries for each name.</summary>
        ''' <param name="locale">The locale code, e.g. <c>en</c>.</param>
        ''' <param name="names">The entry names.</param>
        Public Shared Function Create(locale As String, ParamArray names As String()) As LocalizedVocab
            Return New LocalizedVocab From {{locale, names.Select(Function(n) New VocabEntry(n)).ToList()}}
        End Function

        ''' <summary>Returns the entries for <paramref name="locale"/>, or an empty list.</summary>
        ''' <param name="locale">The locale code, e.g. <c>en</c>.</param>
        Public Function GetEntries(locale As String) As IReadOnlyList(Of VocabEntry)
            Dim v As List(Of VocabEntry) = Nothing
            If locale IsNot Nothing AndAlso TryGetValue(locale, v) AndAlso v IsNot Nothing Then Return v
            Return Array.Empty(Of VocabEntry)()
        End Function

        ''' <summary>Returns just the names for <paramref name="locale"/>, trimmed, or an empty list.</summary>
        ''' <param name="locale">The locale code, e.g. <c>en</c>.</param>
        Public Function GetNames(locale As String) As IReadOnlyList(Of String)
            Return GetEntries(locale).Where(Function(e) e?.Name IsNot Nothing).Select(Function(e) e.Name.Trim()).ToList()
        End Function

    End Class

End Namespace
