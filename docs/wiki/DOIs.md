# DOIs

DOIs must be turned on in the journal under **Distribution > DOIs**. That needs:

* a prefix;
* the object types to give DOIs to;
* a registration agency plugin, such as Crossref or DataCite, if you want to deposit.

While DOIs are off, every call fails with `doi.authorization.enabledRequired`.

You can also turn DOIs on from code. The prefix has to be sent with the switch:

```vb
Await ojs.Contexts.UpdateAsync(1, New ContextEdit With {.EnableDois = True, .DoiPrefix = "10.1234"})
```

## Bulk actions on articles and issues

The bulk actions live under `ojs.Dois.Submissions` (they take submission IDs) and `ojs.Dois.Issues` (they take issue
IDs). The flow is: assign DOIs, export or deposit them, then check their status.

```vb
' 1. Give DOIs to the articles that have none, using the journal's suffix pattern
Dim result = Await ojs.Dois.Submissions.AssignAsync({12, 13, 14})
If Not result.Succeeded Then
    Console.WriteLine(String.Join(Environment.NewLine, result.FailedDoiActions))
End If

' 2a. Download the agency's deposit XML (e.g. Crossref) ...
Using xml = Await ojs.Dois.Submissions.ExportAndDownloadAsync({12, 13, 14})
    Await xml.SaveAsAsync(xml.FileName)
End Using

' 2b. ... or let OJS deposit them (needs the agency's credentials in the plugin settings)
Await ojs.Dois.Submissions.DepositAsync({12, 13, 14})

' 3. Check the status
Dim pub = (Await ojs.Submissions.GetAsync(12)).CurrentPublication
Console.WriteLine($"{pub.DoiObject?.Doi}: {pub.DoiObject?.Status}")
```

```csharp
var result = await ojs.Dois.Submissions.AssignAsync(new[] { 12, 13, 14 });
if (!result.Succeeded)
    Console.WriteLine(string.Join(Environment.NewLine, result.FailedDoiActions));

using (var xml = await ojs.Dois.Submissions.ExportAndDownloadAsync(new[] { 12, 13, 14 }))
    await xml.SaveAsAsync(xml.FileName);

await ojs.Dois.Submissions.DepositAsync(new[] { 12, 13, 14 });

var pub = (await ojs.Submissions.GetAsync(12)).CurrentPublication;
Console.WriteLine($"{pub.DoiObject?.Doi}: {pub.DoiObject?.Status}");
```

```fsharp
let! result = ojs.Dois.Submissions.AssignAsync [| 12; 13; 14 |]
if not result.Succeeded then
    for message in result.FailedDoiActions do printfn "%s" message

use! xml = ojs.Dois.Submissions.ExportAndDownloadAsync [| 12; 13; 14 |]
do! xml.SaveAsAsync xml.FileName

do! ojs.Dois.Submissions.DepositAsync [| 12; 13; 14 |]
```

Issues work the same way:

```vb
Await ojs.Dois.Issues.AssignAsync({4})
```

### Marking the status by hand

Use these when DOIs were registered outside OJS, or to fix a status:

```vb
Await ojs.Dois.Submissions.MarkRegisteredAsync({12})
Await ojs.Dois.Submissions.MarkStaleAsync({12})          ' metadata changed, needs a redeposit
Await ojs.Dois.Submissions.MarkUnregisteredAsync({12})
```

The status values are `DoiStatus.Unregistered`, `Submitted`, `Registered`, `Error` and `Stale`.

## Single DOIs

```vb
' Find DOIs whose deposit failed
Dim failed = Await ojs.Dois.ListAllAsync(New DoiQuery With {.Status = {DoiStatus.Error}})
For Each d In failed
    Console.WriteLine($"{d.Doi} ({d.RegistrationAgency})")
Next

' Change a DOI. The object it belongs to must be given.
Await ojs.Dois.UpdateAsync(9, New DoiEdit With {
    .Doi = "10.1234/joe.2026.12",
    .PubObjectType = DoiObjectType.Publication,
    .PubObjectId = 15})

' Remove a DOI from a galley
Await ojs.Dois.DeleteAsync(9, DoiObjectType.Representation, 22)

' Deposit everything that is not registered yet
Await ojs.Dois.DepositAllAsync()
```

```csharp
var failed = await ojs.Dois.ListAllAsync(new DoiQuery { Status = new[] { DoiStatus.Error } });

await ojs.Dois.UpdateAsync(9, new DoiEdit
{
    Doi = "10.1234/joe.2026.12",
    PubObjectType = DoiObjectType.Publication,
    PubObjectId = 15
});
```

The DOI of a publication is in `Publication.DoiObject`, and the DOI of an issue in `Issue.DoiObject`.
