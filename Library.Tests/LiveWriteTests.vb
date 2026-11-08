Imports OJS3_5.Library.Errors
Imports OJS3_5.Library.Models
Imports OJS3_5.Library.Queries
Imports OJS3_5.Library.Serialization

''' <summary>
''' Write tests against a DUMMY OJS 3.5 journal. Each test creates its own data and removes it again.
''' They only run when OJS_ALLOW_WRITES=1 is set in addition to OJS_BASE_URL, OJS_JOURNAL and OJS_TOKEN.
''' Never point them at a production journal.
''' </summary>
<TestClass>
<DoNotParallelize>
<TestCategory("LiveWrite")>
Public Class LiveWriteTests

    Public Property TestContext As TestContext

    Private Shared Function CreateWriteClient() As OjsClient
        Dim baseUrl = Environment.GetEnvironmentVariable("OJS_BASE_URL")
        Dim token = Environment.GetEnvironmentVariable("OJS_TOKEN")
        Dim journal = Environment.GetEnvironmentVariable("OJS_JOURNAL")
        If Environment.GetEnvironmentVariable("OJS_ALLOW_WRITES") <> "1" OrElse String.IsNullOrEmpty(baseUrl) OrElse
           String.IsNullOrEmpty(token) OrElse String.IsNullOrEmpty(journal) Then
            Assert.Inconclusive("Write tests skipped: set OJS_ALLOW_WRITES=1, OJS_BASE_URL, OJS_JOURNAL and OJS_TOKEN (dummy journal only).")
        End If
        Return New OjsClient(New OjsClientOptions With {
            .BaseUrl = baseUrl, .JournalPath = journal, .ApiToken = token,
            .UseIndexPhp = Environment.GetEnvironmentVariable("OJS_USE_INDEX_PHP") = "1",
            .Timeout = TimeSpan.FromSeconds(60)})
    End Function

    Private Shared ReadOnly Stamp As String = DateTime.UtcNow.ToString("yyyyMMddHHmmss")

    <TestMethod>
    Public Async Function Announcements_CreateUpdateDelete() As Task
        Using ojs = CreateWriteClient()
            Dim created = Await ojs.Announcements.CreateAsync(New AnnouncementEdit With {
                .Title = LocalizedString.Create("en", $"Library test {Stamp}"),
                .DescriptionShort = LocalizedString.Create("en", "Created by the Library test suite."),
                .DateExpire = DateOnly.FromDateTime(DateTime.Today.AddDays(30))})
            Try
                Assert.IsNotNull(created.Id)
                Assert.AreEqual($"Library test {Stamp}", created.Title.GetValue("en"))
                Assert.IsNotNull(created.DatePosted)

                Dim updated = Await ojs.Announcements.UpdateAsync(created.Id.Value, New AnnouncementEdit With {
                    .Title = LocalizedString.Create("en", $"Library test {Stamp} (edited)")})
                StringAssert.EndsWith(updated.Title.GetValue("en"), "(edited)")
                Assert.AreEqual(created.DateExpire, updated.DateExpire, "Partial update must keep other fields.")

                Dim fetched = Await ojs.Announcements.GetAsync(created.Id.Value)
                Assert.AreEqual(updated.Title.GetValue("en"), fetched.Title.GetValue("en"))
            Finally
                ojs.Announcements.DeleteAsync(created.Id.Value).GetAwaiter().GetResult()
            End Try
            Dim ex = Await Assert.ThrowsAsync(Of OjsApiException)(Function() ojs.Announcements.GetAsync(created.Id.Value))
            Assert.AreEqual(Net.HttpStatusCode.NotFound, ex.StatusCode)
        End Using
    End Function

    <TestMethod>
    Public Async Function Institutions_CreateUpdateDelete() As Task
        Using ojs = CreateWriteClient()
            Dim created = Await ojs.Institutions.CreateAsync(New InstitutionEdit With {
                .Name = LocalizedString.Create("en", $"Library Test Institute {Stamp}"),
                .IpRanges = New List(Of String) From {"192.0.2.0/24"}})
            Try
                Assert.IsNotNull(created.Id)
                Assert.Contains("192.0.2.0/24", created.IpRanges)

                Dim updated = Await ojs.Institutions.UpdateAsync(created.Id.Value, New InstitutionEdit With {
                    .IpRanges = New List(Of String) From {"192.0.2.0/24", "198.51.100.7"}})
                Assert.HasCount(2, updated.IpRanges)
                Assert.AreEqual(created.Name.GetValue("en"), updated.Name.GetValue("en"))

                Dim found = Await ojs.Institutions.ListAsync(New SearchQuery With {.SearchPhrase = Stamp})
                Assert.AreEqual(1, found.ItemsMax)
            Finally
                ojs.Institutions.DeleteAsync(created.Id.Value).GetAwaiter().GetResult()
            End Try
        End Using
    End Function

    <TestMethod>
    Public Async Function EmailTemplates_CustomTemplate_CreateUpdateDelete() As Task
        Using ojs = CreateWriteClient()
            Dim mailable = (Await ojs.Mailables.ListAsync()).FirstOrDefault(Function(m) m.SupportsTemplates.GetValueOrDefault())
            If mailable Is Nothing Then Assert.Inconclusive("No mailable supports custom templates.")
            Dim key = $"LIBTEST_{Stamp}"

            Dim created = Await ojs.EmailTemplates.CreateAsync(New EmailTemplateEdit With {
                .Key = key, .AlternateTo = mailable.EmailTemplateKey,
                .Name = LocalizedString.Create("en", "Library test template"),
                .Subject = LocalizedString.Create("en", "Test subject"),
                .Body = LocalizedString.Create("en", "<p>Test body</p>")})
            Try
                Assert.AreEqual(key, created.Key)
                Assert.AreEqual(mailable.EmailTemplateKey, created.AlternateTo)

                Dim updated = Await ojs.EmailTemplates.UpdateAsync(key, New EmailTemplateEdit With {
                    .Subject = LocalizedString.Create("en", "Edited subject")})
                Assert.AreEqual("Edited subject", updated.Subject.GetValue("en"))
                Assert.AreEqual("<p>Test body</p>", updated.Body.GetValue("en"))

                Dim alternates = Await ojs.EmailTemplates.ListAsync(New EmailTemplateQuery With {.AlternateTo = mailable.EmailTemplateKey})
                Assert.IsTrue(alternates.Items.Any(Function(t) t.Key = key))

                Dim withTemplates = Await ojs.Mailables.GetAsync(mailable.EmailTemplateKey)
                Assert.IsTrue(withTemplates.EmailTemplates.Any(Function(t) t.Key = key))
            Finally
                ojs.EmailTemplates.DeleteAsync(key).GetAwaiter().GetResult()
            End Try
        End Using
    End Function

    <TestMethod>
    Public Async Function Context_UpdateSetting_AndRestore() As Task
        Using ojs = CreateWriteClient()
            Dim journalPath = ojs.Options.JournalPath
            Dim journal = (Await ojs.Contexts.ListAllAsync()).Single(Function(c) c.UrlPath = journalPath)
            Dim before = Await ojs.Contexts.GetAsync(journal.Id.Value)
            Dim original = If(before.SearchDescription, New LocalizedString())
            Try
                Dim updated = Await ojs.Contexts.UpdateAsync(journal.Id.Value, New ContextEdit With {
                    .SearchDescription = LocalizedString.Create("en", $"Library test {Stamp}")})
                Assert.AreEqual($"Library test {Stamp}", updated.SearchDescription.GetValue("en"))
                Assert.AreEqual(before.ContactEmail, updated.ContactEmail, "Partial update must keep other settings.")
            Finally
                Dim restore = New LocalizedString()
                For Each kv In original : restore(kv.Key) = If(kv.Value, "") : Next
                If restore.Count = 0 Then restore("en") = ""
                ojs.Contexts.UpdateAsync(journal.Id.Value, New ContextEdit With {.SearchDescription = restore}).GetAwaiter().GetResult()
            End Try

            ' Theme: write back the current values (no visible change).
            Dim theme = Await ojs.Contexts.GetThemeAsync(journal.Id.Value)
            Dim saved = Await ojs.Contexts.UpdateThemeAsync(journal.Id.Value, theme)
            Assert.AreEqual(theme.ThemePluginPath, saved.ThemePluginPath)
        End Using
    End Function

End Class
