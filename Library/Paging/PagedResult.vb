Namespace Paging

    ''' <summary>
    ''' One page of results from an OJS list endpoint: <c>{"itemsMax": 120, "items": [...]}</c>.
    ''' </summary>
    ''' <typeparam name="T">The item type.</typeparam>
    ''' <example>
    ''' <code>
    ''' Dim page = Await ojs.Submissions.ListAsync(New SubmissionQuery With {.Count = 50, .Offset = 100})
    ''' Console.WriteLine($"Showing {page.Items.Count} of {page.ItemsMax}")
    ''' </code>
    ''' </example>
    Public Class PagedResult(Of T)

        ''' <summary>The total number of items matching the query across all pages.</summary>
        Public Property ItemsMax As Integer

        ''' <summary>The items in this page.</summary>
        Public Property Items As List(Of T) = New List(Of T)()

    End Class

End Namespace
