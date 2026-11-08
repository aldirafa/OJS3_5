Imports System.Text.Json.Nodes
Imports System.Threading
Imports Library.Http
Imports Library.Models

Namespace Resources

    ''' <summary>
    ''' <c>/submissions/{submissionId}/publications/{publicationId}/contributors</c> — the authors and other
    ''' contributors of a publication.
    ''' </summary>
    ''' <example>
    ''' <code>
    ''' Dim authors = Await ojs.Contributors.ListAsync(submissionId, publicationId)
    ''' ' Reverse the author order
    ''' Await ojs.Contributors.SaveOrderAsync(submissionId, publicationId, authors.Select(Function(a) a.Id.Value).Reverse())
    ''' </code>
    ''' </example>
    Public NotInheritable Class ContributorsResource
        Inherits ResourceBase

        Friend Sub New(http As OjsHttp)
            MyBase.New(http)
        End Sub

        Private Shared Function PathOf(submissionId As Integer, publicationId As Integer, Optional contributorId As Integer? = Nothing) As String
            Dim p = $"submissions/{submissionId}/publications/{publicationId}/contributors"
            If contributorId.HasValue Then p &= $"/{contributorId.Value}"
            Return p
        End Function

        ''' <summary>Gets all contributors of a publication, in order.</summary>
        ''' <param name="submissionId">The submission ID.</param>
        ''' <param name="publicationId">The publication (version) ID.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim authors = Await ojs.Contributors.ListAsync(12, 15)
        ''' For Each a In authors : Console.WriteLine(a.FullName) : Next
        ''' </code>
        ''' </example>
        Public Function ListAsync(submissionId As Integer, publicationId As Integer,
                                  Optional cancellationToken As CancellationToken = Nothing) As Task(Of List(Of Contributor))
            Return Http.GetAsync(Of List(Of Contributor))(PathOf(submissionId, publicationId), Nothing, cancellationToken)
        End Function

        ''' <summary>Gets one contributor.</summary>
        ''' <param name="submissionId">The submission ID.</param>
        ''' <param name="publicationId">The publication (version) ID.</param>
        ''' <param name="contributorId">The contributor ID.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim author = Await ojs.Contributors.GetAsync(12, 15, 31)
        ''' Console.WriteLine(author.Email)
        ''' </code>
        ''' </example>
        Public Function GetAsync(submissionId As Integer, publicationId As Integer, contributorId As Integer,
                                 Optional cancellationToken As CancellationToken = Nothing) As Task(Of Contributor)
            Return Http.GetAsync(Of Contributor)(PathOf(submissionId, publicationId, contributorId), Nothing, cancellationToken)
        End Function

        ''' <summary>Adds a contributor to a publication.</summary>
        ''' <param name="submissionId">The submission ID.</param>
        ''' <param name="publicationId">The publication (version) ID.</param>
        ''' <param name="contributor">The contributor to add.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim author = Await ojs.Contributors.CreateAsync(12, 15, New ContributorEdit With {
        '''     .GivenName = LocalizedString.Create("en", "Ada"), .FamilyName = LocalizedString.Create("en", "Lovelace"),
        '''     .Email = "ada@example.com", .Country = "GB", .UserGroupId = 14})
        ''' </code>
        ''' </example>
        Public Function CreateAsync(submissionId As Integer, publicationId As Integer, contributor As ContributorEdit,
                                    Optional cancellationToken As CancellationToken = Nothing) As Task(Of Contributor)
            ArgumentNullException.ThrowIfNull(contributor)
            Return Http.PostAsync(Of Contributor)(PathOf(submissionId, publicationId), contributor, cancellationToken)
        End Function

        ''' <summary>Edits a contributor. Only properties you set are changed.</summary>
        ''' <param name="submissionId">The submission ID.</param>
        ''' <param name="publicationId">The publication (version) ID.</param>
        ''' <param name="contributorId">The contributor ID.</param>
        ''' <param name="changes">The properties to change; only the ones you set are sent.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Await ojs.Contributors.UpdateAsync(12, 15, 31, New ContributorEdit With {.Orcid = "https://orcid.org/0000-0002-1825-0097"})
        ''' </code>
        ''' </example>
        Public Function UpdateAsync(submissionId As Integer, publicationId As Integer, contributorId As Integer, changes As ContributorEdit,
                                    Optional cancellationToken As CancellationToken = Nothing) As Task(Of Contributor)
            ArgumentNullException.ThrowIfNull(changes)
            Return Http.PutAsync(Of Contributor)(PathOf(submissionId, publicationId, contributorId), changes, cancellationToken)
        End Function

        ''' <summary>Removes a contributor and returns it as it was.</summary>
        ''' <param name="submissionId">The submission ID.</param>
        ''' <param name="publicationId">The publication (version) ID.</param>
        ''' <param name="contributorId">The contributor ID.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Await ojs.Contributors.DeleteAsync(12, 15, 31)
        ''' </code>
        ''' </example>
        Public Function DeleteAsync(submissionId As Integer, publicationId As Integer, contributorId As Integer,
                                    Optional cancellationToken As CancellationToken = Nothing) As Task(Of Contributor)
            Return Http.DeleteAsync(Of Contributor)(PathOf(submissionId, publicationId, contributorId), ct:=cancellationToken)
        End Function

        ''' <summary>Reorders the contributors of a publication.</summary>
        ''' <param name="submissionId">The submission ID.</param>
        ''' <param name="publicationId">The publication ID.</param>
        ''' <param name="contributorIdsInOrder">Every contributor ID of the publication, in the new order.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <returns>The contributors in their new order.</returns>
        ''' <example>
        ''' <code>
        ''' ' Put contributor 32 first.
        ''' Await ojs.Contributors.SaveOrderAsync(12, 15, {32, 31, 30})
        ''' </code>
        ''' </example>
        Public Function SaveOrderAsync(submissionId As Integer, publicationId As Integer, contributorIdsInOrder As IEnumerable(Of Integer),
                                       Optional cancellationToken As CancellationToken = Nothing) As Task(Of List(Of Contributor))
            ArgumentNullException.ThrowIfNull(contributorIdsInOrder)
            Dim sorted As New JsonArray()
            Dim seq = 0
            For Each id In contributorIdsInOrder
                sorted.Add(New JsonObject From {{"id", id}, {"seq", seq}})
                seq += 1
            Next
            Return Http.PutAsync(Of List(Of Contributor))(PathOf(submissionId, publicationId) & "/saveOrder",
                                                          New JsonObject From {{"sortedAuthors", sorted}}, cancellationToken)
        End Function

    End Class

End Namespace
