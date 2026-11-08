# Vocabularies and RORs

## Controlled vocabularies

OJS suggests keywords, subjects, disciplines, supporting agencies and languages that other submissions in the
journal already use. `VocabNames` has the vocabulary names.

```vb
Dim keywords = Await ojs.Vocabs.ListAsync(VocabNames.Keywords, "en")
Console.WriteLine(String.Join(", ", keywords.Select(Function(k) k.Name)))
```

```csharp
var keywords = await ojs.Vocabs.ListAsync(VocabNames.Keywords, "en");
Console.WriteLine(string.Join(", ", keywords.Select(k => k.Name)));
```

```fsharp
let! keywords = ojs.Vocabs.ListAsync(VocabNames.Keywords, "en")
printfn "%s" (keywords |> Seq.map (fun k -> k.Name) |> String.concat ", ")
```

The available names are `VocabNames.Keywords`, `Subjects`, `Disciplines`, `Agencies` and `Languages`.

## Research Organization Registry (ROR)

OJS 3.5 keeps a local copy of the ROR registry. It uses it for contributor affiliations and institutions.

```vb
Dim found = Await ojs.Rors.ListAsync(New SearchQuery With {.SearchPhrase = "Bandung", .Count = 10})
For Each r In found.Items
    Console.WriteLine($"{r.Ror}  {r.Name.GetValue(r.DisplayLocale)}")
Next
```

```csharp
var found = await ojs.Rors.ListAsync(new SearchQuery { SearchPhrase = "Bandung", Count = 10 });
foreach (var r in found.Items)
    Console.WriteLine($"{r.Ror}  {r.Name.GetValue(r.DisplayLocale)}");
```

```fsharp
let! found = ojs.Rors.ListAsync(SearchQuery(SearchPhrase = "Bandung", Count = 10))
for r in found.Items do
    printfn "%s  %s" r.Ror (r.Name.GetValue r.DisplayLocale)
```

Link an affiliation to a ROR record:

```vb
Dim ror = (Await ojs.Rors.ListAsync(New SearchQuery With {.SearchPhrase = "Institut Teknologi Bandung"})).Items.First()
Await ojs.Contributors.UpdateAsync(12, 15, 31, New ContributorEdit With {
    .Affiliations = New List(Of Affiliation) From {
        New Affiliation With {.Ror = ror.Ror, .Name = ror.Name}}})
```

The registry has well over 100,000 entries, so always search with a `SearchPhrase`. `ListAllAsync` without a filter
would download all of them.

To add a record, or update the one with the same ROR ID:

```vb
Await ojs.Rors.SaveAsync(New RorEdit With {
    .Ror = "https://ror.org/05ezgz346", .DisplayLocale = "en", .IsActive = True,
    .Name = LocalizedString.Create("en", "Example University")})
```
