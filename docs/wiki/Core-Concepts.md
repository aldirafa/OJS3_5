# Core Concepts

## Resources

`OjsClient` groups the API into **resources**, one per area:

* `ojs.Submissions`
* `ojs.Publications`
* `ojs.Issues`
* `ojs.Users`
* `ojs.Dois`
* `ojs.Stats`
* and the others listed on the [Home](Home) page.

Every method is asynchronous and accepts an optional `CancellationToken` as its last argument.

## Read models and edit models

Each kind of object has two classes:

| Read model (returned) | Edit model (sent) |
|---|---|
| `Submission` | `SubmissionEdit` |
| `Publication` | `PublicationEdit` |
| `Contributor` | `ContributorEdit` |
| `Announcement` | `AnnouncementEdit` |
| … | … |

* **Read models** have every property the server returns. List endpoints fill only the summary properties; `GetAsync` fills all of them.
* **Edit models** have only the properties you are allowed to change, and all of them are nullable. **Only the properties you set are sent**, so an update changes exactly those fields and leaves the rest alone.

```vb
' Changes the title only; abstract, keywords, authors ... stay as they are.
Await ojs.Publications.UpdateAsync(12, 15, New PublicationEdit With {
    .Title = LocalizedString.Create("en", "A better title")})
```

```csharp
// Changes the title only; abstract, keywords, authors ... stay as they are.
await ojs.Publications.UpdateAsync(12, 15, new PublicationEdit
{
    Title = LocalizedString.Create("en", "A better title")
});
```

```fsharp
// Changes the title only; abstract, keywords, authors ... stay as they are.
let! _ = ojs.Publications.UpdateAsync(12, 15, PublicationEdit(Title = LocalizedString.Create("en", "A better title")))
```

## Multilingual values

OJS stores most text per locale, for example `{"en": "Title", "id": "Judul"}`. The library has three types for this.

### `LocalizedString`

A dictionary from locale to text. `GetValue` returns the text in the locale you ask for. If that is empty, it tries the
fallback locale, then any locale that has a value.

```vb
Dim pub = Await ojs.Publications.GetAsync(12, 15)
Console.WriteLine(pub.Title.GetValue("id", fallbackLocale:="en"))

Dim title As New LocalizedString From {{"en", "My title"}, {"id", "Judul saya"}}
Dim oneLocale = LocalizedString.Create("en", "Only English")
```

```csharp
var pub = await ojs.Publications.GetAsync(12, 15);
Console.WriteLine(pub.Title.GetValue("id", fallbackLocale: "en"));

var title = new LocalizedString { ["en"] = "My title", ["id"] = "Judul saya" };
var oneLocale = LocalizedString.Create("en", "Only English");
```

```fsharp
let! pub = ojs.Publications.GetAsync(12, 15)
printfn "%s" (pub.Title.GetValue("id", fallbackLocale = "en"))

let title = LocalizedString()
title["en"] <- "My title"
title["id"] <- "Judul saya"
let oneLocale = LocalizedString.Create("en", "Only English")
```

### `LocalizedVocab`

Keywords, subjects, disciplines, supporting agencies and languages are controlled vocabularies. In OJS 3.5 every
entry is an object: `{"en": [{"name": "open access"}]}`. Use `GetNames` for the plain strings, and `GetEntries` for
the full entries, which carry `Name`, `Identifier` and `Source`.

```vb
Dim pub = Await ojs.Publications.GetAsync(12, 15)
Console.WriteLine(String.Join(", ", pub.Keywords.GetNames("en")))

Await ojs.Publications.UpdateAsync(12, 15, New PublicationEdit With {
    .Keywords = LocalizedVocab.Create("en", "open access", "scholarly publishing")})
```

```csharp
var pub = await ojs.Publications.GetAsync(12, 15);
Console.WriteLine(string.Join(", ", pub.Keywords.GetNames("en")));

await ojs.Publications.UpdateAsync(12, 15, new PublicationEdit
{
    Keywords = LocalizedVocab.Create("en", "open access", "scholarly publishing")
});
```

```fsharp
let! pub = ojs.Publications.GetAsync(12, 15)
printfn "%s" (String.Join(", ", pub.Keywords.GetNames "en"))

let! _ =
    ojs.Publications.UpdateAsync(12, 15,
        PublicationEdit(Keywords = LocalizedVocab.Create("en", "open access", "scholarly publishing")))
```

### `LocalizedList`

A per-locale list of plain strings. OJS 3.5 does not use it for keywords any more, but plugins and older versions do.

## Enums

Numeric OJS constants are .NET enums, so you write `WorkflowStage.Production` instead of `5`:

| Enum | OJS constants |
|---|---|
| `WorkflowStage` | `WORKFLOW_STAGE_ID_*` |
| `SubmissionStatus` | `STATUS_*` |
| `SubmissionFileStage` | `SubmissionFile::SUBMISSION_FILE_*` |
| `DoiStatus` | `Doi::STATUS_*` |
| `UserRole` | `Role::ROLE_ID_*` |
| `ReviewMethod` | review methods |
| `PublishingMode` | publishing modes |

An enum property can still hold a value that has no named member, for example a value added by a plugin.

## Dates

PHP sends dates as `"2026-01-31 10:00:00"` or `"2026-01-31"`. The library maps them to .NET types:

* full dates and times become `DateTime?`;
* date-only values become `DateOnly?`;
* `""` and `"0000-00-00"` become `Nothing` / `null`.

When writing, dates are sent back in the same PHP formats.

## Unknown and plugin properties

Plugins add properties such as `crossrefplugin::status`, and newer OJS versions add fields of their own. Every read
model keeps whatever it does not model in `AdditionalProperties`:

```vb
Dim s = Await ojs.Submissions.GetAsync(12)
Dim status As Text.Json.JsonElement
If s.CurrentPublication.AdditionalProperties?.TryGetValue("crossrefplugin::status", status) Then
    Console.WriteLine(status)
End If
```

```csharp
var s = await ojs.Submissions.GetAsync(12);
if (s.CurrentPublication.AdditionalProperties?.TryGetValue("crossrefplugin::status", out var status) == true)
    Console.WriteLine(status);
```

```fsharp
let! s = ojs.Submissions.GetAsync 12
match s.CurrentPublication.AdditionalProperties with
| null -> ()
| extra ->
    match extra.TryGetValue "crossrefplugin::status" with
    | true, status -> printfn "%O" status
    | _ -> ()
```

To add your own helpers to models, such as computed properties, write extension methods. [Advanced](Advanced) shows how.

## PHP quirks the library handles

The OJS backend is written in PHP, which shows in its JSON. The library accepts all of these when reading:

| The server sends | Where the model expects |
|---|---|
| `"5"` | a number |
| `0`, `1`, `"0"` or `"1"` | a boolean |
| `[]` | an empty object or dictionary |
| an object with numeric keys | a list |
| a plain string | a multilingual value |
| a bare array | a page of results, or a page where a list is expected |

The full list is on [OJS 3.5 Quirks](OJS-3.5-Quirks).
