Imports OJS3_5.Library.Http

Namespace Queries

    ''' <summary>
    ''' Base class for list-endpoint filters. Provides the shared <c>count</c>/<c>offset</c> pagination parameters.
    ''' </summary>
    ''' <remarks>
    ''' To add a parameter that this library does not model yet, use <see cref="ExtraParameters"/>.
    ''' </remarks>
    ''' <example>
    ''' <code>
    ''' ' Every list query has Count, Offset and ExtraParameters.
    ''' Dim q As New IssueQuery With {.Count = 50, .Offset = 50,
    '''     .ExtraParameters = New Dictionary(Of String, String) From {{"myPluginFilter", "x"}}}
    ''' Dim secondPage = Await ojs.Issues.ListAsync(q)
    ''' </code>
    ''' </example>
    Public MustInherit Class ListQuery

        ''' <summary>How many results to return in a single request (OJS default <c>20</c>, max <c>100</c>).</summary>
        Public Property Count As Integer?

        ''' <summary>How many results to skip. A <c>Count</c> of 10 and <c>Offset</c> of 15 returns results 15–25.</summary>
        Public Property Offset As Integer?

        ''' <summary>Additional raw query parameters appended as-is (for plugin parameters or new OJS versions).</summary>
        Public Property ExtraParameters As IDictionary(Of String, String)

        ''' <summary>Writes this query's parameters into <paramref name="query"/>.</summary>
        ''' <param name="query">The query string builder to write to.</param>
        Protected Friend Overridable Sub AppendTo(query As QueryBuilder)
            query.Add("count", Count).Add("offset", Offset)
            If ExtraParameters IsNot Nothing Then
                For Each kv In ExtraParameters
                    query.Add(kv.Key, kv.Value)
                Next
            End If
        End Sub

        ''' <summary>Builds a <see cref="QueryBuilder"/> holding this query's parameters.</summary>
        Public Function ToQueryBuilder() As QueryBuilder
            Dim q As New QueryBuilder()
            AppendTo(q)
            Return q
        End Function

        ''' <summary>Returns a shallow copy with the given page window, used by the paging helpers.</summary>
        ''' <param name="offset">The number of items to skip.</param>
        ''' <param name="count">The page size; <c>Nothing</c> keeps the current one.</param>
        Friend Function WithPage(offset As Integer, count As Integer?) As ListQuery
            Dim copy = DirectCast(MemberwiseClone(), ListQuery)
            copy.Offset = offset
            copy.Count = If(count, If(Me.Count, Paging.Pager.MaxPageSize))
            Return copy
        End Function

    End Class

    ''' <summary>A query with only pagination parameters, for endpoints that have no other filters.</summary>
    ''' <example>
    ''' <code>
    ''' Dim q As New PageQuery With {.Count = 100}
    ''' </code>
    ''' </example>
    Public Class PageQuery
        Inherits ListQuery
    End Class

End Namespace
