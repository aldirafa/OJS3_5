# Submissions

A **submission** is an article on its way through the workflow. Its metadata lives in one or more **publications**,
one per version. Each publication has its own:

* title, abstract and keywords;
* **contributors** (authors);
* galleys.

## Finding submissions

`SubmissionQuery` supports these filters:

* status and stage;
* section, category and issue;
* assigned user;
* incomplete, overdue and inactive submissions;
* free-text search.

```vb
Dim page = Await ojs.Submissions.ListAsync(New SubmissionQuery With {
    .StageIds = {WorkflowStage.ExternalReview},
    .IsOverdue = True,
    .OrderBy = SubmissionOrderBy.LastActivity,
    .OrderDirection = SortDirection.Descending})

For Each s In page.Items
    Console.WriteLine($"#{s.Id} {s.CurrentPublication?.Title.GetValue("en")} - last activity {s.DateLastActivity:d}")
Next
```

```csharp
var page = await ojs.Submissions.ListAsync(new SubmissionQuery
{
    StageIds = new[] { WorkflowStage.ExternalReview },
    IsOverdue = true,
    OrderBy = SubmissionOrderBy.LastActivity,
    OrderDirection = SortDirection.Descending
});

foreach (var s in page.Items)
    Console.WriteLine($"#{s.Id} {s.CurrentPublication?.Title.GetValue("en")} - last activity {s.DateLastActivity:d}");
```

```fsharp
let! page =
    ojs.Submissions.ListAsync(
        SubmissionQuery(
            StageIds = [| WorkflowStage.ExternalReview |],
            IsOverdue = true,
            OrderBy = SubmissionOrderBy.LastActivity,
            OrderDirection = SortDirection.Descending))

for s in page.Items do
    printfn "#%d %A" s.Id.Value s.DateLastActivity
```

`Submission.CurrentPublication` picks the current version from `Publications` for you.

## A submission in detail

```vb
Dim s = Await ojs.Submissions.GetAsync(12)
Console.WriteLine($"{s.StatusLabel}, stage {s.StageId}, submitted {s.DateSubmitted:d}")

For Each st In s.Stages
    Console.WriteLine($"  {st.Label}: active={st.IsActiveStage}")
Next
For Each r In s.ReviewAssignments
    Console.WriteLine($"  review by {r.ReviewerFullName}: {r.Status}")
Next
```

## Creating and submitting (author flow)

```vb
' 1. Create the draft
Dim draft = Await ojs.Submissions.CreateAsync(New SubmissionEdit With {.Locale = "en", .SectionId = 1})
Dim pubId = draft.CurrentPublicationId.Value

' 2. Metadata
Await ojs.Publications.UpdateAsync(draft.Id.Value, pubId, New PublicationEdit With {
    .Title = LocalizedString.Create("en", "On the Shoulders of Giants"),
    .Abstract = LocalizedString.Create("en", "<p>We revisit ...</p>"),
    .Keywords = LocalizedVocab.Create("en", "history of science")})

' 3. Authors
Await ojs.Contributors.CreateAsync(draft.Id.Value, pubId, New ContributorEdit With {
    .GivenName = LocalizedString.Create("en", "Ada"),
    .FamilyName = LocalizedString.Create("en", "Lovelace"),
    .Email = "ada@example.com", .Country = "GB", .UserGroupId = 14,
    .Affiliations = New List(Of Affiliation) From {
        New Affiliation With {.Name = LocalizedString.Create("en", "University of London")}}})

' 4. The manuscript
Using pdf = File.OpenRead("manuscript.pdf")
    Await ojs.SubmissionFiles.UploadAsync(draft.Id.Value, pdf, "manuscript.pdf",
        New SubmissionFileEdit With {.FileStage = SubmissionFileStage.Submission, .GenreId = 1})
End Using

' 5. Submit
Await ojs.Submissions.SubmitAsync(draft.Id.Value, confirmCopyright:=True)
```

```csharp
var draft = await ojs.Submissions.CreateAsync(new SubmissionEdit { Locale = "en", SectionId = 1 });
var pubId = draft.CurrentPublicationId.Value;

await ojs.Publications.UpdateAsync(draft.Id.Value, pubId, new PublicationEdit
{
    Title = LocalizedString.Create("en", "On the Shoulders of Giants"),
    Abstract = LocalizedString.Create("en", "<p>We revisit ...</p>"),
    Keywords = LocalizedVocab.Create("en", "history of science")
});

await ojs.Contributors.CreateAsync(draft.Id.Value, pubId, new ContributorEdit
{
    GivenName = LocalizedString.Create("en", "Ada"),
    FamilyName = LocalizedString.Create("en", "Lovelace"),
    Email = "ada@example.com", Country = "GB", UserGroupId = 14,
    Affiliations = new List<Affiliation> { new() { Name = LocalizedString.Create("en", "University of London") } }
});

await using (var pdf = File.OpenRead("manuscript.pdf"))
    await ojs.SubmissionFiles.UploadAsync(draft.Id.Value, pdf, "manuscript.pdf",
        new SubmissionFileEdit { FileStage = SubmissionFileStage.Submission, GenreId = 1 });

await ojs.Submissions.SubmitAsync(draft.Id.Value, confirmCopyright: true);
```

