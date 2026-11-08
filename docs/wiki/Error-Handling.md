# Error Handling

A failed request throws an exception. Which one depends on what went wrong:

| Exception | When |
|---|---|
| `OjsValidationException` | OJS rejected the values you sent: a `400` with per-field messages, or a `422` (invitations). |
| `OjsApiException` | Any other non-success status, such as `401`, `403`, `404` or `500`. |
| `HttpRequestException` / `TaskCanceledException` | The network failed, or the request timed out. |

`OjsApiException` has these properties:

| Property | Content |
|---|---|
| `StatusCode` | The HTTP status code. |
| `Error` | The OJS error key, e.g. `api.404.resourceNotFound`. |
| `ErrorMessage` | The translated message. |
| `ResponseBody` | The raw body. |
| `RequestMethod`, `RequestUri` | Which request failed. |

`OjsValidationException` inherits from it and adds `Errors`, a map from field name to messages. Multilingual
fields are flattened, e.g. `title.en`.

```vb
Try
    Await ojs.Submissions.SubmitAsync(12, confirmCopyright:=True)
Catch ex As OjsValidationException
    For Each field In ex.Errors
        Console.WriteLine($"{field.Key}: {String.Join(" ", field.Value)}")
    Next
Catch ex As OjsApiException When ex.StatusCode = Net.HttpStatusCode.NotFound
    Console.WriteLine("No such submission.")
Catch ex As OjsApiException
    Console.WriteLine($"{CInt(ex.StatusCode)} {ex.Error}: {ex.ErrorMessage}")
End Try
```

```csharp
try
{
    await ojs.Submissions.SubmitAsync(12, confirmCopyright: true);
}
catch (OjsValidationException ex)
{
    foreach (var field in ex.Errors)
        Console.WriteLine($"{field.Key}: {string.Join(" ", field.Value)}");
}
catch (OjsApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
{
    Console.WriteLine("No such submission.");
}
catch (OjsApiException ex)
{
    Console.WriteLine($"{(int)ex.StatusCode} {ex.Error}: {ex.ErrorMessage}");
}
```

```fsharp
try
    let! _ = ojs.Submissions.SubmitAsync(12, confirmCopyright = true)
    ()
with
| :? OjsValidationException as ex ->
    for field in ex.Errors do
        printfn "%s: %s" field.Key (String.Join(" ", field.Value))
| :? OjsApiException as ex when ex.StatusCode = Net.HttpStatusCode.NotFound ->
    printfn "No such submission."
| :? OjsApiException as ex ->
    printfn "%d %s: %s" (int ex.StatusCode) ex.Error ex.ErrorMessage
```

## Status codes you will meet

| Status | Typical `Error` | Meaning |
|---|---|---|
| 401 | `user.authorization.roleBasedAccessDenied` | The token is valid, but the user's role may not do this. OJS uses 401 here, not 403. |
| 401 | `user.authorization.noContext`, `user.authorization.contextRequired` | The endpoint needs a journal, but the client points at the site (`_`), or the journal path is wrong. |
| 403 | `api.submissions.403.*` and similar | The user may use the endpoint, but not on this object. |
| 404 | `api.404.resourceNotFound` | The object does not exist, or the user cannot see it. |
| 400 | `doi.authorization.enabledRequired` | DOIs are switched off in the journal. |
| 400 / 422 | field messages | Validation failed; you get an `OjsValidationException`. |

## Validate before you submit

`SubmitAsync` can run the checks without submitting:

```vb
Try
    Await ojs.Submissions.SubmitAsync(12, confirmCopyright:=True, validateOnly:=True)
    Console.WriteLine("Ready to submit.")
Catch ex As OjsValidationException
    Console.WriteLine(String.Join(Environment.NewLine, ex.Errors.Keys))
End Try
```

## Cancellation and timeouts

```vb
Using cts As New Threading.CancellationTokenSource(TimeSpan.FromSeconds(10))
    Dim issue = Await ojs.Issues.GetCurrentAsync(cts.Token)
End Using
```

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
var issue = await ojs.Issues.GetCurrentAsync(cts.Token);
```
