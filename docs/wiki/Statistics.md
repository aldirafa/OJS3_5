# Statistics

## Editorial statistics

These are the figures from the editorial activity report:

* submissions received, accepted, declined and published;
* days to the first decision;
* the acceptance rate.

```vb
Dim stats = Await ojs.Stats.GetEditorialAsync(New DateOnly(2026, 1, 1), New DateOnly(2026, 12, 31))
For Each s In stats
    Console.WriteLine($"{s.Key,-32} {s.Value}")
Next

Dim averages = Await ojs.Stats.GetEditorialAveragesAsync()   ' yearly averages, -1 when there is not a full year yet
Console.WriteLine(averages("submissionsReceived"))
```

```csharp
var stats = await ojs.Stats.GetEditorialAsync(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31));
foreach (var s in stats)
    Console.WriteLine($"{s.Key,-32} {s.Value}");

var averages = await ojs.Stats.GetEditorialAveragesAsync();
Console.WriteLine(averages["submissionsReceived"]);
```

```fsharp
let! stats = ojs.Stats.GetEditorialAsync(DateOnly(2026, 1, 1), DateOnly(2026, 12, 31))
for s in stats do
    printfn "%-32s %A" s.Key s.Value
```

`Key` stays the same in every language, while `Name` is translated, so match on `Key`.

## Users per role

```vb
For Each u In Await ojs.Stats.GetUsersAsync(status:=UserStatusFilter.Active)
    Console.WriteLine(If(u.IsTotal, $"All users: {u.Value}", $"{u.Role}: {u.Value}"))
Next
```

## Usage statistics

Views and downloads are counted by OJS's daily scheduled task. **Today's visits appear tomorrow**, and only if the
scheduled tasks run through cron or the Acron plugin. Without dates, OJS returns the last 30 days.

| Group | What it counts |
|---|---|
| `ojs.Stats.Publications` | Abstract and galley views per article; file downloads; visitors by country, region and city |
| `ojs.Stats.Issues` | Table of contents and issue galley views |
| `ojs.Stats.Contexts` | Journal homepage views. Listing all journals is site-wide and needs a site administrator. |

Each group has a list, a single-item view and a timeline. Most methods also have a `...CsvAsync` version.

```vb
Dim q As New PublicationStatsQuery With {
    .DateStart = New DateOnly(2026, 1, 1),
    .DateEnd = New DateOnly(2026, 6, 30),
    .Count = 10}

' Most read articles
Dim top = Await ojs.Stats.Publications.ListAsync(q)
For Each p In top.Items
    Console.WriteLine($"{p.Publication.FullTitle.GetValue("en")}: {p.AbstractViews} abstract, {p.PdfViews} PDF")
Next

' Views per month for one article
Dim timeline = Await ojs.Stats.Publications.GetTimelineAsync(12, TimelineResolution.Month)
For Each point In timeline
    Console.WriteLine($"{point.Label}: {point.Value}")
Next

' Where readers come from (needs geographical statistics turned on)
Dim countries = Await ojs.Stats.Publications.ListCountriesAsync(q)
Dim cities = Await ojs.Stats.Publications.ListCitiesAsync(q)
For Each c In cities.Items
    Console.WriteLine($"{c.City}, {c.Region}, {c.Country}: {c.Total} ({c.Unique} unique)")
Next

' The same report as CSV, e.g. for Excel
Dim csv = Await ojs.Stats.Publications.ListCsvAsync(q)
Await File.WriteAllTextAsync("top-articles.csv", csv)
```

```csharp
var q = new PublicationStatsQuery
{
    DateStart = new DateOnly(2026, 1, 1),
    DateEnd = new DateOnly(2026, 6, 30),
    Count = 10
};

var top = await ojs.Stats.Publications.ListAsync(q);
foreach (var p in top.Items)
    Console.WriteLine($"{p.Publication.FullTitle.GetValue("en")}: {p.AbstractViews} abstract, {p.PdfViews} PDF");

var timeline = await ojs.Stats.Publications.GetTimelineAsync(12, TimelineResolution.Month);

var cities = await ojs.Stats.Publications.ListCitiesAsync(q);
foreach (var c in cities.Items)
    Console.WriteLine($"{c.City}, {c.Region}, {c.Country}: {c.Total} ({c.Unique} unique)");

await File.WriteAllTextAsync("top-articles.csv", await ojs.Stats.Publications.ListCsvAsync(q));
```

```fsharp
let q = PublicationStatsQuery(DateStart = DateOnly(2026, 1, 1), DateEnd = DateOnly(2026, 6, 30), Count = 10)

let! top = ojs.Stats.Publications.ListAsync q
for p in top.Items do
    printfn "%s: %A abstract views" (p.Publication.FullTitle.GetValue "en") p.AbstractViews

let! csv = ojs.Stats.Publications.ListCsvAsync q
do! File.WriteAllTextAsync("top-articles.csv", csv)
```

Other reports:

```vb
Dim q As New PublicationStatsQuery With {.DateStart = New DateOnly(2026, 1, 1)}

' Most downloaded files
Dim files = Await ojs.Stats.Publications.ListFilesAsync(q)

' Daily views of all articles, counting only file downloads
Dim daily = Await ojs.Stats.Publications.GetTimelineAsync(q, TimelineResolution.Day, filesOnly:=True)

' Issues
Dim issues = Await ojs.Stats.Issues.ListAsync(New IssueStatsQuery With {.DateStart = New DateOnly(2026, 1, 1)})
Dim oneIssue = Await ojs.Stats.Issues.GetAsync(4)

' Journal homepage
Dim home = Await ojs.Stats.Contexts.GetAsync(1, New DateOnly(2026, 1, 1), New DateOnly(2026, 6, 30))
Console.WriteLine($"Homepage views: {home.Total}")
```

> OJS returns the list reports as pages (`{itemsMax, items}`), although its OpenAPI document says they are arrays.
> The library reads both.