```fsharp
let! draft = ojs.Submissions.CreateAsync(SubmissionEdit(Locale = "en", SectionId = 1))
let sid, pubId = draft.Id.Value, draft.CurrentPublicationId.Value

let! _ =
    ojs.Publications.UpdateAsync(sid, pubId,
        PublicationEdit(
            Title = LocalizedString.Create("en", "On the Shoulders of Giants"),
            Keywords = LocalizedVocab.Create("en", "history of science")))

let! _ =
    ojs.Contributors.CreateAsync(sid, pubId,
        ContributorEdit(
            GivenName = LocalizedString.Create("en", "Ada"),
            FamilyName = LocalizedString.Create("en", "Lovelace"),
            Email = "ada@example.com", Country = "GB", UserGroupId = 14))

let! _ = ojs.Submissions.SubmitAsync(sid, confirmCopyright = true)
```

`UserGroupId` is the ID of the contributor role, usually "Author". You can see the IDs in the `Groups` of any user
returned by `ojs.Users`.

## Contributors

```vb
Dim authors = Await ojs.Contributors.ListAsync(12, 15)
For Each a In authors
    Console.WriteLine($"{a.Seq}. {a.FullName} <{a.Email}> {a.Orcid}")
Next

' Change the order: the given IDs first, in this order
Await ojs.Contributors.SaveOrderAsync(12, 15, authors.Select(Function(a) a.Id.Value).Reverse())
```

## Versions and publishing

```vb
Dim v1 = Await ojs.Publications.GetAsync(12, 15)
Await ojs.Publications.PublishAsync(12, v1.Id.Value)

' Later: a correction as version 2
Dim v2 = Await ojs.Publications.CreateVersionAsync(12, v1.Id.Value)
Await ojs.Publications.UpdateAsync(12, v2.Id.Value, New PublicationEdit With {
    .Abstract = LocalizedString.Create("en", "<p>Corrected abstract.</p>")})
Await ojs.Publications.PublishAsync(12, v2.Id.Value)
```

```csharp
var v1 = await ojs.Publications.GetAsync(12, 15);
await ojs.Publications.PublishAsync(12, v1.Id.Value);

var v2 = await ojs.Publications.CreateVersionAsync(12, v1.Id.Value);
await ojs.Publications.UpdateAsync(12, v2.Id.Value, new PublicationEdit
{
    Abstract = LocalizedString.Create("en", "<p>Corrected abstract.</p>")
});
await ojs.Publications.PublishAsync(12, v2.Id.Value);
```

Publishing checks that the publication is ready, for example that it has an issue and the required metadata. When it
is not, you get an `OjsValidationException`.

## Editorial decisions

Each submission lists the decisions the current user can make in `AvailableEditorialDecisions`. Take the decision
type from there rather than hard-coding a number:

```vb
Dim s = Await ojs.Submissions.GetAsync(12)
Dim sendToReview = s.AvailableEditorialDecisions.First(Function(d) d.Label.Contains("Review"))
Await ojs.Decisions.CreateAsync(12, New DecisionEdit With {.Decision = sendToReview.Id})

Dim history = Await ojs.Decisions.ListAsync(12)
For Each d In history
    Console.WriteLine($"{d.DateDecided:d}: decision {d.Decision} by editor {d.EditorId}")
Next
```

```csharp
var s = await ojs.Submissions.GetAsync(12);
var sendToReview = s.AvailableEditorialDecisions.First(d => d.Label.Contains("Review"));
await ojs.Decisions.CreateAsync(12, new DecisionEdit { Decision = sendToReview.Id });
```

## Participants

```vb
Dim everyone = Await ojs.Submissions.GetParticipantsAsync(12)
Dim production = Await ojs.Submissions.GetParticipantsAsync(12, WorkflowStage.Production)
```

## Reviewer suggestions

Authors can suggest reviewers when the journal turns this on (**Workflow > Review**).

```vb
Await ojs.ReviewerSuggestions.CreateAsync(12, New ReviewerSuggestionEdit With {
    .GivenName = LocalizedString.Create("en", "Grace"),
    .FamilyName = LocalizedString.Create("en", "Hopper"),
    .Email = "grace@example.com",
    .Affiliation = LocalizedString.Create("en", "Example University"),
    .SuggestionReason = LocalizedString.Create("en", "Expert in compilers")})

Dim open = Await ojs.ReviewerSuggestions.ListAsync(12, approved:=False)
```

```csharp
await ojs.ReviewerSuggestions.CreateAsync(12, new ReviewerSuggestionEdit
{
    GivenName = LocalizedString.Create("en", "Grace"),
    FamilyName = LocalizedString.Create("en", "Hopper"),
    Email = "grace@example.com",
    Affiliation = LocalizedString.Create("en", "Example University"),
    SuggestionReason = LocalizedString.Create("en", "Expert in compilers")
});
```
