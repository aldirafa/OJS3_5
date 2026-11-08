Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports OJS3_5.Library.Serialization

Namespace Models

    ''' <summary>An editorial decision recorded for a submission.</summary>
    Partial Public Class Decision

        ''' <summary>The date the decision was taken. Read-only.</summary>
        Public Property DateDecided As DateTime?

        ''' <summary>The decision that was made. One of the <c>Decision::*</c> constants.</summary>
        Public Property Decision As Integer?

        ''' <summary>A user-facing description of this decision type. Read-only.</summary>
        Public Property Description As String

        ''' <summary>The user ID of the editor who took the decision. Read-only.</summary>
        Public Property EditorId As Integer?

        ''' <summary>The URL to this decision in the REST API. Read-only.</summary>
        <JsonPropertyName("_href")>
        Public Property Href As String

        ''' <summary>Id. Read-only.</summary>
        Public Property Id As Integer?

        ''' <summary>A short label or name for this decision type. Read-only.</summary>
        Public Property Label As String

        ''' <summary>The unique ID of the review round when this decision was taken. Do not include this if the decision is not recorded in a review stage. This is a globally unique id. It does not represent whether the decision was taken in the first or second round of reviews for a submission. See <c>round</c> below.</summary>
        Public Property ReviewRoundId As Integer?

        ''' <summary>The sequential review round when this decision was taken. For example, the first, second or third round of review for this submission. Do not include this unless using <c>reviewRoundId</c>.</summary>
        Public Property Round As Integer?

        ''' <summary>The workflow stage when this decision was taken. One of the <c>WORKFLOW_STAGE_ID_</c> constants. Read-only.</summary>
        Public Property StageId As WorkflowStage?

        ''' <summary>The decision applies to this submission. Read-only.</summary>
        Public Property SubmissionId As Integer?

        ''' <summary>
        ''' Properties returned by the server that this class does not model, such as those added by plugins
        ''' (<c>crossrefplugin::status</c>, <c>dataciteplugin::registeredDoi</c>, …).
        ''' </summary>
        <JsonExtensionData>
        Public Property AdditionalProperties As Dictionary(Of String, JsonElement)

    End Class

    ''' <summary>A decision to record. &lt;c&gt;Decision&lt;/c&gt; is one of the &lt;c&gt;Decision::*&lt;/c&gt; constants; &lt;c&gt;Actions&lt;/c&gt; are decision-specific (for example emails to send).</summary>
    Partial Public Class DecisionEdit

        ''' <summary>Optional actions to take with the decision, such as sending an email. Each item is serialized as-is (use a dictionary, an anonymous-like model or a <c>JsonNode</c>).</summary>
        Public Property Actions As List(Of Object)

        ''' <summary>The decision that was made. One of the <c>Decision::*</c> constants.</summary>
        Public Property Decision As Integer?

        ''' <summary>The unique ID of the review round when this decision was taken. Do not include this if the decision is not recorded in a review stage. This is a globally unique id. It does not represent whether the decision was taken in the first or second round of reviews for a submission. See <c>round</c> below.</summary>
        Public Property ReviewRoundId As Integer?

        ''' <summary>The sequential review round when this decision was taken. For example, the first, second or third round of review for this submission. Do not include this unless using <c>reviewRoundId</c>.</summary>
        Public Property Round As Integer?

    End Class

End Namespace
