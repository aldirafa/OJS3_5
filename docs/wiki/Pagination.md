# Pagination

List endpoints return one **page** at a time. OJS defaults to 20 items per page and allows at most 100. The library
offers three ways to read them.

## 1. One page: `ListAsync`

`ListAsync` returns a `PagedResult(Of T)`. `ItemsMax` is the total number of matches; `Items` holds the current page.
Set `Count` and `Offset` on the query to choose the page.

```vb
Dim page = Await ojs.Submissions.ListAsync(New SubmissionQuery With {.Count = 50, .Offset = 100})
Console.WriteLine($"Showing {page.Items.Count} of {page.ItemsMax}")
```

```csharp
var page = await ojs.Submissions.ListAsync(new SubmissionQuery { Count = 50, Offset = 100 });
Console.WriteLine($"Showing {page.Items.Count} of {page.ItemsMax}");
```

```fsharp
let! page = ojs.Submissions.ListAsync(SubmissionQuery(Count = 50, Offset = 100))
printfn "Showing %d of %d" page.Items.Count page.ItemsMax
```

## 2. Everything: `ListAllAsync`

`ListAllAsync` requests page after page and returns one list. `Count` on the query sets the page size, which defaults
to 100. `maxItems` stops early.

```vb
Dim published = Await ojs.Submissions.ListAllAsync(
    New SubmissionQuery With {.Status = {SubmissionStatus.Published}}, maxItems:=1000)
```

```csharp
var published = await ojs.Submissions.ListAllAsync(
    new SubmissionQuery { Status = new[] { SubmissionStatus.Published } }, maxItems: 1000);
```

```fsharp
let! published =
    ojs.Submissions.ListAllAsync(SubmissionQuery(Status = [| SubmissionStatus.Published |]), maxItems = 1000)
```

## 3. Streaming: `Enumerate`

`Enumerate` returns an `IAsyncEnumerable(Of T)` that fetches the next page only when you get to it. Use it to process
large lists without holding them all in memory, or to stop early.

```csharp
await foreach (var issue in ojs.Issues.Enumerate(new IssueQuery { IsPublished = true }))
{
    Console.WriteLine(issue.Identification);
    if (issue.Year < 2020) break;   // later pages are never requested
}
```

VB has no `Await For Each`. Use the enumerator directly:

```vb
Dim e = ojs.Issues.Enumerate(New IssueQuery With {.IsPublished = True}).GetAsyncEnumerator()
Try
    While Await e.MoveNextAsync()
        Console.WriteLine(e.Current.Identification)
    End While
Finally
    e.DisposeAsync().AsTask().GetAwaiter().GetResult()
End Try
```

In F#, use the [FSharp.Control.TaskSeq](https://github.com/fsprojects/FSharp.Control.TaskSeq) package, or
`ListAllAsync`:

```fsharp nocheck
open FSharp.Control
let issues = ojs.Issues.Enumerate(IssueQuery(IsPublished = true))
do! issues |> TaskSeq.iter (fun i -> printfn "%s" i.Identification)
```

## Endpoints without paging

Some endpoints return everything at once as a `List(Of T)`:

* contributors;
* publications of a submission;
* submission files;
* decisions;
* mailables;
* vocabularies;
* the email log.

## Your own endpoints

`Pager.GetAllAsync` and `Pager.Enumerate` work with any function that fetches a page by offset:

```vb
Dim all = Await Paging.Pager.GetAllAsync(Of Issue)(
    Function(offset, ct) ojs.Issues.ListAsync(New IssueQuery With {.Offset = offset, .Count = 100}, ct))
```
