# Advanced

## Raw requests

`SendAsync` can call endpoints the library does not cover, for example plugin APIs or features of a newer OJS. The
path is relative to the API root. With no type argument the response comes back as a `JsonElement`; with one, it is
bound to the type you give, PHP quirks and all.

```vb
Dim json = Await ojs.SendAsync(Net.Http.HttpMethod.Get, "submissions?count=1")
Console.WriteLine(json.GetProperty("itemsMax").GetInt32())

' Bind to a model
Dim s = Await ojs.SendAsync(Of Submission)(Net.Http.HttpMethod.Get, "submissions/12")

' With a body: a model, a JSON string, a JsonNode or a JsonElement
Dim result = Await ojs.SendAsync(Net.Http.HttpMethod.Put, "myPlugin/settings", "{""enabled"": true}")
```

```csharp
var json = await ojs.SendAsync(HttpMethod.Get, "submissions?count=1");
Console.WriteLine(json.GetProperty("itemsMax").GetInt32());

var s = await ojs.SendAsync<Submission>(HttpMethod.Get, "submissions/12");
var result = await ojs.SendAsync(HttpMethod.Put, "myPlugin/settings", "{\"enabled\": true}");
```

```fsharp
let! json = ojs.SendAsync(Net.Http.HttpMethod.Get, "submissions?count=1")
printfn "%d" (json.GetProperty("itemsMax").GetInt32())

let! s = ojs.SendAsync<Submission>(Net.Http.HttpMethod.Get, "submissions/12")
```

Errors raise the same `OjsApiException` as everywhere else.

## Extra query parameters

Every list query has `ExtraParameters` for parameters that plugins add or that the library does not model yet:

```vb
Dim page = Await ojs.Submissions.ListAsync(New SubmissionQuery With {
    .ExtraParameters = New Dictionary(Of String, String) From {{"myPluginFilter", "yes"}}})
```

```csharp
var page = await ojs.Submissions.ListAsync(new SubmissionQuery
{
    ExtraParameters = new Dictionary<string, string> { ["myPluginFilter"] = "yes" }
});
```

## Plugin properties

Plugin data such as `crossrefplugin::status`, and fields from newer OJS versions, end up in `AdditionalProperties`.
Read them as `JsonElement`:

```vb
Dim pub = Await ojs.Publications.GetAsync(12, 15)
For Each kv In pub.AdditionalProperties
    Console.WriteLine($"{kv.Key} = {kv.Value}")
Next
```

## Adding your own helpers

Extension methods add helpers to models and resources:

```vb nocheck
Imports System.Runtime.CompilerServices
Imports Library.Models

Public Module SubmissionExtensions
    <Extension>
    Public Function EnglishTitle(s As Submission) As String
        Return s.CurrentPublication?.Title?.GetValue("en", fallbackLocale:=s.Locale)
    End Function
End Module
```

```csharp nocheck
public static class SubmissionExtensions
{
    public static string EnglishTitle(this Submission s) =>
        s.CurrentPublication?.Title?.GetValue("en", fallbackLocale: s.Locale);
}
```

```fsharp nocheck
type Submission with
    member s.EnglishTitle = s.CurrentPublication.Title.GetValue("en", s.Locale)
```

## Your own `HttpClient`: logging, retries, proxies

Pass an `HttpClient` you configured yourself. For example, use a `DelegatingHandler` to log every call:

```csharp nocheck
class LoggingHandler : DelegatingHandler
{
    public LoggingHandler() : base(new HttpClientHandler()) { }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var started = DateTime.UtcNow;
        var response = await base.SendAsync(request, ct);
        Console.WriteLine($"{request.Method} {request.RequestUri} -> {(int)response.StatusCode} in {(DateTime.UtcNow - started).TotalMilliseconds:F0} ms");
        return response;
    }
}

var ojs = new OjsClient(new HttpClient(new LoggingHandler()), new OjsClientOptions { /* ... */ });
```

The library has no built-in retry policy. Add one with `Microsoft.Extensions.Http.Resilience` (Polly) on a named
`HttpClient`, as in [Getting Started](Getting-Started#using-ihttpclientfactory--dependency-injection). Shared hosting
in particular may now and then answer 500 under parallel load.

## Using the JSON handling directly

`OjsJson` is the serializer the library uses. It understands PHP-style JSON, which helps when you read JSON you saved
earlier or JSON from OJS webhooks and exports:

```vb
Dim s = OjsJson.Deserialize(Of Submission)(File.ReadAllText("submission.json"))
Dim json = OjsJson.Serialize(New PublicationEdit With {.Title = LocalizedString.Create("en", "x")})
' {"title":{"en":"x"}}
```

```csharp
var s = OjsJson.Deserialize<Submission>(File.ReadAllText("submission.json"));
var json = OjsJson.Serialize(new PublicationEdit { Title = LocalizedString.Create("en", "x") });
```

## Thread safety and lifetime

* **Sharing:** `OjsClient` and its resources are thread-safe. Keep one instance per journal.
* **Other journals and the site:** `ForJournal` and `ForSite` return lightweight clients that share the HTTP connection.
* **Disposing:** dispose the original client last.
