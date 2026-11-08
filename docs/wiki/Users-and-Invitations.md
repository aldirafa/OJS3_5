# Users and Invitations

## Users

```vb
' Section editors and managers, by family name
Dim editors = Await ojs.Users.ListAllAsync(New UserQuery With {
    .RoleIds = {UserRole.Manager, UserRole.SubEditor},
    .OrderBy = UserOrderBy.FamilyName})

For Each u In editors
    Dim roles = String.Join(", ", u.Groups.Select(Function(g) g.Name.GetValue("en")))
    Console.WriteLine($"{u.FullName} <{u.Email}>: {roles}")
Next

' Everyone assigned to a submission in the review stage
Dim reviewStageUsers = Await ojs.Users.ListAsync(New UserQuery With {
    .AssignedToSubmission = 12, .AssignedToSubmissionStage = WorkflowStage.ExternalReview})
```

```csharp
var editors = await ojs.Users.ListAllAsync(new UserQuery
{
    RoleIds = new[] { UserRole.Manager, UserRole.SubEditor },
    OrderBy = UserOrderBy.FamilyName
});

foreach (var u in editors)
{
    var roles = string.Join(", ", u.Groups.Select(g => g.Name.GetValue("en")));
    Console.WriteLine($"{u.FullName} <{u.Email}>: {roles}");
}
```

```fsharp
let! editors =
    ojs.Users.ListAllAsync(UserQuery(RoleIds = [| UserRole.Manager; UserRole.SubEditor |], OrderBy = UserOrderBy.FamilyName))
for u in editors do
    let roles = u.Groups |> Seq.map (fun g -> g.Name.GetValue "en") |> String.concat ", "
    printfn "%s <%s>: %s" u.FullName u.Email roles
```

Remove a role from a user. The `userGroupId` comes from the user's `Groups`:

```vb
Await ojs.Users.EndRoleAsync(userId:=7, userGroupId:=14)
```

## Reviewers

`ListReviewersAsync` returns `ReviewerSummary` items, which are users with their review statistics:

* `ReviewsActive`, `ReviewsCompleted`, `ReviewsDeclined` and `ReviewsCancelled`;
* `AverageReviewCompletionDays`;
* `ReviewerRating`.

The count filters take a single number, which means a minimum, or a range such as `"2-5"`.

```vb
' Experienced reviewers who are not busy
Dim available = Await ojs.Users.ListReviewersAsync(New ReviewerQuery With {
    .ReviewsCompleted = "3",
    .ReviewsActive = "0-1",
    .ReviewerRating = 4,
    .SearchPhrase = "linguistics"})

For Each r In available.Items
    Console.WriteLine($"{r.FullName}: {r.ReviewsCompleted} done, {r.ReviewsActive} active, ~{r.AverageReviewCompletionDays} days")
Next
```

```csharp
var available = await ojs.Users.ListReviewersAsync(new ReviewerQuery
{
    ReviewsCompleted = "3", ReviewsActive = "0-1", ReviewerRating = 4, SearchPhrase = "linguistics"
});
foreach (var r in available.Items)
    Console.WriteLine($"{r.FullName}: {r.ReviewsCompleted} done, {r.ReviewsActive} active");
```

## Invitations

Invitations ask someone to take on roles in the journal, for example to become a section editor or a reviewer. The
flow has three steps:

1. **Create** the invitation for an existing user or an email address. Its status is `INITIALIZED` and nothing is sent yet.
2. **Populate** it with the roles to add and, optionally, the email text.
3. **Invite**: OJS sends the email and the status becomes `PENDING`. The invitee accepts or declines through the link in that email.

```vb
Dim id = Await ojs.Invitations.CreateForUserAsync(InvitationTypes.UserRoleAssignment, userId:=5)

Await ojs.Invitations.PopulateAsync(id, New UserRoleAssignmentData With {
    .UserGroupsToAdd = New List(Of InvitationUserGroup) From {
        New InvitationUserGroup With {.UserGroupId = 5, .Masthead = True, .DateStart = DateOnly.FromDateTime(Date.Today)}},
    .EmailComposer = New InvitationEmail With {
        .EmailSubject = "Join our editorial team",
        .EmailBody = "<p>We would be glad to have you as a section editor.</p>"}})

Dim preview = Await ojs.Invitations.GetMailableAsync(id)   ' what will be sent
Dim sent = Await ojs.Invitations.InviteAsync(id)
Console.WriteLine($"{sent.StatusValue}, expires {sent.ExpiryDate:g}")
```

```csharp
var id = await ojs.Invitations.CreateForUserAsync(InvitationTypes.UserRoleAssignment, userId: 5);

await ojs.Invitations.PopulateAsync(id, new UserRoleAssignmentData
{
    UserGroupsToAdd = new List<InvitationUserGroup>
    {
        new() { UserGroupId = 5, Masthead = true, DateStart = DateOnly.FromDateTime(DateTime.Today) }
    },
    EmailComposer = new InvitationEmail
    {
        EmailSubject = "Join our editorial team",
        EmailBody = "<p>We would be glad to have you as a section editor.</p>"
    }
});

var sent = await ojs.Invitations.InviteAsync(id);
Console.WriteLine($"{sent.StatusValue}, expires {sent.ExpiryDate:g}");
```

```fsharp
let! id = ojs.Invitations.CreateForUserAsync(InvitationTypes.UserRoleAssignment, userId = 5)

let groups = Collections.Generic.List<InvitationUserGroup>()
groups.Add(InvitationUserGroup(UserGroupId = 5, Masthead = true, DateStart = DateOnly.FromDateTime DateTime.Today))
let! _ = ojs.Invitations.PopulateAsync(id, UserRoleAssignmentData(UserGroupsToAdd = groups))

let! sent = ojs.Invitations.InviteAsync id
printfn "%A" sent.StatusValue
```

To invite someone who has no account, use `CreateForEmailAsync(type, "person@example.com")`. When they accept, they
choose a user name and password.

Validation errors (a `422` response) arrive as `OjsValidationException`. Sending without roles, for example, gives
`userGroupsToAdd: This field is required.`

Manage invitations:

```vb
Dim pending = Await ojs.Invitations.ListAsync(InvitationTypes.UserRoleAssignment,
    New InvitationQuery With {.Status = InvitationStatus.Pending})
For Each inv In pending.Items
    Console.WriteLine($"{inv.Id}: {inv.ExistingUser?.Email} until {inv.ExpiryDate:d}")
Next
Await ojs.Invitations.CancelAsync(id)   ' only pending invitations can be cancelled
```

> In OJS 3.5.0, `GET /invitations/{id}` answers 401 even for the inviter, so use `ListAsync` instead of `GetAsync`.
> Cancelled invitations disappear from every list. See [OJS 3.5 Quirks](OJS-3.5-Quirks).
