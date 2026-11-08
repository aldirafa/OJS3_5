Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports OJS3_5.Library.Serialization

Namespace Models

    ''' <summary>A contributor (author, translator, ...) of a publication. Returned by the contributors endpoints and in &lt;see cref="Publication.Authors"/&gt;.</summary>
    Partial Public Class Contributor

        ''' <summary>The scholarly institution this contributor is employed by or affiliated with. Read-only.</summary>
        Public Property Affiliations As List(Of Affiliation)

        ''' <summary>Biography.</summary>
        Public Property Biography As LocalizedString

        ''' <summary>Competing interests.</summary>
        Public Property CompetingInterests As LocalizedString

        ''' <summary>The author's country in a two-letter code (ISO 3166-1). See all codes.</summary>
        Public Property Country As String

        ''' <summary>Email.</summary>
        Public Property Email As String

        ''' <summary>Family name.</summary>
        Public Property FamilyName As LocalizedString

        ''' <summary>The full name of the author. This will be the preferredPublicName or, if that is not available, a string containing the givenName and familyName. Read-only.</summary>
        Public Property FullName As String

        ''' <summary>Given name.</summary>
        Public Property GivenName As LocalizedString

        ''' <summary>Id.</summary>
        Public Property Id As Integer?

        ''' <summary>Whether or not to include this contributor in author lists when the publication appears in search results, tables of content and catalog entries.</summary>
        Public Property IncludeInBrowse As Boolean?

        ''' <summary>The ORCID of this contributor. See: https://orcid.org/</summary>
        Public Property Orcid As String

        ''' <summary>ORCID display value. Read-only.</summary>
        Public Property OrcidDisplayValue As String

        ''' <summary>ORCID is verified.</summary>
        Public Property OrcidIsVerified As Boolean?

        ''' <summary>ORCID verification requested.</summary>
        Public Property OrcidVerificationRequested As Boolean?

        ''' <summary>Preferred public name.</summary>
        Public Property PreferredPublicName As LocalizedString

        ''' <summary>Publication ID.</summary>
        Public Property PublicationId As Integer?

        ''' <summary>The order in which this contributor should appear in a list of contributors.</summary>
        Public Property Seq As Integer?

        ''' <summary>The primary locale of the submission this author is attached to. This locale is used as the fallback when a language is missing from a multilingual property. Read-only.</summary>
        Public Property SubmissionLocale As String

        ''' <summary>An optional URL to this contributor's webpage.</summary>
        Public Property Url As String

        ''' <summary>The ID of this contributor's assigned user group. See userGroupName.</summary>
        Public Property UserGroupId As Integer?

        ''' <summary>The name of the contributor role (user group). Read-only.</summary>
        Public Property UserGroupName As LocalizedString

        ''' <summary>
        ''' Properties returned by the server that this class does not model, such as those added by plugins
        ''' (<c>crossrefplugin::status</c>, <c>dataciteplugin::registeredDoi</c>, …).
        ''' </summary>
        <JsonExtensionData>
        Public Property AdditionalProperties As Dictionary(Of String, JsonElement)

    End Class

    ''' <summary>The writable properties of a contributor. Only properties you set are sent.</summary>
    ''' <example>
    ''' <code>
    ''' Await ojs.Contributors.CreateAsync(submissionId, publicationId, New ContributorEdit With {
    '''     .GivenName = LocalizedString.Create("en", "Ada"), .FamilyName = LocalizedString.Create("en", "Lovelace"),
    '''     .Email = "ada@example.com", .Country = "GB", .UserGroupId = 14,
    '''     .Affiliations = New List(Of Affiliation) From {New Affiliation With {.Name = LocalizedString.Create("en", "University")}}})
    ''' </code>
    ''' </example>
    Partial Public Class ContributorEdit

        ''' <summary>The contributor's institutional affiliations (OJS 3.5+). Each may link to a ROR record.</summary>
        Public Property Affiliations As List(Of Affiliation)

        ''' <summary>Biography.</summary>
        Public Property Biography As LocalizedString

        ''' <summary>Competing interests.</summary>
        Public Property CompetingInterests As LocalizedString

        ''' <summary>The author's country in a two-letter code (ISO 3166-1). See all codes.</summary>
        Public Property Country As String

        ''' <summary>Email.</summary>
        Public Property Email As String

        ''' <summary>Family name.</summary>
        Public Property FamilyName As LocalizedString

        ''' <summary>Given name.</summary>
        Public Property GivenName As LocalizedString

        ''' <summary>Whether or not to include this contributor in author lists when the publication appears in search results, tables of content and catalog entries.</summary>
        Public Property IncludeInBrowse As Boolean?

        ''' <summary>The ORCID of this contributor. See: https://orcid.org/</summary>
        Public Property Orcid As String

        ''' <summary>Preferred public name.</summary>
        Public Property PreferredPublicName As LocalizedString

        ''' <summary>The order in which this contributor should appear in a list of contributors.</summary>
        Public Property Seq As Integer?

        ''' <summary>An optional URL to this contributor's webpage.</summary>
        Public Property Url As String

        ''' <summary>The ID of this contributor's assigned user group. See userGroupName.</summary>
        Public Property UserGroupId As Integer?

    End Class

    ''' <summary>An institutional affiliation of a contributor, optionally linked to a Research Organization Registry (ROR) record.</summary>
    Partial Public Class Affiliation

        ''' <summary>The author with which this affiliation is associated. Read-only.</summary>
        Public Property AuthorId As Integer?

        ''' <summary>The unique ID of an affiliation in the database. Read-only.</summary>
        Public Property Id As Integer?

        ''' <summary>Name.</summary>
        Public Property Name As LocalizedString

        ''' <summary>The ROR ID of this institution.</summary>
        Public Property Ror As String

        ''' <summary>Ror object.</summary>
        Public Property RorObject As Ror

        ''' <summary>
        ''' Properties returned by the server that this class does not model, such as those added by plugins
        ''' (<c>crossrefplugin::status</c>, <c>dataciteplugin::registeredDoi</c>, …).
        ''' </summary>
        <JsonExtensionData>
        Public Property AdditionalProperties As Dictionary(Of String, JsonElement)

    End Class

    ''' <summary>A record from the Research Organization Registry (ROR) cache.</summary>
    Partial Public Class Ror

        ''' <summary>The display language on ror.org of this institution.</summary>
        Public Property DisplayLocale As String

        ''' <summary>Href. Read-only.</summary>
        <JsonPropertyName("_href")>
        Public Property Href As String

        ''' <summary>The unique ID of this ROR record in the database. Read-only.</summary>
        Public Property Id As Integer?

        ''' <summary>Whether this institution is active or inactive.</summary>
        Public Property IsActive As Boolean?

        ''' <summary>Name.</summary>
        Public Property Name As LocalizedString

        ''' <summary>The ROR ID of this institution.</summary>
        Public Property Ror As String

        ''' <summary>
        ''' Properties returned by the server that this class does not model, such as those added by plugins
        ''' (<c>crossrefplugin::status</c>, <c>dataciteplugin::registeredDoi</c>, …).
        ''' </summary>
        <JsonExtensionData>
        Public Property AdditionalProperties As Dictionary(Of String, JsonElement)

    End Class

End Namespace
