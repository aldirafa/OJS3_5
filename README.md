# OJS 3.5 .NET Client

A .NET client for the **[Open Journal Systems](https://pkp.sfu.ca/software/ojs/) 3.5 REST API**. It is written in
VB.NET and works from any .NET language: VB.NET, C#, F#, PowerShell and others.

* **Complete.** It covers every public endpoint of OJS 3.5:
  * submissions, publications, contributors, files, decisions and reviews;
  * issues, sections, users and invitations;
  * journals, site settings, themes, announcements and institutions;
  * email templates, DOIs, ORCID, statistics, vocabularies and RORs.
* **Typed.** Models with enums for OJS constants, and multilingual values (`LocalizedString`, `LocalizedVocab`).
  Edit models send only the fields you set.
* **Tested against real servers.** The PHP backend's quirks are handled for you: string numbers, `"Y-m-d H:i:s"`
  dates, `[]` for empty objects, responses that differ from the OpenAPI document, and more. See
  [OJS 3.5 Quirks](docs/wiki/OJS-3.5-Quirks.md).
* **Convenient.** Automatic paging, `IAsyncEnumerable` streaming, typed exceptions with validation messages, file
  uploads and downloads, and CSV statistics.
* **No dependencies.** It needs only `System.Net.Http` and `System.Text.Json` from .NET 10.

## Install

```
dotnet add package Ojs.Api.Client --prerelease
```

The package contains the `Library` assembly; the types are in the `Library` namespace (`Library.OjsClient`, `Library.Models`, ...).

## Quick start

You need an API token: in OJS, open **User Profile > API Key**. The server must have `api_key_secret` set in
`config.inc.php`.

**VB.NET**

```vb
Imports Library
Imports Library.Models
Imports Library.Queries

Using ojs As New OjsClient(New OjsClientOptions With {
    .BaseUrl = "https://journals.example.com",
    .JournalPath = "jpk",
    .ApiToken = Environment.GetEnvironmentVariable("OJS_TOKEN")})

    Dim current = Await ojs.Issues.GetCurrentAsync()
    Console.WriteLine(current.Identification)
    For Each article In current.Articles
        Console.WriteLine("  " & article.CurrentPublication?.Title.GetValue("en"))
    Next
End Using
```

**C#**

```csharp
using Library;

using var ojs = new OjsClient(new OjsClientOptions
{
    BaseUrl = "https://journals.example.com",
    JournalPath = "jpk",
    ApiToken = Environment.GetEnvironmentVariable("OJS_TOKEN")
});

var current = await ojs.Issues.GetCurrentAsync();
Console.WriteLine(current.Identification);
foreach (var article in current.Articles)
    Console.WriteLine("  " + article.CurrentPublication?.Title.GetValue("en"));
```

**F#**

```fsharp
open Library

task {
    use ojs =
        new OjsClient(OjsClientOptions(
            BaseUrl = "https://journals.example.com",
            JournalPath = "jpk",
            ApiToken = Environment.GetEnvironmentVariable "OJS_TOKEN"))
    let! current = ojs.Issues.GetCurrentAsync()
    printfn "%s" current.Identification
}
```

If the installation's URLs contain `index.php` (e.g. `https://example.com/index.php/jpk`), set `UseIndexPhp = True`.

## Examples

### Pagination

```vb
' One page
Dim page = Await ojs.Submissions.ListAsync(New SubmissionQuery With {.Status = {SubmissionStatus.Published}, .Count = 50})
Console.WriteLine($"{page.Items.Count} of {page.ItemsMax}")

' Every page
Dim all = Await ojs.Submissions.ListAllAsync(New SubmissionQuery With {.StageIds = {WorkflowStage.ExternalReview}})
```

```csharp
// Stream page by page
await foreach (var issue in ojs.Issues.Enumerate(new IssueQuery { IsPublished = true }))
    Console.WriteLine(issue.Identification);
```

### Multilingual metadata and partial updates

```vb
Dim pub = Await ojs.Publications.GetAsync(12, 15)
Console.WriteLine(pub.Title.GetValue("id", fallbackLocale:="en"))
Console.WriteLine(String.Join(", ", pub.Keywords.GetNames("en")))

' Only the properties you set are sent; everything else stays as it is.
Await ojs.Publications.UpdateAsync(12, 15, New PublicationEdit With {
    .Title = New LocalizedString From {{"en", "A better title"}, {"id", "Judul yang lebih baik"}},
    .Keywords = LocalizedVocab.Create("en", "open access", "scholarly publishing")})
```

### Uploads

```vb
' Images and other settings files: upload, then reference
Dim tmp = Await ojs.TemporaryFiles.UploadAsync("cover.jpg")
Await ojs.Publications.UpdateAsync(12, 15, New PublicationEdit With {
    .CoverImage = New Dictionary(Of String, CoverImage) From {{"en", New CoverImage With {.TemporaryFileId = tmp.Id}}}})

' Submission files
Using pdf = File.OpenRead("manuscript.pdf")
    Await ojs.SubmissionFiles.UploadAsync(12, pdf, "manuscript.pdf",
        New SubmissionFileEdit With {.FileStage = SubmissionFileStage.Submission, .GenreId = 1})
End Using
```

### Errors

```csharp
try
{
    await ojs.Submissions.SubmitAsync(12, confirmCopyright: true);
}
catch (OjsValidationException ex)
{
    foreach (var (field, messages) in ex.Errors)
        Console.WriteLine($"{field}: {string.Join(" ", messages)}");
}
catch (OjsApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
{
    Console.WriteLine("No such submission.");
}
```

### Several journals

```vb
For Each journal In Await ojs.Contexts.ListAllAsync()
    Dim client = ojs.ForJournal(journal.UrlPath)   ' shares the connection
    Console.WriteLine($"{journal.Name.GetValue("en")}: {(Await client.Submissions.ListAsync()).ItemsMax} submissions")
Next
```

### DOIs and statistics

```vb
Await ojs.Dois.Submissions.AssignAsync({12, 13})
Using xml = Await ojs.Dois.Submissions.ExportAndDownloadAsync({12, 13})
    Await xml.SaveAsAsync(xml.FileName)
End Using

Dim top = Await ojs.Stats.Publications.ListAsync(New PublicationStatsQuery With {.DateStart = New DateOnly(2026, 1, 1), .Count = 10})
Dim csv = Await ojs.Stats.Publications.ListCountriesCsvAsync()
```

## API overview

| `OjsClient` property | Endpoints |
|---|---|
| `Submissions`, `Publications`, `Contributors`, `SubmissionFiles`, `Decisions`, `ReviewerSuggestions` | the editorial workflow |
| `Issues`, `Sections` | journal structure |
| `Users`, `Invitations` | accounts, roles and reviewers |
| `Contexts`, `Site` | journals, site settings and themes |
| `Announcements`, `Institutions` | news, and institutions for statistics |
| `EmailTemplates`, `Mailables`, `Emails` | emails |
| `Dois`, `Reviews`, `Orcid` | DOIs, review exports and ORCID |
| `Stats` | editorial, user and usage statistics |
| `Vocabs`, `Rors` | keyword suggestions and the Research Organization Registry |
| `TemporaryFiles` | uploads referenced by other requests |
| `SendAsync` | raw access to any endpoint |

## Documentation

* **Wiki** (in [`docs/wiki`](docs/wiki/Home.md)): one page per area, with examples in VB.NET, C# and F#.
* **XML documentation:** every public member has a summary and an example, shown in IntelliSense.
* **Sample app:** [`Samples/Library.Sample`](Samples/Library.Sample/Program.vb) is a small command-line tool:

  ```
  set OJS_BASE_URL=https://journals.example.com
  set OJS_JOURNAL=jpk
  set OJS_TOKEN=...
  dotnet run --project Samples/Library.Sample -- issues
  dotnet run --project Samples/Library.Sample -- submission 12
  dotnet run --project Samples/Library.Sample -- stats
  ```

## Building and testing

```
dotnet build OJS3_5.slnx
dotnet test OJS3_5.slnx
```

The unit tests need no server. The live tests run against a real journal when you set the `OJS_*` environment
variables, and the tests that change data also need `OJS_ALLOW_WRITES=1`. Run those against a test journal only. See
[Testing and Contributing](docs/wiki/Testing-and-Contributing.md).

`tools/doc-check` compiles every code example in the XML documentation and the wiki, in VB.NET, C# and F#.

## Compatibility

* **OJS:** 3.5.x. Tested against OJS 3.5.0.
* **.NET:** .NET 10 or later.

## License

[MIT](LICENSE.txt)
