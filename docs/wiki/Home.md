# OJS 3.5 .NET Client

A .NET library for the [Open Journal Systems](https://pkp.sfu.ca/software/ojs/) **3.5** REST API. It is written in
VB.NET and works from any .NET language: VB.NET, C#, F#, PowerShell and others.

It covers every public endpoint of the OJS 3.5 API:

* the editorial workflow: submissions, publications, contributors, files, decisions, reviews;
* journal content and configuration: issues, sections, users, journals, site settings, themes, announcements, email templates;
* integrations: DOIs, ORCID, statistics.

It also smooths over the quirks of the PHP backend, for example dates such as `"2026-01-31 10:00:00"`, numbers sent as
strings, and `[]` sent for an empty object.

```vb
Imports Library
Imports Library.Models
Imports Library.Queries

Using ojs As New OjsClient(New OjsClientOptions With {
    .BaseUrl = "https://journals.example.com",
    .JournalPath = "jpk",
    .ApiToken = Environment.GetEnvironmentVariable("OJS_TOKEN")})

    Dim page = Await ojs.Submissions.ListAsync(New SubmissionQuery With {.Status = {SubmissionStatus.Published}})
    For Each s In page.Items
        Console.WriteLine(s.CurrentPublication?.Title.GetValue("en"))
    Next
End Using
```

```csharp
using Library;
using Library.Models;
using Library.Queries;

using var ojs = new OjsClient(new OjsClientOptions
{
    BaseUrl = "https://journals.example.com",
    JournalPath = "jpk",
    ApiToken = Environment.GetEnvironmentVariable("OJS_TOKEN")
});

var page = await ojs.Submissions.ListAsync(new SubmissionQuery { Status = new[] { SubmissionStatus.Published } });
foreach (var s in page.Items)
    Console.WriteLine(s.CurrentPublication?.Title.GetValue("en"));
```

```fsharp
use ojs =
    new OjsClient(OjsClientOptions(
        BaseUrl = "https://journals.example.com",
        JournalPath = "jpk",
        ApiToken = Environment.GetEnvironmentVariable "OJS_TOKEN"))

let! page = ojs.Submissions.ListAsync(SubmissionQuery(Status = [| SubmissionStatus.Published |]))
for s in page.Items do
    match s.CurrentPublication with
    | null -> ()
    | p -> printfn "%s" (p.Title.GetValue "en")
```

## Pages

| Topic | What it covers |
|---|---|
| [Getting Started](Getting-Started) | Installing, API tokens, client options, multiple journals, dependency injection |
| [Core Concepts](Core-Concepts) | Read and edit models, multilingual values, enums, dates, unknown properties |
| [Pagination](Pagination) | `ListAsync`, `ListAllAsync`, `Enumerate` |
| [Error Handling](Error-Handling) | `OjsApiException`, validation errors, status codes |
| [Files](Files) | Temporary files, submission files, JATS, downloads |
| [Submissions](Submissions) | Submissions, publications, contributors, decisions, participants, reviewer suggestions |
| [Issues and Sections](Issues-and-Sections) | Issues, the current issue, sections |
| [Users and Invitations](Users-and-Invitations) | Users, reviewers, roles, invitations |
| [Journals and Site](Journals-and-Site) | Journals (contexts), site settings, themes, announcements, institutions |
| [Emails](Emails) | Email templates, mailables, the email log |
| [DOIs](DOIs) | Assigning, exporting, depositing and marking DOIs |
| [Reviews and ORCID](Reviews-and-ORCID) | Review exports, history, confirming on behalf of a reviewer, ORCID |
| [Statistics](Statistics) | Editorial, user and usage statistics, JSON or CSV |
| [Vocabularies and RORs](Vocabularies-and-RORs) | Keyword suggestions, Research Organization Registry |
| [Advanced](Advanced) | Raw requests, plugin parameters, extending models, custom `HttpClient` |
| [OJS 3.5 Quirks](OJS-3.5-Quirks) | Places where the server differs from its OpenAPI document, and how the library handles them |
| [Testing and Contributing](Testing-and-Contributing) | Running the unit and live tests |

## Conventions used in these pages

* Examples are shown in VB.NET, C# and F#.
* `ojs` is an `OjsClient` created as in [Getting Started](Getting-Started).
* The VB and C# examples run inside an `Async` method. The F# examples run inside a `task { }` block.
* C# examples assume the SDK's implicit usings, which provide `System`, `System.IO`, `System.Linq`, `System.Net.Http` and others.
* Numbers such as `12` (a submission ID) or `15` (a publication ID) stand in for IDs from your own journal.
* Add these namespace imports to your files:
  * `Library` (`OjsClient`, `OjsClientOptions`)
  * `Library.Models`
  * `Library.Queries`
  * `Library.Serialization` (`LocalizedString`, `LocalizedVocab`)
  * `Library.Errors`
  * `Library.Resources` (`VocabNames`)
