Imports System.Threading
Imports OJS3_5.Library.Http
Imports OJS3_5.Library.Paging
Imports OJS3_5.Library.Queries

Namespace Resources

    ''' <summary>Base class for the API resource groups exposed by <see cref="OjsClient"/>.</summary>
    ''' <example>
    ''' <code>
    ''' ' Every API area is a ResourceBase exposed by OjsClient:
    ''' Dim submissions As ResourceBase = ojs.Submissions
    ''' </code>
    ''' </example>
    Public MustInherit Class ResourceBase

        Friend ReadOnly Http As OjsHttp

        Friend Sub New(http As OjsHttp)
            Me.Http = http
        End Sub

        ''' <summary>Fetches every page of a list endpoint.</summary>
        ''' <typeparam name="TItem">The type.</typeparam>
        ''' <typeparam name="TQuery">The type.</typeparam>
        ''' <param name="maxItems">Stops after this many items.</param>
        ''' <param name="ct">Cancels the request.</param>
        ''' <param name="query">The filters; paging is set by the helper.</param>
        ''' <param name="fetch">Fetches one page for a query.</param>
        Friend Function ListAllCoreAsync(Of TItem, TQuery As ListQuery)(query As TQuery,
                                                                          fetch As Func(Of TQuery, CancellationToken, Task(Of PagedResult(Of TItem))),
                                                                          maxItems As Integer?, ct As CancellationToken) As Task(Of List(Of TItem))
            Return Pager.GetAllAsync(Of TItem)(
                Function(offset, token) fetch(DirectCast(query.WithPage(offset, Nothing), TQuery), token),
                maxItems, ct)
        End Function

        ''' <summary>Streams every page of a list endpoint.</summary>
        ''' <typeparam name="TItem">The type.</typeparam>
        ''' <typeparam name="TQuery">The type.</typeparam>
        ''' <param name="query">The filters; paging is set by the helper.</param>
        ''' <param name="fetch">Fetches one page for a query.</param>
        Friend Function EnumerateCore(Of TItem, TQuery As ListQuery)(query As TQuery,
                                                                      fetch As Func(Of TQuery, CancellationToken, Task(Of PagedResult(Of TItem)))) As IAsyncEnumerable(Of TItem)
            Return Pager.Enumerate(Of TItem)(
                Function(offset, token) fetch(DirectCast(query.WithPage(offset, Nothing), TQuery), token))
        End Function

    End Class

End Namespace
