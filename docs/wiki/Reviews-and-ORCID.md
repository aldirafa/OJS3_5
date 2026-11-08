# Reviews and ORCID

## Review assignments of a submission

Review assignments come with the submission:

```vb
Dim s = Await ojs.Submissions.GetAsync(12)
For Each r In s.ReviewAssignments
    Console.WriteLine($"#{r.Id} round {r.Round}: {r.ReviewerFullName} - {r.Status} (due {r.DateDue})")
Next
```

```csharp
var s = await ojs.Submissions.GetAsync(12);
foreach (var r in s.ReviewAssignments)
    Console.WriteLine($"#{r.Id} round {r.Round}: {r.ReviewerFullName} - {r.Status} (due {r.DateDue})");
```

```fsharp
let! s = ojs.Submissions.GetAsync 12
for r in s.ReviewAssignments do
    printfn "#%d round %A: %s - %s" r.Id.Value r.Round r.ReviewerFullName r.Status
```

## Exporting a review

A review can be exported as a PDF, or as JATS XML. With `authorFriendly:=True` the export leaves out the reviewer's
identity and the comments meant only for the editor, so you can share it with the author.

```vb
Using pdf = Await ojs.Reviews.DownloadPdfAsync(12, 21, authorFriendly:=True)
    Await pdf.SaveAsAsync("review-for-author.pdf")
End Using

Using xml = Await ojs.Reviews.DownloadXmlAsync(12, 21, authorFriendly:=False)
    Await xml.SaveAsAsync("review.xml")
End Using
```

```csharp
using (var pdf = await ojs.Reviews.DownloadPdfAsync(12, 21, authorFriendly: true))
    await pdf.SaveAsAsync("review-for-author.pdf");
```

```fsharp
use! pdf = ojs.Reviews.DownloadPdfAsync(12, 21, authorFriendly = true)
do! pdf.SaveAsAsync "review-for-author.pdf"
```

`ExportPdfAsync` and `ExportXmlAsync` return the file ID. Download it later with `DownloadExportAsync(submissionId, fileId)`.

## Accepting a request on the reviewer's behalf

`ConfirmAsync` is the editor's **accept or decline on behalf of the reviewer** action. Only journal managers and site
administrators can call it. A reviewer's own token gets `401`.

```vb
Await ojs.Reviews.ConfirmAsync(12, 21, accept:=True)

' OJS 3.5.0 returns the assignment as it was before the change, so read it again:
Dim updated = (Await ojs.Submissions.GetAsync(12)).ReviewAssignments.First(Function(r) r.Id.GetValueOrDefault() = 21)
Console.WriteLine(updated.Status)   ' "This review has been accepted."
```

## Review history (as the reviewer)

`GetHistoryAsync` returns what a reviewer submitted in a round:

* the recommendation;
* the comments for the author and the private comments for the editor;
* the files.

OJS looks the review up by the **current user**, so call it with the reviewer's own token. Everyone else, including
administrators, gets `404`.

```vb
Using asReviewer As New OjsClient(New OjsClientOptions With {
    .BaseUrl = "https://journals.example.com", .JournalPath = "jpk", .ApiToken = "<reviewer token>"})
    Dim history = Await asReviewer.Reviews.GetHistoryAsync(12, reviewRoundId:=5)
    Console.WriteLine($"{history.PublicationTitle.GetValue("en")}: {history.Recommendation}")
    For Each c In history.Comments
        Console.WriteLine(c)
    Next
End Using
```

```csharp
using var reviewer = new OjsClient(new OjsClientOptions { BaseUrl = "https://journals.example.com", JournalPath = "jpk", ApiToken = "<reviewer token>" });
var history = await reviewer.Reviews.GetHistoryAsync(12, reviewRoundId: 5);
Console.WriteLine($"{history.Recommendation}: {string.Join(" ", history.Comments)}");
```

## ORCID

These need the journal's ORCID settings, found under **Users & Roles > ORCID**.

```vb
' Email the author a link to connect their ORCID iD to this contribution
Await ojs.Orcid.RequestAuthorVerificationAsync(authorId:=31)

' Remove the ORCID iD and its access tokens from a contribution
Await ojs.Orcid.DeleteForAuthorAsync(authorId:=31)

' Queue a completed review for deposit to the reviewer's ORCID record
Await ojs.Reviews.SendToOrcidAsync(12, 21)
```

```csharp
await ojs.Orcid.RequestAuthorVerificationAsync(authorId: 31);
await ojs.Orcid.DeleteForAuthorAsync(authorId: 31);
await ojs.Reviews.SendToOrcidAsync(12, 21);
```

`authorId` is a **contributor** ID (`Contributor.Id`), not a user ID.

Writing reviews or works to ORCID records requires the **Member API**, and the reviewer must have connected their
ORCID iD. `SendToOrcidAsync` only queues a job, so a failure shows up in the OJS job log rather than as an exception.
