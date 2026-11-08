Imports System.Globalization
Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports OJS3_5.Library.Serialization

Namespace Models

    ''' <summary>Base class of the statistics records; keeps properties this library does not model.</summary>
    ''' <example>
    ''' <code>
    ''' Dim top = Await ojs.Stats.Publications.ListAsync()
    ''' For Each extra In top.Items(0).AdditionalProperties : Console.WriteLine(extra.Key) : Next
    ''' </code>
    ''' </example>
    Public MustInherit Class StatsRecord

        ''' <summary>Other properties returned by the server.</summary>
        <JsonExtensionData>
        Public Property AdditionalProperties As Dictionary(Of String, JsonElement)

    End Class

    ''' <summary>One editorial activity figure, such as the number of submissions received.</summary>
    ''' <example>
    ''' <code>
    ''' For Each s In Await ojs.Stats.GetEditorialAsync(New DateOnly(2026, 1, 1), New DateOnly(2026, 12, 31))
    '''     Console.WriteLine($"{s.Name}: {s.Value}")
    ''' Next
    ''' </code>
    ''' </example>
    Public Class EditorialStat
        Inherits StatsRecord

        ''' <summary>A stable identifier, e.g. <c>submissionsReceived</c> or <c>daysToDecision</c>.</summary>
        Public Property Key As String

        ''' <summary>The translated name of the figure (may change with the user's locale).</summary>
        Public Property Name As String

        ''' <summary>The value: a count, a number of days or a rate, depending on <see cref="Key"/>.</summary>
        Public Property Value As Double?

    End Class

    ''' <summary>The number of users with a role, as returned by <c>StatsResource.GetUsersAsync</c>.</summary>
    ''' <example>
    ''' <code>
    ''' For Each s In Await ojs.Stats.GetUsersAsync()
    '''     Console.WriteLine(If(s.IsTotal, $"All users: {s.Value}", $"{s.Role}: {s.Value}"))
    ''' Next
    ''' </code>
    ''' </example>
    Public Class UserStat
        Inherits StatsRecord

        ''' <summary>The role ID as text, or <c>total</c> for all users. See <see cref="Role"/>.</summary>
        Public Property Id As String

        ''' <summary>The translated role name.</summary>
        Public Property Name As String

        ''' <summary>The number of users.</summary>
        Public Property Value As Integer?

        ''' <summary>The role, or <c>Nothing</c> for the <c>total</c> row.</summary>
        <JsonIgnore>
        Public ReadOnly Property Role As UserRole?
            Get
                Dim n As Integer
                If Integer.TryParse(Id, NumberStyles.Integer, CultureInfo.InvariantCulture, n) Then Return CType(n, UserRole)
                Return Nothing
            End Get
        End Property

        ''' <summary><c>True</c> for the row that counts all users.</summary>
        <JsonIgnore>
        Public ReadOnly Property IsTotal As Boolean
            Get
                Return String.Equals(Id, "total", StringComparison.OrdinalIgnoreCase)
            End Get
        End Property

    End Class

    ''' <summary>A point on a usage timeline: the views in one day or month.</summary>
    ''' <example>
    ''' <code>
    ''' Dim days = Await ojs.Stats.Publications.GetTimelineAsync(resolution:=TimelineResolution.Day)
    ''' For Each d In days : Console.WriteLine($"{d.Date}: {d.Value}") : Next
    ''' </code>
    ''' </example>
    Public Class TimelineInterval
        Inherits StatsRecord

        ''' <summary>The day (<c>yyyy-MM-dd</c>) or month (<c>yyyy-MM</c>) as sent by OJS.</summary>
        Public Property [Date] As String

        ''' <summary>A display label for the interval.</summary>
        Public Property Label As String

        ''' <summary>The number of views.</summary>
        Public Property Value As Integer?

    End Class

    ''' <summary>Homepage views of a journal.</summary>
    ''' <example>
    ''' <code>
    ''' Dim home = Await ojs.Stats.Contexts.GetAsync(1)
    ''' Console.WriteLine($"{home.Context.UrlPath}: {home.Total} homepage views")
    ''' </code>
    ''' </example>
    Public Class ContextStats
        Inherits StatsRecord

        ''' <summary>The number of homepage views.</summary>
        Public Property Total As Integer?

        ''' <summary>The journal (summary properties).</summary>
        Public Property Context As Context

    End Class

    ''' <summary>Views of an issue.</summary>
    ''' <example>
    ''' <code>
    ''' Dim s = Await ojs.Stats.Issues.GetAsync(4)
    ''' Console.WriteLine($"{s.Issue.Identification}: {s.TocViews} TOC, {s.IssueGalleyViews} galley views")
    ''' </code>
    ''' </example>
    Public Class IssueStats
        Inherits StatsRecord

        ''' <summary>Table of contents views plus issue galley views.</summary>
        Public Property TotalViews As Integer?

        ''' <summary>Table of contents views.</summary>
        Public Property TocViews As Integer?

        ''' <summary>Issue galley views and downloads.</summary>
        Public Property IssueGalleyViews As Integer?

        ''' <summary>The issue (a few summary properties).</summary>
        Public Property Issue As Issue

    End Class

    ''' <summary>The publication an item of <see cref="PublicationStats"/> refers to.</summary>
    ''' <example>
    ''' <code>
    ''' Dim s = Await ojs.Stats.Publications.GetAsync(12)
    ''' Console.WriteLine($"{s.Publication.ShortAuthorString}: {s.Publication.UrlPublished}")
    ''' </code>
    ''' </example>
    Public Class PublicationStatsItem
        Inherits StatsRecord

        ''' <summary>The submission ID.</summary>
        Public Property Id As Integer?

        ''' <summary>The URL of the submission in the API.</summary>
        <JsonPropertyName("_href")>
        Public Property Href As String

        ''' <summary>The full title (with prefix and subtitle).</summary>
        Public Property FullTitle As LocalizedString

        ''' <summary>A short author string, e.g. <c>Lovelace et al.</c>.</summary>
        Public Property ShortAuthorString As String

        ''' <summary>The public URL of the article.</summary>
        Public Property UrlPublished As String

        ''' <summary>The URL of the article in the editorial workflow.</summary>
        Public Property UrlWorkflow As String

    End Class

    ''' <summary>Views of an article: abstract views and galley views by type.</summary>
    ''' <example>
    ''' <code>
    ''' Dim top = Await ojs.Stats.Publications.ListAsync(New PublicationStatsQuery With {.Count = 10})
    ''' For Each p In top.Items
    '''     Console.WriteLine($"{p.Publication.FullTitle.GetValue("en")}: {p.AbstractViews} abstract, {p.GalleyViews} files")
    ''' Next
    ''' </code>
    ''' </example>
    Public Class PublicationStats
        Inherits StatsRecord

        ''' <summary>Abstract page views.</summary>
        Public Property AbstractViews As Integer?

        ''' <summary>Galley views and downloads of every type.</summary>
        Public Property GalleyViews As Integer?

        ''' <summary>PDF galley views and downloads.</summary>
        Public Property PdfViews As Integer?

        ''' <summary>HTML galley views.</summary>
        Public Property HtmlViews As Integer?

        ''' <summary>Views and downloads of other galley types.</summary>
        Public Property OtherViews As Integer?

        ''' <summary>The article.</summary>
        Public Property Publication As PublicationStatsItem

    End Class

    ''' <summary>Downloads of one published file.</summary>
    ''' <example>
    ''' <code>
    ''' Dim files = Await ojs.Stats.Publications.ListFilesAsync()
    ''' For Each f In files.Items : Console.WriteLine($"{f.FileName}: {f.Downloads} downloads") : Next
    ''' </code>
    ''' </example>
    Public Class PublicationFileStats
        Inherits StatsRecord

        ''' <summary>The submission ID.</summary>
        Public Property SubmissionId As Integer?

        ''' <summary>The submission title.</summary>
        Public Property SubmissionTitle As String

        ''' <summary>The submission file ID.</summary>
        Public Property SubmissionFileId As Integer?

        ''' <summary>The file name.</summary>
        Public Property FileName As String

        ''' <summary>The number of downloads.</summary>
        Public Property Downloads As Integer?

    End Class

    ''' <summary>
    ''' Article views from one country, region or city. <see cref="Region"/> and <see cref="City"/> are only filled
    ''' by the region and city endpoints.
    ''' </summary>
    ''' <example>
    ''' <code>
    ''' Dim cities = Await ojs.Stats.Publications.ListCitiesAsync()
    ''' For Each c In cities.Items : Console.WriteLine($"{c.City}, {c.Region}, {c.Country}: {c.Total}") : Next
    ''' </code>
    ''' </example>
    Public Class GeoStats
        Inherits StatsRecord

        ''' <summary>The country code (ISO 3166-1 alpha-2) or name.</summary>
        Public Property Country As String

        ''' <summary>The region, for region and city statistics.</summary>
        Public Property Region As String

        ''' <summary>The city, for city statistics.</summary>
        Public Property City As String

        ''' <summary>All views.</summary>
        Public Property Total As Integer?

        ''' <summary>Unique views (one per visitor and item per day).</summary>
        Public Property Unique As Integer?

    End Class

End Namespace
