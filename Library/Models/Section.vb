Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports Library.Serialization

Namespace Models

    ''' <summary>A journal section, such as Articles or Reviews.</summary>
    Partial Public Class Section

        ''' <summary>Abbrev.</summary>
        Public Property Abbrev As LocalizedString

        ''' <summary>Abstracts not required.</summary>
        Public Property AbstractsNotRequired As Boolean?

        ''' <summary>Context ID.</summary>
        Public Property ContextId As Integer?

        ''' <summary>Editor restricted.</summary>
        Public Property EditorRestricted As Boolean?

        ''' <summary>Hide author.</summary>
        Public Property HideAuthor As Boolean?

        ''' <summary>Hide title.</summary>
        Public Property HideTitle As Boolean?

        ''' <summary>Id.</summary>
        Public Property Id As Integer?

        ''' <summary>Identify type.</summary>
        Public Property IdentifyType As LocalizedString

        ''' <summary>Is inactive.</summary>
        Public Property IsInactive As Boolean?

        ''' <summary>Meta indexed.</summary>
        Public Property MetaIndexed As Boolean?

        ''' <summary>Meta reviewed.</summary>
        Public Property MetaReviewed As Boolean?

        ''' <summary>Policy.</summary>
        Public Property Policy As LocalizedString

        ''' <summary>Review form ID.</summary>
        Public Property ReviewFormId As Integer?

        ''' <summary>Sequence.</summary>
        Public Property Sequence As Integer?

        ''' <summary>Title.</summary>
        Public Property Title As LocalizedString

        ''' <summary>Word count.</summary>
        Public Property WordCount As Integer?

        ''' <summary>
        ''' Properties returned by the server that this class does not model, such as those added by plugins
        ''' (<c>crossrefplugin::status</c>, <c>dataciteplugin::registeredDoi</c>, …).
        ''' </summary>
        <JsonExtensionData>
        Public Property AdditionalProperties As Dictionary(Of String, JsonElement)

    End Class

End Namespace
