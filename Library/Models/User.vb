Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports Library.Serialization

Namespace Models

    ''' <summary>A user account. List endpoints only fill the summary properties.</summary>
    Partial Public Class User

        ''' <summary>Affiliation.</summary>
        Public Property Affiliation As LocalizedString

        ''' <summary>Average number of days the user takes to complete a review (reviewers only).</summary>
        Public Property AverageReviewCompletionDays As Integer?

        ''' <summary>Billing address.</summary>
        Public Property BillingAddress As String

        ''' <summary>Biography.</summary>
        Public Property Biography As LocalizedString

        ''' <summary>Whether the current user can log in as this user. Read-only.</summary>
        Public Property CanLoginAs As Boolean?

        ''' <summary>Whether the current user can merge this user account with another. Read-only.</summary>
        Public Property CanMergeUsers As Boolean?

        ''' <summary>Country.</summary>
        Public Property Country As String

        ''' <summary>Date last email.</summary>
        Public Property DateLastEmail As DateTime?

        ''' <summary>Date last login.</summary>
        Public Property DateLastLogin As DateTime?

        ''' <summary>When the user was last assigned a review (reviewers only).</summary>
        Public Property DateLastReviewAssignment As DateTime?

        ''' <summary>Date registered.</summary>
        Public Property DateRegistered As DateTime?

        ''' <summary>Date validated.</summary>
        Public Property DateValidated As DateTime?

        ''' <summary>Disabled.</summary>
        Public Property Disabled As Boolean?

        ''' <summary>Disabled reason.</summary>
        Public Property DisabledReason As String

        ''' <summary>The initials that will be displayed for a user.</summary>
        Public Property DisplayInitials As String

        ''' <summary>Email.</summary>
        Public Property Email As String

        ''' <summary>Family name.</summary>
        Public Property FamilyName As LocalizedString

        ''' <summary>Full name. Read-only.</summary>
        Public Property FullName As String

        ''' <summary>Given name.</summary>
        Public Property GivenName As LocalizedString

        ''' <summary>Gossip.</summary>
        Public Property Gossip As String

        ''' <summary>Groups. Read-only.</summary>
        Public Property Groups As List(Of UserGroup)

        ''' <summary>Href. Read-only.</summary>
        <JsonPropertyName("_href")>
        Public Property Href As String

        ''' <summary>Id. Read-only.</summary>
        Public Property Id As Integer?

        ''' <summary>Inline help.</summary>
        Public Property InlineHelp As Boolean?

        ''' <summary>Interests. Read-only.</summary>
        Public Property Interests As List(Of UserInterest)

        ''' <summary>Locales.</summary>
        Public Property Locales As List(Of String)

        ''' <summary>Mailing address.</summary>
        Public Property MailingAddress As String

        ''' <summary>Must change password.</summary>
        Public Property MustChangePassword As Boolean?

        ''' <summary>ORCID.</summary>
        Public Property Orcid As String

        ''' <summary>ORCID display value. Read-only.</summary>
        Public Property OrcidDisplayValue As String

        ''' <summary>ORCID is verified.</summary>
        Public Property OrcidIsVerified As Boolean?

        ''' <summary>Phone.</summary>
        Public Property Phone As String

        ''' <summary>An optional field for users to specify the initials that they want to be displayed in their avatars.</summary>
        Public Property PreferredAvatarInitials As String

        ''' <summary>Preferred public name.</summary>
        Public Property PreferredPublicName As LocalizedString

        ''' <summary>Profile image.</summary>
        Public Property ProfileImage As JsonElement?

        ''' <summary>The average rating editors gave this reviewer.</summary>
        Public Property ReviewerRating As Integer?

        ''' <summary>Number of active review assignments.</summary>
        Public Property ReviewsActive As Integer?

        ''' <summary>Number of cancelled review assignments.</summary>
        Public Property ReviewsCancelled As Integer?

        ''' <summary>Number of completed review assignments.</summary>
        Public Property ReviewsCompleted As Integer?

        ''' <summary>Number of declined review assignments.</summary>
        Public Property ReviewsDeclined As Integer?

        ''' <summary>Signature.</summary>
        Public Property Signature As LocalizedString

        ''' <summary>Stage assignments. Read-only.</summary>
        Public Property StageAssignments As List(Of JsonElement)

        ''' <summary>Url.</summary>
        Public Property Url As String

        ''' <summary>User name.</summary>
        Public Property UserName As String

        ''' <summary>
        ''' Properties returned by the server that this class does not model, such as those added by plugins
        ''' (<c>crossrefplugin::status</c>, <c>dataciteplugin::registeredDoi</c>, …).
        ''' </summary>
        <JsonExtensionData>
        Public Property AdditionalProperties As Dictionary(Of String, JsonElement)

    End Class

    ''' <summary>A user group (role), such as Journal Manager, Section Editor, Author or Reviewer.</summary>
    Partial Public Class UserGroup

        ''' <summary>Abbrev.</summary>
        Public Property Abbrev As LocalizedString

        ''' <summary>The locale key of the group abbreviation.</summary>
        Public Property AbbrevLocaleKey As String

        ''' <summary>Context ID.</summary>
        Public Property ContextId As Integer?

        ''' <summary>Id.</summary>
        Public Property Id As Integer?

        ''' <summary>Is default.</summary>
        Public Property IsDefault As Boolean?

        ''' <summary>Masthead.</summary>
        Public Property Masthead As Boolean?

        ''' <summary>Name.</summary>
        Public Property Name As LocalizedString

        ''' <summary>The locale key of the group name.</summary>
        Public Property NameLocaleKey As String

        ''' <summary>Name plural.</summary>
        Public Property NamePlural As LocalizedString

        ''' <summary>Permit metadata edit.</summary>
        Public Property PermitMetadataEdit As Boolean?

        ''' <summary>Permit self registration.</summary>
        Public Property PermitSelfRegistration As Boolean?

        ''' <summary>Permit settings.</summary>
        Public Property PermitSettings As Boolean?

        ''' <summary>Recommend only.</summary>
        Public Property RecommendOnly As Boolean?

        ''' <summary>Role ID.</summary>
        Public Property RoleId As Integer?

        ''' <summary>Show title.</summary>
        Public Property ShowTitle As Boolean?

        ''' <summary>
        ''' Properties returned by the server that this class does not model, such as those added by plugins
        ''' (<c>crossrefplugin::status</c>, <c>dataciteplugin::registeredDoi</c>, …).
        ''' </summary>
        <JsonExtensionData>
        Public Property AdditionalProperties As Dictionary(Of String, JsonElement)

    End Class

    ''' <summary>A reviewing interest of a user.</summary>
    Partial Public Class UserInterest

        ''' <summary>Id.</summary>
        Public Property Id As Integer?

        ''' <summary>Interest.</summary>
        Public Property Interest As String

        ''' <summary>
        ''' Properties returned by the server that this class does not model, such as those added by plugins
        ''' (<c>crossrefplugin::status</c>, <c>dataciteplugin::registeredDoi</c>, …).
        ''' </summary>
        <JsonExtensionData>
        Public Property AdditionalProperties As Dictionary(Of String, JsonElement)

    End Class

End Namespace
