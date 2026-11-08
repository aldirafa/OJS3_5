# Getting Started

## Install

```
dotnet add package Ojs.Api.Client --prerelease
```

The package (`Ojs.Api.Client`) contains `OJS3_5.Library.dll`, whose types live in the `OJS3_5.Library` namespace. It targets **.NET 10** and has no dependencies outside the .NET base library.

## Get an API token

1. **Set the secret on the server.** The OJS installation needs a secret for signing tokens. In `config.inc.php`, set:

   ```
   [security]
   api_key_secret = "a long random string"
   ```

2. **Generate the token.** Log in to OJS, open **User Profile > API Key**, tick **Enable external applications to use the API key to access this account**, and click **Generate API Key**.
3. **Keep it out of your code.** Store the token somewhere safe, such as an environment variable or a secrets manager. The token acts with the permissions of that user, so use an account with the smallest role that can do the job.

The library sends the token as `Authorization: Bearer <token>` on every request.

## Create a client

```vb
Dim ojs As New OjsClient(New OjsClientOptions With {
    .BaseUrl = "https://journals.example.com",
    .JournalPath = "jpk",
    .ApiToken = Environment.GetEnvironmentVariable("OJS_TOKEN")})
```

```csharp
var ojs = new OjsClient(new OjsClientOptions
{
    BaseUrl = "https://journals.example.com",
    JournalPath = "jpk",
    ApiToken = Environment.GetEnvironmentVariable("OJS_TOKEN")
});
```

```fsharp
let ojs =
    new OjsClient(OjsClientOptions(
        BaseUrl = "https://journals.example.com",
        JournalPath = "jpk",
        ApiToken = Environment.GetEnvironmentVariable "OJS_TOKEN"))
```

`OjsClient` is thread-safe. Create one per journal and keep it for the lifetime of your application. Dispose it when
you are done; that also disposes the `HttpClient` it created.

### Options

| Option | Meaning |
|---|---|
| `BaseUrl` | The root URL of the OJS installation, e.g. `https://journals.example.com` or `https://example.com/ojs`. |
| `JournalPath` | The journal's URL path (the `jpk` in `https://journals.example.com/jpk`). Use `OjsClientOptions.SiteWidePath` (`_`) for site-wide endpoints. |
| `UseIndexPhp` | Set to `True` when the installation's URLs contain `index.php`, i.e. it runs without URL rewriting: `https://example.com/index.php/jpk`. |
| `ApiToken` | The API token from the user's profile. |
| `Timeout` | The request timeout (used only when the client creates its own `HttpClient`). |
| `UserAgent` | The `User-Agent` header (default `Ojs.Api.Client/<version>`). |

Use `ojs.ApiRoot` to check which URL requests go to. For example, `https://example.com/index.php/jpk/api/v1/`.

## Several journals on one installation

`ForJournal` returns a client for another journal that shares the same connection. Journal and site settings use
the site-wide API, which `ForSite` gives you. `Contexts` and `Site` already go to the site-wide API on their own.

```vb
For Each journal In Await ojs.Contexts.ListAllAsync()
    Dim client = ojs.ForJournal(journal.UrlPath)
    Dim issue = Await client.Issues.GetCurrentAsync()
    Console.WriteLine($"{journal.Name.GetValue("en")}: {issue.Identification}")
Next
```

```csharp
foreach (var journal in await ojs.Contexts.ListAllAsync())
{
    var client = ojs.ForJournal(journal.UrlPath);
    var issue = await client.Issues.GetCurrentAsync();
    Console.WriteLine($"{journal.Name.GetValue("en")}: {issue.Identification}");
}
```

```fsharp
let! journals = ojs.Contexts.ListAllAsync()
for journal in journals do
    let client = ojs.ForJournal journal.UrlPath
    let! issue = client.Issues.GetCurrentAsync()
    printfn "%s: %s" (journal.Name.GetValue "en") issue.Identification
```

## Using `IHttpClientFactory` / dependency injection

Pass your own `HttpClient` to the constructor. The client then does not dispose it.

```csharp nocheck
// Program.cs (ASP.NET Core)
builder.Services.AddHttpClient("ojs", c => c.Timeout = TimeSpan.FromSeconds(60));
builder.Services.AddSingleton(sp =>
{
    var http = sp.GetRequiredService<IHttpClientFactory>().CreateClient("ojs");
    return new OjsClient(http, new OjsClientOptions
    {
        BaseUrl = builder.Configuration["Ojs:BaseUrl"],
        JournalPath = builder.Configuration["Ojs:Journal"],
        ApiToken = builder.Configuration["Ojs:Token"]
    });
});
```

```vb
Dim http As New Net.Http.HttpClient() With {.Timeout = TimeSpan.FromMinutes(2)}
Dim ojs As New OjsClient(http, New OjsClientOptions With {
    .BaseUrl = "https://journals.example.com", .JournalPath = "jpk", .ApiToken = "..."})
```

## PowerShell

The library targets .NET 10, so it needs a PowerShell that runs on .NET 10 (PowerShell 7.6 or later).
Load the DLL from the package folder:

```powershell
Add-Type -Path ./OJS3_5.Library.dll
$ojs = [OJS3_5.Library.OjsClient]::new([OJS3_5.Library.OjsClientOptions]@{
    BaseUrl = 'https://journals.example.com'; JournalPath = 'jpk'; ApiToken = $env:OJS_TOKEN })
$issue = $ojs.Issues.GetCurrentAsync().GetAwaiter().GetResult()
$issue.Identification
```

## Next steps

* [Core Concepts](Core-Concepts): how models, multilingual values and partial updates work.
* [Submissions](Submissions): working with the editorial workflow.
