Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports Library.Serialization

Namespace Models

    ''' <summary>A submission: the article as it moves through the editorial workflow. List endpoints only fill the summary properties; &lt;c&gt;GetAsync&lt;/c&gt; fills all of them.</summary>
    ''' <example>
    ''' <code>
    ''' Dim s = Await ojs.Submissions.GetAsync(342)
    ''' Console.WriteLine($"{s.Id} {s.StatusLabel} stage={s.StageId}")
    ''' Dim current = s.CurrentPublication
    ''' Console.WriteLine(current?.Title.GetValue(s.Locale))
    ''' </code>
    ''' </example>
    Partial Public Class Submission

        ''' <summary>The editorial decisions that are available for this stage.</summary>
        Public Property AvailableEditorialDecisions As List(Of EditorialDecisionOption)

        ''' <summary>Whether the current user has access to the publication metadata Read-only.</summary>
        Public Property CanCurrentUserChangeMetadata As Boolean?

        ''' <summary>Optional. Comments from the submitting author to the editors. This is only available for submissions that have not yet been submitted. Once submitted, the message is converted to a discussion and you can not read or write to this property.</summary>
        Public Property CommentsForTheEditors As String

        ''' <summary>The ID of the context this submission was created in. Read-only.</summary>
        Public Property ContextId As Integer?

        ''' <summary>Which publication is the latest published version. Read-only.</summary>
        Public Property CurrentPublicationId As Integer?

        ''' <summary>The last time activity was recorded related to this submission. Read-only.</summary>
        Public Property DateLastActivity As DateTime?

        ''' <summary>The date this submission was submitted. Read-only.</summary>
        Public Property DateSubmitted As DateTime?

        ''' <summary>Whether deciding editor has been assigned to the submission. Read-only.</summary>
        Public Property EditorAssigned As Boolean?

        ''' <summary>A URL to this object in the REST API. Read-only.</summary>
        <JsonPropertyName("_href")>
        Public Property Href As String

        ''' <summary>The ID of this submission. Read-only.</summary>
        Public Property Id As Integer?

        ''' <summary>The issue this submission is assigned to be published in, if any.</summary>
        Public Property IssueToBePublished As IssueReference

        ''' <summary>The last time a modification was made to this submission or any of its associated objects. Read-only.</summary>
        Public Property LastModified As DateTime?

        ''' <summary>The primary language of this submission.</summary>
        Public Property Locale As String

        ''' <summary>The locales metadata can be entered in, as locale code =&gt; locale name. Read-only.</summary>
        Public Property MetadataLocales As Dictionary(Of String, String)

        ''' <summary>A list of users who are assigned to this submission (e.g. authors, editors, or other stage participants). Read-only.</summary>
        Public Property Participants As List(Of SubmissionParticipant)

        ''' <summary>A list of publications that have been created for this submission. Read-only.</summary>
        Public Property Publications As List(Of Publication)

        ''' <summary>Whether all recommending editors have submitted a recommendation. Read-only.</summary>
        Public Property RecommendationsIn As Boolean?

        ''' <summary>Information about pending and completed review assignments. Read-only.</summary>
        Public Property ReviewAssignments As List(Of SubmissionReviewAssignment)

        ''' <summary>Whether all needed reviewers are assigned to the submission. Related to the <c>numReviewsPerSubmission</c> context setting. Read-only.</summary>
        Public Property ReviewersNotAssigned As Boolean?

        ''' <summary>Information about suggested reviewers at the time of submission. Read-only.</summary>
        Public Property ReviewerSuggestions As List(Of ReviewerSuggestion)

        ''' <summary>A list of review rounds that have been opened for this submission. Read-only.</summary>
        Public Property ReviewRounds As List(Of ReviewRound)

        ''' <summary>There is a revisions requested decision in the current review round and no revisions have been uploaded to that round. Read-only.</summary>
        Public Property RevisionsRequested As Boolean?

        ''' <summary>There is a revisions requested decision in the current review round and 1 or more revision files have been uploaded to that round. Read-only.</summary>
        Public Property RevisionsSubmitted As Boolean?

        ''' <summary>The submission status is <c>STATUS_SCHEDULED</c>. It shows the issue it is scheduled in. Read-only.</summary>
        Public Property ScheduledIn As Integer?

        ''' <summary>The stage of the editorial workflow that this submission is currently in. One of the <c>WORKFLOW_STAGE_ID_</c> constants. Default is <c>WORKFLOW_STAGE_ID_SUBMISSION</c>. Read-only.</summary>
        Public Property StageId As WorkflowStage?

        ''' <summary>Key data about the status, files and discussions of each stage. Read-only.</summary>
        Public Property Stages As List(Of SubmissionStage)

        ''' <summary>Whether the submission is Published, Declined, Scheduled or Queued (still in the workflow). One of the <c>PKPSubmission::STATUS_*</c> constants. Default is <c>PKPSubmission::STATUS_QUEUED</c>. Read-only.</summary>
        Public Property Status As SubmissionStatus?

        ''' <summary>A human-readable version of the submission's status. It will be Published, Declined, Queued (still in the workflow) or Scheduled. Read-only.</summary>
        Public Property StatusLabel As String

        ''' <summary>This value tracks the submission's progress in the submission wizard. It will be set to <c>start</c> or the current step when a submission is in progress. After it is submitted, this will be an empty string, indicating it is ready for an editor's consideration. Read-only.</summary>
        Public Property SubmissionProgress As String

        ''' <summary>A URL to this submission for the author's editorial workflow. Read-only.</summary>
        Public Property UrlAuthorWorkflow As String

        ''' <summary>A URL to this submission for the editors' and assistants' editorial workflow. Read-only.</summary>
        Public Property UrlEditorialWorkflow As String

        ''' <summary>A URL to view the published version of this submission. If it is not published, the URL will point to the location where it will be published in the future. Read-only.</summary>
        Public Property UrlPublished As String

        ''' <summary>A URL to the submission wizard for this submission. The submission wizard can not be accessed after a submission has been submitted. Read-only.</summary>
        Public Property UrlSubmissionWizard As String

        ''' <summary>A URL to this submission for the workflow most appropriate for the current user. It will match the <c>urlAuthorWorkflow</c> or <c>urlEditorialWorkflow</c>, depending on what roles the user is assigned. Read-only.</summary>
        Public Property UrlWorkflow As String

        ''' <summary>
        ''' Properties returned by the server that this class does not model, such as those added by plugins
        ''' (<c>crossrefplugin::status</c>, <c>dataciteplugin::registeredDoi</c>, …).
        ''' </summary>
        <JsonExtensionData>
        Public Property AdditionalProperties As Dictionary(Of String, JsonElement)

    End Class

    ''' <summary>The writable properties of a submission, for &lt;c&gt;CreateAsync&lt;/c&gt; and &lt;c&gt;UpdateAsync&lt;/c&gt;. Only properties you set are sent.</summary>
    ''' <example>
    ''' <code>
    ''' Dim created = Await ojs.Submissions.CreateAsync(New SubmissionEdit With {
    '''     .Locale = "en", .SectionId = 1, .CommentsForTheEditors = "Thanks!"})
    ''' </code>
    ''' </example>
    Partial Public Class SubmissionEdit

        ''' <summary>Optional. Comments from the submitting author to the editors. This is only available for submissions that have not yet been submitted. Once submitted, the message is converted to a discussion and you can not read or write to this property.</summary>
        Public Property CommentsForTheEditors As String

        ''' <summary>The primary language of this submission.</summary>
        Public Property Locale As String

        ''' <summary>The section this submission should be assigned to. This can only be passed when creating a new submission and the data is assigned to the first publication. After the submission is created, edit the publication to change the <c>sectionId</c>.</summary>
        Public Property SectionId As Integer?

        ''' <summary>Optional. The submitting user will be assigned to the submission in the user group under which they submit. Leave this empty to have the user automatically assigned as an author, when an author user group allows self-registration. Changing this property will have no effect after a submission is submitted.</summary>
        Public Property UserGroupId As Integer?

    End Class

End Namespace
