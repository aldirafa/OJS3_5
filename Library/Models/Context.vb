Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports OJS3_5.Library.Serialization

Namespace Models

    ''' <summary>A journal (called a context in OJS) and its settings. &lt;c&gt;ListAsync&lt;/c&gt; only fills the summary properties; &lt;c&gt;GetAsync&lt;/c&gt; fills all of them.</summary>
    ''' <example>
    ''' <code>
    ''' Dim journals = Await ojs.Contexts.ListAllAsync()
    ''' For Each j In journals
    '''     Console.WriteLine($"{j.Id} {j.UrlPath}: {j.Name.GetValue("en")}")
    ''' Next
    ''' </code>
    ''' </example>
    Partial Public Class Context

        ''' <summary>Abbreviation.</summary>
        Public Property Abbreviation As LocalizedString

        ''' <summary>About.</summary>
        Public Property About As LocalizedString

        ''' <summary>Acronym.</summary>
        Public Property Acronym As LocalizedString

        ''' <summary>Additional home content.</summary>
        Public Property AdditionalHomeContent As LocalizedString

        ''' <summary>Enable agencies metadata. <c>0</c> is disabled. <c>enable</c> will make it available in the workflow. <c>request</c> will allow an author to enter a value during submission. <c>require</c> will require that the author enter a value during submission.</summary>
        Public Property Agencies As String

        ''' <summary>Announcements introduction.</summary>
        Public Property AnnouncementsIntroduction As LocalizedString

        ''' <summary>Author guidelines.</summary>
        Public Property AuthorGuidelines As LocalizedString

        ''' <summary>Author information.</summary>
        Public Property AuthorInformation As LocalizedString

        ''' <summary>Automatic DOI deposit.</summary>
        Public Property AutomaticDoiDeposit As Boolean?

        ''' <summary>Begin submission help.</summary>
        Public Property BeginSubmissionHelp As LocalizedString

        ''' <summary>Enable citations metadata. <c>0</c> is disabled. <c>enable</c> will make it available in the workflow. <c>request</c> will allow an author to enter a value during submission. <c>require</c> will require that the author enter a value during submission.</summary>
        Public Property Citations As String

        ''' <summary>Clockss license.</summary>
        Public Property ClockssLicense As LocalizedString

        ''' <summary>Competing interests.</summary>
        Public Property CompetingInterests As LocalizedString

        ''' <summary>Contact affiliation.</summary>
        Public Property ContactAffiliation As LocalizedString

        ''' <summary>Contact email.</summary>
        Public Property ContactEmail As String

        ''' <summary>Contact name.</summary>
        Public Property ContactName As String

        ''' <summary>Contact phone.</summary>
        Public Property ContactPhone As String

        ''' <summary>Contributors help.</summary>
        Public Property ContributorsHelp As LocalizedString

        ''' <summary>Copyright holder other.</summary>
        Public Property CopyrightHolderOther As LocalizedString

        ''' <summary>Copyright holder type.</summary>
        Public Property CopyrightHolderType As String

        ''' <summary>Copyright notice.</summary>
        Public Property CopyrightNotice As LocalizedString

        ''' <summary>Copyright year basis.</summary>
        Public Property CopyrightYearBasis As String

        ''' <summary>A copy of the submission acknowledgement email will be sent to any of the email addresses entered here. Separate multiple email addresses with a comma. Example: one@example.com,two@example.com</summary>
        Public Property CopySubmissionAckAddress As String

        ''' <summary>A copy of the submission acknowledgement email will be sent to this context's primary contact when true.</summary>
        Public Property CopySubmissionAckPrimaryContact As Boolean?

        ''' <summary>Country.</summary>
        Public Property Country As String

        ''' <summary>Enable coverage metadata. <c>0</c> is disabled. <c>enable</c> will make it available in the workflow. <c>request</c> will allow an author to enter a value during submission. <c>require</c> will require that the author enter a value during submission.</summary>
        Public Property Coverage As String

        ''' <summary>Currency.</summary>
        Public Property Currency As String

        ''' <summary>Current issue ID.</summary>
        Public Property CurrentIssueId As Integer?

        ''' <summary>Custom headers.</summary>
        Public Property CustomHeaders As LocalizedString

        ''' <summary>Whether to not request, request or require a Data Availability Statement from the authors.</summary>
        Public Property DataAvailability As String

        ''' <summary>Date format long.</summary>
        Public Property DateFormatLong As LocalizedString

        ''' <summary>Date format short.</summary>
        Public Property DateFormatShort As LocalizedString

        ''' <summary>Datetime format long.</summary>
        Public Property DatetimeFormatLong As LocalizedString

        ''' <summary>Datetime format short.</summary>
        Public Property DatetimeFormatShort As LocalizedString

        ''' <summary>Default metric type.</summary>
        Public Property DefaultMetricType As String

        ''' <summary>The default review method for new review assignments.</summary>
        Public Property DefaultReviewMode As ReviewMethod?

        ''' <summary>Delayed open access duration.</summary>
        Public Property DelayedOpenAccessDuration As Integer?

        ''' <summary>Delayed open access policy.</summary>
        Public Property DelayedOpenAccessPolicy As LocalizedString

        ''' <summary>Description.</summary>
        Public Property Description As LocalizedString

        ''' <summary>Details help.</summary>
        Public Property DetailsHelp As LocalizedString

        ''' <summary>User groups that a manager will be prevented from sending bulk email to using the notify feature. Only the admin may edit this property.</summary>
        Public Property DisableBulkEmailUserGroups As List(Of Integer)

        ''' <summary>Disable submissions.</summary>
        Public Property DisableSubmissions As Boolean?

        ''' <summary>Disable user reg.</summary>
        Public Property DisableUserReg As Boolean?

        ''' <summary>Enable disciplines metadata. <c>0</c> is disabled. <c>enable</c> will make it available in the workflow. <c>request</c> will allow an author to enter a value during submission. <c>require</c> will require that the author enter a value during submission.</summary>
        Public Property Disciplines As String

        ''' <summary>DOI creation time.</summary>
        Public Property DoiCreationTime As String

        ''' <summary>DOI issue suffix pattern.</summary>
        Public Property DoiIssueSuffixPattern As String

        ''' <summary>DOI prefix.</summary>
        Public Property DoiPrefix As String

        ''' <summary>DOI publication suffix pattern.</summary>
        Public Property DoiPublicationSuffixPattern As String

        ''' <summary>DOI representation suffix pattern.</summary>
        Public Property DoiRepresentationSuffixPattern As String

        ''' <summary>DOI suffix type.</summary>
        Public Property DoiSuffixType As String

        ''' <summary>Whether each publication version should receive a unique DOI</summary>
        Public Property DoiVersioning As Boolean?

        ''' <summary>Editorial history.</summary>
        Public Property EditorialHistory As LocalizedString

        ''' <summary>Whether or not to send the monthly editorial statistics email to editors. Default: <c>true</c></summary>
        Public Property EditorialStatsEmail As Boolean?

        ''' <summary>Email signature.</summary>
        Public Property EmailSignature As String

        ''' <summary>Enable announcements.</summary>
        Public Property EnableAnnouncements As Boolean?

        ''' <summary>Enable clockss.</summary>
        Public Property EnableClockss As Boolean?

        ''' <summary>Enabled.</summary>
        Public Property Enabled As Boolean?

        ''' <summary>Enabled DOI types.</summary>
        Public Property EnabledDoiTypes As List(Of String)

        ''' <summary>Enable DOIs.</summary>
        Public Property EnableDois As Boolean?

        ''' <summary>Masthead setting applied to users enrolled in editorial roles (OJS 3.5+).</summary>
        Public Property EnableEnrollmentMasthead As Boolean?

        ''' <summary>Masthead setting applied to users enrolled as reviewers (OJS 3.5+).</summary>
        Public Property EnableEnrollmentMastheadReviewers As Boolean?

        ''' <summary>Whether to track usage statistics by location, and whether to track at the country, region or city level.</summary>
        Public Property EnableGeoUsageStats As String

        ''' <summary>Whether to track usage statistics by institutions. Requires institutional IP ranges to be configured.</summary>
        Public Property EnableInstitutionUsageStats As Boolean?

        ''' <summary>Enable lockss.</summary>
        Public Property EnableLockss As Boolean?

        ''' <summary>Enable oai.</summary>
        Public Property EnableOai As Boolean?

        ''' <summary>Enable open access notification.</summary>
        Public Property EnableOpenAccessNotification As Boolean?

        ''' <summary>Enable publisher ID.</summary>
        Public Property EnablePublisherId As List(Of String)

        ''' <summary>Enable subscription online payment notification purchase individual.</summary>
        Public Property EnableSubscriptionOnlinePaymentNotificationPurchaseIndividual As Boolean?

        ''' <summary>Enable subscription online payment notification purchase institutional.</summary>
        Public Property EnableSubscriptionOnlinePaymentNotificationPurchaseInstitutional As Boolean?

        ''' <summary>Enable subscription online payment notification renew individual.</summary>
        Public Property EnableSubscriptionOnlinePaymentNotificationRenewIndividual As Boolean?

        ''' <summary>Enable subscription online payment notification renew institutional.</summary>
        Public Property EnableSubscriptionOnlinePaymentNotificationRenewInstitutional As Boolean?

        ''' <summary>Envelope sender.</summary>
        Public Property EnvelopeSender As String

        ''' <summary>The favicon for each locale.</summary>
        Public Property Favicon As Dictionary(Of String, ImageFile)

        ''' <summary>For the editors help.</summary>
        Public Property ForTheEditorsHelp As LocalizedString

        ''' <summary>The homepage image for each locale.</summary>
        Public Property HomepageImage As Dictionary(Of String, ImageFile)

        ''' <summary>Href. Read-only.</summary>
        <JsonPropertyName("_href")>
        Public Property Href As String

        ''' <summary>Id. Read-only.</summary>
        Public Property Id As Integer?

        ''' <summary>Whether or not the access to the COUNTER SUSHI statistics API is public. If false, only users with admin or manager roles can access the API.</summary>
        Public Property IsSushiApiPublic As Boolean?

        ''' <summary>Items per page.</summary>
        Public Property ItemsPerPage As Integer?

        ''' <summary>The journal thumbnail for each locale.</summary>
        Public Property JournalThumbnail As Dictionary(Of String, ImageFile)

        ''' <summary>Enable keywords metadata. <c>0</c> is disabled. <c>enable</c> will make it available in the workflow. <c>request</c> will allow an author to enter a value during submission. <c>require</c> will require that the author enter a value during submission.</summary>
        Public Property Keywords As String

        ''' <summary>Librarian information.</summary>
        Public Property LibrarianInformation As LocalizedString

        ''' <summary>License terms.</summary>
        Public Property LicenseTerms As LocalizedString

        ''' <summary>License URL.</summary>
        Public Property LicenseUrl As String

        ''' <summary>Lockss license.</summary>
        Public Property LockssLicense As LocalizedString

        ''' <summary>Mailing address.</summary>
        Public Property MailingAddress As String

        ''' <summary>Masthead user group IDs.</summary>
        Public Property MastheadUserGroupIds As List(Of Integer)

        ''' <summary>Membership fee.</summary>
        Public Property MembershipFee As Double?

        ''' <summary>Membership fee description.</summary>
        Public Property MembershipFeeDescription As LocalizedString

        ''' <summary>Membership fee name.</summary>
        Public Property MembershipFeeName As LocalizedString

        ''' <summary>Name.</summary>
        Public Property Name As LocalizedString

        ''' <summary>When enabled, all authors of a submission will receive a notification when an editorial decision is made regarding a submission. When disabled, only authors assigned to the submission will be notified.</summary>
        Public Property NotifyAllAuthors As Boolean?

        ''' <summary>Num announcements homepage.</summary>
        Public Property NumAnnouncementsHomepage As Integer?

        ''' <summary>Num days after review response reminder due.</summary>
        Public Property NumDaysAfterReviewResponseReminderDue As Integer?

        ''' <summary>Num days after review submit reminder due.</summary>
        Public Property NumDaysAfterReviewSubmitReminderDue As Integer?

        ''' <summary>Num days before review response reminder due.</summary>
        Public Property NumDaysBeforeReviewResponseReminderDue As Integer?

        ''' <summary>Num days before review submit reminder due.</summary>
        Public Property NumDaysBeforeReviewSubmitReminderDue As Integer?

        ''' <summary>Num months after subscription expiry reminder.</summary>
        Public Property NumMonthsAfterSubscriptionExpiryReminder As Integer?

        ''' <summary>Num months before subscription expiry reminder.</summary>
        Public Property NumMonthsBeforeSubscriptionExpiryReminder As Integer?

        ''' <summary>Num page links.</summary>
        Public Property NumPageLinks As Integer?

        ''' <summary>Num reviews per submission.</summary>
        Public Property NumReviewsPerSubmission As Integer?

        ''' <summary>Num weeks after subscription expiry reminder.</summary>
        Public Property NumWeeksAfterSubscriptionExpiryReminder As Integer?

        ''' <summary>Num weeks before subscription expiry reminder.</summary>
        Public Property NumWeeksBeforeSubscriptionExpiryReminder As Integer?

        ''' <summary>Num weeks per response.</summary>
        Public Property NumWeeksPerResponse As Integer?

        ''' <summary>Num weeks per review.</summary>
        Public Property NumWeeksPerReview As Integer?

        ''' <summary>Online issn.</summary>
        Public Property OnlineIssn As String

        ''' <summary>Open access policy.</summary>
        Public Property OpenAccessPolicy As LocalizedString

        ''' <summary>ORCID api type.</summary>
        Public Property OrcidApiType As String

        ''' <summary>ORCID city.</summary>
        Public Property OrcidCity As String

        ''' <summary>ORCID client ID.</summary>
        Public Property OrcidClientId As String

        ''' <summary>ORCID client secret.</summary>
        Public Property OrcidClientSecret As String

        ''' <summary>Whether ORCID functionality is enabled</summary>
        Public Property OrcidEnabled As Boolean?

        ''' <summary>ORCID log level.</summary>
        Public Property OrcidLogLevel As String

        ''' <summary>ORCID send mail to authors on publication.</summary>
        Public Property OrcidSendMailToAuthorsOnPublication As Boolean?

        ''' <summary>Page footer.</summary>
        Public Property PageFooter As LocalizedString

        ''' <summary>The logo shown in the page header for each locale.</summary>
        Public Property PageHeaderLogoImage As Dictionary(Of String, ImageFile)

        ''' <summary>Payment plugin name.</summary>
        Public Property PaymentPluginName As String

        ''' <summary>Payments enabled.</summary>
        Public Property PaymentsEnabled As Boolean?

        ''' <summary>Primary locale.</summary>
        Public Property PrimaryLocale As String

        ''' <summary>Print issn.</summary>
        Public Property PrintIssn As String

        ''' <summary>Privacy statement.</summary>
        Public Property PrivacyStatement As LocalizedString

        ''' <summary>Publication fee.</summary>
        Public Property PublicationFee As Double?

        ''' <summary>Publication fee description.</summary>
        Public Property PublicationFeeDescription As LocalizedString

        ''' <summary>Publication fee name.</summary>
        Public Property PublicationFeeName As LocalizedString

        ''' <summary>Publisher institution.</summary>
        Public Property PublisherInstitution As String

        ''' <summary>Publisher URL.</summary>
        Public Property PublisherUrl As String

        ''' <summary>Whether content is open access, subscription-based or not published on the site.</summary>
        Public Property PublishingMode As PublishingMode?

        ''' <summary>Purchase article fee.</summary>
        Public Property PurchaseArticleFee As Double?

        ''' <summary>Purchase article fee description.</summary>
        Public Property PurchaseArticleFeeDescription As LocalizedString

        ''' <summary>Purchase article fee enabled.</summary>
        Public Property PurchaseArticleFeeEnabled As Boolean?

        ''' <summary>Purchase article fee name.</summary>
        Public Property PurchaseArticleFeeName As LocalizedString

        ''' <summary>Purchase issue fee.</summary>
        Public Property PurchaseIssueFee As Double?

        ''' <summary>Purchase issue fee enabled.</summary>
        Public Property PurchaseIssueFeeEnabled As Boolean?

        ''' <summary>Rate reviewer on quality.</summary>
        Public Property RateReviewerOnQuality As Boolean?

        ''' <summary>Reader information.</summary>
        Public Property ReaderInformation As LocalizedString

        ''' <summary>Registration agency.</summary>
        Public Property RegistrationAgency As String

        ''' <summary>Require author competing interests.</summary>
        Public Property RequireAuthorCompetingInterests As Boolean?

        ''' <summary>Restrict article access.</summary>
        Public Property RestrictArticleAccess As Boolean?

        ''' <summary>Restrict only pdf.</summary>
        Public Property RestrictOnlyPdf As Boolean?

        ''' <summary>Restrict reviewer file access.</summary>
        Public Property RestrictReviewerFileAccess As Boolean?

        ''' <summary>Restrict site access.</summary>
        Public Property RestrictSiteAccess As Boolean?

        ''' <summary>Reviewer access keys enabled.</summary>
        Public Property ReviewerAccessKeysEnabled As Boolean?

        ''' <summary>Reviewer suggestion enabled.</summary>
        Public Property ReviewerSuggestionEnabled As Boolean?

        ''' <summary>Reviewer suggestions help.</summary>
        Public Property ReviewerSuggestionsHelp As LocalizedString

        ''' <summary>Review guidelines.</summary>
        Public Property ReviewGuidelines As LocalizedString

        ''' <summary>Review help.</summary>
        Public Property ReviewHelp As LocalizedString

        ''' <summary>Enable rights metadata. <c>0</c> is disabled. <c>enable</c> will make it available in the workflow. <c>request</c> will allow an author to enter a value during submission. <c>require</c> will require that the author enter a value during submission.</summary>
        Public Property Rights As String

        ''' <summary>Search description.</summary>
        Public Property SearchDescription As LocalizedString

        ''' <summary>Seq.</summary>
        Public Property Seq As Integer?

        ''' <summary>Show ensuring link.</summary>
        Public Property ShowEnsuringLink As Boolean?

        ''' <summary>Sidebar.</summary>
        Public Property Sidebar As List(Of String)

        ''' <summary>Enable source metadata. <c>0</c> is disabled. <c>enable</c> will make it available in the workflow. <c>request</c> will allow an author to enter a value during submission. <c>require</c> will require that the author enter a value during submission.</summary>
        Public Property Source As String

        ''' <summary>A custom stylesheet uploaded for the journal.</summary>
        Public Property StyleSheet As ImageFile

        ''' <summary>Enable subjects metadata. <c>0</c> is disabled. <c>enable</c> will make it available in the workflow. <c>request</c> will allow an author to enter a value during submission. <c>require</c> will require that the author enter a value during submission.</summary>
        Public Property Subjects As String

        ''' <summary>Which authors should receive an email confirmation when a new submission is submitted, which acknowledges that their submission was submitted. When empty, the author will not be sent an email. Default: <c>allAuthors</c></summary>
        Public Property SubmissionAcknowledgement As String

        ''' <summary>Submission checklist.</summary>
        Public Property SubmissionChecklist As LocalizedString

        ''' <summary>Submission fee.</summary>
        Public Property SubmissionFee As Double?

        ''' <summary>Submission fee description.</summary>
        Public Property SubmissionFeeDescription As LocalizedString

        ''' <summary>Submission fee name.</summary>
        Public Property SubmissionFeeName As LocalizedString

        ''' <summary>Whether or not submitting authors should be asked to select categories when they make a new submission.</summary>
        Public Property SubmitWithCategories As Boolean?

        ''' <summary>Subscription additional information.</summary>
        Public Property SubscriptionAdditionalInformation As LocalizedString

        ''' <summary>Subscription email.</summary>
        Public Property SubscriptionEmail As String

        ''' <summary>Subscription expiry partial.</summary>
        Public Property SubscriptionExpiryPartial As Boolean?

        ''' <summary>Subscription mailing address.</summary>
        Public Property SubscriptionMailingAddress As String

        ''' <summary>Subscription name.</summary>
        Public Property SubscriptionName As String

        ''' <summary>Subscription phone.</summary>
        Public Property SubscriptionPhone As String

        ''' <summary>Supported added submission locales.</summary>
        Public Property SupportedAddedSubmissionLocales As List(Of String)

        ''' <summary>Supported default submission locale.</summary>
        Public Property SupportedDefaultSubmissionLocale As String

        ''' <summary>Supported form locales.</summary>
        Public Property SupportedFormLocales As List(Of String)

        ''' <summary>Supported locales.</summary>
        Public Property SupportedLocales As List(Of String)

        ''' <summary>Supported submission locales.</summary>
        Public Property SupportedSubmissionLocales As List(Of String)

        ''' <summary>Supported submission metadata locales.</summary>
        Public Property SupportedSubmissionMetadataLocales As List(Of String)

        ''' <summary>Support email.</summary>
        Public Property SupportEmail As String

        ''' <summary>Support name.</summary>
        Public Property SupportName As String

        ''' <summary>Support phone.</summary>
        Public Property SupportPhone As String

        ''' <summary>Theme plugin path.</summary>
        Public Property ThemePluginPath As String

        ''' <summary>Time format.</summary>
        Public Property TimeFormat As LocalizedString

        ''' <summary>Enable types metadata. <c>0</c> is disabled. <c>enable</c> will make it available in the workflow. <c>request</c> will allow an author to enter a value during submission. <c>require</c> will require that the author enter a value during submission.</summary>
        Public Property [Type] As String

        ''' <summary>Upload files help.</summary>
        Public Property UploadFilesHelp As LocalizedString

        ''' <summary>Url. Read-only.</summary>
        Public Property Url As String

        ''' <summary>Url path.</summary>
        Public Property UrlPath As String

        ''' <summary>
        ''' Properties returned by the server that this class does not model, such as those added by plugins
        ''' (<c>crossrefplugin::status</c>, <c>dataciteplugin::registeredDoi</c>, …).
        ''' </summary>
        <JsonExtensionData>
        Public Property AdditionalProperties As Dictionary(Of String, JsonElement)

    End Class

    ''' <summary>The writable settings of a journal, for &lt;c&gt;CreateAsync&lt;/c&gt; and &lt;c&gt;UpdateAsync&lt;/c&gt;. Only properties you set are sent.</summary>
    ''' <example>
    ''' <code>
    ''' Dim journal = Await ojs.Contexts.CreateAsync(New ContextEdit With {
    '''     .Name = LocalizedString.Create("en", "Journal of Examples"),
    '''     .UrlPath = "joe", .PrimaryLocale = "en", .SupportedLocales = New List(Of String) From {"en"},
    '''     .ContactName = "Editor", .ContactEmail = "editor@example.com", .Country = "ID", .Enabled = True})
    ''' </code>
    ''' </example>
    Partial Public Class ContextEdit

        ''' <summary>Abbreviation.</summary>
        Public Property Abbreviation As LocalizedString

        ''' <summary>About.</summary>
        Public Property About As LocalizedString

        ''' <summary>Acronym.</summary>
        Public Property Acronym As LocalizedString

        ''' <summary>Additional home content.</summary>
        Public Property AdditionalHomeContent As LocalizedString

        ''' <summary>Enable agencies metadata. <c>0</c> is disabled. <c>enable</c> will make it available in the workflow. <c>request</c> will allow an author to enter a value during submission. <c>require</c> will require that the author enter a value during submission.</summary>
        Public Property Agencies As String

        ''' <summary>Announcements introduction.</summary>
        Public Property AnnouncementsIntroduction As LocalizedString

        ''' <summary>Author guidelines.</summary>
        Public Property AuthorGuidelines As LocalizedString

        ''' <summary>Author information.</summary>
        Public Property AuthorInformation As LocalizedString

        ''' <summary>Automatic DOI deposit.</summary>
        Public Property AutomaticDoiDeposit As Boolean?

        ''' <summary>Begin submission help.</summary>
        Public Property BeginSubmissionHelp As LocalizedString

        ''' <summary>Enable citations metadata. <c>0</c> is disabled. <c>enable</c> will make it available in the workflow. <c>request</c> will allow an author to enter a value during submission. <c>require</c> will require that the author enter a value during submission.</summary>
        Public Property Citations As String

        ''' <summary>Clockss license.</summary>
        Public Property ClockssLicense As LocalizedString

        ''' <summary>Competing interests.</summary>
        Public Property CompetingInterests As LocalizedString

        ''' <summary>Contact affiliation.</summary>
        Public Property ContactAffiliation As LocalizedString

        ''' <summary>Contact email.</summary>
        Public Property ContactEmail As String

        ''' <summary>Contact name.</summary>
        Public Property ContactName As String

        ''' <summary>Contact phone.</summary>
        Public Property ContactPhone As String

        ''' <summary>Contributors help.</summary>
        Public Property ContributorsHelp As LocalizedString

        ''' <summary>Copyright holder other.</summary>
        Public Property CopyrightHolderOther As LocalizedString

        ''' <summary>Copyright holder type.</summary>
        Public Property CopyrightHolderType As String

        ''' <summary>Copyright notice.</summary>
        Public Property CopyrightNotice As LocalizedString

        ''' <summary>Copyright year basis.</summary>
        Public Property CopyrightYearBasis As String

        ''' <summary>A copy of the submission acknowledgement email will be sent to any of the email addresses entered here. Separate multiple email addresses with a comma. Example: one@example.com,two@example.com</summary>
        Public Property CopySubmissionAckAddress As String

        ''' <summary>A copy of the submission acknowledgement email will be sent to this context's primary contact when true.</summary>
        Public Property CopySubmissionAckPrimaryContact As Boolean?

        ''' <summary>Country.</summary>
        Public Property Country As String

        ''' <summary>Enable coverage metadata. <c>0</c> is disabled. <c>enable</c> will make it available in the workflow. <c>request</c> will allow an author to enter a value during submission. <c>require</c> will require that the author enter a value during submission.</summary>
        Public Property Coverage As String

        ''' <summary>Currency.</summary>
        Public Property Currency As String

        ''' <summary>Current issue ID.</summary>
        Public Property CurrentIssueId As Integer?

        ''' <summary>Custom headers.</summary>
        Public Property CustomHeaders As LocalizedString

        ''' <summary>Whether to not request, request or require a Data Availability Statement from the authors.</summary>
        Public Property DataAvailability As String

        ''' <summary>Date format long.</summary>
        Public Property DateFormatLong As LocalizedString

        ''' <summary>Date format short.</summary>
        Public Property DateFormatShort As LocalizedString

        ''' <summary>Datetime format long.</summary>
        Public Property DatetimeFormatLong As LocalizedString

        ''' <summary>Datetime format short.</summary>
        Public Property DatetimeFormatShort As LocalizedString

        ''' <summary>Default metric type.</summary>
        Public Property DefaultMetricType As String

        ''' <summary>The default review method for new review assignments.</summary>
        Public Property DefaultReviewMode As ReviewMethod?

        ''' <summary>Delayed open access duration.</summary>
        Public Property DelayedOpenAccessDuration As Integer?

        ''' <summary>Delayed open access policy.</summary>
        Public Property DelayedOpenAccessPolicy As LocalizedString

        ''' <summary>Description.</summary>
        Public Property Description As LocalizedString

        ''' <summary>Details help.</summary>
        Public Property DetailsHelp As LocalizedString

        ''' <summary>User groups that a manager will be prevented from sending bulk email to using the notify feature. Only the admin may edit this property.</summary>
        Public Property DisableBulkEmailUserGroups As List(Of Integer)

        ''' <summary>Disable submissions.</summary>
        Public Property DisableSubmissions As Boolean?

        ''' <summary>Disable user reg.</summary>
        Public Property DisableUserReg As Boolean?

        ''' <summary>Enable disciplines metadata. <c>0</c> is disabled. <c>enable</c> will make it available in the workflow. <c>request</c> will allow an author to enter a value during submission. <c>require</c> will require that the author enter a value during submission.</summary>
        Public Property Disciplines As String

        ''' <summary>DOI creation time.</summary>
        Public Property DoiCreationTime As String

        ''' <summary>DOI issue suffix pattern.</summary>
        Public Property DoiIssueSuffixPattern As String

        ''' <summary>DOI prefix.</summary>
        Public Property DoiPrefix As String

        ''' <summary>DOI publication suffix pattern.</summary>
        Public Property DoiPublicationSuffixPattern As String

        ''' <summary>DOI representation suffix pattern.</summary>
        Public Property DoiRepresentationSuffixPattern As String

        ''' <summary>DOI suffix type.</summary>
        Public Property DoiSuffixType As String

        ''' <summary>Whether each publication version should receive a unique DOI</summary>
        Public Property DoiVersioning As Boolean?

        ''' <summary>Editorial history.</summary>
        Public Property EditorialHistory As LocalizedString

        ''' <summary>Whether or not to send the monthly editorial statistics email to editors. Default: <c>true</c></summary>
        Public Property EditorialStatsEmail As Boolean?

        ''' <summary>Email signature.</summary>
        Public Property EmailSignature As String

        ''' <summary>Enable announcements.</summary>
        Public Property EnableAnnouncements As Boolean?

        ''' <summary>Enable clockss.</summary>
        Public Property EnableClockss As Boolean?

        ''' <summary>Enabled.</summary>
        Public Property Enabled As Boolean?

        ''' <summary>Enabled DOI types.</summary>
        Public Property EnabledDoiTypes As List(Of String)

        ''' <summary>Enable DOIs.</summary>
        Public Property EnableDois As Boolean?

        ''' <summary>Masthead setting applied to users enrolled in editorial roles (OJS 3.5+).</summary>
        Public Property EnableEnrollmentMasthead As Boolean?

        ''' <summary>Masthead setting applied to users enrolled as reviewers (OJS 3.5+).</summary>
        Public Property EnableEnrollmentMastheadReviewers As Boolean?

        ''' <summary>Whether to track usage statistics by location, and whether to track at the country, region or city level.</summary>
        Public Property EnableGeoUsageStats As String

        ''' <summary>Whether to track usage statistics by institutions. Requires institutional IP ranges to be configured.</summary>
        Public Property EnableInstitutionUsageStats As Boolean?

        ''' <summary>Enable lockss.</summary>
        Public Property EnableLockss As Boolean?

        ''' <summary>Enable oai.</summary>
        Public Property EnableOai As Boolean?

        ''' <summary>Enable open access notification.</summary>
        Public Property EnableOpenAccessNotification As Boolean?

        ''' <summary>Enable publisher ID.</summary>
        Public Property EnablePublisherId As List(Of String)

        ''' <summary>Enable subscription online payment notification purchase individual.</summary>
        Public Property EnableSubscriptionOnlinePaymentNotificationPurchaseIndividual As Boolean?

        ''' <summary>Enable subscription online payment notification purchase institutional.</summary>
        Public Property EnableSubscriptionOnlinePaymentNotificationPurchaseInstitutional As Boolean?

        ''' <summary>Enable subscription online payment notification renew individual.</summary>
        Public Property EnableSubscriptionOnlinePaymentNotificationRenewIndividual As Boolean?

        ''' <summary>Enable subscription online payment notification renew institutional.</summary>
        Public Property EnableSubscriptionOnlinePaymentNotificationRenewInstitutional As Boolean?

        ''' <summary>Envelope sender.</summary>
        Public Property EnvelopeSender As String

        ''' <summary>The favicon for each locale. To upload one, set <see cref="ImageFile.TemporaryFileId"/>.</summary>
        Public Property Favicon As Dictionary(Of String, ImageFile)

        ''' <summary>For the editors help.</summary>
        Public Property ForTheEditorsHelp As LocalizedString

        ''' <summary>The homepage image for each locale. To upload one, set <see cref="ImageFile.TemporaryFileId"/>.</summary>
        Public Property HomepageImage As Dictionary(Of String, ImageFile)

        ''' <summary>Whether or not the access to the COUNTER SUSHI statistics API is public. If false, only users with admin or manager roles can access the API.</summary>
        Public Property IsSushiApiPublic As Boolean?

        ''' <summary>Items per page.</summary>
        Public Property ItemsPerPage As Integer?

        ''' <summary>The journal thumbnail for each locale. To upload one, set <see cref="ImageFile.TemporaryFileId"/>.</summary>
        Public Property JournalThumbnail As Dictionary(Of String, ImageFile)

        ''' <summary>Enable keywords metadata. <c>0</c> is disabled. <c>enable</c> will make it available in the workflow. <c>request</c> will allow an author to enter a value during submission. <c>require</c> will require that the author enter a value during submission.</summary>
        Public Property Keywords As String

        ''' <summary>Librarian information.</summary>
        Public Property LibrarianInformation As LocalizedString

        ''' <summary>License terms.</summary>
        Public Property LicenseTerms As LocalizedString

        ''' <summary>License URL.</summary>
        Public Property LicenseUrl As String

        ''' <summary>Lockss license.</summary>
        Public Property LockssLicense As LocalizedString

        ''' <summary>Mailing address.</summary>
        Public Property MailingAddress As String

        ''' <summary>Masthead user group IDs.</summary>
        Public Property MastheadUserGroupIds As List(Of Integer)

        ''' <summary>Membership fee.</summary>
        Public Property MembershipFee As Double?

        ''' <summary>Membership fee description.</summary>
        Public Property MembershipFeeDescription As LocalizedString

        ''' <summary>Membership fee name.</summary>
        Public Property MembershipFeeName As LocalizedString

        ''' <summary>Name.</summary>
        Public Property Name As LocalizedString

        ''' <summary>When enabled, all authors of a submission will receive a notification when an editorial decision is made regarding a submission. When disabled, only authors assigned to the submission will be notified.</summary>
        Public Property NotifyAllAuthors As Boolean?

        ''' <summary>Num announcements homepage.</summary>
        Public Property NumAnnouncementsHomepage As Integer?

        ''' <summary>Num days after review response reminder due.</summary>
        Public Property NumDaysAfterReviewResponseReminderDue As Integer?

        ''' <summary>Num days after review submit reminder due.</summary>
        Public Property NumDaysAfterReviewSubmitReminderDue As Integer?

        ''' <summary>Num days before review response reminder due.</summary>
        Public Property NumDaysBeforeReviewResponseReminderDue As Integer?

        ''' <summary>Num days before review submit reminder due.</summary>
        Public Property NumDaysBeforeReviewSubmitReminderDue As Integer?

        ''' <summary>Num months after subscription expiry reminder.</summary>
        Public Property NumMonthsAfterSubscriptionExpiryReminder As Integer?

        ''' <summary>Num months before subscription expiry reminder.</summary>
        Public Property NumMonthsBeforeSubscriptionExpiryReminder As Integer?

        ''' <summary>Num page links.</summary>
        Public Property NumPageLinks As Integer?

        ''' <summary>Num reviews per submission.</summary>
        Public Property NumReviewsPerSubmission As Integer?

        ''' <summary>Num weeks after subscription expiry reminder.</summary>
        Public Property NumWeeksAfterSubscriptionExpiryReminder As Integer?

        ''' <summary>Num weeks before subscription expiry reminder.</summary>
        Public Property NumWeeksBeforeSubscriptionExpiryReminder As Integer?

        ''' <summary>Num weeks per response.</summary>
        Public Property NumWeeksPerResponse As Integer?

        ''' <summary>Num weeks per review.</summary>
        Public Property NumWeeksPerReview As Integer?

        ''' <summary>Online issn.</summary>
        Public Property OnlineIssn As String

        ''' <summary>Open access policy.</summary>
        Public Property OpenAccessPolicy As LocalizedString

        ''' <summary>ORCID api type.</summary>
        Public Property OrcidApiType As String

        ''' <summary>ORCID city.</summary>
        Public Property OrcidCity As String

        ''' <summary>ORCID client ID.</summary>
        Public Property OrcidClientId As String

        ''' <summary>ORCID client secret.</summary>
        Public Property OrcidClientSecret As String

        ''' <summary>Whether ORCID functionality is enabled</summary>
        Public Property OrcidEnabled As Boolean?

        ''' <summary>ORCID log level.</summary>
        Public Property OrcidLogLevel As String

        ''' <summary>ORCID send mail to authors on publication.</summary>
        Public Property OrcidSendMailToAuthorsOnPublication As Boolean?

        ''' <summary>Page footer.</summary>
        Public Property PageFooter As LocalizedString

        ''' <summary>The page header logo for each locale. To upload one, set <see cref="ImageFile.TemporaryFileId"/>.</summary>
        Public Property PageHeaderLogoImage As Dictionary(Of String, ImageFile)

        ''' <summary>Payment plugin name.</summary>
        Public Property PaymentPluginName As String

        ''' <summary>Payments enabled.</summary>
        Public Property PaymentsEnabled As Boolean?

        ''' <summary>Primary locale.</summary>
        Public Property PrimaryLocale As String

        ''' <summary>Print issn.</summary>
        Public Property PrintIssn As String

        ''' <summary>Privacy statement.</summary>
        Public Property PrivacyStatement As LocalizedString

        ''' <summary>Publication fee.</summary>
        Public Property PublicationFee As Double?

        ''' <summary>Publication fee description.</summary>
        Public Property PublicationFeeDescription As LocalizedString

        ''' <summary>Publication fee name.</summary>
        Public Property PublicationFeeName As LocalizedString

        ''' <summary>Publisher institution.</summary>
        Public Property PublisherInstitution As String

        ''' <summary>Publisher URL.</summary>
        Public Property PublisherUrl As String

        ''' <summary>Whether content is open access, subscription-based or not published on the site.</summary>
        Public Property PublishingMode As PublishingMode?

        ''' <summary>Purchase article fee.</summary>
        Public Property PurchaseArticleFee As Double?

        ''' <summary>Purchase article fee description.</summary>
        Public Property PurchaseArticleFeeDescription As LocalizedString

        ''' <summary>Purchase article fee enabled.</summary>
        Public Property PurchaseArticleFeeEnabled As Boolean?

        ''' <summary>Purchase article fee name.</summary>
        Public Property PurchaseArticleFeeName As LocalizedString

        ''' <summary>Purchase issue fee.</summary>
        Public Property PurchaseIssueFee As Double?

        ''' <summary>Purchase issue fee enabled.</summary>
        Public Property PurchaseIssueFeeEnabled As Boolean?

        ''' <summary>Rate reviewer on quality.</summary>
        Public Property RateReviewerOnQuality As Boolean?

        ''' <summary>Reader information.</summary>
        Public Property ReaderInformation As LocalizedString

        ''' <summary>Registration agency.</summary>
        Public Property RegistrationAgency As String

        ''' <summary>Require author competing interests.</summary>
        Public Property RequireAuthorCompetingInterests As Boolean?

        ''' <summary>Restrict article access.</summary>
        Public Property RestrictArticleAccess As Boolean?

        ''' <summary>Restrict only pdf.</summary>
        Public Property RestrictOnlyPdf As Boolean?

        ''' <summary>Restrict reviewer file access.</summary>
        Public Property RestrictReviewerFileAccess As Boolean?

        ''' <summary>Restrict site access.</summary>
        Public Property RestrictSiteAccess As Boolean?

        ''' <summary>Reviewer access keys enabled.</summary>
        Public Property ReviewerAccessKeysEnabled As Boolean?

        ''' <summary>Reviewer suggestion enabled.</summary>
        Public Property ReviewerSuggestionEnabled As Boolean?

        ''' <summary>Reviewer suggestions help.</summary>
        Public Property ReviewerSuggestionsHelp As LocalizedString

        ''' <summary>Review guidelines.</summary>
        Public Property ReviewGuidelines As LocalizedString

        ''' <summary>Review help.</summary>
        Public Property ReviewHelp As LocalizedString

        ''' <summary>Enable rights metadata. <c>0</c> is disabled. <c>enable</c> will make it available in the workflow. <c>request</c> will allow an author to enter a value during submission. <c>require</c> will require that the author enter a value during submission.</summary>
        Public Property Rights As String

        ''' <summary>Search description.</summary>
        Public Property SearchDescription As LocalizedString

        ''' <summary>Seq.</summary>
        Public Property Seq As Integer?

        ''' <summary>Show ensuring link.</summary>
        Public Property ShowEnsuringLink As Boolean?

        ''' <summary>Sidebar.</summary>
        Public Property Sidebar As List(Of String)

        ''' <summary>Enable source metadata. <c>0</c> is disabled. <c>enable</c> will make it available in the workflow. <c>request</c> will allow an author to enter a value during submission. <c>require</c> will require that the author enter a value during submission.</summary>
        Public Property Source As String

        ''' <summary>A custom stylesheet. To upload one, set <see cref="ImageFile.TemporaryFileId"/>.</summary>
        Public Property StyleSheet As ImageFile

        ''' <summary>Enable subjects metadata. <c>0</c> is disabled. <c>enable</c> will make it available in the workflow. <c>request</c> will allow an author to enter a value during submission. <c>require</c> will require that the author enter a value during submission.</summary>
        Public Property Subjects As String

        ''' <summary>Which authors should receive an email confirmation when a new submission is submitted, which acknowledges that their submission was submitted. When empty, the author will not be sent an email. Default: <c>allAuthors</c></summary>
        Public Property SubmissionAcknowledgement As String

        ''' <summary>Submission checklist.</summary>
        Public Property SubmissionChecklist As LocalizedString

        ''' <summary>Submission fee.</summary>
        Public Property SubmissionFee As Double?

        ''' <summary>Submission fee description.</summary>
        Public Property SubmissionFeeDescription As LocalizedString

        ''' <summary>Submission fee name.</summary>
        Public Property SubmissionFeeName As LocalizedString

        ''' <summary>Whether or not submitting authors should be asked to select categories when they make a new submission.</summary>
        Public Property SubmitWithCategories As Boolean?

        ''' <summary>Subscription additional information.</summary>
        Public Property SubscriptionAdditionalInformation As LocalizedString

        ''' <summary>Subscription email.</summary>
        Public Property SubscriptionEmail As String

        ''' <summary>Subscription expiry partial.</summary>
        Public Property SubscriptionExpiryPartial As Boolean?

        ''' <summary>Subscription mailing address.</summary>
        Public Property SubscriptionMailingAddress As String

        ''' <summary>Subscription name.</summary>
        Public Property SubscriptionName As String

        ''' <summary>Subscription phone.</summary>
        Public Property SubscriptionPhone As String

        ''' <summary>Supported added submission locales.</summary>
        Public Property SupportedAddedSubmissionLocales As List(Of String)

        ''' <summary>Supported default submission locale.</summary>
        Public Property SupportedDefaultSubmissionLocale As String

        ''' <summary>Supported form locales.</summary>
        Public Property SupportedFormLocales As List(Of String)

        ''' <summary>Supported locales.</summary>
        Public Property SupportedLocales As List(Of String)

        ''' <summary>Supported submission locales.</summary>
        Public Property SupportedSubmissionLocales As List(Of String)

        ''' <summary>Supported submission metadata locales.</summary>
        Public Property SupportedSubmissionMetadataLocales As List(Of String)

        ''' <summary>Support email.</summary>
        Public Property SupportEmail As String

        ''' <summary>Support name.</summary>
        Public Property SupportName As String

        ''' <summary>Support phone.</summary>
        Public Property SupportPhone As String

        ''' <summary>Theme plugin path.</summary>
        Public Property ThemePluginPath As String

        ''' <summary>Time format.</summary>
        Public Property TimeFormat As LocalizedString

        ''' <summary>Enable types metadata. <c>0</c> is disabled. <c>enable</c> will make it available in the workflow. <c>request</c> will allow an author to enter a value during submission. <c>require</c> will require that the author enter a value during submission.</summary>
        Public Property [Type] As String

        ''' <summary>Upload files help.</summary>
        Public Property UploadFilesHelp As LocalizedString

        ''' <summary>Url path.</summary>
        Public Property UrlPath As String

    End Class

End Namespace
