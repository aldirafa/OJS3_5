Imports System.Net
Imports System.Net.Http
Imports System.Text.Json
Imports OJS3_5.Library.Errors
Imports OJS3_5.Library.Http
Imports OJS3_5.Library.Paging
Imports OJS3_5.Library.Serialization

<TestClass>
Public Class OptionsTests

    <TestMethod>
    Public Sub ApiRoot_JournalPath()
        Dim o As New OjsClientOptions With {.BaseUrl = "https://example.com/", .JournalPath = "jpk"}
        Assert.AreEqual("https://example.com/jpk/api/v1/", o.GetApiRoot().ToString())
    End Sub

    <TestMethod>
    Public Sub ApiRoot_IndexPhp_SubDirectory()
        Dim o As New OjsClientOptions With {.BaseUrl = "https://example.com/ojs", .JournalPath = "/jpk/", .UseIndexPhp = True}
        Assert.AreEqual("https://example.com/ojs/index.php/jpk/api/v1/", o.GetApiRoot().ToString())
    End Sub

    <TestMethod>
    Public Sub ApiRoot_SiteWide_And_NoJournal()
        Assert.AreEqual("https://example.com/_/api/v1/",
                        New OjsClientOptions With {.BaseUrl = "https://example.com", .JournalPath = OjsClientOptions.SiteWidePath}.GetApiRoot().ToString())
        Assert.AreEqual("https://example.com/api/v1/",
                        New OjsClientOptions With {.BaseUrl = "https://example.com"}.GetApiRoot().ToString())
    End Sub

    <TestMethod>
    Public Sub ApiRoot_InvalidBaseUrl_Throws()
        Assert.ThrowsExactly(Of InvalidOperationException)(Sub() Call New OjsClientOptions With {.BaseUrl = "ftp://x"}.GetApiRoot())
        Assert.ThrowsExactly(Of InvalidOperationException)(Sub() Call New OjsClientOptions().GetApiRoot())
    End Sub

    <TestMethod>
    Public Sub ForJournal_SharesClientButChangesPath()
        Dim handler As New FakeHandler()
        Using ojs = TestClient.Create(handler)
            Assert.AreEqual("https://ojs.test/_/api/v1/", ojs.ForSite().ApiRoot.ToString())
            Assert.AreEqual("https://ojs.test/other/api/v1/", ojs.ForJournal("other").ApiRoot.ToString())
            Assert.AreEqual("https://ojs.test/jpk/api/v1/", ojs.ApiRoot.ToString())
        End Using
    End Sub

End Class

<TestClass>
Public Class QueryBuilderTests

    <TestMethod>
    Public Sub Builds_CommaSeparatedArrays_AndSkipsNulls()
        Dim q As New QueryBuilder()
        q.Add("count", 30).Add("offset", CType(Nothing, Integer?)).Add("stageIds", {1, 3}).
          Add("searchPhrase", "a b&c").Add("isOverdue", True).Add("empty", Array.Empty(Of Integer)())
        Assert.AreEqual("?count=30&stageIds=1,3&searchPhrase=a%20b%26c&isOverdue=true", q.ToString())
    End Sub

    <TestMethod>
    Public Sub Empty_ReturnsEmptyString()
        Assert.AreEqual(String.Empty, New QueryBuilder().ToString())
    End Sub

    <TestMethod>
    Public Sub Enums_AsNumbers()
        Dim q As New QueryBuilder()
        q.AddEnums("days", {DayOfWeek.Monday, DayOfWeek.Friday}).AddEnum("one", CType(DayOfWeek.Sunday, DayOfWeek?))
        Assert.AreEqual("?days=1,5&one=0", q.ToString())
    End Sub

End Class

