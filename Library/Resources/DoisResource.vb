Imports System.Net.Http
Imports System.Text.Json
Imports System.Threading
Imports Library.Http
Imports Library.Models
Imports Library.Paging
Imports Library.Queries

Namespace Resources

    ''' <summary><c>/dois</c> — DOIs and their registration.</summary>
    ''' <remarks>
    ''' DOIs must be enabled in the journal (<c>ContextEdit.EnableDois</c>), otherwise every call fails with
    ''' <c>doi.authorization.enabledRequired</c>. Bulk actions on articles and issues are under
    ''' <see cref="Submissions"/> and <see cref="Issues"/>.
    ''' </remarks>
    ''' <example>
    ''' <code>
    ''' Dim result = Await ojs.Dois.Submissions.AssignAsync({12, 13})
    ''' If Not result.Succeeded Then Console.WriteLine(String.Join(Environment.NewLine, result.FailedDoiActions))
    '''
    ''' Using xml = Await ojs.Dois.Submissions.ExportAndDownloadAsync({12, 13})
    '''     Await xml.SaveAsAsync("C:\exports\" &amp; xml.FileName)
    ''' End Using
    ''' </code>
    ''' </example>
    Public NotInheritable Class DoisResource
        Inherits ResourceBase

        Friend Sub New(http As OjsHttp)
            MyBase.New(http)
            Submissions = New DoiBulkActions(http, "submissions", Me)
            Issues = New DoiBulkActions(http, "issues", Me)
        End Sub

        ''' <summary>Bulk DOI actions on articles (<c>/dois/submissions/...</c>); IDs are submission IDs.</summary>
        Public ReadOnly Property Submissions As DoiBulkActions

        ''' <summary>Bulk DOI actions on issues (<c>/dois/issues/...</c>); IDs are issue IDs.</summary>
        Public ReadOnly Property Issues As DoiBulkActions

        ''' <summary>Gets one page of DOIs.</summary>
        ''' <param name="query">Filters and paging; <c>Nothing</c> for the first page.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim page = Await ojs.Dois.ListAsync(New DoiQuery With {.Status = {DoiStatus.Error}})
        ''' Console.WriteLine($"{page.ItemsMax} in total")
        ''' For Each item In page.Items : Console.WriteLine(item.Doi) : Next
        ''' </code>
        ''' </example>
        Public Function ListAsync(Optional query As DoiQuery = Nothing,
                                  Optional cancellationToken As CancellationToken = Nothing) As Task(Of PagedResult(Of Doi))
            Return Http.GetAsync(Of PagedResult(Of Doi))("dois", If(query, New DoiQuery()).ToQueryBuilder(), cancellationToken)
        End Function

        ''' <summary>Gets every DOI matching <paramref name="query"/>.</summary>
        ''' <param name="query">Filters; <see cref="ListQuery.Count"/> sets the page size.</param>
        ''' <param name="maxItems">Stops after this many DOIs.</param>
        ''' <param name="cancellationToken">Cancels the requests.</param>
        ''' <example>
        ''' <code>
        ''' Dim all = Await ojs.Dois.ListAllAsync(New DoiQuery With {.Status = {DoiStatus.Unregistered}})
        ''' For Each item In all : Console.WriteLine(item.Doi) : Next
        ''' </code>
        ''' </example>
        Public Function ListAllAsync(Optional query As DoiQuery = Nothing, Optional maxItems As Integer? = Nothing,
                                     Optional cancellationToken As CancellationToken = Nothing) As Task(Of List(Of Doi))
            Return ListAllCoreAsync(Of Doi, DoiQuery)(If(query, New DoiQuery()), AddressOf ListAsync, maxItems, cancellationToken)
        End Function

        ''' <summary>Streams every DOI matching <paramref name="query"/> page by page.</summary>
        ''' <param name="query">Filters; <see cref="ListQuery.Count"/> sets the page size.</param>
        ''' <example>
        ''' <code>
        ''' ' C#: await foreach (var item in ojs.Dois.Enumerate()) Console.WriteLine(item.Doi);
        ''' ' VB has no Await For Each; use the enumerator (or ListAllAsync):
        ''' Dim e = ojs.Dois.Enumerate().GetAsyncEnumerator()
        ''' While Await e.MoveNextAsync()
        '''     Console.WriteLine(e.Current.Doi)
        ''' End While
        ''' Await e.DisposeAsync()
        ''' </code>
        ''' </example>
        Public Function Enumerate(Optional query As DoiQuery = Nothing) As IAsyncEnumerable(Of Doi)
            Return EnumerateCore(Of Doi, DoiQuery)(If(query, New DoiQuery()), AddressOf ListAsync)
        End Function

        ''' <summary>Gets a DOI.</summary>
        ''' <param name="doiId">The internal DOI ID (see <c>Publication.DoiId</c>, <c>Issue.DoiId</c>).</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim doi = Await ojs.Dois.GetAsync(9)
        ''' Console.WriteLine($"{doi.Doi} {doi.Status} {doi.ResolvingUrl}")
        ''' </code>
        ''' </example>
        Public Function GetAsync(doiId As Integer, Optional cancellationToken As CancellationToken = Nothing) As Task(Of Doi)
            Return Http.GetAsync(Of Doi)($"dois/{doiId}", Nothing, cancellationToken)
        End Function

        ''' <summary>Creates a DOI record.</summary>
        ''' <param name="doi">The DOI. <see cref="DoiEdit.Doi"/> is required.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <remarks>To give an article or issue a DOI using the journal's suffix pattern, prefer <see cref="DoiBulkActions.AssignAsync"/>.</remarks>
        ''' <example>
        ''' <code>
        ''' Dim doi = Await ojs.Dois.CreateAsync(New DoiEdit With {.Doi = "10.1234/abc-123"})
        ''' </code>
        ''' </example>
        Public Function CreateAsync(doi As DoiEdit, Optional cancellationToken As CancellationToken = Nothing) As Task(Of Doi)
            ArgumentNullException.ThrowIfNull(doi)
            Return Http.PostAsync(Of Doi)("dois", doi, cancellationToken)
        End Function

        ''' <summary>Changes a DOI, or attaches a new DOI to an object.</summary>
        ''' <param name="doiId">The DOI ID.</param>
        ''' <param name="changes">The changes. <see cref="DoiEdit.PubObjectType"/> and <see cref="DoiEdit.PubObjectId"/> are required.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Await ojs.Dois.UpdateAsync(9, New DoiEdit With {
        '''     .Doi = "10.1234/abc-124", .PubObjectType = DoiObjectType.Publication, .PubObjectId = 15})
        ''' </code>
        ''' </example>
        Public Function UpdateAsync(doiId As Integer, changes As DoiEdit, Optional cancellationToken As CancellationToken = Nothing) As Task(Of Doi)
            ArgumentNullException.ThrowIfNull(changes)
            Return Http.PutAsync(Of Doi)($"dois/{doiId}", changes, cancellationToken)
        End Function

        ''' <summary>Removes a DOI from the object it belongs to and deletes it.</summary>
        ''' <param name="doiId">The DOI ID.</param>
        ''' <param name="objectType">The kind of object the DOI belongs to.</param>
        ''' <param name="objectId">The ID of that object.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <returns>The deleted DOI.</returns>
        ''' <example>
        ''' <code>
        ''' Await ojs.Dois.DeleteAsync(9, DoiObjectType.Publication, 15)
        ''' </code>
        ''' </example>
        Public Function DeleteAsync(doiId As Integer, objectType As DoiObjectType, objectId As Integer,
                                    Optional cancellationToken As CancellationToken = Nothing) As Task(Of Doi)
            Dim body As New DoiEdit With {.PubObjectType = objectType, .PubObjectId = objectId}
            Return Http.DeleteAsync(Of Doi)($"dois/{doiId}", Nothing, body, cancellationToken)
        End Function

        ''' <summary>Schedules every DOI that is not registered yet for deposit with the registration agency.</summary>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Await ojs.Dois.DepositAllAsync()
        ''' </code>
        ''' </example>
        Public Function DepositAllAsync(Optional cancellationToken As CancellationToken = Nothing) As Task
            Return Http.PutAsync(Of JsonElement)("dois/depositAll", Nothing, cancellationToken)
        End Function

        ''' <summary>Downloads an export file created by <see cref="DoiBulkActions.ExportAsync"/>.</summary>
        ''' <param name="fileId">The temporary file ID returned by the export.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <returns>The file; dispose it when done.</returns>
        ''' <example>
        ''' <code>
        ''' Dim fileId = Await ojs.Dois.Submissions.ExportAsync({12, 13})
        ''' Using xml = Await ojs.Dois.DownloadExportAsync(fileId)
        '''     Await xml.SaveAsAsync("crossref.xml")
        ''' End Using
        ''' </code>
        ''' </example>
        Public Function DownloadExportAsync(fileId As Integer, Optional cancellationToken As CancellationToken = Nothing) As Task(Of DownloadedFile)
            Return Http.DownloadAsync(HttpMethod.Get, $"dois/exports/{fileId}", Nothing, Nothing, cancellationToken)
        End Function

    End Class

    ''' <summary>
    ''' Bulk DOI actions on articles or issues. Use <see cref="DoisResource.Submissions"/> or <see cref="DoisResource.Issues"/>.
    ''' </summary>
    ''' <example>
    ''' <code>
    ''' Await ojs.Dois.Submissions.AssignAsync({12, 13})
    ''' Await ojs.Dois.Issues.AssignAsync({4})
    ''' </code>
    ''' </example>
    Public NotInheritable Class DoiBulkActions

        Private ReadOnly _http As OjsHttp
        Private ReadOnly _segment As String
        Private ReadOnly _dois As DoisResource

        Friend Sub New(http As OjsHttp, segment As String, dois As DoisResource)
            _http = http
            _segment = segment
            _dois = dois
        End Sub

        Private Function Body(ids As IEnumerable(Of Integer)) As Object
            ArgumentNullException.ThrowIfNull(ids)
            Return New Dictionary(Of String, Object) From {{"ids", ids.ToArray()}}
        End Function

        ''' <summary>Creates DOIs (using the journal's suffix pattern) for the objects that do not have one yet.</summary>
        ''' <param name="ids">Submission or issue IDs.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <returns>The objects for which no DOI could be created.</returns>
        ''' <example>
        ''' <code>
        ''' Dim result = Await ojs.Dois.Submissions.AssignAsync({12, 13})
        ''' If Not result.Succeeded Then Console.WriteLine(String.Join(Environment.NewLine, result.FailedDoiActions))
        ''' </code>
        ''' </example>
        Public Function AssignAsync(ids As IEnumerable(Of Integer), Optional cancellationToken As CancellationToken = Nothing) As Task(Of DoiActionResult)
            Return _http.PostAsync(Of DoiActionResult)($"dois/{_segment}/assignDois", Body(ids), cancellationToken)
        End Function

        ''' <summary>Builds the registration agency's deposit XML for the objects.</summary>
        ''' <param name="ids">Submission or issue IDs.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <returns>The temporary file ID; download it with <see cref="DoisResource.DownloadExportAsync"/>.</returns>
        ''' <example>
        ''' <code>
        ''' Dim fileId = Await ojs.Dois.Submissions.ExportAsync({12, 13})
        ''' Using xml = Await ojs.Dois.DownloadExportAsync(fileId)
        '''     Await xml.SaveAsAsync(xml.FileName)
        ''' End Using
        ''' </code>
        ''' </example>
        Public Async Function ExportAsync(ids As IEnumerable(Of Integer), Optional cancellationToken As CancellationToken = Nothing) As Task(Of Integer)
            Dim result = Await _http.PutAsync(Of TemporaryFileReference)($"dois/{_segment}/export", Body(ids), cancellationToken).ConfigureAwait(False)
            If result?.TemporaryFileId Is Nothing Then Throw New InvalidOperationException("OJS did not return an export file ID.")
            Return result.TemporaryFileId.Value
        End Function

        ''' <summary>Exports the objects and downloads the resulting file in one call.</summary>
        ''' <param name="ids">Submission or issue IDs.</param>
        ''' <param name="cancellationToken">Cancels the requests.</param>
        ''' <returns>The export file; dispose it when done.</returns>
        ''' <example>
        ''' <code>
        ''' Using xml = Await ojs.Dois.Submissions.ExportAndDownloadAsync({12, 13})
        '''     Await xml.SaveAsAsync("C:\exports\" &amp; xml.FileName)
        ''' End Using
        ''' </code>
        ''' </example>
        Public Async Function ExportAndDownloadAsync(ids As IEnumerable(Of Integer), Optional cancellationToken As CancellationToken = Nothing) As Task(Of DownloadedFile)
            Dim fileId = Await ExportAsync(ids, cancellationToken).ConfigureAwait(False)
            Return Await _dois.DownloadExportAsync(fileId, cancellationToken).ConfigureAwait(False)
        End Function

        ''' <summary>Schedules the objects' DOIs for deposit with the registration agency.</summary>
        ''' <param name="ids">Submission or issue IDs.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Await ojs.Dois.Submissions.DepositAsync({12, 13})
        ''' </code>
        ''' </example>
        Public Function DepositAsync(ids As IEnumerable(Of Integer), Optional cancellationToken As CancellationToken = Nothing) As Task
            Return _http.PutAsync(Of JsonElement)($"dois/{_segment}/deposit", Body(ids), cancellationToken)
        End Function

        ''' <summary>Marks the objects' DOIs as registered without depositing them.</summary>
        ''' <param name="ids">Submission or issue IDs.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Await ojs.Dois.Submissions.MarkRegisteredAsync({12})
        ''' ' For issues: Await ojs.Dois.Issues.MarkRegisteredAsync({4})
        ''' </code>
        ''' </example>
        Public Function MarkRegisteredAsync(ids As IEnumerable(Of Integer), Optional cancellationToken As CancellationToken = Nothing) As Task
            Return _http.PutAsync(Of JsonElement)($"dois/{_segment}/markRegistered", Body(ids), cancellationToken)
        End Function

        ''' <summary>Marks the objects' DOIs as not registered.</summary>
        ''' <param name="ids">Submission or issue IDs.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Await ojs.Dois.Submissions.MarkUnregisteredAsync({12})
        ''' </code>
        ''' </example>
        Public Function MarkUnregisteredAsync(ids As IEnumerable(Of Integer), Optional cancellationToken As CancellationToken = Nothing) As Task
            Return _http.PutAsync(Of JsonElement)($"dois/{_segment}/markUnregistered", Body(ids), cancellationToken)
        End Function

        ''' <summary>Marks the objects' registered DOIs as needing their metadata redeposited.</summary>
        ''' <param name="ids">Submission or issue IDs.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' ' After changing a published article's metadata:
        ''' Await ojs.Dois.Submissions.MarkStaleAsync({12})
        ''' </code>
        ''' </example>
        Public Function MarkStaleAsync(ids As IEnumerable(Of Integer), Optional cancellationToken As CancellationToken = Nothing) As Task
            Return _http.PutAsync(Of JsonElement)($"dois/{_segment}/markStale", Body(ids), cancellationToken)
        End Function

    End Class

End Namespace
