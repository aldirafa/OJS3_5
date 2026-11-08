Imports System.Threading

Namespace Paging

    ''' <summary>
    ''' Walks through every page of an OJS list endpoint using <c>count</c>/<c>offset</c>.
    ''' </summary>
    ''' <remarks>
    ''' VB.NET cannot <c>Await For Each</c>, so VB callers usually use <see cref="GetAllAsync"/> or the
    ''' <c>ListAllAsync</c> methods on each resource. C# callers can use <see cref="Enumerate"/> with
    ''' <c>await foreach</c> to stream items page by page.
    ''' </remarks>
    ''' <example>
    ''' <code>
    ''' ' VB: collect everything (optionally capped)
    ''' Dim all = Await ojs.Issues.ListAllAsync(New IssueQuery With {.IsPublished = True}, maxItems:=500)
    '''
    ''' ' C#: stream page by page
    ''' ' await foreach (var issue in ojs.Issues.Enumerate(new IssueQuery { IsPublished = true })) Console.WriteLine(issue.Identification);
    ''' </code>
    ''' </example>
    Public NotInheritable Class Pager

        ''' <summary>The maximum page size accepted by most OJS endpoints.</summary>
        Public Const MaxPageSize As Integer = 100

        Private Sub New()
        End Sub

        ''' <summary>Fetches every page and returns all items.</summary>
        ''' <param name="fetchPage">Fetches the page starting at the given offset.</param>
        ''' <param name="maxItems">Optional cap on the number of items returned.</param>
        ''' <param name="cancellationToken">Cancels the operation.</param>
        ''' <typeparam name="T">The type to bind the response to.</typeparam>
        Public Shared Async Function GetAllAsync(Of T)(fetchPage As Func(Of Integer, CancellationToken, Task(Of PagedResult(Of T))),
                                                       Optional maxItems As Integer? = Nothing,
                                                       Optional cancellationToken As CancellationToken = Nothing) As Task(Of List(Of T))
            ArgumentNullException.ThrowIfNull(fetchPage)
            Dim result As New List(Of T)()
            Dim offset = 0
            Do
                cancellationToken.ThrowIfCancellationRequested()
                Dim page = Await fetchPage(offset, cancellationToken).ConfigureAwait(False)
                If page Is Nothing OrElse page.Items Is Nothing OrElse page.Items.Count = 0 Then Exit Do
                For Each item In page.Items
                    result.Add(item)
                    If maxItems.HasValue AndAlso result.Count >= maxItems.Value Then Return result
                Next
                offset += page.Items.Count
                If offset >= page.ItemsMax Then Exit Do
            Loop
            Return result
        End Function

        ''' <summary>Returns an <see cref="IAsyncEnumerable(Of T)"/> that fetches pages lazily.</summary>
        ''' <param name="fetchPage">Fetches the page starting at the given offset.</param>
        ''' <typeparam name="T">The type to bind the response to.</typeparam>
        Public Shared Function Enumerate(Of T)(fetchPage As Func(Of Integer, CancellationToken, Task(Of PagedResult(Of T)))) As IAsyncEnumerable(Of T)
            ArgumentNullException.ThrowIfNull(fetchPage)
            Return New PagedAsyncEnumerable(Of T)(fetchPage)
        End Function

        Private NotInheritable Class PagedAsyncEnumerable(Of TItem)
            Implements IAsyncEnumerable(Of TItem)

            Private ReadOnly _fetch As Func(Of Integer, CancellationToken, Task(Of PagedResult(Of TItem)))

            Public Sub New(fetch As Func(Of Integer, CancellationToken, Task(Of PagedResult(Of TItem))))
                _fetch = fetch
            End Sub

            ''' <summary>Starts a new enumeration from the first page.</summary>
            ''' <param name="cancellationToken">Cancels the page requests.</param>
            Public Function GetAsyncEnumerator(Optional cancellationToken As CancellationToken = Nothing) As IAsyncEnumerator(Of TItem) _
                Implements IAsyncEnumerable(Of TItem).GetAsyncEnumerator
                Return New Enumerator(_fetch, cancellationToken)
            End Function

            Private NotInheritable Class Enumerator
                Implements IAsyncEnumerator(Of TItem)

                Private ReadOnly _fetch As Func(Of Integer, CancellationToken, Task(Of PagedResult(Of TItem)))
                Private ReadOnly _ct As CancellationToken
                Private _buffer As List(Of TItem)
                Private _index As Integer = -1
                Private _offset As Integer
                Private _total As Integer = Integer.MaxValue
                Private _current As TItem

                Public Sub New(fetch As Func(Of Integer, CancellationToken, Task(Of PagedResult(Of TItem))), ct As CancellationToken)
                    _fetch = fetch
                    _ct = ct
                End Sub

                ''' <summary>The current item.</summary>
                Public ReadOnly Property Current As TItem Implements IAsyncEnumerator(Of TItem).Current
                    Get
                        Return _current
                    End Get
                End Property

                ''' <summary>Moves to the next item, fetching the next page when needed.</summary>
                Public Function MoveNextAsync() As ValueTask(Of Boolean) Implements IAsyncEnumerator(Of TItem).MoveNextAsync
                    If _buffer IsNot Nothing AndAlso _index + 1 < _buffer.Count Then
                        _index += 1
                        _current = _buffer(_index)
                        Return New ValueTask(Of Boolean)(True)
                    End If
                    Return New ValueTask(Of Boolean)(FetchNextAsync())
                End Function

                Private Async Function FetchNextAsync() As Task(Of Boolean)
                    If _buffer IsNot Nothing AndAlso (_buffer.Count = 0 OrElse _offset >= _total) Then Return False
                    _ct.ThrowIfCancellationRequested()
                    Dim page = Await _fetch(_offset, _ct).ConfigureAwait(False)
                    _buffer = If(page?.Items, New List(Of TItem)())
                    _total = If(page Is Nothing, 0, page.ItemsMax)
                    _offset += _buffer.Count
                    _index = -1
                    If _buffer.Count = 0 Then Return False
                    _index = 0
                    _current = _buffer(0)
                    Return True
                End Function

                ''' <summary>Ends the enumeration.</summary>
                Public Function DisposeAsync() As ValueTask Implements IAsyncDisposable.DisposeAsync
                    Return ValueTask.CompletedTask
                End Function

            End Class

        End Class

    End Class

End Namespace
