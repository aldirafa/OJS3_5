Imports System.Threading
Imports OJS3_5.Library.Http
Imports OJS3_5.Library.Models
Imports OJS3_5.Library.Paging
Imports OJS3_5.Library.Queries

Namespace Resources

    ''' <summary><c>/announcements</c> — news items of the journal (or of the site, for a site-wide client).</summary>
    ''' <example>
    ''' <code>
    ''' Dim created = Await ojs.Announcements.CreateAsync(New AnnouncementEdit With {
    '''     .Title = LocalizedString.Create("en", "Call for papers"),
    '''     .Description = LocalizedString.Create("en", "&lt;p&gt;Submissions are open.&lt;/p&gt;")})
    ''' Console.WriteLine(created.Url)
    ''' </code>
    ''' </example>
    Public NotInheritable Class AnnouncementsResource
        Inherits ResourceBase

        Friend Sub New(http As OjsHttp)
            MyBase.New(http)
        End Sub

        ''' <summary>Gets one page of announcements, newest first.</summary>
        ''' <param name="query">Filters and paging; <c>Nothing</c> for the first page.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim page = Await ojs.Announcements.ListAsync(New AnnouncementQuery With {.SearchPhrase = "call"})
        ''' Console.WriteLine($"{page.ItemsMax} in total")
        ''' For Each item In page.Items : Console.WriteLine(item.Title.GetValue("en")) : Next
        ''' </code>
        ''' </example>
        Public Function ListAsync(Optional query As AnnouncementQuery = Nothing,
                                  Optional cancellationToken As CancellationToken = Nothing) As Task(Of PagedResult(Of Announcement))
            Return Http.GetAsync(Of PagedResult(Of Announcement))("announcements", If(query, New AnnouncementQuery()).ToQueryBuilder(), cancellationToken)
        End Function

        ''' <summary>Gets every announcement matching <paramref name="query"/>.</summary>
        ''' <param name="query">Filters; <see cref="ListQuery.Count"/> sets the page size.</param>
        ''' <param name="maxItems">Stops after this many announcements.</param>
        ''' <param name="cancellationToken">Cancels the requests.</param>
        ''' <example>
        ''' <code>
        ''' Dim all = Await ojs.Announcements.ListAllAsync(New AnnouncementQuery With {.SearchPhrase = "call"})
        ''' For Each item In all : Console.WriteLine(item.Title.GetValue("en")) : Next
        ''' </code>
        ''' </example>
        Public Function ListAllAsync(Optional query As AnnouncementQuery = Nothing, Optional maxItems As Integer? = Nothing,
                                     Optional cancellationToken As CancellationToken = Nothing) As Task(Of List(Of Announcement))
            Return ListAllCoreAsync(Of Announcement, AnnouncementQuery)(If(query, New AnnouncementQuery()), AddressOf ListAsync, maxItems, cancellationToken)
        End Function

        ''' <summary>Streams every announcement matching <paramref name="query"/> page by page.</summary>
        ''' <param name="query">Filters; <see cref="ListQuery.Count"/> sets the page size.</param>
        ''' <example>
        ''' <code>
        ''' ' C#: await foreach (var item in ojs.Announcements.Enumerate()) Console.WriteLine(item.Title.GetValue("en"));
        ''' ' VB has no Await For Each; use the enumerator (or ListAllAsync):
        ''' Dim e = ojs.Announcements.Enumerate(New AnnouncementQuery With {.SearchPhrase = "call"}).GetAsyncEnumerator()
        ''' While Await e.MoveNextAsync()
        '''     Console.WriteLine(e.Current.Title.GetValue("en"))
        ''' End While
        ''' Await e.DisposeAsync()
        ''' </code>
        ''' </example>
        Public Function Enumerate(Optional query As AnnouncementQuery = Nothing) As IAsyncEnumerable(Of Announcement)
            Return EnumerateCore(Of Announcement, AnnouncementQuery)(If(query, New AnnouncementQuery()), AddressOf ListAsync)
        End Function

        ''' <summary>Gets an announcement.</summary>
        ''' <param name="announcementId">The announcement ID.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim announcement = Await ojs.Announcements.GetAsync(3)
        ''' Console.WriteLine(announcement.Title.GetValue("en"))
        ''' </code>
        ''' </example>
        Public Function GetAsync(announcementId As Integer, Optional cancellationToken As CancellationToken = Nothing) As Task(Of Announcement)
            Return Http.GetAsync(Of Announcement)($"announcements/{announcementId}", Nothing, cancellationToken)
        End Function

        ''' <summary>Creates an announcement.</summary>
        ''' <param name="announcement">The announcement. <c>Title</c> is required.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <returns>The new announcement.</returns>
        ''' <example>
        ''' <code>
        ''' Dim created = Await ojs.Announcements.CreateAsync(New AnnouncementEdit With {.Title = LocalizedString.Create("en", "Call for papers"), .DateExpire = New DateOnly(2027, 6, 30)})
        ''' Console.WriteLine(created.Id)
        ''' </code>
        ''' </example>
        Public Function CreateAsync(announcement As AnnouncementEdit, Optional cancellationToken As CancellationToken = Nothing) As Task(Of Announcement)
            ArgumentNullException.ThrowIfNull(announcement)
            Return Http.PostAsync(Of Announcement)("announcements", announcement, cancellationToken)
        End Function

        ''' <summary>Changes an announcement. Only the properties you set are changed.</summary>
        ''' <param name="announcementId">The announcement ID.</param>
        ''' <param name="changes">The properties to change.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <returns>The updated announcement.</returns>
        ''' <example>
        ''' <code>
        ''' Dim updated = Await ojs.Announcements.UpdateAsync(3, New AnnouncementEdit With {.DescriptionShort = LocalizedString.Create("en", "Deadline extended.")})
        ''' </code>
        ''' </example>
        Public Function UpdateAsync(announcementId As Integer, changes As AnnouncementEdit,
                                    Optional cancellationToken As CancellationToken = Nothing) As Task(Of Announcement)
            ArgumentNullException.ThrowIfNull(changes)
            Return Http.PutAsync(Of Announcement)($"announcements/{announcementId}", changes, cancellationToken)
        End Function

        ''' <summary>Deletes an announcement.</summary>
        ''' <param name="announcementId">The announcement ID.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <returns>The deleted announcement.</returns>
        ''' <example>
        ''' <code>
        ''' Await ojs.Announcements.DeleteAsync(3)
        ''' </code>
        ''' </example>
        Public Function DeleteAsync(announcementId As Integer, Optional cancellationToken As CancellationToken = Nothing) As Task(Of Announcement)
            Return Http.DeleteAsync(Of Announcement)($"announcements/{announcementId}", Nothing, Nothing, cancellationToken)
        End Function

    End Class

    ''' <summary><c>/institutions</c> — institutions used for usage statistics and subscriptions.</summary>
    ''' <example>
    ''' <code>
    ''' Dim found = Await ojs.Institutions.ListAsync(New SearchQuery With {.SearchPhrase = "university"})
    ''' Console.WriteLine($"{found.ItemsMax} institutions")
    ''' </code>
    ''' </example>
    Public NotInheritable Class InstitutionsResource
        Inherits ResourceBase

        Friend Sub New(http As OjsHttp)
            MyBase.New(http)
        End Sub

        ''' <summary>Gets one page of institutions.</summary>
        ''' <param name="query">Filters and paging; <c>Nothing</c> for the first page.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim page = Await ojs.Institutions.ListAsync(New SearchQuery With {.SearchPhrase = "university"})
        ''' Console.WriteLine($"{page.ItemsMax} in total")
        ''' For Each item In page.Items : Console.WriteLine(item.Name.GetValue("en")) : Next
        ''' </code>
        ''' </example>
        Public Function ListAsync(Optional query As SearchQuery = Nothing,
                                  Optional cancellationToken As CancellationToken = Nothing) As Task(Of PagedResult(Of Institution))
            Return Http.GetAsync(Of PagedResult(Of Institution))("institutions", If(query, New SearchQuery()).ToQueryBuilder(), cancellationToken)
        End Function

        ''' <summary>Gets every institution matching <paramref name="query"/>.</summary>
        ''' <param name="query">Filters; <see cref="ListQuery.Count"/> sets the page size.</param>
        ''' <param name="maxItems">Stops after this many institutions.</param>
        ''' <param name="cancellationToken">Cancels the requests.</param>
        ''' <example>
        ''' <code>
        ''' Dim all = Await ojs.Institutions.ListAllAsync(New SearchQuery With {.SearchPhrase = "university"})
        ''' For Each item In all : Console.WriteLine(item.Name.GetValue("en")) : Next
        ''' </code>
        ''' </example>
        Public Function ListAllAsync(Optional query As SearchQuery = Nothing, Optional maxItems As Integer? = Nothing,
                                     Optional cancellationToken As CancellationToken = Nothing) As Task(Of List(Of Institution))
            Return ListAllCoreAsync(Of Institution, SearchQuery)(If(query, New SearchQuery()), AddressOf ListAsync, maxItems, cancellationToken)
        End Function

        ''' <summary>Streams every institution matching <paramref name="query"/> page by page.</summary>
        ''' <param name="query">Filters; <see cref="ListQuery.Count"/> sets the page size.</param>
        ''' <example>
        ''' <code>
        ''' ' C#: await foreach (var item in ojs.Institutions.Enumerate()) Console.WriteLine(item.Name.GetValue("en"));
        ''' ' VB has no Await For Each; use the enumerator (or ListAllAsync):
        ''' Dim e = ojs.Institutions.Enumerate(New SearchQuery With {.SearchPhrase = "university"}).GetAsyncEnumerator()
        ''' While Await e.MoveNextAsync()
        '''     Console.WriteLine(e.Current.Name.GetValue("en"))
        ''' End While
        ''' Await e.DisposeAsync()
        ''' </code>
        ''' </example>
        Public Function Enumerate(Optional query As SearchQuery = Nothing) As IAsyncEnumerable(Of Institution)
            Return EnumerateCore(Of Institution, SearchQuery)(If(query, New SearchQuery()), AddressOf ListAsync)
        End Function

        ''' <summary>Gets an institution.</summary>
        ''' <param name="institutionId">The institution ID.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim institution = Await ojs.Institutions.GetAsync(2)
        ''' Console.WriteLine(institution.Name.GetValue("en"))
        ''' </code>
        ''' </example>
        Public Function GetAsync(institutionId As Integer, Optional cancellationToken As CancellationToken = Nothing) As Task(Of Institution)
            Return Http.GetAsync(Of Institution)($"institutions/{institutionId}", Nothing, cancellationToken)
        End Function

        ''' <summary>Creates an institution.</summary>
        ''' <param name="institution">The institution. <c>Name</c> is required.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <returns>The new institution.</returns>
        ''' <example>
        ''' <code>
        ''' Dim created = Await ojs.Institutions.CreateAsync(New InstitutionEdit With {.Name = LocalizedString.Create("en", "Example University"), .IpRanges = New List(Of String) From {"192.0.2.0/24"}})
        ''' Console.WriteLine(created.Id)
        ''' </code>
        ''' </example>
        Public Function CreateAsync(institution As InstitutionEdit, Optional cancellationToken As CancellationToken = Nothing) As Task(Of Institution)
            ArgumentNullException.ThrowIfNull(institution)
            Return Http.PostAsync(Of Institution)("institutions", institution, cancellationToken)
        End Function

        ''' <summary>Changes an institution. Only the properties you set are changed.</summary>
        ''' <param name="institutionId">The institution ID.</param>
        ''' <param name="changes">The properties to change.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <returns>The updated institution.</returns>
        ''' <example>
        ''' <code>
        ''' Dim updated = Await ojs.Institutions.UpdateAsync(2, New InstitutionEdit With {.Ror = "https://ror.org/05ezgz346"})
        ''' </code>
        ''' </example>
        Public Function UpdateAsync(institutionId As Integer, changes As InstitutionEdit,
                                    Optional cancellationToken As CancellationToken = Nothing) As Task(Of Institution)
            ArgumentNullException.ThrowIfNull(changes)
            Return Http.PutAsync(Of Institution)($"institutions/{institutionId}", changes, cancellationToken)
        End Function

        ''' <summary>Deletes an institution.</summary>
        ''' <param name="institutionId">The institution ID.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <returns>The deleted institution.</returns>
        ''' <example>
        ''' <code>
        ''' Await ojs.Institutions.DeleteAsync(2)
        ''' </code>
        ''' </example>
        Public Function DeleteAsync(institutionId As Integer, Optional cancellationToken As CancellationToken = Nothing) As Task(Of Institution)
            Return Http.DeleteAsync(Of Institution)($"institutions/{institutionId}", Nothing, Nothing, cancellationToken)
        End Function

    End Class

End Namespace
