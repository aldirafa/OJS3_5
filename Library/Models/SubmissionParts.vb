Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports Library.Serialization

Namespace Models

    ''' <summary>An editorial decision the current user can take in a stage (<c>availableEditorialDecisions</c>).</summary>
    ''' <example>
    ''' <code>
    ''' Dim s = Await ojs.Submissions.GetAsync(12)
    ''' For Each o In s.AvailableEditorialDecisions : Console.WriteLine($"{o.Id}: {o.Label}") : Next
    ''' </code>
    ''' </example>
    Public Class EditorialDecisionOption

        ''' <summary>The decision type (one of the <c>Decision::*</c> constants).</summary>
        Public Property Id As Integer?

        ''' <summary>The workflow stage the decision applies to.</summary>
        Public Property StageId As WorkflowStage?

        ''' <summary>A human-readable label, e.g. <c>Accept Submission</c>.</summary>
        Public Property Label As String

    End Class

    ''' <summary>A lightweight reference to an issue (<c>issueToBePublished</c>).</summary>
    ''' <example>
    ''' <code>
    ''' Dim s = Await ojs.Submissions.GetAsync(12)
    ''' Console.WriteLine(s.IssueToBePublished?.Label)
    ''' </code>
    ''' </example>
    Public Class IssueReference

        ''' <summary>The issue ID.</summary>
        Public Property Id As Integer?

        ''' <summary>The issue identification, e.g. <c>Vol. 8 No. 1 (2023)</c>.</summary>
        Public Property Label As String

    End Class

    ''' <summary>A user assigned to a submission, as listed in <see cref="Submission.Participants"/>.</summary>
    ''' <example>
    ''' <code>
    ''' Dim s = Await ojs.Submissions.GetAsync(12)
    ''' For Each p In s.Participants : Console.WriteLine(p.FullName) : Next
    ''' </code>
    ''' </example>
    Public Class SubmissionParticipant

        ''' <summary>The user ID.</summary>
        Public Property Id As Integer?

        ''' <summary>The user's full name.</summary>
        Public Property FullName As String

        ''' <summary>Whether the current user may log in as this user.</summary>
        Public Property CanLoginAs As Boolean?

    End Class

    ''' <summary>Key information about one workflow stage of a submission (<see cref="Submission.Stages"/>).</summary>
    ''' <example>
    ''' <code>
    ''' Dim s = Await ojs.Submissions.GetAsync(12)
    ''' For Each st In s.Stages : Console.WriteLine($"{st.Label}: active={st.IsActiveStage}") : Next
    ''' </code>
    ''' </example>
    Public Class SubmissionStage

        ''' <summary>The stage.</summary>
        Public Property Id As WorkflowStage?

        ''' <summary>The stage name, e.g. <c>Review</c>.</summary>
        Public Property Label As String

        ''' <summary>Whether the submission is currently in this stage.</summary>
        Public Property IsActiveStage As Boolean?

        ''' <summary>The role IDs the current user is assigned with in this stage.</summary>
        Public Property CurrentUserAssignedRoles As List(Of Integer)

        ''' <summary>Whether an editor is assigned to this stage.</summary>
        Public Property EditorAssigned As Boolean?

        ''' <summary>Whether a deciding editor is assigned to this stage.</summary>
        Public Property IsDecidingEditorAssigned As Boolean?

        ''' <summary>Whether the current user is the deciding editor.</summary>
        Public Property IsCurrentUserDecidingEditor As Boolean?

        ''' <summary>Whether the current user can only recommend (not decide).</summary>
        Public Property CurrentUserCanRecommendOnly As Boolean?

        ''' <summary>The current user's recommendation, if any.</summary>
        Public Property CurrentUserRecommendation As JsonElement?

        ''' <summary>Recommendations made in this stage.</summary>
        Public Property Recommendations As JsonElement?

        ''' <summary>The number of files uploaded in this stage.</summary>
        Public Property UploadedFilesCount As Integer?

        ''' <summary>Other properties returned by the server.</summary>
        <JsonExtensionData>
        Public Property AdditionalProperties As Dictionary(Of String, JsonElement)

    End Class

    ''' <summary>Summary information about a review assignment, as listed in <see cref="Submission.ReviewAssignments"/>.</summary>
    ''' <example>
    ''' <code>
    ''' Dim s = Await ojs.Submissions.GetAsync(12)
    ''' For Each r In s.ReviewAssignments : Console.WriteLine($"{r.ReviewerFullName}: {r.Status} (due {r.DateDue})") : Next
    ''' </code>
    ''' </example>
    Public Class SubmissionReviewAssignment

        ''' <summary>The review assignment ID.</summary>
        Public Property Id As Integer?

        ''' <summary>Whether the current user is the assigned reviewer.</summary>
        Public Property IsCurrentUserAssigned As Boolean?

        ''' <summary>The <c>REVIEW_ASSIGNMENT_STATUS_*</c> constant.</summary>
        Public Property StatusId As Integer?

        ''' <summary>A human-readable description of the status.</summary>
        Public Property Status As String

        ''' <summary>When the review is due.</summary>
        Public Property DateDue As DateOnly?

        ''' <summary>When the reviewer must respond to the request.</summary>
        Public Property DateResponseDue As DateOnly?

        ''' <summary>When the reviewer accepted the request.</summary>
        Public Property DateConfirmed As DateOnly?

        ''' <summary>When the review was completed.</summary>
        Public Property DateCompleted As DateOnly?

        ''' <summary>When the editor considered the review.</summary>
        Public Property DateConsidered As DateOnly?

        ''' <summary>When the reviewer was assigned.</summary>
        Public Property DateAssigned As DateOnly?

        ''' <summary>When the assignment was cancelled.</summary>
        Public Property DateCancelled As DateOnly?

        ''' <summary>The reviewer's competing interests statement.</summary>
        Public Property CompetingInterests As String

        ''' <summary>The review round number.</summary>
        Public Property Round As Integer?

        ''' <summary>The review round ID.</summary>
        Public Property RoundId As Integer?

        ''' <summary>The reviewer's recommendation (<c>SUBMISSION_REVIEWER_RECOMMENDATION_*</c>).</summary>
        Public Property Recommendation As Integer?

        ''' <summary>The reviewer's user ID.</summary>
        Public Property ReviewerId As Integer?

        ''' <summary>The reviewer's full name.</summary>
        Public Property ReviewerFullName As String

        ''' <summary>The reviewer's initials.</summary>
        Public Property ReviewerDisplayInitials As String

        ''' <summary>The review method (<c>SUBMISSION_REVIEW_METHOD_*</c>: 1 anonymous, 2 double-anonymous, 3 open).</summary>
        Public Property ReviewMethod As Integer?

        ''' <summary>Whether the reviewer has an ORCID iD.</summary>
        Public Property ReviewerHasOrcid As Boolean?

        ''' <summary>Whether the current user may log in as the reviewer.</summary>
        Public Property CanLoginAs As Boolean?

        ''' <summary>Whether the current user may add editorial notes about the reviewer.</summary>
        Public Property CanGossip As Boolean?

        ''' <summary>Other properties returned by the server.</summary>
        <JsonExtensionData>
        Public Property AdditionalProperties As Dictionary(Of String, JsonElement)

    End Class

    ''' <summary>A previous revision of a submission file.</summary>
    ''' <example>
    ''' <code>
    ''' Dim f = Await ojs.SubmissionFiles.GetAsync(12, 40, WorkflowStage.Production)
    ''' For Each r In f.Revisions : Console.WriteLine($"{r.FileId} {r.Mimetype} {r.Url}") : Next
    ''' </code>
    ''' </example>
    Public Class SubmissionFileRevision

        ''' <summary>The underlying file ID.</summary>
        Public Property FileId As Integer?

        ''' <summary>The detected document type.</summary>
        Public Property DocumentType As String

        ''' <summary>The MIME type.</summary>
        Public Property Mimetype As String

        ''' <summary>The storage path.</summary>
        Public Property Path As String

        ''' <summary>The download URL.</summary>
        Public Property Url As String

    End Class

    ''' <summary>
    ''' A cover image of a publication. When writing, set <see cref="TemporaryFileId"/> to an uploaded temporary file
    ''' (see <see cref="Resources.TemporaryFilesResource"/>).
    ''' </summary>
    ''' <example>
    ''' <code>
    ''' Dim tmp = Await ojs.TemporaryFiles.UploadAsync("cover.jpg")
    ''' Await ojs.Publications.UpdateAsync(12, 15, New PublicationEdit With {
    '''     .CoverImage = New Dictionary(Of String, CoverImage) From {{"en", New CoverImage With {.TemporaryFileId = tmp.Id, .AltText = "Cover"}}}})
    ''' </code>
    ''' </example>
    Public Class CoverImage

        ''' <summary>The stored file name.</summary>
        Public Property UploadName As String

        ''' <summary>When the image was uploaded.</summary>
        Public Property DateUploaded As String

        ''' <summary>Alternative text for the image.</summary>
        Public Property AltText As String

        ''' <summary>When uploading a new image: the temporary file ID.</summary>
        Public Property TemporaryFileId As Integer?

    End Class

End Namespace
