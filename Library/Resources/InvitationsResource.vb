Imports System.Threading
Imports OJS3_5.Library.Http
Imports OJS3_5.Library.Models
Imports OJS3_5.Library.Paging
Imports OJS3_5.Library.Queries

Namespace Resources

    ''' <summary><c>/invitations</c> — invite users to roles and manage the invitations.</summary>
    ''' <remarks>
    ''' <para>
    ''' The usual flow is: <see cref="CreateAsync"/> (status <c>initialized</c>), <see cref="PopulateAsync"/> with the
    ''' invitee's details and roles, then <see cref="InviteAsync"/> to send the email (status <c>pending</c>).
    ''' </para>
    ''' <para>
    ''' The key-based methods (<see cref="GetByKeyAsync"/>, <see cref="FinalizeAsync"/>, ...) are what the invitee's
    ''' accept page uses; the key comes from the link in the invitation email.
    ''' </para>
    ''' <para>The payloads depend on the invitation type, so they are passed as any JSON-serializable object.</para>
    ''' </remarks>
    ''' <example>
    ''' <code>
    ''' Dim id = Await ojs.Invitations.CreateForUserAsync(InvitationTypes.UserRoleAssignment, userId:=5)
    ''' Await ojs.Invitations.PopulateAsync(id, New UserRoleAssignmentData With {
    '''     .UserGroupsToAdd = New List(Of InvitationUserGroup) From {
    '''         New InvitationUserGroup With {.UserGroupId = 3, .Masthead = False, .DateStart = DateOnly.FromDateTime(Date.Today)}}})
    ''' Dim sent = Await ojs.Invitations.InviteAsync(id)
    ''' Console.WriteLine(sent.Status) ' PENDING
    ''' </code>
    ''' </example>
    Public NotInheritable Class InvitationsResource
        Inherits ResourceBase

        Friend Sub New(http As OjsHttp)
            MyBase.New(http)
        End Sub

        ''' <summary>Gets one page of invitations of a type.</summary>
        ''' <param name="type">The invitation type; see <see cref="InvitationTypes"/>.</param>
        ''' <param name="query">Filters and paging; <c>Nothing</c> for the first page.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim page = Await ojs.Invitations.ListAsync(InvitationTypes.UserRoleAssignment, New InvitationQuery With {.Status = InvitationStatus.Pending})
        ''' For Each i In page.Items : Console.WriteLine($"{i.ExistingUser?.Email} expires {i.ExpiryDate:d}") : Next
        ''' </code>
        ''' </example>
        Public Function ListAsync(type As String, Optional query As InvitationQuery = Nothing,
                                  Optional cancellationToken As CancellationToken = Nothing) As Task(Of PagedResult(Of Invitation))
            ArgumentException.ThrowIfNullOrEmpty(type)
            Return Http.GetAsync(Of PagedResult(Of Invitation))($"invitations/{OjsHttp.Seg(type)}", If(query, New InvitationQuery()).ToQueryBuilder(), cancellationToken)
        End Function

        ''' <summary>Gets every invitation of a type matching <paramref name="query"/>.</summary>
        ''' <param name="type">The invitation type.</param>
        ''' <param name="query">Filters; <see cref="ListQuery.Count"/> sets the page size.</param>
        ''' <param name="maxItems">Stops after this many invitations.</param>
        ''' <param name="cancellationToken">Cancels the requests.</param>
        ''' <example>
        ''' <code>
        ''' Dim all = Await ojs.Invitations.ListAllAsync(InvitationTypes.UserRoleAssignment)
        ''' Console.WriteLine($"{all.Count} invitations")
        ''' </code>
        ''' </example>
        Public Function ListAllAsync(type As String, Optional query As InvitationQuery = Nothing, Optional maxItems As Integer? = Nothing,
                                     Optional cancellationToken As CancellationToken = Nothing) As Task(Of List(Of Invitation))
            ArgumentException.ThrowIfNullOrEmpty(type)
            Return ListAllCoreAsync(Of Invitation, InvitationQuery)(If(query, New InvitationQuery()),
                Function(q, ct) ListAsync(type, q, ct), maxItems, cancellationToken)
        End Function

        ''' <summary>Gets an invitation.</summary>
        ''' <remarks>
        ''' OJS 3.5.0 answers this route with 401 for every invitation we tried (it collides with
        ''' <c>GET /invitations/{type}</c>); use <see cref="ListAsync"/> with a status filter instead.
        ''' </remarks>
        ''' <param name="invitationId">The invitation ID.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim invitation = Await ojs.Invitations.GetAsync(6)
        ''' </code>
        ''' </example>
        Public Function GetAsync(invitationId As Integer, Optional cancellationToken As CancellationToken = Nothing) As Task(Of Invitation)
            Return Http.GetAsync(Of Invitation)($"invitations/{invitationId}", Nothing, cancellationToken)
        End Function

        Private NotInheritable Class CreatedEnvelope
            ''' <summary>The new invitation's ID.</summary>
            Public Property InvitationId As Integer?
        End Class

        Private Shared Function Wrap(data As Object) As Object
            Return New Dictionary(Of String, Object) From {{"invitationData", data}}
        End Function

        ''' <summary>Creates an invitation for an existing user (status <c>initialized</c>; nothing is sent yet).</summary>
        ''' <param name="type">The invitation type; see <see cref="InvitationTypes"/>.</param>
        ''' <param name="userId">The user to invite.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <returns>The new invitation's ID.</returns>
        ''' <example>
        ''' <code>
        ''' Dim id = Await ojs.Invitations.CreateForUserAsync(InvitationTypes.UserRoleAssignment, userId:=5)
        ''' </code>
        ''' </example>
        Public Function CreateForUserAsync(type As String, userId As Integer,
                                           Optional cancellationToken As CancellationToken = Nothing) As Task(Of Integer)
            Return CreateAsync(type, New Dictionary(Of String, Object) From {{"userId", userId}}, cancellationToken)
        End Function

        ''' <summary>Creates an invitation for someone without an account (status <c>initialized</c>; nothing is sent yet).</summary>
        ''' <param name="type">The invitation type; see <see cref="InvitationTypes"/>.</param>
        ''' <param name="email">The invitee's email address.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <returns>The new invitation's ID.</returns>
        ''' <example>
        ''' <code>
        ''' Dim id = Await ojs.Invitations.CreateForEmailAsync(InvitationTypes.UserRoleAssignment, "new.editor@example.com")
        ''' </code>
        ''' </example>
        Public Function CreateForEmailAsync(type As String, email As String,
                                            Optional cancellationToken As CancellationToken = Nothing) As Task(Of Integer)
            ArgumentException.ThrowIfNullOrEmpty(email)
            Return CreateAsync(type, New Dictionary(Of String, Object) From {{"inviteeEmail", email}}, cancellationToken)
        End Function

        ''' <summary>Creates an invitation from raw invitation data (sent as <c>invitationData</c>).</summary>
        ''' <param name="type">The invitation type; see <see cref="InvitationTypes"/>.</param>
        ''' <param name="invitationData">The initial data, e.g. <c>New With {.userId = 5}</c> or <c>New With {.inviteeEmail = "x@example.com"}</c>.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <returns>The new invitation's ID.</returns>
        ''' <exception cref="Errors.OjsValidationException">The user does not exist, or both or neither of user and email were given.</exception>
        ''' <example>
        ''' <code>
        ''' Dim id = Await ojs.Invitations.CreateAsync(InvitationTypes.UserRoleAssignment, New With {.userId = 5})
        ''' </code>
        ''' </example>
        Public Async Function CreateAsync(type As String, invitationData As Object,
                                          Optional cancellationToken As CancellationToken = Nothing) As Task(Of Integer)
            ArgumentException.ThrowIfNullOrEmpty(type)
            ArgumentNullException.ThrowIfNull(invitationData)
            Dim result = Await Http.PostAsync(Of CreatedEnvelope)($"invitations/add/{OjsHttp.Seg(type)}", Wrap(invitationData), cancellationToken).ConfigureAwait(False)
            If result?.InvitationId Is Nothing Then Throw New InvalidOperationException("OJS did not return an invitation ID.")
            Return result.InvitationId.Value
        End Function

        ''' <summary>Adds or changes data of an invitation that has not been sent yet.</summary>
        ''' <param name="invitationId">The invitation ID.</param>
        ''' <param name="invitationData">The data to set (sent as <c>invitationData</c>), e.g. a <see cref="UserRoleAssignmentData"/>.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <returns>The updated invitation.</returns>
        ''' <exception cref="Errors.OjsValidationException">The data is invalid (OJS answers 422).</exception>
        ''' <example>
        ''' <code>
        ''' Await ojs.Invitations.PopulateAsync(id, New UserRoleAssignmentData With {
        '''     .UserGroupsToAdd = New List(Of InvitationUserGroup) From {
        '''         New InvitationUserGroup With {.UserGroupId = 3, .Masthead = False, .DateStart = DateOnly.FromDateTime(Date.Today)}}})
        ''' </code>
        ''' </example>
        Public Function PopulateAsync(invitationId As Integer, invitationData As Object,
                                      Optional cancellationToken As CancellationToken = Nothing) As Task(Of Invitation)
            ArgumentNullException.ThrowIfNull(invitationData)
            Return Http.PutAsync(Of Invitation)($"invitations/{invitationId}/populate", Wrap(invitationData), cancellationToken)
        End Function

        ''' <summary>Sends the invitation email to the invitee.</summary>
        ''' <param name="invitationId">The invitation ID.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim sent = Await ojs.Invitations.InviteAsync(id)
        ''' Console.WriteLine(sent.StatusValue) ' Pending
        ''' </code>
        ''' </example>
        Public Function InviteAsync(invitationId As Integer, Optional cancellationToken As CancellationToken = Nothing) As Task(Of Invitation)
            Return Http.PutAsync(Of Invitation)($"invitations/{invitationId}/invite", Nothing, cancellationToken)
        End Function

        Private NotInheritable Class MailableEnvelope
            ''' <summary>The email preview.</summary>
            Public Property Mailable As InvitationEmailPreview
        End Class

        ''' <summary>Gets a preview of the email that will be sent with the invitation.</summary>
        ''' <param name="invitationId">The invitation ID.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim preview = Await ojs.Invitations.GetMailableAsync(id)
        ''' Console.WriteLine(preview.Subject)
        ''' </code>
        ''' </example>
        Public Async Function GetMailableAsync(invitationId As Integer, Optional cancellationToken As CancellationToken = Nothing) As Task(Of InvitationEmailPreview)
            Dim result = Await Http.GetAsync(Of MailableEnvelope)($"invitations/{invitationId}/getMailable", Nothing, cancellationToken).ConfigureAwait(False)
            Return result?.Mailable
        End Function

        ''' <summary>Cancels a sent (<c>pending</c>) invitation. Invitations that were never sent cannot be cancelled.</summary>
        ''' <param name="invitationId">The invitation ID.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Await ojs.Invitations.CancelAsync(id)
        ''' </code>
        ''' </example>
        Public Function CancelAsync(invitationId As Integer, Optional cancellationToken As CancellationToken = Nothing) As Task
            Return Http.PutAsync(Of System.Text.Json.JsonElement)($"invitations/{invitationId}/cancel", Nothing, cancellationToken)
        End Function

        ''' <summary>Gets an invitation with the access key from the invitation link.</summary>
        ''' <param name="invitationId">The invitation ID.</param>
        ''' <param name="key">The access key.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' ' id and key come from the link in the invitation email.
        ''' Dim invitation = Await ojs.Invitations.GetByKeyAsync(id, key)
        ''' </code>
        ''' </example>
        Public Function GetByKeyAsync(invitationId As Integer, key As String, Optional cancellationToken As CancellationToken = Nothing) As Task(Of Invitation)
            Return Http.GetAsync(Of Invitation)(KeyPath(invitationId, key, Nothing), Nothing, cancellationToken)
        End Function

        ''' <summary>Changes the invitee's details before accepting, with the access key.</summary>
        ''' <param name="invitationId">The invitation ID.</param>
        ''' <param name="key">The access key.</param>
        ''' <param name="payload">The data to change (sent as <c>invitationData</c>), e.g. a new user's name and password.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Await ojs.Invitations.RefineAsync(id, key, New UserRoleAssignmentData With {
        '''     .GivenName = LocalizedString.Create("en", "Ada"), .FamilyName = LocalizedString.Create("en", "Lovelace"),
        '''     .UserCountry = "GB", .Username = "ada", .Password = "a long secret passphrase"})
        ''' </code>
        ''' </example>
        Public Function RefineAsync(invitationId As Integer, key As String, payload As Object,
                                    Optional cancellationToken As CancellationToken = Nothing) As Task(Of Invitation)
            ArgumentNullException.ThrowIfNull(payload)
            Return Http.PutAsync(Of Invitation)(KeyPath(invitationId, key, "refine"), Wrap(payload), cancellationToken)
        End Function

        ''' <summary>Accepts an invitation with the access key.</summary>
        ''' <param name="invitationId">The invitation ID.</param>
        ''' <param name="key">The access key.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim accepted = Await ojs.Invitations.FinalizeAsync(id, key)
        ''' </code>
        ''' </example>
        Public Function FinalizeAsync(invitationId As Integer, key As String, Optional cancellationToken As CancellationToken = Nothing) As Task(Of Invitation)
            Return Http.PutAsync(Of Invitation)(KeyPath(invitationId, key, "finalize"), Nothing, cancellationToken)
        End Function

        ''' <summary>Declines an invitation with the access key.</summary>
        ''' <param name="invitationId">The invitation ID.</param>
        ''' <param name="key">The access key.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Await ojs.Invitations.DeclineAsync(id, key)
        ''' </code>
        ''' </example>
        Public Function DeclineAsync(invitationId As Integer, key As String, Optional cancellationToken As CancellationToken = Nothing) As Task(Of Invitation)
            Return Http.PutAsync(Of Invitation)(KeyPath(invitationId, key, "decline"), Nothing, cancellationToken)
        End Function

        Private Shared Function KeyPath(invitationId As Integer, key As String, action As String) As String
            ArgumentException.ThrowIfNullOrEmpty(key)
            Dim p = $"invitations/{invitationId}/key/{OjsHttp.Seg(key)}"
            Return If(action Is Nothing, p, p & "/" & action)
        End Function

    End Class

End Namespace
