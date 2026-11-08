# Emails

## Mailables and templates

A **mailable** is a kind of email OJS sends, for example the submission acknowledgement. Each mailable has a default
**template**, whose key matches the mailable (e.g. `SUBMISSION_ACK`). Mailables with `SupportsTemplates` can also
have custom alternate templates.

```vb
For Each m In Await ojs.Mailables.ListAsync()
    Console.WriteLine($"{m.EmailTemplateKey}: {m.Name} [{String.Join(", ", m.GroupIds)}]")
Next

' The variables a template may use
Dim ack = Await ojs.Mailables.GetAsync("SUBMISSION_ACK")
For Each v In ack.DataDescriptions
    Console.WriteLine($"{{${v.Key}}} - {v.Value}")
Next
```

```csharp
foreach (var m in await ojs.Mailables.ListAsync())
    Console.WriteLine($"{m.EmailTemplateKey}: {m.Name} [{string.Join(", ", m.GroupIds)}]");

var ack = await ojs.Mailables.GetAsync("SUBMISSION_ACK");
foreach (var v in ack.DataDescriptions)
    Console.WriteLine($"{{${v.Key}}} - {v.Value}");
```

```fsharp
let! mailables = ojs.Mailables.ListAsync()
for m in mailables do
    printfn "%s: %s" m.EmailTemplateKey m.Name
```

### Editing templates

```vb
' Change the default text
Await ojs.EmailTemplates.UpdateAsync("SUBMISSION_ACK", New EmailTemplateEdit With {
    .Subject = LocalizedString.Create("en", "We received: {$submissionTitle}")})

' Add a custom alternate to a mailable that supports templates
Await ojs.EmailTemplates.CreateAsync(New EmailTemplateEdit With {
    .Key = "WARM_THANKS",
    .AlternateTo = "DISCUSSION_NOTIFICATION_SUBMISSION",
    .Name = LocalizedString.Create("en", "Warm thanks"),
    .Subject = LocalizedString.Create("en", "Thank you, {$recipientName}"),
    .Body = LocalizedString.Create("en", "<p>Thank you for your message.</p>")})

' Which templates have been changed from the defaults?
Dim changed = Await ojs.EmailTemplates.ListAllAsync(New EmailTemplateQuery With {.IsModified = True})
```

```csharp
await ojs.EmailTemplates.UpdateAsync("SUBMISSION_ACK", new EmailTemplateEdit
{
    Subject = LocalizedString.Create("en", "We received: {$submissionTitle}")
});

var changed = await ojs.EmailTemplates.ListAllAsync(new EmailTemplateQuery { IsModified = true });
```

`DeleteAsync(key)` removes a custom template. For a default template it resets the text to the original instead.
`RestoreDefaultsAsync()` resets every template and deletes all custom ones. Use it with care.

## The email log

```vb
For Each e In Await ojs.Emails.ListAuthorEmailsAsync(12)
    Console.WriteLine($"{e.DateSent:g} to {e.Recipients}: {e.Subject}")
Next
```

```csharp
foreach (var e in await ojs.Emails.ListAuthorEmailsAsync(12))
    Console.WriteLine($"{e.DateSent:g} to {e.Recipients}: {e.Subject}");
```

> For test installations, set `[email] default = log` in `config.inc.php`. OJS then writes emails to its log instead
> of sending them, which is useful while you try out invitations and decisions.
