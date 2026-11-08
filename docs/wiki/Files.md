# Files

## Temporary files

Several endpoints take a file by reference rather than directly:

* cover images;
* journal logos and favicons;
* announcement images;
* stylesheets.

For these you upload the file first with `TemporaryFiles.UploadAsync`, then pass the returned ID.

```vb
Dim tmp = Await ojs.TemporaryFiles.UploadAsync("C:\images\cover.jpg")

Await ojs.Publications.UpdateAsync(12, 15, New PublicationEdit With {
    .CoverImage = New Dictionary(Of String, CoverImage) From {
        {"en", New CoverImage With {.TemporaryFileId = tmp.Id, .AltText = "Cover of issue 3"}}}})
```

```csharp
var tmp = await ojs.TemporaryFiles.UploadAsync(@"C:\images\cover.jpg");

await ojs.Publications.UpdateAsync(12, 15, new PublicationEdit
{
    CoverImage = new Dictionary<string, CoverImage>
    {
        ["en"] = new CoverImage { TemporaryFileId = tmp.Id, AltText = "Cover of issue 3" }
    }
});
```

```fsharp
let! tmp = ojs.TemporaryFiles.UploadAsync @"C:\images\cover.jpg"
let cover = Collections.Generic.Dictionary<string, CoverImage>()
cover["en"] <- CoverImage(TemporaryFileId = tmp.Id, AltText = "Cover of issue 3")
let! _ = ojs.Publications.UpdateAsync(12, 15, PublicationEdit(CoverImage = cover))
```

Journal and site images (logo, favicon, thumbnail, homepage image) use `ImageFile` the same way:

```vb
Dim logo = Await ojs.TemporaryFiles.UploadAsync("logo.png")
Await ojs.Contexts.UpdateAsync(1, New ContextEdit With {
    .PageHeaderLogoImage = New Dictionary(Of String, ImageFile) From {
        {"en", New ImageFile With {.TemporaryFileId = logo.Id, .AltText = "Journal logo"}}}})
```

You can also upload from a `Stream`, which is useful for in-memory data and web uploads:

```csharp
await using var stream = File.OpenRead("figure.png");
var tmp = await ojs.TemporaryFiles.UploadAsync(stream, "figure.png", "image/png");
```

## Submission files

Submission files belong to a **file stage**, such as `Submission`, `ReviewFile`, `Copyedit` or `ProductionReady`.
Reading or changing a file also needs the **workflow stage** you access it in, because OJS checks the user's access
per stage.

```vb
' Upload a manuscript to the submission stage
Using pdf = File.OpenRead("manuscript.pdf")
    Dim uploaded = Await ojs.SubmissionFiles.UploadAsync(12, pdf, "manuscript.pdf", New SubmissionFileEdit With {
        .FileStage = SubmissionFileStage.Submission,
        .GenreId = 1})   ' "Article Text" in a default installation
    Console.WriteLine(uploaded.Id)
End Using

' List the production-ready files
Dim files = Await ojs.SubmissionFiles.ListAsync(12, {SubmissionFileStage.ProductionReady})
For Each f In files
    Console.WriteLine($"{f.Id} {f.Name.GetValue("en")} {f.Mimetype}")
Next

' Promote a copyedited file to production
Await ojs.SubmissionFiles.CopyAsync(12, 40, WorkflowStage.Editing, SubmissionFileStage.ProductionReady)
```

```csharp
await using (var pdf = File.OpenRead("manuscript.pdf"))
{
    var uploaded = await ojs.SubmissionFiles.UploadAsync(12, pdf, "manuscript.pdf", new SubmissionFileEdit
    {
        FileStage = SubmissionFileStage.Submission,
        GenreId = 1
    });
    Console.WriteLine(uploaded.Id);
}

var files = await ojs.SubmissionFiles.ListAsync(12, new[] { SubmissionFileStage.ProductionReady });
foreach (var f in files)
    Console.WriteLine($"{f.Id} {f.Name.GetValue("en")} {f.Mimetype}");

await ojs.SubmissionFiles.CopyAsync(12, 40, WorkflowStage.Editing, SubmissionFileStage.ProductionReady);
```

```fsharp
use pdf = File.OpenRead "manuscript.pdf"
let! uploaded =
    ojs.SubmissionFiles.UploadAsync(12, pdf, "manuscript.pdf",
        SubmissionFileEdit(FileStage = SubmissionFileStage.Submission, GenreId = 1))
printfn "%d" uploaded.Id.Value

let! files = ojs.SubmissionFiles.ListAsync(12, [| SubmissionFileStage.ProductionReady |])
for f in files do
    printfn "%d %s %s" f.Id.Value (f.Name.GetValue "en") f.Mimetype
```

## JATS XML

```vb
Dim jats = Await ojs.Publications.GetJatsAsync(12, 15)
File.WriteAllText("article.xml", jats.JatsContent)

Using xml = File.OpenRead("article-edited.xml")
    Await ojs.Publications.UploadJatsAsync(12, 15, xml, "article.xml")
End Using
```

## Downloads

Downloads return a `DownloadedFile`: DOI exports, review PDF and XML exports, and the files of `SendAsync` calls. It
gives you:

* `Content`, a stream;
* `FileName` and `ContentType`, taken from the response headers;
* `ReadAllBytesAsync` and `SaveAsAsync`.

**Always dispose it.** It holds the HTTP connection open.

```vb
Using pdf = Await ojs.Reviews.DownloadPdfAsync(12, 21, authorFriendly:=True)
    Await pdf.SaveAsAsync(Path.Combine("exports", If(pdf.FileName, "review.pdf")))
End Using
```

```csharp
using var pdf = await ojs.Reviews.DownloadPdfAsync(12, 21, authorFriendly: true);
await pdf.SaveAsAsync(Path.Combine("exports", pdf.FileName ?? "review.pdf"));
```

```fsharp
use! pdf = ojs.Reviews.DownloadPdfAsync(12, 21, authorFriendly = true)
do! pdf.SaveAsAsync(Path.Combine("exports", (if isNull pdf.FileName then "review.pdf" else pdf.FileName)))
```