<TestClass>
Public Class JsonTests

    Public Class Sample
        Public Property Id As Integer
        Public Property Count As Integer?
        Public Property Enabled As Boolean
        Public Property MaybeEnabled As Boolean?
        Public Property Name As String
        Public Property Title As LocalizedString
        Public Property Keywords As LocalizedList
        Public Property DateSubmitted As DateTime?
        Public Property DatePublished As DateOnly?
        Public Property Day As DayOfWeek?
        Public Property Children As List(Of Sample)
        Public Property Child As Sample
        Public Property Ids As List(Of Integer)
        <Text.Json.Serialization.JsonExtensionData>
        Public Property AdditionalProperties As Dictionary(Of String, JsonElement)
    End Class

    <TestMethod>
    Public Sub Read_PhpQuirks()
        Dim json = "{
            ""id"": ""12"", ""count"": """", ""enabled"": 1, ""maybeEnabled"": ""0"", ""name"": 42,
            ""title"": [], ""keywords"": {""en"": [""a"", ""b""], ""fr_CA"": []},
            ""dateSubmitted"": ""2024-03-05 14:15:16"", ""datePublished"": ""2024-03-06"",
            ""day"": ""2"", ""children"": {""0"": {""id"": 1}, ""5"": {""id"": 2}}, ""child"": [],
            ""ids"": [""1"", 2], ""crossrefplugin::status"": ""registered""
        }"
        Dim s = OjsJson.Deserialize(Of Sample)(json)
        Assert.AreEqual(12, s.Id)
        Assert.IsNull(s.Count)
        Assert.IsTrue(s.Enabled)
        Assert.IsFalse(s.MaybeEnabled.Value)
        Assert.AreEqual("42", s.Name)
        Assert.IsEmpty(s.Title)
        CollectionAssert.AreEqual({"a", "b"}, s.Keywords.GetValues("en").ToArray())
        Assert.AreEqual(New DateTime(2024, 3, 5, 14, 15, 16), s.DateSubmitted.Value)
        Assert.AreEqual(New DateOnly(2024, 3, 6), s.DatePublished.Value)
        Assert.AreEqual(DayOfWeek.Tuesday, s.Day.Value)
        Assert.HasCount(2, s.Children)
        Assert.AreEqual(2, s.Children(1).Id)
        Assert.IsNotNull(s.Child)
        CollectionAssert.AreEqual({1, 2}, s.Ids.ToArray())
        Assert.AreEqual("registered", s.AdditionalProperties("crossrefplugin::status").GetString())
    End Sub

    <TestMethod>
    Public Sub Read_EmptyAndZeroDates_AreNull()
        Dim s = OjsJson.Deserialize(Of Sample)("{""dateSubmitted"": """", ""datePublished"": ""0000-00-00"", ""enabled"": null}")
        Assert.IsNull(s.DateSubmitted)
        Assert.IsNull(s.DatePublished)
        Assert.IsFalse(s.Enabled)
    End Sub

    <TestMethod>
    Public Sub LocalizedString_PlainString_AndFallback()
        Dim s = OjsJson.Deserialize(Of Sample)("{""title"": {""en"": ""Hello"", ""id"": """", ""fr_CA"": null}}")
        Assert.AreEqual("Hello", s.Title.GetValue("id", "en"))
        Assert.AreEqual("Hello", s.Title.GetValue("de"))
        Dim p = OjsJson.Deserialize(Of Sample)("{""title"": ""Plain""}")
        Assert.AreEqual("Plain", p.Title.ToString())
    End Sub

    <TestMethod>
    Public Sub Write_OmitsNulls_FormatsDates()
        Dim json = OjsJson.Serialize(New Sample With {
            .Id = 3, .DateSubmitted = New DateTime(2024, 1, 2, 3, 4, 5), .DatePublished = New DateOnly(2024, 1, 2),
            .Title = LocalizedString.Create("en", "T"), .Keywords = LocalizedList.Create("en", "k1")})
        Assert.AreEqual("{""id"":3,""enabled"":false,""title"":{""en"":""T""},""keywords"":{""en"":[""k1""]},""dateSubmitted"":""2024-01-02 03:04:05"",""datePublished"":""2024-01-02""}", json)
    End Sub

    <TestMethod>
    Public Sub Paged_Result()
        Dim r = OjsJson.Deserialize(Of PagedResult(Of Sample))("{""itemsMax"": ""3"", ""items"": [{""id"": 1}]}")
        Assert.AreEqual(3, r.ItemsMax)
        Assert.AreEqual(1, r.Items(0).Id)
    End Sub

End Class

<TestClass>
Public Class HttpTests

    <TestMethod>
    Public Async Function SendsBearerToken_AndParsesJson() As Task
        Dim handler = New FakeHandler().RespondJson("{""itemsMax"": 1}")
        Using ojs = TestClient.Create(handler)
            Dim r = Await ojs.SendAsync(HttpMethod.Get, "submissions?count=1")
            Assert.AreEqual(1, r.GetProperty("itemsMax").GetInt32())
        End Using
        Assert.AreEqual("Bearer TOKEN", handler.Last.Authorization)
        Assert.AreEqual("https://ojs.test/jpk/api/v1/submissions?count=1", handler.Last.Uri.ToString())
    End Function

    <TestMethod>
    Public Async Function NotFound_Maps_ErrorResponse() As Task
        Dim handler = New FakeHandler().RespondJson("{""error"": ""api.404.resourceNotFound"", ""errorMessage"": ""Not found.""}", HttpStatusCode.NotFound)
        Using ojs = TestClient.Create(handler)
            Dim ex = Await Assert.ThrowsExactlyAsync(Of OjsApiException)(Function() ojs.SendAsync(HttpMethod.Get, "submissions/9"))
            Assert.AreEqual(HttpStatusCode.NotFound, ex.StatusCode)
            Assert.AreEqual("api.404.resourceNotFound", ex.Error)
            Assert.AreEqual("Not found.", ex.ErrorMessage)
        End Using
    End Function

    <TestMethod>
    Public Async Function BadRequest_Maps_ValidationErrors() As Task
        Dim handler = New FakeHandler().RespondJson("{""name"": [""Name required.""], ""title"": {""en"": [""Title required.""]}}", HttpStatusCode.BadRequest)
        Using ojs = TestClient.Create(handler)
            Dim ex = Await Assert.ThrowsExactlyAsync(Of OjsValidationException)(Function() ojs.SendAsync(HttpMethod.Post, "announcements", "{}"))
            Assert.AreEqual("Name required.", ex.Errors("name")(0))
            Assert.AreEqual("Title required.", ex.Errors("title.en")(0))
        End Using
    End Function

    <TestMethod>
    Public Async Function NonJsonError_KeepsBody() As Task
        Dim handler = New FakeHandler().Respond(New StringContent("<html>oops</html>"), HttpStatusCode.InternalServerError)
        Using ojs = TestClient.Create(handler)
            Dim ex = Await Assert.ThrowsExactlyAsync(Of OjsApiException)(Function() ojs.SendAsync(HttpMethod.Get, "site"))
            Assert.AreEqual(HttpStatusCode.InternalServerError, ex.StatusCode)
            Assert.AreEqual("<html>oops</html>", ex.ResponseBody)
        End Using
    End Function

    <TestMethod>
    Public Async Function TemporaryFiles_Upload_Multipart() As Task
        Dim handler = New FakeHandler().RespondJson("{""id"": 77, ""name"": ""a.txt"", ""mimetype"": ""text/plain"", ""documentType"": ""default""}")
        Using ojs = TestClient.Create(handler)
            Using ms As New IO.MemoryStream(Text.Encoding.UTF8.GetBytes("hello"))
                Dim tmp = Await ojs.TemporaryFiles.UploadAsync(ms, "a.txt", "text/plain")
                Assert.AreEqual(77, tmp.Id)
            End Using
        End Using
        Assert.AreEqual(HttpMethod.Post, handler.Last.Method)
        Assert.AreEqual("multipart/form-data", handler.Last.ContentType)
        StringAssert.Contains(handler.Last.Body, "name=file")
        StringAssert.Contains(handler.Last.Body, "hello")
    End Function

End Class

<TestClass>
Public Class PagerTests

    Private Shared Function FakePages(total As Integer, pageSize As Integer, calls As List(Of Integer)) As Func(Of Integer, Threading.CancellationToken, Task(Of PagedResult(Of Integer)))
        Return Function(offset, ct)
                   calls.Add(offset)
                   Dim items = Enumerable.Range(offset, Math.Max(0, Math.Min(pageSize, total - offset))).ToList()
                   Return Task.FromResult(New PagedResult(Of Integer) With {.ItemsMax = total, .Items = items})
               End Function
    End Function

    <TestMethod>
    Public Async Function GetAll_WalksAllPages() As Task
        Dim calls As New List(Of Integer)()
        Dim all = Await Pager.GetAllAsync(FakePages(25, 10, calls))
        Assert.HasCount(25, all)
        CollectionAssert.AreEqual({0, 10, 20}, calls.ToArray())
    End Function

    <TestMethod>
    Public Async Function GetAll_RespectsMaxItems() As Task
        Dim calls As New List(Of Integer)()
        Dim all = Await Pager.GetAllAsync(FakePages(25, 10, calls), maxItems:=12)
        Assert.HasCount(12, all)
        Assert.HasCount(2, calls)
    End Function

    <TestMethod>
    Public Async Function Enumerate_StreamsItems() As Task
        Dim calls As New List(Of Integer)()
        Dim e = Pager.Enumerate(FakePages(15, 10, calls)).GetAsyncEnumerator()
        Dim seen As New List(Of Integer)()
        While Await e.MoveNextAsync()
            seen.Add(e.Current)
        End While
        CollectionAssert.AreEqual(Enumerable.Range(0, 15).ToArray(), seen.ToArray())
        CollectionAssert.AreEqual({0, 10}, calls.ToArray())
    End Function

End Class
