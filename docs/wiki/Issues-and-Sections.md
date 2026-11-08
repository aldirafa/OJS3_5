# Issues and Sections

## Issues

```vb
' The current issue, with its articles
Dim current = Await ojs.Issues.GetCurrentAsync()
Console.WriteLine($"{current.Identification} - published {current.DatePublished:d}")
For Each article In current.Articles
    Console.WriteLine("  " & article.CurrentPublication?.Title.GetValue("en"))
Next

' The archive: published issues, newest first
Dim archive = Await ojs.Issues.ListAllAsync(New IssueQuery With {
    .IsPublished = True, .OrderBy = IssueOrderBy.DatePublished, .OrderDirection = SortDirection.Descending})

' Issues of particular volumes or years
Dim vol5 = Await ojs.Issues.ListAsync(New IssueQuery With {.Volumes = {5}, .Years = {2025, 2026}})
```

```csharp
var current = await ojs.Issues.GetCurrentAsync();
Console.WriteLine($"{current.Identification} - published {current.DatePublished:d}");
foreach (var article in current.Articles)
    Console.WriteLine("  " + article.CurrentPublication?.Title.GetValue("en"));

var archive = await ojs.Issues.ListAllAsync(new IssueQuery
{
    IsPublished = true, OrderBy = IssueOrderBy.DatePublished, OrderDirection = SortDirection.Descending
});

var vol5 = await ojs.Issues.ListAsync(new IssueQuery { Volumes = new[] { 5 }, Years = new[] { 2025, 2026 } });
```

```fsharp
let! current = ojs.Issues.GetCurrentAsync()
printfn "%s - published %A" current.Identification current.DatePublished
for article in current.Articles do
    match article.CurrentPublication with
    | null -> ()
    | p -> printfn "  %s" (p.Title.GetValue "en")

let! archive =
    ojs.Issues.ListAllAsync(
        IssueQuery(IsPublished = true, OrderBy = IssueOrderBy.DatePublished, OrderDirection = SortDirection.Descending))
```

`GetAsync(issueId)` returns the same full issue for any issue. Issue properties include:

* `Volume`, `Number`, `Year` and `Identification`;
* `Title` and `Description`;
* `Galleys`;
* the DOI, in `DoiObject`.

## Sections

```vb
For Each s In Await ojs.Sections.ListAllAsync(New SectionQuery With {.IsInactive = False})
    Console.WriteLine($"{s.Id} {s.Abbrev.GetValue("en")}: {s.Title.GetValue("en")} (word limit {s.WordCount})")
Next
```

```csharp
foreach (var s in await ojs.Sections.ListAllAsync(new SectionQuery { IsInactive = false }))
    Console.WriteLine($"{s.Id} {s.Abbrev.GetValue("en")}: {s.Title.GetValue("en")} (word limit {s.WordCount})");
```

```fsharp
let! sections = ojs.Sections.ListAllAsync(SectionQuery(IsInactive = false))
for s in sections do
    printfn "%d %s" s.Id.Value (s.Title.GetValue "en")
```

The section ID is what `SubmissionEdit.SectionId` and `SubmissionQuery.SectionIds` expect.
