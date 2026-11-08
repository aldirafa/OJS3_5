Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports OJS3_5.Library.Serialization

Namespace Models

    ''' <summary>A galley: a published representation of a publication, such as a PDF or HTML file, or a remote URL.</summary>
    Partial Public Class Galley

        ''' <summary>DOI object.</summary>
        Public Property DoiObject As Doi

        ''' <summary>File.</summary>
        Public Property File As SubmissionFile

        ''' <summary>Id. Read-only.</summary>
        Public Property Id As Integer?

        ''' <summary>Is approved.</summary>
        Public Property IsApproved As Boolean?

        ''' <summary>A human-readable label for this galley. For the primary article, it usually indicates the format like PDF, ePub or full text. For supplementary files it often indicates the content, such as source data or interview transcripts.</summary>
        Public Property Label As String

        ''' <summary>The primary locale of this galley.</summary>
        Public Property Locale As String

        ''' <summary>A unique ID provided by the publisher. This is often used to record the ID of this galley in an external database, in cases where the publisher maintains separate records.</summary>
        <JsonPropertyName("pub-id::publisher-id")>
        Public Property PubIdPublisherId As String

        ''' <summary>Publication ID.</summary>
        Public Property PublicationId As Integer?

        ''' <summary>In what order this galley should appear in lists of galleys for this publication.</summary>
        Public Property Seq As Integer?

        ''' <summary>Submission file ID.</summary>
        Public Property SubmissionFileId As Integer?

        ''' <summary>An optional path to use in the URL instead of the ID.</summary>
        Public Property UrlPath As String

        ''' <summary>The URL where this galley can be viewed or downloaded. Read-only.</summary>
        Public Property UrlPublished As String

        ''' <summary>An optional URL where the galley can be viewed or downloaded.</summary>
        Public Property UrlRemote As String

        ''' <summary>
        ''' Properties returned by the server that this class does not model, such as those added by plugins
        ''' (<c>crossrefplugin::status</c>, <c>dataciteplugin::registeredDoi</c>, …).
        ''' </summary>
        <JsonExtensionData>
        Public Property AdditionalProperties As Dictionary(Of String, JsonElement)

    End Class

    ''' <summary>The writable properties of a galley. Only properties you set are sent.</summary>
    Partial Public Class GalleyEdit

        ''' <summary>Reference to the DOI for this galley</summary>
        Public Property DoiId As Integer?

        ''' <summary>Is approved.</summary>
        Public Property IsApproved As Boolean?

        ''' <summary>A human-readable label for this galley. For the primary article, it usually indicates the format like PDF, ePub or full text. For supplementary files it often indicates the content, such as source data or interview transcripts.</summary>
        Public Property Label As String

        ''' <summary>The primary locale of this galley.</summary>
        Public Property Locale As String

        ''' <summary>A unique ID provided by the publisher. This is often used to record the ID of this galley in an external database, in cases where the publisher maintains separate records.</summary>
        <JsonPropertyName("pub-id::publisher-id")>
        Public Property PubIdPublisherId As String

        ''' <summary>Publication ID.</summary>
        Public Property PublicationId As Integer?

        ''' <summary>In what order this galley should appear in lists of galleys for this publication.</summary>
        Public Property Seq As Integer?

        ''' <summary>Submission file ID.</summary>
        Public Property SubmissionFileId As Integer?

        ''' <summary>An optional path to use in the URL instead of the ID.</summary>
        Public Property UrlPath As String

        ''' <summary>An optional URL where the galley can be viewed or downloaded.</summary>
        Public Property UrlRemote As String

    End Class

End Namespace
