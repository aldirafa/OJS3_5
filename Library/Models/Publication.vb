Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports Library.Serialization

Namespace Models

    ''' <summary>One version of a submission's metadata (title, abstract, authors, galleys, ...). A submission has one publication per version.</summary>
    ''' <example>
    ''' <code>
    ''' Dim p = Await ojs.Publications.GetAsync(submissionId, publicationId)
    ''' Console.WriteLine(p.Title.GetValue("en"))
    ''' For Each a In p.Authors : Console.WriteLine(a.FullName) : Next
    ''' </code>
    ''' </example>
    Partial Public Class Publication

        ''' <summary>Abstract.</summary>
        Public Property Abstract As LocalizedString

        ''' <summary>Access status.</summary>
        Public Property AccessStatus As Integer?

        ''' <summary>A list of the authors for this publication. Read-only.</summary>
        Public Property Authors As List(Of Contributor)

        ''' <summary>All of the authors rendered with the appropriate separators according to the locale. Read-only.</summary>
        Public Property AuthorsString As String

        ''' <summary>A list of authors that are included in publication lists, like tables of content and search results, rendered with the appropriate separators according to the locale. Read-only.</summary>
        Public Property AuthorsStringIncludeInBrowse As String

        ''' <summary>A shortened version of the authors rendered with the appropriate separators according to the locale. Read-only.</summary>
        Public Property AuthorsStringShort As String

        ''' <summary>Category IDs. Read-only.</summary>
        Public Property CategoryIds As List(Of Integer)

        ''' <summary>Optional metadata that contains an array of references for works cited in this submission. References have been split and parsed from the raw text. Read-only.</summary>
        Public Property Citations As List(Of String)

        ''' <summary>Optional metadata that contains references for works cited in this submission as raw text. Read-only.</summary>
        Public Property CitationsRaw As String

        ''' <summary>Copyright holder.</summary>
        Public Property CopyrightHolder As LocalizedString

        ''' <summary>The copyright year for this publication.</summary>
        Public Property CopyrightYear As Integer?

        ''' <summary>Coverage.</summary>
        Public Property Coverage As LocalizedString

        ''' <summary>The cover image for each locale. Values may be <c>Nothing</c> for locales without an image.</summary>
        Public Property CoverImage As Dictionary(Of String, CoverImage)

        ''' <summary>Data availability.</summary>
        Public Property DataAvailability As LocalizedString

        ''' <summary>Date published.</summary>
        Public Property DatePublished As DateOnly?

        ''' <summary>Optional metadata that describes the submission's types of study or branches of knowledge.</summary>
        Public Property Disciplines As LocalizedVocab

        ''' <summary>DOI object.</summary>
        Public Property DoiObject As Doi

        ''' <summary>Full title.</summary>
        Public Property FullTitle As LocalizedString

        ''' <summary>Galleys. Read-only.</summary>
        Public Property Galleys As List(Of Galley)

        ''' <summary>Whether the contributor details should be displayed for this publication. Commonly used for editorial statements without individual authorship.</summary>
        Public Property HideAuthor As Boolean?

        ''' <summary>Href. Read-only.</summary>
        <JsonPropertyName("_href")>
        Public Property Href As String

        ''' <summary>Id. Read-only.</summary>
        Public Property Id As Integer?

        ''' <summary>Issue ID.</summary>
        Public Property IssueId As Integer?

        ''' <summary>Optional metadata, usually one- to three-word phrases that are used to indicate the main topics of a submission.</summary>
        Public Property Keywords As LocalizedVocab

        ''' <summary>Last modified.</summary>
        Public Property LastModified As DateTime?

        ''' <summary>A URL to a webpage describing the license terms for this publication.</summary>
        Public Property LicenseUrl As String

        ''' <summary>The primary locale of the submission this publication is attached to. This locale is used as the fallback when a language is missing from a multilingual property. Read-only.</summary>
        Public Property Locale As String

        ''' <summary>The pages of the issue in which this article appears if it is published in a physical format.</summary>
        Public Property Pages As String

        ''' <summary>Prefix.</summary>
        Public Property Prefix As LocalizedString

        ''' <summary>Which <c>contributor</c> is the primary contact for this publication.</summary>
        Public Property PrimaryContactId As Integer?

        ''' <summary>A unique ID provided by the publisher. It will be used in the publication's URL path instead of the <c>id</c> when present.</summary>
        <JsonPropertyName("pub-id::publisher-id")>
        Public Property PubIdPublisherId As String

        ''' <summary>Rights.</summary>
        Public Property Rights As LocalizedString

        ''' <summary>Section ID.</summary>
        Public Property SectionId As Integer?

        ''' <summary>Seq.</summary>
        Public Property Seq As Integer?

        ''' <summary>Source.</summary>
        Public Property Source As LocalizedString

        ''' <summary>Whether the publication is Queued (not yet scheduled for publication), Declined, Published or Scheduled (scheduled for publication at a future date). One of the <c>PKPSubmission::STATUS_*</c> constants. Default is <c>PKPSubmission::STATUS_QUEUED</c>.</summary>
        Public Property Status As SubmissionStatus?

        ''' <summary>Optional metadata that consists of keywords, key phrases or classification codes that describe the submission's topics.</summary>
        Public Property Subjects As LocalizedVocab

        ''' <summary>Submission ID.</summary>
        Public Property SubmissionId As Integer?

        ''' <summary>Subtitle.</summary>
        Public Property Subtitle As LocalizedString

        ''' <summary>Optional metadata that indicates the source of research funding or other institutional support.</summary>
        Public Property SupportingAgencies As LocalizedVocab

        ''' <summary>Title.</summary>
        Public Property Title As LocalizedString

        ''' <summary>Type.</summary>
        Public Property [Type] As LocalizedString

        ''' <summary>An optional path to use in the URL instead of the ID.</summary>
        Public Property UrlPath As String

        ''' <summary>The public URL for this publication or where it will be available if it has not yet been published. Read-only.</summary>
        Public Property UrlPublished As String

        ''' <summary>The version number of this publication. Version numbers should increment by one so that the third publication of a submission will have the version number <c>3</c>. Version numbers should typically be set automatically and editorial staff should not be encouraged to modify them.</summary>
        Public Property Version As Integer?

        ''' <summary>
        ''' Properties returned by the server that this class does not model, such as those added by plugins
        ''' (<c>crossrefplugin::status</c>, <c>dataciteplugin::registeredDoi</c>, …).
        ''' </summary>
        <JsonExtensionData>
        Public Property AdditionalProperties As Dictionary(Of String, JsonElement)

    End Class

    ''' <summary>The writable properties of a publication, for &lt;c&gt;CreateAsync&lt;/c&gt; and &lt;c&gt;UpdateAsync&lt;/c&gt;. Only properties you set are sent.</summary>
    ''' <example>
    ''' <code>
    ''' Await ojs.Publications.UpdateAsync(submissionId, publicationId, New PublicationEdit With {
    '''     .Title = LocalizedString.Create("en", "New title"),
    '''     .Keywords = LocalizedVocab.Create("en", "open access", "publishing")})
    ''' </code>
    ''' </example>
    Partial Public Class PublicationEdit

        ''' <summary>Abstract.</summary>
        Public Property Abstract As LocalizedString

        ''' <summary>Access status.</summary>
        Public Property AccessStatus As Integer?

        ''' <summary>Copyright holder.</summary>
        Public Property CopyrightHolder As LocalizedString

        ''' <summary>The copyright year for this publication.</summary>
        Public Property CopyrightYear As Integer?

        ''' <summary>Coverage.</summary>
        Public Property Coverage As LocalizedString

        ''' <summary>Cover image.</summary>
        Public Property CoverImage As Dictionary(Of String, CoverImage)

        ''' <summary>Data availability.</summary>
        Public Property DataAvailability As LocalizedString

        ''' <summary>Date published.</summary>
        Public Property DatePublished As DateOnly?

        ''' <summary>Optional metadata that describes the submission's types of study or branches of knowledge.</summary>
        Public Property Disciplines As LocalizedVocab

        ''' <summary>The ID of the DOI to assign to this publication.</summary>
        Public Property DoiId As Integer?

        ''' <summary>Whether the contributor details should be displayed for this publication. Commonly used for editorial statements without individual authorship.</summary>
        Public Property HideAuthor As Boolean?

        ''' <summary>Issue ID.</summary>
        Public Property IssueId As Integer?

        ''' <summary>Optional metadata, usually one- to three-word phrases that are used to indicate the main topics of a submission.</summary>
        Public Property Keywords As LocalizedVocab

        ''' <summary>A URL to a webpage describing the license terms for this publication.</summary>
        Public Property LicenseUrl As String

        ''' <summary>The pages of the issue in which this article appears if it is published in a physical format.</summary>
        Public Property Pages As String

        ''' <summary>Prefix.</summary>
        Public Property Prefix As LocalizedString

        ''' <summary>Which <c>contributor</c> is the primary contact for this publication.</summary>
        Public Property PrimaryContactId As Integer?

        ''' <summary>A unique ID provided by the publisher. It will be used in the publication's URL path instead of the <c>id</c> when present.</summary>
        <JsonPropertyName("pub-id::publisher-id")>
        Public Property PubIdPublisherId As String

        ''' <summary>Rights.</summary>
        Public Property Rights As LocalizedString

        ''' <summary>Section ID.</summary>
        Public Property SectionId As Integer?

        ''' <summary>Seq.</summary>
        Public Property Seq As Integer?

        ''' <summary>Source.</summary>
        Public Property Source As LocalizedString

        ''' <summary>Whether the publication is Queued (not yet scheduled for publication), Declined, Published or Scheduled (scheduled for publication at a future date). One of the <c>PKPSubmission::STATUS_*</c> constants. Default is <c>PKPSubmission::STATUS_QUEUED</c>.</summary>
        Public Property Status As SubmissionStatus?

        ''' <summary>Optional metadata that consists of keywords, key phrases or classification codes that describe the submission's topics.</summary>
        Public Property Subjects As LocalizedVocab

        ''' <summary>Subtitle.</summary>
        Public Property Subtitle As LocalizedString

        ''' <summary>Optional metadata that indicates the source of research funding or other institutional support.</summary>
        Public Property SupportingAgencies As LocalizedVocab

        ''' <summary>Title.</summary>
        Public Property Title As LocalizedString

        ''' <summary>Type.</summary>
        Public Property [Type] As LocalizedString

        ''' <summary>An optional path to use in the URL instead of the ID.</summary>
        Public Property UrlPath As String

        ''' <summary>The version number of this publication. Version numbers should increment by one so that the third publication of a submission will have the version number <c>3</c>. Version numbers should typically be set automatically and editorial staff should not be encouraged to modify them.</summary>
        Public Property Version As Integer?

    End Class

End Namespace
