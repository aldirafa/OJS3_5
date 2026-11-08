Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports OJS3_5.Library.Serialization

Namespace Models

#Region "DOIs"

    ''' <summary>The kind of object a DOI belongs to.</summary>
    Public Enum DoiObjectType
        ''' <summary>An article version (<c>publication</c>).</summary>
        <JsonStringEnumMemberName("publication")>
        Publication
        ''' <summary>A galley (<c>representation</c>).</summary>
        <JsonStringEnumMemberName("representation")>
        Representation
        ''' <summary>An issue (<c>issue</c>).</summary>
        <JsonStringEnumMemberName("issue")>
        Issue
    End Enum

    ''' <summary>The writable properties of a DOI, for <c>DoisResource.CreateAsync</c> and <c>UpdateAsync</c>. Only properties you set are sent.</summary>
    ''' <example>
    ''' <code>
    ''' Await ojs.Dois.UpdateAsync(doiId, New DoiEdit With {
    '''     .Doi = "10.1234/abc-123", .PubObjectType = DoiObjectType.Publication, .PubObjectId = publicationId})
    ''' </code>
    ''' </example>
    Partial Public Class DoiEdit

        ''' <summary>The DOI itself, such as <c>10.1234/5a6b-7c8d</c>.</summary>
        Public Property Doi As String

        ''' <summary>The registration agency (e.g. <c>crossrefplugin</c>); <c>Nothing</c> when marked registered by hand.</summary>
        Public Property RegistrationAgency As String

        ''' <summary>The deposit status.</summary>
        Public Property Status As DoiStatus?

        ''' <summary>The kind of object the DOI belongs to. Required by <c>UpdateAsync</c>.</summary>
        <JsonConverter(GetType(JsonStringEnumConverter(Of DoiObjectType)))>
        Public Property PubObjectType As DoiObjectType?

        ''' <summary>The ID of the publication, galley or issue the DOI belongs to. Required by <c>UpdateAsync</c>.</summary>
        Public Property PubObjectId As Integer?

    End Class

    ''' <summary>The result of a bulk DOI action such as assigning DOIs.</summary>
    Partial Public Class DoiActionResult

        ''' <summary>Messages for the items the action failed for; empty when everything succeeded.</summary>
        Public Property FailedDoiActions As List(Of String)

        ''' <summary><c>True</c> when no item failed.</summary>
        <JsonIgnore>
        Public ReadOnly Property Succeeded As Boolean
            Get
                Return FailedDoiActions Is Nothing OrElse FailedDoiActions.Count = 0
            End Get
        End Property

        ''' <summary>Other properties returned by the server.</summary>
        <JsonExtensionData>
        Public Property AdditionalProperties As Dictionary(Of String, JsonElement)

    End Class

#End Region

#Region "Reviews"

    ''' <summary>
    ''' The history of one reviewer's review in a review round, as returned by <c>ReviewsResource.GetHistoryAsync</c>.
    ''' </summary>
    ''' <remarks>OJS only returns this to the reviewer who wrote the review; other users get a 404 error.</remarks>
    Partial Public Class ReviewHistory

        ''' <summary>The title of the reviewed publication.</summary>
        Public Property PublicationTitle As LocalizedString

        ''' <summary>The type of the reviewed publication.</summary>
        Public Property PublicationType As LocalizedString

        ''' <summary>The abstract of the reviewed publication.</summary>
        Public Property PublicationAbstract As LocalizedString

        ''' <summary>The keywords of the reviewed publication.</summary>
        Public Property PublicationKeywords As LocalizedVocab

        ''' <summary>The email sent when the reviewer declined, if they did.</summary>
        Public Property DeclineEmail As JsonElement?

        ''' <summary>The review assignment.</summary>
        Public Property ReviewAssignment As ReviewAssignment

        ''' <summary>The reviewer's recommendation, as display text (e.g. <c>Revisions Required</c>).</summary>
        Public Property Recommendation As String

        ''' <summary>The reviewer's comments for the author and editor (HTML).</summary>
        Public Property Comments As List(Of String)

        ''' <summary>The reviewer's comments for the editor only (HTML).</summary>
        Public Property PrivateComments As List(Of String)

        ''' <summary>Files the reviewer attached to the review.</summary>
        Public Property Attachments As List(Of SubmissionFile)

        ''' <summary>Files the reviewer was given to review.</summary>
        Public Property Files As List(Of SubmissionFile)

        ''' <summary>Other properties returned by the server.</summary>
        <JsonExtensionData>
        Public Property AdditionalProperties As Dictionary(Of String, JsonElement)

    End Class

#End Region

#Region "Invitations"

    ''' <summary>Invitation states.</summary>
    Public Enum InvitationStatus
        ''' <summary>Created but not yet sent (<c>initialized</c>).</summary>
        Initialized
        ''' <summary>Sent and waiting for an answer (<c>pending</c>).</summary>
        Pending
        ''' <summary>Accepted (<c>accepted</c>).</summary>
        Accepted
        ''' <summary>Declined (<c>declined</c>).</summary>
        Declined
        ''' <summary>Cancelled by the inviter (<c>cancelled</c>).</summary>
        Cancelled
    End Enum

    ''' <summary>Names of the invitation types accepted by <c>InvitationsResource</c>.</summary>
    ''' <example>
    ''' <code>
    ''' Dim id = Await ojs.Invitations.CreateForUserAsync(InvitationTypes.UserRoleAssignment, userId:=5)
    ''' </code>
    ''' </example>
    Public NotInheritable Class InvitationTypes

        Private Sub New()
        End Sub

        ''' <summary>Invites a new or existing user to one or more roles (user groups).</summary>
        Public Const UserRoleAssignment As String = "userRoleAssignment"

    End Class

    ''' <summary>A user group (role) to assign when an invitation is accepted.</summary>
    Partial Public Class InvitationUserGroup

        ''' <summary>The user group ID.</summary>
        Public Property UserGroupId As Integer?

        ''' <summary>The user group name (read-only).</summary>
        Public Property UserGroupName As String

        ''' <summary>Whether the user is shown on the masthead in this role.</summary>
        Public Property Masthead As Boolean?

        ''' <summary>When the role starts.</summary>
        Public Property DateStart As DateOnly?

        ''' <summary>When the role ends, if it does.</summary>
        Public Property DateEnd As DateOnly?

    End Class

    ''' <summary>The email that will be sent with an invitation.</summary>
    Partial Public Class InvitationEmail

        ''' <summary>The email subject.</summary>
        Public Property EmailSubject As String

        ''' <summary>The email body (HTML).</summary>
        Public Property EmailBody As String

    End Class

    ''' <summary>
    ''' The data of a user role assignment invitation, for <c>InvitationsResource.PopulateAsync</c> and
    ''' <c>RefineAsync</c>. Only properties you set are sent.
    ''' </summary>
    ''' <remarks>
    ''' For an existing user only <see cref="UserGroupsToAdd"/> (and optionally <see cref="EmailComposer"/>) may be
    ''' set; the name, affiliation and country fields are for new users. <see cref="UserGroupsToAdd"/> is required
    ''' before the invitation can be sent, and each entry needs <c>UserGroupId</c>, <c>Masthead</c> and <c>DateStart</c>.
    ''' </remarks>
    ''' <example>
    ''' <code>
    ''' Await ojs.Invitations.PopulateAsync(invitationId, New UserRoleAssignmentData With {
    '''     .UserGroupsToAdd = New List(Of InvitationUserGroup) From {
    '''         New InvitationUserGroup With {.UserGroupId = 5, .Masthead = False, .DateStart = DateOnly.FromDateTime(Date.Today)}},
    '''     .EmailComposer = New InvitationEmail With {.EmailSubject = "Join our journal", .EmailBody = "&lt;p&gt;Hello!&lt;/p&gt;"}})
    ''' </code>
    ''' </example>
    Partial Public Class UserRoleAssignmentData

        ''' <summary>The roles to assign when the invitation is accepted.</summary>
        Public Property UserGroupsToAdd As List(Of InvitationUserGroup)

        ''' <summary>The invitation email to send.</summary>
        Public Property EmailComposer As InvitationEmail

        ''' <summary>A new user's given name.</summary>
        Public Property GivenName As LocalizedString

        ''' <summary>A new user's family name.</summary>
        Public Property FamilyName As LocalizedString

        ''' <summary>A new user's affiliation.</summary>
        Public Property Affiliation As LocalizedString

        ''' <summary>A new user's country (ISO 3166-1 alpha-2).</summary>
        Public Property UserCountry As String

        ''' <summary>A new user's user name (when accepting).</summary>
        Public Property Username As String

        ''' <summary>A new user's password (when accepting).</summary>
        Public Property Password As String

        ''' <summary>The invitee's ORCID iD.</summary>
        Public Property Orcid As String

    End Class

    ''' <summary>The invitee of an invitation (an existing user, or the details entered for a new one).</summary>
    Partial Public Class InvitationUser

        ''' <summary>The email address.</summary>
        Public Property Email As String

        ''' <summary>The full name.</summary>
        Public Property FullName As String

        ''' <summary>The given name.</summary>
        Public Property GivenName As LocalizedString

        ''' <summary>The family name.</summary>
        Public Property FamilyName As LocalizedString

        ''' <summary>The affiliation.</summary>
        Public Property Affiliation As LocalizedString

        ''' <summary>The country (ISO 3166-1 alpha-2).</summary>
        Public Property Country As String

        ''' <summary>The ORCID iD.</summary>
        Public Property Orcid As String

        ''' <summary>Whether the ORCID iD was verified.</summary>
        Public Property OrcidIsVerified As Boolean?

        ''' <summary>Other properties returned by the server.</summary>
        <JsonExtensionData>
        Public Property AdditionalProperties As Dictionary(Of String, JsonElement)

    End Class

    ''' <summary>
    ''' A preview of the email an invitation will send, as returned by <c>InvitationsResource.GetMailableAsync</c>.
    ''' </summary>
    ''' <remarks>
    ''' The template variables (such as <c>{$recipientName}</c>) in <see cref="View"/> are filled from
    ''' <see cref="ViewData"/> when the email is sent.
    ''' </remarks>
    Partial Public Class InvitationEmailPreview

        ''' <summary>The email subject.</summary>
        Public Property Subject As String

        ''' <summary>The HTML body template.</summary>
        Public Property View As String

        ''' <summary>The locale of the email, if set.</summary>
        Public Property Locale As String

        ''' <summary>The sender(s).</summary>
        Public Property From As List(Of JsonElement)

        ''' <summary>The recipient(s).</summary>
        Public Property [To] As List(Of JsonElement)

        ''' <summary>The values of the template variables.</summary>
        Public Property ViewData As Dictionary(Of String, JsonElement)

        ''' <summary>Other properties returned by the server.</summary>
        <JsonExtensionData>
        Public Property AdditionalProperties As Dictionary(Of String, JsonElement)

    End Class

    ''' <summary>
    ''' An invitation, such as a user role assignment invitation. Properties not listed here (they depend on the
    ''' invitation type) are kept in <see cref="AdditionalProperties"/>.
    ''' </summary>
    ''' <example>
    ''' <code>
    ''' Dim pending = Await ojs.Invitations.ListAsync(InvitationTypes.UserRoleAssignment,
    '''     New InvitationQuery With {.Status = InvitationStatus.Pending})
    ''' For Each i In pending.Items
    '''     Console.WriteLine($"{i.Email} expires {i.ExpiryDate:d}")
    ''' Next
    ''' </code>
    ''' </example>
    Partial Public Class Invitation

        ''' <summary>The invitation ID.</summary>
        Public Property Id As Integer?

        ''' <summary>The invitation type, e.g. <c>userRoleAssignment</c>.</summary>
        Public Property Type As String

        ''' <summary>The invitation status as sent by OJS (e.g. <c>PENDING</c>). See <see cref="StatusValue"/>.</summary>
        Public Property Status As String

        ''' <summary>When the invitation was created.</summary>
        Public Property CreatedAt As DateTime?

        ''' <summary>When the invitation was last changed.</summary>
        Public Property UpdatedAt As DateTime?

        ''' <summary>When the invitation key expires.</summary>
        Public Property ExpiryDate As DateTime?

        ''' <summary>The journal the invitation belongs to.</summary>
        Public Property ContextId As Integer?

        ''' <summary>The user who sent the invitation.</summary>
        Public Property InviterId As Integer?

        ''' <summary>The invited user, or <c>Nothing</c> when the invitation is for a new user.</summary>
        Public Property UserId As Integer?

        ''' <summary>The invitee's email address.</summary>
        Public Property Email As String

        ''' <summary>The invitee's given name.</summary>
        Public Property GivenName As LocalizedString

        ''' <summary>The invitee's family name.</summary>
        Public Property FamilyName As LocalizedString

        ''' <summary>The invitee's affiliation.</summary>
        Public Property Affiliation As LocalizedString

        ''' <summary>The invitee's country (ISO 3166-1 alpha-2).</summary>
        Public Property Country As String

        ''' <summary>The proposed user name for a new user.</summary>
        Public Property Username As String

        ''' <summary>The address the invitation email is sent to.</summary>
        Public Property SendEmailAddress As String

        ''' <summary>The invitation email subject.</summary>
        Public Property EmailSubject As String

        ''' <summary>The invitation email body.</summary>
        Public Property EmailBody As String

        ''' <summary>The invitee's ORCID iD.</summary>
        Public Property Orcid As String

        ''' <summary>Whether the ORCID iD was verified.</summary>
        Public Property OrcidIsVerified As Boolean?

        ''' <summary>The roles to assign when the invitation is accepted.</summary>
        Public Property UserGroupsToAdd As List(Of InvitationUserGroup)

        ''' <summary>The existing user being invited, if any.</summary>
        Public Property ExistingUser As InvitationUser

        ''' <summary>The details of a new user being invited, if any.</summary>
        Public Property NewUser As InvitationUser

        ''' <summary><see cref="Status"/> as an enum, or <c>Nothing</c> for an unknown value.</summary>
        <JsonIgnore>
        Public ReadOnly Property StatusValue As InvitationStatus?
            Get
                Dim value As InvitationStatus
                If [Enum].TryParse(Status, ignoreCase:=True, result:=value) Then Return value
                Return Nothing
            End Get
        End Property

        ''' <summary>Other properties returned by the server (they depend on the invitation type).</summary>
        <JsonExtensionData>
        Public Property AdditionalProperties As Dictionary(Of String, JsonElement)

    End Class

#End Region

End Namespace
