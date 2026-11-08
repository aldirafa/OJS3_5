Imports System.Threading
Imports Library.Http
Imports Library.Models
Imports Library.Paging
Imports Library.Queries
Imports Library.Serialization

Namespace Resources

    ''' <summary>Names of the controlled vocabularies accepted by <see cref="VocabsResource.ListAsync"/>.</summary>
    ''' <example>
    ''' <code>
    ''' Dim subjects = Await ojs.Vocabs.ListAsync(VocabNames.Subjects, "en")
    ''' </code>
    ''' </example>
    Public NotInheritable Class VocabNames

        Private Sub New()
        End Sub

        ''' <summary>Submission keywords.</summary>
        Public Const Keywords As String = "submissionKeyword"
        ''' <summary>Submission subjects.</summary>
        Public Const Subjects As String = "submissionSubject"
        ''' <summary>Submission disciplines.</summary>
        Public Const Disciplines As String = "submissionDiscipline"
        ''' <summary>Supporting agencies.</summary>
        Public Const Agencies As String = "submissionAgency"
        ''' <summary>Submission languages.</summary>
        Public Const Languages As String = "submissionLanguage"

    End Class

    ''' <summary><c>/vocabs</c> — suggestions for keywords, subjects and other controlled vocabularies.</summary>
    ''' <example>
    ''' <code>
    ''' Dim keywords = Await ojs.Vocabs.ListAsync(VocabNames.Keywords, "en")
    ''' Console.WriteLine(String.Join(", ", keywords.Select(Function(k) k.Name)))
    ''' </code>
    ''' </example>
    Public NotInheritable Class VocabsResource
        Inherits ResourceBase

        Friend Sub New(http As OjsHttp)
            MyBase.New(http)
        End Sub

        ''' <summary>Gets the entries of a controlled vocabulary used in the journal.</summary>
        ''' <param name="vocab">The vocabulary name; see <see cref="VocabNames"/>.</param>
        ''' <param name="locale">Only entries in this locale (default: the user's locale).</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <remarks>Older OJS versions return plain strings; they are converted to entries with only a name.</remarks>
        ''' <example>
        ''' <code>
        ''' Dim keywords = Await ojs.Vocabs.ListAsync(VocabNames.Keywords, "en")
        ''' Console.WriteLine(String.Join(", ", keywords.Select(Function(k) k.Name)))
        ''' </code>
        ''' </example>
        Public Function ListAsync(vocab As String, Optional locale As String = Nothing,
                                  Optional cancellationToken As CancellationToken = Nothing) As Task(Of List(Of VocabEntry))
            ArgumentException.ThrowIfNullOrEmpty(vocab)
            Return Http.GetAsync(Of List(Of VocabEntry))("vocabs", New QueryBuilder().Add("vocab", vocab).Add("locale", locale), cancellationToken)
        End Function

    End Class

    ''' <summary><c>/rors</c> — the local copy of the Research Organization Registry, used for affiliations.</summary>
    ''' <example>
    ''' <code>
    ''' Dim found = Await ojs.Rors.ListAsync(New SearchQuery With {.SearchPhrase = "Indonesia", .Count = 10})
    ''' For Each r In found.Items
    '''     Console.WriteLine($"{r.Ror} {r.Name.GetValue(r.DisplayLocale)}")
    ''' Next
    ''' </code>
    ''' </example>
    Public NotInheritable Class RorsResource
        Inherits ResourceBase

        Friend Sub New(http As OjsHttp)
            MyBase.New(http)
        End Sub

        ''' <summary>Gets one page of ROR records.</summary>
        ''' <param name="query">Filters and paging; <c>Nothing</c> for the first page.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim page = Await ojs.Rors.ListAsync(New SearchQuery With {.SearchPhrase = "Indonesia"})
        ''' Console.WriteLine($"{page.ItemsMax} in total")
        ''' For Each item In page.Items : Console.WriteLine(item.Name.GetValue(item.DisplayLocale)) : Next
        ''' </code>
        ''' </example>
        Public Function ListAsync(Optional query As SearchQuery = Nothing,
                                  Optional cancellationToken As CancellationToken = Nothing) As Task(Of PagedResult(Of Ror))
            Return Http.GetAsync(Of PagedResult(Of Ror))("rors", If(query, New SearchQuery()).ToQueryBuilder(), cancellationToken)
        End Function

        ''' <summary>Gets every ROR record matching <paramref name="query"/>. Without a search phrase this can be a very large list.</summary>
        ''' <param name="query">Filters; <see cref="ListQuery.Count"/> sets the page size.</param>
        ''' <param name="maxItems">Stops after this many records.</param>
        ''' <param name="cancellationToken">Cancels the requests.</param>
        ''' <example>
        ''' <code>
        ''' Dim all = Await ojs.Rors.ListAllAsync(New SearchQuery With {.SearchPhrase = "Indonesia"})
        ''' For Each item In all : Console.WriteLine(item.Name.GetValue(item.DisplayLocale)) : Next
        ''' </code>
        ''' </example>
        Public Function ListAllAsync(Optional query As SearchQuery = Nothing, Optional maxItems As Integer? = Nothing,
                                     Optional cancellationToken As CancellationToken = Nothing) As Task(Of List(Of Ror))
            Return ListAllCoreAsync(Of Ror, SearchQuery)(If(query, New SearchQuery()), AddressOf ListAsync, maxItems, cancellationToken)
        End Function

        ''' <summary>Streams every ROR record matching <paramref name="query"/> page by page.</summary>
        ''' <param name="query">Filters; <see cref="ListQuery.Count"/> sets the page size.</param>
        ''' <example>
        ''' <code>
        ''' ' C#: await foreach (var item in ojs.Rors.Enumerate()) Console.WriteLine(item.Name.GetValue(item.DisplayLocale));
        ''' ' VB has no Await For Each; use the enumerator (or ListAllAsync):
        ''' Dim e = ojs.Rors.Enumerate(New SearchQuery With {.SearchPhrase = "Indonesia"}).GetAsyncEnumerator()
        ''' While Await e.MoveNextAsync()
        '''     Console.WriteLine(e.Current.Name.GetValue(e.Current.DisplayLocale))
        ''' End While
        ''' Await e.DisposeAsync()
        ''' </code>
        ''' </example>
        Public Function Enumerate(Optional query As SearchQuery = Nothing) As IAsyncEnumerable(Of Ror)
            Return EnumerateCore(Of Ror, SearchQuery)(If(query, New SearchQuery()), AddressOf ListAsync)
        End Function

        ''' <summary>Gets a ROR record.</summary>
        ''' <param name="rorId">The local record ID (not the ROR URL).</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim ror = Await ojs.Rors.GetAsync(1)
        ''' Console.WriteLine(ror.Name.GetValue(ror.DisplayLocale))
        ''' </code>
        ''' </example>
        Public Function GetAsync(rorId As Integer, Optional cancellationToken As CancellationToken = Nothing) As Task(Of Ror)
            Return Http.GetAsync(Of Ror)($"rors/{rorId}", Nothing, cancellationToken)
        End Function

        ''' <summary>Adds a ROR record, or updates the record with the same <see cref="RorEdit.Ror"/>.</summary>
        ''' <param name="ror">The record.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <returns>The saved record.</returns>
        ''' <example>
        ''' <code>
        ''' Await ojs.Rors.SaveAsync(New RorEdit With {
        '''     .Ror = "https://ror.org/05ezgz346", .DisplayLocale = "en", .IsActive = True,
        '''     .Name = LocalizedString.Create("en", "Example University")})
        ''' </code>
        ''' </example>
        Public Function SaveAsync(ror As RorEdit, Optional cancellationToken As CancellationToken = Nothing) As Task(Of Ror)
            ArgumentNullException.ThrowIfNull(ror)
            Return Http.PostAsync(Of Ror)("rors", ror, cancellationToken)
        End Function

    End Class

End Namespace
