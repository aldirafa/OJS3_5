Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports Library.Serialization

Namespace Models

    ''' <summary>A file attached to a submission at some stage of the workflow.</summary>
    Partial Public Class SubmissionFile

        ''' <summary>Used with <c>assocType</c> to associate this file with an object such as a galley.</summary>
        Public Property AssocId As Integer?

        ''' <summary>Used with <c>assocId</c> to associate this file with an object such as a galley. One of the following constants: <c>ASSOC_TYPE_SUBMISSION_FILE</c> (dependent files), <c>ASSOC_TYPE_REVIEW_ASSIGNMENT</c> (files uploaded by a reviewer), <c>ASSOC_TYPE_NOTE</c> (files uploaded with a discussion), <c>ASSOC_TYPE_REPRESENTATION</c> (files uploaded to a galley or publication format), <c>ASSOC_TYPE_REVIEW_ROUND</c> (review files and revisions for a particular review round).</summary>
        Public Property AssocType As Integer?

        ''' <summary>A caption for this file. Used with artwork such as images.</summary>
        Public Property Caption As String

        ''' <summary>The copyright owner of this file. Used with artwork such as images.</summary>
        Public Property CopyrightOwner As String

        ''' <summary>The date this submission file object was created in the system. Set automatically by the software.</summary>
        Public Property CreatedAt As DateTime?

        ''' <summary>Creator.</summary>
        Public Property Creator As LocalizedString

        ''' <summary>An attribution for this file, such as a photographer's name. Used with artwork such as images.</summary>
        Public Property Credit As String

        ''' <summary>Public metadata about when this content was created. Used with supplementary files.</summary>
        Public Property DateCreated As String

        ''' <summary>Any dependent files, typically images or audio files that are loaded by an HTML file. Read-only.</summary>
        Public Property DependentFiles As List(Of SubmissionFile)

        ''' <summary>Description.</summary>
        Public Property Description As LocalizedString

        ''' <summary>Document type. Read-only.</summary>
        Public Property DocumentType As String

        ''' <summary>File ID.</summary>
        Public Property FileId As Integer?

        ''' <summary>File stage.</summary>
        Public Property FileStage As SubmissionFileStage?

        ''' <summary>The genre ID of this file</summary>
        Public Property GenreId As Integer?

        ''' <summary>Whether or not this is a dependent file, such as images displayed by an HTML file, that are displayed with published content. Read-only.</summary>
        Public Property GenreIsDependent As Boolean?

        ''' <summary>Whether or not this is a supplementary file, such as data sets and research materials, that are displayed separately from the main publication files. Read-only.</summary>
        Public Property GenreIsSupplementary As Boolean?

        ''' <summary>The genre name of this file, such as Article Text or Data Set. Read-only.</summary>
        Public Property GenreName As LocalizedString

        ''' <summary>Href. Read-only.</summary>
        <JsonPropertyName("_href")>
        Public Property Href As String

        ''' <summary>Id. Read-only.</summary>
        Public Property Id As Integer?

        ''' <summary>The language of this file if it contains text. Used with supplementary files.</summary>
        Public Property Language As String

        ''' <summary>The primary locale of the submission this file is attached to. This locale is used as the fallback when a language is missing from a multilingual property. Read-only.</summary>
        Public Property Locale As String

        ''' <summary>Mimetype. Read-only.</summary>
        Public Property Mimetype As String

        ''' <summary>Name.</summary>
        Public Property Name As LocalizedString

        ''' <summary>Path. Read-only.</summary>
        Public Property Path As String

        ''' <summary>Publisher.</summary>
        Public Property Publisher As LocalizedString

        ''' <summary>Previous versions of this file. Read-only.</summary>
        Public Property Revisions As List(Of SubmissionFileRevision)

        ''' <summary>Source.</summary>
        Public Property Source As LocalizedString

        ''' <summary>When a submission file is promoted from one stage to another, this property identifies the source file from which this file was promoted. Default: <c>null</c>.</summary>
        Public Property SourceSubmissionFileId As Integer?

        ''' <summary>Sponsor.</summary>
        Public Property Sponsor As LocalizedString

        ''' <summary>Subject.</summary>
        Public Property Subject As LocalizedString

        ''' <summary>Submission ID.</summary>
        Public Property SubmissionId As Integer?

        ''' <summary>Licensing terms for this file. Used with artwork such as images.</summary>
        Public Property Terms As String

        ''' <summary>When this object was last updated.</summary>
        Public Property UpdatedAt As DateTime?

        ''' <summary>Uploader user ID.</summary>
        Public Property UploaderUserId As Integer?

        ''' <summary>Uploader user name. Read-only.</summary>
        Public Property UploaderUserName As String

        ''' <summary>Url. Read-only.</summary>
        Public Property Url As String

        ''' <summary>Viewable.</summary>
        Public Property Viewable As Boolean?

        ''' <summary>
        ''' Properties returned by the server that this class does not model, such as those added by plugins
        ''' (<c>crossrefplugin::status</c>, <c>dataciteplugin::registeredDoi</c>, …).
        ''' </summary>
        <JsonExtensionData>
        Public Property AdditionalProperties As Dictionary(Of String, JsonElement)

    End Class

    ''' <summary>The writable properties of a submission file. When uploading, the file content is passed separately.</summary>
    Partial Public Class SubmissionFileEdit

        ''' <summary>Used with <c>assocType</c> to associate this file with an object such as a galley.</summary>
        Public Property AssocId As Integer?

        ''' <summary>Used with <c>assocId</c> to associate this file with an object such as a galley. One of the following constants: <c>ASSOC_TYPE_SUBMISSION_FILE</c> (dependent files), <c>ASSOC_TYPE_REVIEW_ASSIGNMENT</c> (files uploaded by a reviewer), <c>ASSOC_TYPE_NOTE</c> (files uploaded with a discussion), <c>ASSOC_TYPE_REPRESENTATION</c> (files uploaded to a galley or publication format), <c>ASSOC_TYPE_REVIEW_ROUND</c> (review files and revisions for a particular review round).</summary>
        Public Property AssocType As Integer?

        ''' <summary>A caption for this file. Used with artwork such as images.</summary>
        Public Property Caption As String

        ''' <summary>The copyright owner of this file. Used with artwork such as images.</summary>
        Public Property CopyrightOwner As String

        ''' <summary>Creator.</summary>
        Public Property Creator As LocalizedString

        ''' <summary>An attribution for this file, such as a photographer's name. Used with artwork such as images.</summary>
        Public Property Credit As String

        ''' <summary>Description.</summary>
        Public Property Description As LocalizedString

        ''' <summary>File ID.</summary>
        Public Property FileId As Integer?

        ''' <summary>File stage.</summary>
        Public Property FileStage As SubmissionFileStage?

        ''' <summary>The genre ID of this file</summary>
        Public Property GenreId As Integer?

        ''' <summary>The language of this file if it contains text. Used with supplementary files.</summary>
        Public Property Language As String

        ''' <summary>Name.</summary>
        Public Property Name As LocalizedString

        ''' <summary>Publisher.</summary>
        Public Property Publisher As LocalizedString

        ''' <summary>Source.</summary>
        Public Property Source As LocalizedString

        ''' <summary>When a submission file is promoted from one stage to another, this property identifies the source file from which this file was promoted. Default: <c>null</c>.</summary>
        Public Property SourceSubmissionFileId As Integer?

        ''' <summary>Sponsor.</summary>
        Public Property Sponsor As LocalizedString

        ''' <summary>Subject.</summary>
        Public Property Subject As LocalizedString

        ''' <summary>Licensing terms for this file. Used with artwork such as images.</summary>
        Public Property Terms As String

        ''' <summary>Uploader user ID.</summary>
        Public Property UploaderUserId As Integer?

        ''' <summary>Viewable.</summary>
        Public Property Viewable As Boolean?

    End Class

End Namespace
