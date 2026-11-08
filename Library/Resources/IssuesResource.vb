Imports System.Threading
Imports OJS3_5.Library.Http
Imports OJS3_5.Library.Models
Imports OJS3_5.Library.Paging
Imports OJS3_5.Library.Queries

Namespace Resources

    ''' <summary><c>/issues</c> — journal issues.</summary>
    ''' <example>
    ''' <code>
    ''' Dim current = Await ojs.Issues.GetCurrentAsync()
    ''' For Each article In current.Articles
    '''     Console.WriteLine(article.CurrentPublication?.Title.GetValue("en"))
    ''' Next
    ''' </code>
    ''' </example>
    Public NotInheritable Class IssuesResource
        Inherits ResourceBase

        Friend Sub New(http As OjsHttp)
            MyBase.New(http)
        End Sub

        ''' <summary>Gets one page of issues. Items contain the summary properties only.</summary>
        ''' <param name="query">Filters and paging; <c>Nothing</c> for the first page.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim page = Await ojs.Issues.ListAsync(New IssueQuery With {.IsPublished = True, .Years = {2026}})
        ''' Console.WriteLine($"{page.ItemsMax} in total")
        ''' For Each item In page.Items : Console.WriteLine(item.Identification) : Next
        ''' </code>
        ''' </example>
        Public Function ListAsync(Optional query As IssueQuery = Nothing,
                                  Optional cancellationToken As CancellationToken = Nothing) As Task(Of PagedResult(Of Issue))
            Return Http.GetAsync(Of PagedResult(Of Issue))("issues", If(query, New IssueQuery()).ToQueryBuilder(), cancellationToken)
        End Function

        ''' <summary>Gets every issue matching <paramref name="query"/>.</summary>
        ''' <param name="query">Filters and paging; <c>Nothing</c> for the first page.</param>
        ''' <param name="maxItems">Stops after this many items.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim all = Await ojs.Issues.ListAllAsync(New IssueQuery With {.IsPublished = True})
        ''' For Each item In all : Console.WriteLine(item.Identification) : Next
        ''' </code>
        ''' </example>
        Public Function ListAllAsync(Optional query As IssueQuery = Nothing, Optional maxItems As Integer? = Nothing,
                                     Optional cancellationToken As CancellationToken = Nothing) As Task(Of List(Of Issue))
            Return ListAllCoreAsync(Of Issue, IssueQuery)(If(query, New IssueQuery()), AddressOf ListAsync, maxItems, cancellationToken)
        End Function

        ''' <summary>Streams every issue matching <paramref name="query"/> page by page.</summary>
        ''' <param name="query">Filters and paging; <c>Nothing</c> for the first page.</param>
        ''' <example>
        ''' <code>
        ''' ' C#: await foreach (var item in ojs.Issues.Enumerate()) Console.WriteLine(item.Identification);
        ''' ' VB has no Await For Each; use the enumerator (or ListAllAsync):
        ''' Dim e = ojs.Issues.Enumerate().GetAsyncEnumerator()
        ''' While Await e.MoveNextAsync()
        '''     Console.WriteLine(e.Current.Identification)
        ''' End While
        ''' Await e.DisposeAsync()
        ''' </code>
        ''' </example>
        Public Function Enumerate(Optional query As IssueQuery = Nothing) As IAsyncEnumerable(Of Issue)
            Return EnumerateCore(Of Issue, IssueQuery)(If(query, New IssueQuery()), AddressOf ListAsync)
        End Function

        ''' <summary>Gets an issue with all its properties, including its articles.</summary>
        ''' <param name="issueId">The issue ID.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim issue = Await ojs.Issues.GetAsync(4)
        ''' For Each a In issue.Articles : Console.WriteLine(a.CurrentPublication?.Title.GetValue("en")) : Next
        ''' </code>
        ''' </example>
        Public Function GetAsync(issueId As Integer, Optional cancellationToken As CancellationToken = Nothing) As Task(Of Issue)
            Return Http.GetAsync(Of Issue)($"issues/{issueId}", Nothing, cancellationToken)
        End Function

        ''' <summary>Gets the journal's current issue.</summary>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim current = Await ojs.Issues.GetCurrentAsync()
        ''' Console.WriteLine($"{current.Identification}: {current.Articles.Count} articles")
        ''' </code>
        ''' </example>
        Public Function GetCurrentAsync(Optional cancellationToken As CancellationToken = Nothing) As Task(Of Issue)
            Return Http.GetAsync(Of Issue)("issues/current", Nothing, cancellationToken)
        End Function

    End Class

    ''' <summary><c>/sections</c> — journal sections (OJS 3.5+).</summary>
    ''' <example>
    ''' <code>
    ''' For Each s In Await ojs.Sections.ListAllAsync()
    '''     Console.WriteLine($"{s.Id}: {s.Title.GetValue("en")}")
    ''' Next
    ''' </code>
    ''' </example>
    Public NotInheritable Class SectionsResource
        Inherits ResourceBase

        Friend Sub New(http As OjsHttp)
            MyBase.New(http)
        End Sub

        ''' <summary>Gets one page of sections.</summary>
        ''' <param name="query">Filters and paging; <c>Nothing</c> for the first page.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim page = Await ojs.Sections.ListAsync(New SectionQuery With {.IsInactive = False})
        ''' Console.WriteLine($"{page.ItemsMax} in total")
        ''' For Each item In page.Items : Console.WriteLine(item.Title.GetValue("en")) : Next
        ''' </code>
        ''' </example>
        Public Function ListAsync(Optional query As SectionQuery = Nothing,
                                  Optional cancellationToken As CancellationToken = Nothing) As Task(Of PagedResult(Of Section))
            Return Http.GetAsync(Of PagedResult(Of Section))("sections", If(query, New SectionQuery()).ToQueryBuilder(), cancellationToken)
        End Function

        ''' <summary>Gets every section matching <paramref name="query"/>.</summary>
        ''' <param name="query">Filters and paging; <c>Nothing</c> for the first page.</param>
        ''' <param name="maxItems">Stops after this many items.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim all = Await ojs.Sections.ListAllAsync()
        ''' For Each item In all : Console.WriteLine(item.Title.GetValue("en")) : Next
        ''' </code>
        ''' </example>
        Public Function ListAllAsync(Optional query As SectionQuery = Nothing, Optional maxItems As Integer? = Nothing,
                                     Optional cancellationToken As CancellationToken = Nothing) As Task(Of List(Of Section))
            Return ListAllCoreAsync(Of Section, SectionQuery)(If(query, New SectionQuery()), AddressOf ListAsync, maxItems, cancellationToken)
        End Function

        ''' <summary>Gets a section.</summary>
        ''' <param name="sectionId">The section ID.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim section = Await ojs.Sections.GetAsync(1)
        ''' Console.WriteLine(section.Policy.GetValue("en"))
        ''' </code>
        ''' </example>
        Public Function GetAsync(sectionId As Integer, Optional cancellationToken As CancellationToken = Nothing) As Task(Of Section)
            Return Http.GetAsync(Of Section)($"sections/{sectionId}", Nothing, cancellationToken)
        End Function

    End Class

End Namespace
