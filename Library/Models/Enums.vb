Namespace Models

    ''' <summary>Editorial workflow stages (<c>WORKFLOW_STAGE_ID_*</c> constants).</summary>
    ''' <remarks>Unknown values sent by the server are preserved as their numeric value.</remarks>
    Public Enum WorkflowStage
        ''' <summary><c>WORKFLOW_STAGE_ID_SUBMISSION</c></summary>
        Submission = 1
        ''' <summary><c>WORKFLOW_STAGE_ID_INTERNAL_REVIEW</c> (not used by OJS)</summary>
        InternalReview = 2
        ''' <summary><c>WORKFLOW_STAGE_ID_EXTERNAL_REVIEW</c></summary>
        ExternalReview = 3
        ''' <summary><c>WORKFLOW_STAGE_ID_EDITING</c> (copyediting)</summary>
        Editing = 4
        ''' <summary><c>WORKFLOW_STAGE_ID_PRODUCTION</c></summary>
        Production = 5
    End Enum

    ''' <summary>Submission and publication status (<c>PKPSubmission::STATUS_*</c> constants).</summary>
    Public Enum SubmissionStatus
        ''' <summary>Still in the editorial workflow (<c>STATUS_QUEUED</c>).</summary>
        Queued = 1
        ''' <summary>Published (<c>STATUS_PUBLISHED</c>).</summary>
        Published = 3
        ''' <summary>Declined (<c>STATUS_DECLINED</c>).</summary>
        Declined = 4
        ''' <summary>Scheduled for publication in a future issue (<c>STATUS_SCHEDULED</c>).</summary>
        Scheduled = 5
    End Enum

    ''' <summary>Where a submission file lives in the workflow (<c>SubmissionFile::SUBMISSION_FILE_*</c> constants).</summary>
    Public Enum SubmissionFileStage
        ''' <summary>Files uploaded with the submission.</summary>
        Submission = 2
        ''' <summary>Files attached to a discussion note.</summary>
        Note = 3
        ''' <summary>Files sent out for review.</summary>
        ReviewFile = 4
        ''' <summary>Files uploaded by reviewers.</summary>
        ReviewAttachment = 5
        ''' <summary>Final (draft) files ready for copyediting.</summary>
        Final = 6
        ''' <summary>Copyedited files.</summary>
        Copyedit = 9
        ''' <summary>Galley/proof files.</summary>
        Proof = 10
        ''' <summary>Production-ready files.</summary>
        ProductionReady = 11
        ''' <summary>Attachments.</summary>
        Attachment = 13
        ''' <summary>Revisions uploaded during external review.</summary>
        ReviewRevision = 15
        ''' <summary>Dependent files (e.g. images used by an HTML galley).</summary>
        Dependent = 17
        ''' <summary>Files attached to queries.</summary>
        Query = 18
        ''' <summary>Files sent out for internal review.</summary>
        InternalReviewFile = 19
        ''' <summary>Revisions uploaded during internal review.</summary>
        InternalReviewRevision = 20
        ''' <summary>JATS XML source files.</summary>
        Jats = 21
    End Enum

    ''' <summary>DOI deposit status (<c>Doi::STATUS_*</c> constants).</summary>
    Public Enum DoiStatus
        ''' <summary>Not yet deposited.</summary>
        Unregistered = 1
        ''' <summary>Submitted to the registration agency.</summary>
        Submitted = 2
        ''' <summary>Successfully registered.</summary>
        Registered = 3
        ''' <summary>The deposit failed.</summary>
        [Error] = 4
        ''' <summary>Registered, but the metadata changed and needs to be redeposited.</summary>
        Stale = 5
    End Enum

    ''' <summary>User roles (<c>Role::ROLE_ID_*</c> constants), used by user groups and user filters.</summary>
    Public Enum UserRole
        ''' <summary>Site administrator.</summary>
        SiteAdmin = 1
        ''' <summary>Journal manager.</summary>
        Manager = 16
        ''' <summary>Section editor.</summary>
        SubEditor = 17
        ''' <summary>Reviewer.</summary>
        Reviewer = 4096
        ''' <summary>Assistant (copyeditor, layout editor, proofreader, ...).</summary>
        Assistant = 4097
        ''' <summary>Author.</summary>
        Author = 65536
        ''' <summary>Reader.</summary>
        Reader = 1048576
        ''' <summary>Subscription manager.</summary>
        SubscriptionManager = 2097152
    End Enum

    ''' <summary>Peer review methods (<c>ReviewAssignment::SUBMISSION_REVIEW_METHOD_*</c> constants).</summary>
    Public Enum ReviewMethod
        ''' <summary>The author's identity is hidden from the reviewer.</summary>
        Anonymous = 1
        ''' <summary>The author and reviewer identities are hidden from each other.</summary>
        DoubleAnonymous = 2
        ''' <summary>Identities are visible to both sides.</summary>
        Open = 3
    End Enum

    ''' <summary>How a journal makes its content available (<c>Journal::PUBLISHING_MODE_*</c> constants).</summary>
    Public Enum PublishingMode
        ''' <summary>Open access.</summary>
        Open = 0
        ''' <summary>Subscription-based access.</summary>
        Subscription = 1
        ''' <summary>The journal does not publish content on this site.</summary>
        None = 2
    End Enum

    ''' <summary>Sort direction for list endpoints.</summary>
    Public Enum SortDirection
        ''' <summary>Ascending (<c>ASC</c>).</summary>
        Ascending
        ''' <summary>Descending (<c>DESC</c>).</summary>
        Descending
    End Enum

End Namespace
