Imports System.IO
Imports System.Net.Http
Imports System.Text.Json.Nodes
Imports System.Threading
Imports Library.Http
Imports Library.Models

Namespace Resources

    ''' <summary>
    ''' <c>/submissions/{submissionId}/publications</c> — versions of a submission's metadata: read, edit, publish,
    ''' unpublish, create new versions and manage the JATS XML.
    ''' </summary>
    ''' <example>
    ''' <code>
    ''' Dim s = Await ojs.Submissions.GetAsync(342)
    ''' Dim p = Await ojs.Publications.UpdateAsync(s.Id.Value, s.CurrentPublicationId.Value, New PublicationEdit With {
    '''     .Pages = "1-12",
    '''     .Keywords = LocalizedVocab.Create("en", "sociology", "education")})
    ''' Await ojs.Publications.PublishAsync(s.Id.Value, p.Id.Value)
    ''' </code>
    ''' </example>
    Public NotInheritable Class PublicationsResource
        Inherits ResourceBase

        Friend Sub New(http As OjsHttp)
            MyBase.New(http)
        End Sub

        Private Shared Function PathOf(submissionId As Integer, Optional publicationId As Integer? = Nothing, Optional suffix As String = Nothing) As String
            Dim p = $"submissions/{submissionId}/publications"
            If publicationId.HasValue Then p &= $"/{publicationId.Value}"
            If suffix IsNot Nothing Then p &= "/" & suffix
            Return p
        End Function

        ''' <summary>Gets all publications (versions) of a submission.</summary>
        ''' <param name="submissionId">The submission ID.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim versions = Await ojs.Publications.ListAsync(12)
        ''' For Each p In versions : Console.WriteLine($"v{p.Version} {p.Status}") : Next
        ''' </code>
        ''' </example>
        Public Function ListAsync(submissionId As Integer, Optional cancellationToken As CancellationToken = Nothing) As Task(Of List(Of Publication))
            Return Http.GetAsync(Of List(Of Publication))(PathOf(submissionId), Nothing, cancellationToken)
        End Function

        ''' <summary>Gets a publication with all its properties.</summary>
        ''' <param name="submissionId">The submission ID.</param>
        ''' <param name="publicationId">The publication (version) ID.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim pub = Await ojs.Publications.GetAsync(12, 15)
        ''' Console.WriteLine(pub.Title.GetValue("en"))
        ''' </code>
        ''' </example>
        Public Function GetAsync(submissionId As Integer, publicationId As Integer,
                                 Optional cancellationToken As CancellationToken = Nothing) As Task(Of Publication)
            Return Http.GetAsync(Of Publication)(PathOf(submissionId, publicationId), Nothing, cancellationToken)
        End Function

        ''' <summary>Creates a publication for a submission.</summary>
        ''' <param name="submissionId">The submission ID.</param>
        ''' <param name="publication">The properties of the new publication.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <remarks>To start a new version based on an existing publication, use <see cref="CreateVersionAsync"/>.</remarks>
        ''' <example>
        ''' <code>
        ''' Dim pub = Await ojs.Publications.CreateAsync(12, New PublicationEdit With {.Title = LocalizedString.Create("en", "A new version")})
        ''' </code>
        ''' </example>
        Public Function CreateAsync(submissionId As Integer, publication As PublicationEdit,
                                    Optional cancellationToken As CancellationToken = Nothing) As Task(Of Publication)
            ArgumentNullException.ThrowIfNull(publication)
            Return Http.PostAsync(Of Publication)(PathOf(submissionId), publication, cancellationToken)
        End Function

        ''' <summary>Edits a publication. Only properties you set are changed. Published publications cannot be edited.</summary>
        ''' <param name="submissionId">The submission ID.</param>
        ''' <param name="publicationId">The publication (version) ID.</param>
        ''' <param name="changes">The properties to change; only the ones you set are sent.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Await ojs.Publications.UpdateAsync(12, 15, New PublicationEdit With {
        '''     .Title = LocalizedString.Create("en", "Revised title"),
        '''     .Keywords = LocalizedVocab.Create("en", "open access", "publishing")})
        ''' </code>
        ''' </example>
        Public Function UpdateAsync(submissionId As Integer, publicationId As Integer, changes As PublicationEdit,
                                    Optional cancellationToken As CancellationToken = Nothing) As Task(Of Publication)
            ArgumentNullException.ThrowIfNull(changes)
            Return Http.PutAsync(Of Publication)(PathOf(submissionId, publicationId), changes, cancellationToken)
        End Function

        ''' <summary>Deletes an unpublished publication and returns it as it was.</summary>
        ''' <param name="submissionId">The submission ID.</param>
        ''' <param name="publicationId">The publication (version) ID.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Await ojs.Publications.DeleteAsync(12, 16)
        ''' </code>
        ''' </example>
        Public Function DeleteAsync(submissionId As Integer, publicationId As Integer,
                                    Optional cancellationToken As CancellationToken = Nothing) As Task(Of Publication)
            Return Http.DeleteAsync(Of Publication)(PathOf(submissionId, publicationId), ct:=cancellationToken)
        End Function

        ''' <summary>Creates a new version by copying this publication. Returns the new publication.</summary>
        ''' <param name="submissionId">The submission ID.</param>
        ''' <param name="publicationId">The publication (version) ID.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim v2 = Await ojs.Publications.CreateVersionAsync(12, 15)
        ''' Console.WriteLine(v2.Version)
        ''' </code>
        ''' </example>
        Public Function CreateVersionAsync(submissionId As Integer, publicationId As Integer,
                                           Optional cancellationToken As CancellationToken = Nothing) As Task(Of Publication)
            Return Http.PostAsync(Of Publication)(PathOf(submissionId, publicationId, "version"), New JsonObject(), cancellationToken)
        End Function

        ''' <summary>
        ''' Publishes a publication, or schedules it when it is assigned to a future issue. Throws
        ''' <see cref="Errors.OjsValidationException"/> when publication requirements are not met.
        ''' </summary>
        ''' <param name="submissionId">The submission ID.</param>
        ''' <param name="publicationId">The publication (version) ID.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim published = Await ojs.Publications.PublishAsync(12, 15)
        ''' Console.WriteLine(published.DatePublished)
        ''' </code>
        ''' </example>
        Public Function PublishAsync(submissionId As Integer, publicationId As Integer,
                                     Optional cancellationToken As CancellationToken = Nothing) As Task(Of Publication)
            Return Http.PutAsync(Of Publication)(PathOf(submissionId, publicationId, "publish"), New JsonObject(), cancellationToken)
        End Function

        ''' <summary>Unpublishes a publication so that it can be edited again.</summary>
        ''' <param name="submissionId">The submission ID.</param>
        ''' <param name="publicationId">The publication (version) ID.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Await ojs.Publications.UnpublishAsync(12, 15)
        ''' </code>
        ''' </example>
        Public Function UnpublishAsync(submissionId As Integer, publicationId As Integer,
                                       Optional cancellationToken As CancellationToken = Nothing) As Task(Of Publication)
            Return Http.PutAsync(Of Publication)(PathOf(submissionId, publicationId, "unpublish"), New JsonObject(), cancellationToken)
        End Function

        ''' <summary>Changes the primary locale of a publication (OJS 3.5+). Returns the updated submission.</summary>
        ''' <param name="submissionId">The submission ID.</param>
        ''' <param name="publicationId">The publication ID.</param>
        ''' <param name="locale">The new primary locale, e.g. <c>en</c>.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim submission = Await ojs.Publications.ChangeLocaleAsync(12, 15, "id")
        ''' </code>
        ''' </example>
        Public Function ChangeLocaleAsync(submissionId As Integer, publicationId As Integer, locale As String,
                                          Optional cancellationToken As CancellationToken = Nothing) As Task(Of Submission)
            ArgumentException.ThrowIfNullOrEmpty(locale)
            Return Http.PutAsync(Of Submission)(PathOf(submissionId, publicationId, "changeLocale"),
                                                 New JsonObject From {{"locale", locale}}, cancellationToken)
        End Function

#Region "JATS"

        ''' <summary>
        ''' Gets the JATS XML of a publication (OJS 3.5+): the uploaded source file, or XML generated by OJS when none
        ''' was uploaded (<see cref="JatsFile.IsDefaultContent"/>).
        ''' </summary>
        ''' <param name="submissionId">The submission ID.</param>
        ''' <param name="publicationId">The publication (version) ID.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim jats = Await ojs.Publications.GetJatsAsync(12, 15)
        ''' File.WriteAllText("article.xml", jats.JatsContent)
        ''' </code>
        ''' </example>
        Public Function GetJatsAsync(submissionId As Integer, publicationId As Integer,
                                     Optional cancellationToken As CancellationToken = Nothing) As Task(Of JatsFile)
            Return Http.GetAsync(Of JatsFile)(PathOf(submissionId, publicationId, "jats"), Nothing, cancellationToken)
        End Function

        ''' <summary>Uploads a JATS XML source file for a publication, replacing any existing one.</summary>
        ''' <param name="submissionId">The submission ID.</param>
        ''' <param name="publicationId">The publication ID.</param>
        ''' <param name="content">The XML content.</param>
        ''' <param name="fileName">The file name, e.g. <c>article.xml</c>.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Using xml = File.OpenRead("article.xml")
        '''     Await ojs.Publications.UploadJatsAsync(12, 15, xml, "article.xml")
        ''' End Using
        ''' </code>
        ''' </example>
        Public Function UploadJatsAsync(submissionId As Integer, publicationId As Integer, content As Stream, fileName As String,
                                        Optional cancellationToken As CancellationToken = Nothing) As Task(Of JatsFile)
            ArgumentNullException.ThrowIfNull(content)
            ArgumentException.ThrowIfNullOrEmpty(fileName)
            Dim form As New MultipartFormDataContent()
            form.Add(TemporaryFilesResource.CreateFilePart(content, "application/xml"), "file", fileName)
            Return Http.PostMultipartAsync(Of JatsFile)(PathOf(submissionId, publicationId, "jats"), form, cancellationToken)
        End Function

        ''' <summary>Deletes the uploaded JATS XML source file of a publication.</summary>
        ''' <param name="submissionId">The submission ID.</param>
        ''' <param name="publicationId">The publication (version) ID.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Await ojs.Publications.DeleteJatsAsync(12, 15)
        ''' </code>
        ''' </example>
        Public Function DeleteJatsAsync(submissionId As Integer, publicationId As Integer,
                                        Optional cancellationToken As CancellationToken = Nothing) As Task(Of JatsFile)
            Return Http.DeleteAsync(Of JatsFile)(PathOf(submissionId, publicationId, "jats"), ct:=cancellationToken)
        End Function

#End Region

    End Class

End Namespace
