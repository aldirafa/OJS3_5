# Journals and Site

## Journals (contexts)

OJS calls a journal a *context*. `ojs.Contexts` always uses the site-wide API, whichever journal the client targets.

```vb
For Each j In Await ojs.Contexts.ListAllAsync(New ContextQuery With {.IsEnabled = True})
    Console.WriteLine($"{j.Id} {j.UrlPath}: {j.Name.GetValue("en")} - {j.Url}")
Next

Dim journal = Await ojs.Contexts.GetAsync(1)   ' all ~180 settings
Console.WriteLine($"{journal.OnlineIssn} {journal.DoiPrefix} {journal.PrimaryLocale}")
```

```csharp
foreach (var j in await ojs.Contexts.ListAllAsync(new ContextQuery { IsEnabled = true }))
    Console.WriteLine($"{j.Id} {j.UrlPath}: {j.Name.GetValue("en")} - {j.Url}");

var journal = await ojs.Contexts.GetAsync(1);
Console.WriteLine($"{journal.OnlineIssn} {journal.DoiPrefix} {journal.PrimaryLocale}");
```

```fsharp
let! journals = ojs.Contexts.ListAllAsync(ContextQuery(IsEnabled = true))
for j in journals do
    printfn "%d %s: %s" j.Id.Value j.UrlPath (j.Name.GetValue "en")
```

### Changing settings

```vb
Await ojs.Contexts.UpdateAsync(1, New ContextEdit With {
    .Description = LocalizedString.Create("en", "<p>A journal about examples.</p>"),
    .OnlineIssn = "1234-5678",
    .DefaultReviewMode = ReviewMethod.DoubleAnonymous})
```

```csharp
await ojs.Contexts.UpdateAsync(1, new ContextEdit
{
    Description = LocalizedString.Create("en", "<p>A journal about examples.</p>"),
    OnlineIssn = "1234-5678",
    DefaultReviewMode = ReviewMethod.DoubleAnonymous
});
```

OJS only accepts changes to a journal through that journal's own API. `UpdateAsync` and `UpdateThemeAsync` look up
the journal's path once and send the change there for you.

> Some settings are validated together. For example, turning on DOIs needs the prefix in the same request:
> `New ContextEdit With {.EnableDois = True, .DoiPrefix = "10.1234"}`.

### Creating and deleting journals (site administrators)

```vb
Dim created = Await ojs.Contexts.CreateAsync(New ContextEdit With {
    .Name = LocalizedString.Create("en", "Journal of Examples"),
    .UrlPath = "joe",
    .PrimaryLocale = "en",
    .SupportedLocales = New List(Of String) From {"en"},
    .ContactName = "Editorial Office",
    .ContactEmail = "editor@example.com",
    .Country = "ID",
    .Enabled = True})

' Deletes the journal and all its content. There is no undo.
Await ojs.Contexts.DeleteAsync(created.Id.Value)
```

## Themes

The options of a theme differ from theme to theme, so `ThemeOptions` keeps them in a dictionary.
`ThemePluginPath` is required when saving.

```vb
Dim theme = Await ojs.Contexts.GetThemeAsync(1)
Console.WriteLine($"{theme.ThemePluginPath}: colour {theme.GetString("baseColour")}")

Await ojs.Contexts.UpdateThemeAsync(1, New ThemeOptions With {.ThemePluginPath = "default"}.
    SetOption("baseColour", "#1E6292").
    SetOption("typography", "notoSans_notoSerif"))
```

```csharp
var theme = await ojs.Contexts.GetThemeAsync(1);
Console.WriteLine($"{theme.ThemePluginPath}: colour {theme.GetString("baseColour")}");

await ojs.Contexts.UpdateThemeAsync(1, new ThemeOptions { ThemePluginPath = "default" }
    .SetOption("baseColour", "#1E6292")
    .SetOption("typography", "notoSans_notoSerif"));
```

The site theme works the same way, through `ojs.Site.GetThemeAsync()` and `ojs.Site.UpdateThemeAsync(theme)`.

## Site settings

```vb
Dim site = Await ojs.Site.GetAsync()
Console.WriteLine($"{site.Title.GetValue(site.PrimaryLocale)}: {String.Join(", ", site.SupportedLocales)}")

Await ojs.Site.UpdateAsync(New SiteEdit With {.MinPasswordLength = 12})
```

## Announcements

```vb
Dim cfp = Await ojs.Announcements.CreateAsync(New AnnouncementEdit With {
    .Title = LocalizedString.Create("en", "Call for papers: special issue"),
    .DescriptionShort = LocalizedString.Create("en", "Deadline 30 June."),
    .Description = LocalizedString.Create("en", "<p>Full call ...</p>"),
    .DateExpire = New DateOnly(2027, 6, 30)})

Dim recent = Await ojs.Announcements.ListAsync(New AnnouncementQuery With {.Count = 5})
For Each a In recent.Items
    Console.WriteLine($"{a.DatePosted:d} {a.Title.GetValue("en")}")
Next
```

```csharp
var cfp = await ojs.Announcements.CreateAsync(new AnnouncementEdit
{
    Title = LocalizedString.Create("en", "Call for papers: special issue"),
    DescriptionShort = LocalizedString.Create("en", "Deadline 30 June."),
    DateExpire = new DateOnly(2027, 6, 30)
});
```

```fsharp
let! cfp =
    ojs.Announcements.CreateAsync(
        AnnouncementEdit(
            Title = LocalizedString.Create("en", "Call for papers: special issue"),
            DateExpire = DateOnly(2027, 6, 30)))
printfn "%s" cfp.Url
```

A client for the site (`ojs.ForSite()`) reads and writes site-wide announcements.

## Institutions

Institutions are used for institutional usage statistics and subscriptions.

```vb
Await ojs.Institutions.CreateAsync(New InstitutionEdit With {
    .Name = LocalizedString.Create("en", "Example University"),
    .Ror = "https://ror.org/05ezgz346",
    .IpRanges = New List(Of String) From {"192.0.2.0/24", "198.51.100.1-198.51.100.50"}})
```

```csharp
await ojs.Institutions.CreateAsync(new InstitutionEdit
{
    Name = LocalizedString.Create("en", "Example University"),
    Ror = "https://ror.org/05ezgz346",
    IpRanges = new List<string> { "192.0.2.0/24", "198.51.100.1-198.51.100.50" }
});
```
