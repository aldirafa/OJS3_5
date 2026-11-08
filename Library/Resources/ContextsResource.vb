Imports System.Collections.Concurrent
Imports System.Threading
Imports OJS3_5.Library.Http
Imports OJS3_5.Library.Models
Imports OJS3_5.Library.Paging
Imports OJS3_5.Library.Queries

Namespace Resources

    ''' <summary><c>/contexts</c> — the journals hosted on the installation.</summary>
    ''' <remarks>
    ''' Listing, reading, creating and deleting journals use the site-wide API (<c>/_/api/v1/contexts</c>), whatever
    ''' journal the client targets. OJS only accepts changes to a journal (settings and theme) through that journal's
    ''' own API, so <see cref="UpdateAsync"/> and <see cref="UpdateThemeAsync"/> look up the journal's path first
    ''' (once per journal) and send the change there. Creating and deleting journals requires a site administrator token.
    ''' </remarks>
    ''' <example>
    ''' <code>
    ''' For Each journal In Await ojs.Contexts.ListAllAsync(New ContextQuery With {.IsEnabled = True})
    '''     Dim client = ojs.ForJournal(journal.UrlPath)
    '''     Dim current = Await client.Issues.GetCurrentAsync()
    '''     Console.WriteLine($"{journal.Name.GetValue("en")}: {current.Identification}")
    ''' Next
    ''' </code>
    ''' </example>
    Public NotInheritable Class ContextsResource
        Inherits ResourceBase

        Private ReadOnly _paths As New ConcurrentDictionary(Of Integer, String)()

        Friend Sub New(http As OjsHttp)
            MyBase.New(http.ForSite())
        End Sub

        ''' <summary>Returns a transport for the journal's own API, where OJS accepts changes to it.</summary>
        Private Async Function JournalHttpAsync(contextId As Integer, ct As CancellationToken) As Task(Of OjsHttp)
            Dim path As String = Nothing
            If Not _paths.TryGetValue(contextId, path) Then
                Dim context = Await GetAsync(contextId, ct).ConfigureAwait(False)
                path = context.UrlPath
                If String.IsNullOrEmpty(path) Then Throw New InvalidOperationException($"Journal {contextId} has no URL path.")
                _paths(contextId) = path
            End If
            Return Http.ForJournal(path)
        End Function

        ''' <summary>Gets one page of journals. Items contain the summary properties only.</summary>
        ''' <param name="query">Filters and paging; <c>Nothing</c> for the first page.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim page = Await ojs.Contexts.ListAsync(New ContextQuery With {.IsEnabled = True})
        ''' Console.WriteLine($"{page.ItemsMax} in total")
        ''' For Each item In page.Items : Console.WriteLine(item.Name.GetValue("en")) : Next
        ''' </code>
        ''' </example>
        Public Function ListAsync(Optional query As ContextQuery = Nothing,
                                  Optional cancellationToken As CancellationToken = Nothing) As Task(Of PagedResult(Of Context))
            Return Http.GetAsync(Of PagedResult(Of Context))("contexts", If(query, New ContextQuery()).ToQueryBuilder(), cancellationToken)
        End Function

        ''' <summary>Gets every journal matching <paramref name="query"/>.</summary>
        ''' <param name="query">Filters; <see cref="ListQuery.Count"/> sets the page size.</param>
        ''' <param name="maxItems">Stops after this many journals.</param>
        ''' <param name="cancellationToken">Cancels the requests.</param>
        ''' <example>
        ''' <code>
        ''' Dim all = Await ojs.Contexts.ListAllAsync(New ContextQuery With {.IsEnabled = True})
        ''' For Each item In all : Console.WriteLine(item.Name.GetValue("en")) : Next
        ''' </code>
        ''' </example>
        Public Function ListAllAsync(Optional query As ContextQuery = Nothing, Optional maxItems As Integer? = Nothing,
                                     Optional cancellationToken As CancellationToken = Nothing) As Task(Of List(Of Context))
            Return ListAllCoreAsync(Of Context, ContextQuery)(If(query, New ContextQuery()), AddressOf ListAsync, maxItems, cancellationToken)
        End Function

        ''' <summary>Streams every journal matching <paramref name="query"/> page by page.</summary>
        ''' <param name="query">Filters; <see cref="ListQuery.Count"/> sets the page size.</param>
        ''' <example>
        ''' <code>
        ''' ' C#: await foreach (var item in ojs.Contexts.Enumerate()) Console.WriteLine(item.Name.GetValue("en"));
        ''' ' VB has no Await For Each; use the enumerator (or ListAllAsync):
        ''' Dim e = ojs.Contexts.Enumerate(New ContextQuery With {.IsEnabled = True}).GetAsyncEnumerator()
        ''' While Await e.MoveNextAsync()
        '''     Console.WriteLine(e.Current.Name.GetValue("en"))
        ''' End While
        ''' Await e.DisposeAsync()
        ''' </code>
        ''' </example>
        Public Function Enumerate(Optional query As ContextQuery = Nothing) As IAsyncEnumerable(Of Context)
            Return EnumerateCore(Of Context, ContextQuery)(If(query, New ContextQuery()), AddressOf ListAsync)
        End Function

        ''' <summary>Gets a journal with all its settings.</summary>
        ''' <param name="contextId">The journal ID.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim journal = Await ojs.Contexts.GetAsync(1)
        ''' Console.WriteLine(journal.Name.GetValue("en"))
        ''' </code>
        ''' </example>
        Public Function GetAsync(contextId As Integer, Optional cancellationToken As CancellationToken = Nothing) As Task(Of Context)
            Return Http.GetAsync(Of Context)($"contexts/{contextId}", Nothing, cancellationToken)
        End Function

        ''' <summary>Creates a journal. Requires a site administrator token.</summary>
        ''' <param name="context">The settings. <c>Name</c>, <c>UrlPath</c>, <c>PrimaryLocale</c>, <c>SupportedLocales</c>, <c>ContactName</c>, <c>ContactEmail</c> and <c>Country</c> are required.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <returns>The new journal.</returns>
        ''' <exception cref="Errors.OjsValidationException">A setting is missing or invalid.</exception>
        ''' <example>
        ''' <code>
        ''' Dim created = Await ojs.Contexts.CreateAsync(New ContextEdit With {.Name = LocalizedString.Create("en", "Journal of Examples"), .UrlPath = "joe", .PrimaryLocale = "en", .SupportedLocales = New List(Of String) From {"en"}, .ContactName = "Editor", .ContactEmail = "editor@example.com", .Country = "ID"})
        ''' Console.WriteLine(created.Id)
        ''' </code>
        ''' </example>
        Public Function CreateAsync(context As ContextEdit, Optional cancellationToken As CancellationToken = Nothing) As Task(Of Context)
            ArgumentNullException.ThrowIfNull(context)
            Return Http.PostAsync(Of Context)("contexts", context, cancellationToken)
        End Function

        ''' <summary>Changes a journal's settings. Only the properties you set are changed.</summary>
        ''' <param name="contextId">The journal ID.</param>
        ''' <param name="changes">The settings to change.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <returns>The updated journal.</returns>
        ''' <example>
        ''' <code>
        ''' Dim updated = Await ojs.Contexts.UpdateAsync(1, New ContextEdit With {.Description = LocalizedString.Create("en", "&lt;p&gt;A journal about examples.&lt;/p&gt;")})
        ''' </code>
        ''' </example>
        Public Async Function UpdateAsync(contextId As Integer, changes As ContextEdit,
                                          Optional cancellationToken As CancellationToken = Nothing) As Task(Of Context)
            ArgumentNullException.ThrowIfNull(changes)
            Dim target = Await JournalHttpAsync(contextId, cancellationToken).ConfigureAwait(False)
            Return Await target.PutAsync(Of Context)($"contexts/{contextId}", changes, cancellationToken).ConfigureAwait(False)
        End Function

        ''' <summary>Deletes a journal and all its content. Requires a site administrator token. This cannot be undone.</summary>
        ''' <param name="contextId">The journal ID.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <returns>The deleted journal.</returns>
        ''' <example>
        ''' <code>
        ''' ' Deletes the journal and ALL its content. This cannot be undone.
        ''' Await ojs.Contexts.DeleteAsync(journalId)
        ''' </code>
        ''' </example>
        Public Function DeleteAsync(contextId As Integer, Optional cancellationToken As CancellationToken = Nothing) As Task(Of Context)
            Dim removed As String = Nothing
            _paths.TryRemove(contextId, removed)
            Return Http.DeleteAsync(Of Context)($"contexts/{contextId}", Nothing, Nothing, cancellationToken)
        End Function

        ''' <summary>Gets a journal's active theme and its option values.</summary>
        ''' <param name="contextId">The journal ID.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim theme = Await ojs.Contexts.GetThemeAsync(1)
        ''' Console.WriteLine($"{theme.ThemePluginPath}: {theme.GetString("baseColour")}")
        ''' </code>
        ''' </example>
        Public Function GetThemeAsync(contextId As Integer, Optional cancellationToken As CancellationToken = Nothing) As Task(Of ThemeOptions)
            Return Http.GetAsync(Of ThemeOptions)($"contexts/{contextId}/theme", Nothing, cancellationToken)
        End Function

        ''' <summary>Changes a journal's theme or theme options.</summary>
        ''' <param name="contextId">The journal ID.</param>
        ''' <param name="theme">The theme; <see cref="ThemeOptions.ThemePluginPath"/> is required.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Await ojs.Contexts.UpdateThemeAsync(1, New ThemeOptions With {.ThemePluginPath = "default"}.SetOption("baseColour", "#1E6292"))
        ''' </code>
        ''' </example>
        Public Async Function UpdateThemeAsync(contextId As Integer, theme As ThemeOptions,
                                               Optional cancellationToken As CancellationToken = Nothing) As Task(Of ThemeOptions)
            ArgumentNullException.ThrowIfNull(theme)
            Dim target = Await JournalHttpAsync(contextId, cancellationToken).ConfigureAwait(False)
            Return Await target.PutAsync(Of ThemeOptions)($"contexts/{contextId}/theme", theme, cancellationToken).ConfigureAwait(False)
        End Function

    End Class

    ''' <summary><c>/site</c> — site-wide settings and theme.</summary>
    ''' <remarks>Requests always go to <c>/_/api/v1/site</c>. Changing settings requires a site administrator token.</remarks>
    ''' <example>
    ''' <code>
    ''' Dim site = Await ojs.Site.GetAsync()
    ''' Console.WriteLine(String.Join(", ", site.SupportedLocales))
    ''' </code>
    ''' </example>
    Public NotInheritable Class SiteResource
        Inherits ResourceBase

        Friend Sub New(http As OjsHttp)
            MyBase.New(http.ForSite())
        End Sub

        ''' <summary>Gets the site settings.</summary>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim site = Await ojs.Site.GetAsync()
        ''' Console.WriteLine(site.Title.GetValue(site.PrimaryLocale))
        ''' </code>
        ''' </example>
        Public Function GetAsync(Optional cancellationToken As CancellationToken = Nothing) As Task(Of Site)
            Return Http.GetAsync(Of Site)("site", Nothing, cancellationToken)
        End Function

        ''' <summary>Changes site settings. Only the properties you set are changed.</summary>
        ''' <param name="changes">The settings to change.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <returns>The updated site settings.</returns>
        ''' <example>
        ''' <code>
        ''' Await ojs.Site.UpdateAsync(New SiteEdit With {.MinPasswordLength = 10})
        ''' </code>
        ''' </example>
        Public Function UpdateAsync(changes As SiteEdit, Optional cancellationToken As CancellationToken = Nothing) As Task(Of Site)
            ArgumentNullException.ThrowIfNull(changes)
            Return Http.PutAsync(Of Site)("site", changes, cancellationToken)
        End Function

        ''' <summary>Gets the site's active theme and its option values.</summary>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim theme = Await ojs.Site.GetThemeAsync()
        ''' Console.WriteLine(theme.ThemePluginPath)
        ''' </code>
        ''' </example>
        Public Function GetThemeAsync(Optional cancellationToken As CancellationToken = Nothing) As Task(Of ThemeOptions)
            Return Http.GetAsync(Of ThemeOptions)("site/theme", Nothing, cancellationToken)
        End Function

        ''' <summary>Changes the site's theme or theme options.</summary>
        ''' <param name="theme">The theme; <see cref="ThemeOptions.ThemePluginPath"/> is required.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Await ojs.Site.UpdateThemeAsync(New ThemeOptions With {.ThemePluginPath = "default"}.SetOption("typography", "notoSans"))
        ''' </code>
        ''' </example>
        Public Function UpdateThemeAsync(theme As ThemeOptions, Optional cancellationToken As CancellationToken = Nothing) As Task(Of ThemeOptions)
            ArgumentNullException.ThrowIfNull(theme)
            Return Http.PutAsync(Of ThemeOptions)("site/theme", theme, cancellationToken)
        End Function

    End Class

End Namespace
