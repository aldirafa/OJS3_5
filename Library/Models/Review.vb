Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports Library.Serialization

Namespace Models

    ''' <summary>A round of review for a submission.</summary>
    Partial Public Class ReviewRound

        ''' <summary>Id.</summary>
        Public Property Id As Integer?

        ''' <summary>Round.</summary>
        Public Property Round As Integer?

        ''' <summary>Stage ID.</summary>
        Public Property StageId As WorkflowStage?

        ''' <summary>Status. Read-only.</summary>
        Public Property Status As String

        ''' <summary>Status ID.</summary>
        Public Property StatusId As Integer?

        ''' <summary>
        ''' Properties returned by the server that this class does not model, such as those added by plugins
        ''' (<c>crossrefplugin::status</c>, <c>dataciteplugin::registeredDoi</c>, …).
        ''' </summary>
        <JsonExtensionData>
        Public Property AdditionalProperties As Dictionary(Of String, JsonElement)

    End Class

    ''' <summary>A reviewer's assignment to review a submission in a review round.</summary>
    Partial Public Class ReviewAssignment

        ''' <summary>Indicates whether review assignment was cancelled</summary>
        Public Property Cancelled As Boolean?

        ''' <summary>Comments from a reviewer</summary>
        Public Property Comments As String

        ''' <summary>Reviewer's competing interests</summary>
        Public Property CompetingInterests As String

        ''' <summary>Whether editor considered a review assignment request</summary>
        Public Property Considered As Integer?

        ''' <summary>Date the editor acknowledged the review</summary>
        Public Property DateAcknowledged As DateTime?

        ''' <summary>Date the reviewer was assigned to the submission</summary>
        Public Property DateAssigned As DateTime?

        ''' <summary>Date the reviewer cancelled the assignment</summary>
        Public Property DateCancelled As DateTime?

        ''' <summary>Date the assignment was completed</summary>
        Public Property DateCompleted As DateTime?

        ''' <summary>Date the reviewer confirmed the assignment to the submission</summary>
        Public Property DateConfirmed As DateTime?

        ''' <summary>Date the editor confirmed the review</summary>
        Public Property DateConsidered As DateTime?

        ''' <summary>Date the review assignment should be completed</summary>
        Public Property DateDue As DateTime?

        ''' <summary>Date the reviewer was notified about the assignment</summary>
        Public Property DateNotified As DateTime?

        ''' <summary>Date the submission was rated by the reviewer</summary>
        Public Property DateRated As DateTime?

        ''' <summary>Date the reviewer was reminded about the assignment</summary>
        Public Property DateReminded As DateTime?

        ''' <summary>Date until which the response from reviewer is expected</summary>
        Public Property DateResponseDue As DateTime?

        ''' <summary>Whether review assignment was declined by the reviewer</summary>
        Public Property Declined As Boolean?

        ''' <summary>A URL to this object in the REST API. Read-only.</summary>
        <JsonPropertyName("_href")>
        Public Property Href As String

        ''' <summary>The ID of this review assignment Read-only.</summary>
        Public Property Id As Integer?

        ''' <summary>Date of the most recent assignment update Read-only.</summary>
        Public Property LastModified As DateTime?

        ''' <summary>Put Code for a review work submitted to ORCID.</summary>
        Public Property OrcidReviewPutCode As String

        ''' <summary>Publication title.</summary>
        Public Property PublicationTitle As LocalizedString

        ''' <summary>Assessment of the review quality</summary>
        Public Property Quality As Integer?

        ''' <summary>Reviewer's recommendation</summary>
        Public Property Recommendation As Integer?

        ''' <summary>Whether the assignment reminder for the reviewer was sent automatically</summary>
        Public Property ReminderWasAutomatic As Boolean?

        ''' <summary>Whether the review request was resent to the reviewer</summary>
        Public Property RequestResent As Boolean?

        ''' <summary>The full name of the reviewer Read-only.</summary>
        Public Property ReviewerFullName As String

        ''' <summary>Reviewer's unique ID</summary>
        Public Property ReviewerId As Integer?

        ''' <summary>The ID of the custom review form</summary>
        Public Property ReviewFormId As Integer?

        ''' <summary>The method used in the review. One of the SUBMISSION_REVIEW_METHOD_ constants</summary>
        Public Property ReviewMethod As Integer?

        ''' <summary>The ID of the review round</summary>
        Public Property ReviewRoundId As Integer?

        ''' <summary>The round of the review stage</summary>
        Public Property Round As Integer?

        ''' <summary>The ID of the review stage: external or internal review</summary>
        Public Property StageId As WorkflowStage?

        ''' <summary>The status of the review assignment. One of the REVIEW_ASSIGNMENT_STATUS_ constants</summary>
        Public Property Status As Integer?

        ''' <summary>The current step of the reviewer form</summary>
        Public Property [Step] As Integer?

        ''' <summary>The ID of the submission associated with the assignment</summary>
        Public Property SubmissionId As Integer?

        ''' <summary>The locale of the submission associated with the assignment</summary>
        Public Property SubmissionLocale As String

        ''' <summary>The stage of the editorial workflow that this submission is currently in. One of the <c>WORKFLOW_STAGE_ID_</c> constants. Default is <c>WORKFLOW_STAGE_ID_SUBMISSION</c>. Read-only.</summary>
        Public Property SubmissionStageId As WorkflowStage?

        ''' <summary>
        ''' Properties returned by the server that this class does not model, such as those added by plugins
        ''' (<c>crossrefplugin::status</c>, <c>dataciteplugin::registeredDoi</c>, …).
        ''' </summary>
        <JsonExtensionData>
        Public Property AdditionalProperties As Dictionary(Of String, JsonElement)

    End Class

    ''' <summary>A reviewer suggested by the author during submission.</summary>
    Partial Public Class ReviewerSuggestion

        ''' <summary>Affiliation.</summary>
        Public Property Affiliation As LocalizedString

        ''' <summary>The date the suggestion was approved by an editor (ISO 8601 date-time, or <c>null</c> if not approved).</summary>
        Public Property ApprovedAt As String

        ''' <summary>Display initials.</summary>
        Public Property DisplayInitials As LocalizedString

        ''' <summary>Email.</summary>
        Public Property Email As String

        ''' <summary>If the suggested reviewer matches an existing user in the system, the user ID of that user. Read-only.</summary>
        Public Property ExistingUserId As Integer?

        ''' <summary>Family name.</summary>
        Public Property FamilyName As LocalizedString

        ''' <summary>Full name.</summary>
        Public Property FullName As LocalizedString

        ''' <summary>Given name.</summary>
        Public Property GivenName As LocalizedString

        ''' <summary>Whether the matching existing user already has a reviewer role in the current context. Read-only.</summary>
        Public Property HasExistingReviewerRole As Boolean?

        ''' <summary>Id. Read-only.</summary>
        Public Property Id As Integer?

        ''' <summary>ORCID ID.</summary>
        Public Property OrcidId As String

        ''' <summary>Reviewer.</summary>
        Public Property Reviewer As ReviewerSummary

        ''' <summary>The user ID assigned to review this submission once the suggestion has been accepted and a reviewer assignment has been created. Read-only.</summary>
        Public Property ReviewerId As Integer?

        ''' <summary>Submission ID.</summary>
        Public Property SubmissionId As Integer?

        ''' <summary>The ID of the user who created this suggestion (typically the submitting author). Read-only.</summary>
        Public Property SuggestingUserId As Integer?

        ''' <summary>Suggestion reason.</summary>
        Public Property SuggestionReason As LocalizedString

        ''' <summary>
        ''' Properties returned by the server that this class does not model, such as those added by plugins
        ''' (<c>crossrefplugin::status</c>, <c>dataciteplugin::registeredDoi</c>, …).
        ''' </summary>
        <JsonExtensionData>
        Public Property AdditionalProperties As Dictionary(Of String, JsonElement)

    End Class

    ''' <summary>The writable properties of a reviewer suggestion. Only properties you set are sent.</summary>
    Partial Public Class ReviewerSuggestionEdit

        ''' <summary>Affiliation.</summary>
        Public Property Affiliation As LocalizedString

        ''' <summary>Email.</summary>
        Public Property Email As String

        ''' <summary>Family name.</summary>
        Public Property FamilyName As LocalizedString

        ''' <summary>Given name.</summary>
        Public Property GivenName As LocalizedString

        ''' <summary>ORCID ID.</summary>
        Public Property OrcidId As String

        ''' <summary>Suggestion reason.</summary>
        Public Property SuggestionReason As LocalizedString

    End Class

    ''' <summary>
    ''' A reviewer as returned by <c>GET /users/reviewers</c>: the user's summary properties plus review statistics
    ''' (<see cref="User.ReviewsActive"/>, <see cref="User.ReviewsCompleted"/>, <see cref="User.ReviewerRating"/>, ...).
    ''' </summary>
    Partial Public Class ReviewerSummary
        Inherits User
    End Class

End Namespace
