Imports System.Threading
Imports Library.Http
Imports Library.Models
Imports Library.Paging
Imports Library.Queries

Namespace Resources

    ''' <summary><c>/users</c> — user accounts of the journal, and reviewers.</summary>
    ''' <remarks>Most of these endpoints require a journal manager or site administrator token.</remarks>
    ''' <example>
    ''' <code>
    ''' Dim page = Await ojs.Users.ListAsync(New UserQuery With {.RoleIds = {UserRole.Reviewer}, .Count = 50})
    ''' For Each u In page.Items
    '''     Console.WriteLine($"{u.FullName} &lt;{u.Email}&gt;")
    ''' Next
    ''' </code>
    ''' </example>
    Public NotInheritable Class UsersResource
        Inherits ResourceBase

        Friend Sub New(http As OjsHttp)
            MyBase.New(http)
        End Sub

        ''' <summary>Gets one page of users. Items contain the summary properties only.</summary>
        ''' <param name="query">Filters and paging; <c>Nothing</c> for the first page of active users.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim page = Await ojs.Users.ListAsync(New UserQuery With {.RoleIds = {UserRole.Reviewer}, .SearchPhrase = "biology"})
        ''' Console.WriteLine($"{page.ItemsMax} in total")
        ''' For Each item In page.Items : Console.WriteLine(item.FullName) : Next
        ''' </code>
        ''' </example>
        Public Function ListAsync(Optional query As UserQuery = Nothing,
                                  Optional cancellationToken As CancellationToken = Nothing) As Task(Of PagedResult(Of User))
            Return Http.GetAsync(Of PagedResult(Of User))("users", If(query, New UserQuery()).ToQueryBuilder(), cancellationToken)
        End Function

        ''' <summary>Gets every user matching <paramref name="query"/>, requesting as many pages as needed.</summary>
        ''' <param name="query">Filters; <see cref="ListQuery.Count"/> sets the page size.</param>
        ''' <param name="maxItems">Stops after this many users.</param>
        ''' <param name="cancellationToken">Cancels the requests.</param>
        ''' <example>
        ''' <code>
        ''' Dim all = Await ojs.Users.ListAllAsync(New UserQuery With {.RoleIds = {UserRole.Reviewer}, .SearchPhrase = "biology"})
        ''' For Each item In all : Console.WriteLine(item.FullName) : Next
        ''' </code>
        ''' </example>
        Public Function ListAllAsync(Optional query As UserQuery = Nothing, Optional maxItems As Integer? = Nothing,
                                     Optional cancellationToken As CancellationToken = Nothing) As Task(Of List(Of User))
            Return ListAllCoreAsync(Of User, UserQuery)(If(query, New UserQuery()), AddressOf ListAsync, maxItems, cancellationToken)
        End Function

        ''' <summary>Streams every user matching <paramref name="query"/> page by page (C#: <c>await foreach</c>).</summary>
        ''' <param name="query">Filters; <see cref="ListQuery.Count"/> sets the page size.</param>
        ''' <example>
        ''' <code>
        ''' ' C#: await foreach (var item in ojs.Users.Enumerate()) Console.WriteLine(item.FullName);
        ''' ' VB has no Await For Each; use the enumerator (or ListAllAsync):
        ''' Dim e = ojs.Users.Enumerate(New UserQuery With {.RoleIds = {UserRole.Reviewer}, .SearchPhrase = "biology"}).GetAsyncEnumerator()
        ''' While Await e.MoveNextAsync()
        '''     Console.WriteLine(e.Current.FullName)
        ''' End While
        ''' Await e.DisposeAsync()
        ''' </code>
        ''' </example>
        Public Function Enumerate(Optional query As UserQuery = Nothing) As IAsyncEnumerable(Of User)
            Return EnumerateCore(Of User, UserQuery)(If(query, New UserQuery()), AddressOf ListAsync)
        End Function

        ''' <summary>Gets a user with all their properties.</summary>
        ''' <param name="userId">The user ID.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim user = Await ojs.Users.GetAsync(7)
        ''' For Each g In user.Groups : Console.WriteLine(g.Name.GetValue("en")) : Next
        ''' </code>
        ''' </example>
        Public Function GetAsync(userId As Integer, Optional cancellationToken As CancellationToken = Nothing) As Task(Of User)
            Return Http.GetAsync(Of User)($"users/{userId}", Nothing, cancellationToken)
        End Function

        ''' <summary>Ends a user's membership of a user group (role) in this journal.</summary>
        ''' <param name="userId">The user ID.</param>
        ''' <param name="userGroupId">The user group to remove the user from.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <returns>The updated user.</returns>
        ''' <example>
        ''' <code>
        ''' ' Remove user 7 from user group 14.
        ''' Await ojs.Users.EndRoleAsync(7, 14)
        ''' </code>
        ''' </example>
        Public Function EndRoleAsync(userId As Integer, userGroupId As Integer,
                                     Optional cancellationToken As CancellationToken = Nothing) As Task(Of User)
            Return Http.PutAsync(Of User)($"users/{userId}/endRole/{userGroupId}", Nothing, cancellationToken)
        End Function

        ''' <summary>Gets one page of reviewers with their review statistics.</summary>
        ''' <param name="query">Filters and paging; <c>Nothing</c> for the first page.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim page = Await ojs.Users.ListReviewersAsync(New ReviewerQuery With {.ReviewsActive = "0-1", .ReviewerRating = 4})
        ''' For Each r In page.Items : Console.WriteLine($"{r.FullName}: {r.ReviewsActive} active") : Next
        ''' </code>
        ''' </example>
        Public Function ListReviewersAsync(Optional query As ReviewerQuery = Nothing,
                                           Optional cancellationToken As CancellationToken = Nothing) As Task(Of PagedResult(Of ReviewerSummary))
            Return Http.GetAsync(Of PagedResult(Of ReviewerSummary))("users/reviewers", If(query, New ReviewerQuery()).ToQueryBuilder(), cancellationToken)
        End Function

        ''' <summary>Gets every reviewer matching <paramref name="query"/>.</summary>
        ''' <param name="query">Filters; <see cref="ListQuery.Count"/> sets the page size.</param>
        ''' <param name="maxItems">Stops after this many reviewers.</param>
        ''' <param name="cancellationToken">Cancels the requests.</param>
        ''' <example>
        ''' <code>
        ''' Dim all = Await ojs.Users.ListAllReviewersAsync(New ReviewerQuery With {.ReviewsActive = "0-1", .ReviewerRating = 4})
        ''' For Each item In all : Console.WriteLine(item.FullName) : Next
        ''' </code>
        ''' </example>
        Public Function ListAllReviewersAsync(Optional query As ReviewerQuery = Nothing, Optional maxItems As Integer? = Nothing,
                                              Optional cancellationToken As CancellationToken = Nothing) As Task(Of List(Of ReviewerSummary))
            Return ListAllCoreAsync(Of ReviewerSummary, ReviewerQuery)(If(query, New ReviewerQuery()), AddressOf ListReviewersAsync, maxItems, cancellationToken)
        End Function

        ''' <summary>Streams every reviewer matching <paramref name="query"/> page by page.</summary>
        ''' <param name="query">Filters; <see cref="ListQuery.Count"/> sets the page size.</param>
        ''' <example>
        ''' <code>
        ''' ' C#: await foreach (var item in ojs.Users.EnumerateReviewers()) Console.WriteLine(item.FullName);
        ''' ' VB has no Await For Each; use the enumerator (or ListAllAsync):
        ''' Dim e = ojs.Users.EnumerateReviewers().GetAsyncEnumerator()
        ''' While Await e.MoveNextAsync()
        '''     Console.WriteLine(e.Current.FullName)
        ''' End While
        ''' Await e.DisposeAsync()
        ''' </code>
        ''' </example>
        Public Function EnumerateReviewers(Optional query As ReviewerQuery = Nothing) As IAsyncEnumerable(Of ReviewerSummary)
            Return EnumerateCore(Of ReviewerSummary, ReviewerQuery)(If(query, New ReviewerQuery()), AddressOf ListReviewersAsync)
        End Function

    End Class

End Namespace
