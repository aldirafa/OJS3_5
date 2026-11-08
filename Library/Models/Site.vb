Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports Library.Serialization

Namespace Models

    ''' <summary>The site-wide settings shared by every journal on the installation.</summary>
    ''' <example>
    ''' <code>
    ''' Dim site = Await ojs.Site.GetAsync()
    ''' Console.WriteLine($"{site.Title.GetValue(site.PrimaryLocale)} ({String.Join(", ", site.SupportedLocales)})")
    ''' </code>
    ''' </example>
    Partial Public Class Site

        ''' <summary>About.</summary>
        Public Property About As LocalizedString

        ''' <summary>Announcements introduction.</summary>
        Public Property AnnouncementsIntroduction As LocalizedString

        ''' <summary>Whether or not to compress usage statistics log files after they have been processed and archived.</summary>
        Public Property CompressStatsLogs As Boolean?

        ''' <summary>Contact email.</summary>
        Public Property ContactEmail As LocalizedString

        ''' <summary>Contact name.</summary>
        Public Property ContactName As LocalizedString

        ''' <summary>Counter r5 start date.</summary>
        Public Property CounterR5StartDate As DateOnly?

        ''' <summary>Specify whether reviewer statistics can be shared with other contexts of the installation.</summary>
        Public Property DisableSharedReviewerStatistics As Boolean?

        ''' <summary>Enable announcements.</summary>
        Public Property EnableAnnouncements As Boolean?

        ''' <summary>The IDs of the journals whose managers may send bulk emails.</summary>
        Public Property EnableBulkEmails As List(Of Integer)

        ''' <summary>Whether to track usage statistics by location, and whether to track at the country, region or city level.</summary>
        Public Property EnableGeoUsageStats As String

        ''' <summary>Whether to track usage statistics by institutions. Requires institutional IP ranges to be configured.</summary>
        Public Property EnableInstitutionUsageStats As Boolean?

        ''' <summary>Installed locales.</summary>
        Public Property InstalledLocales As List(Of String)

        ''' <summary>Whether or not to designate the site as the platform in COUNTER SUSHI statistics. Set this to true when all of the contexts (journals, presses or servers) of this site are published, owned or operated by the same provider.</summary>
        Public Property IsSiteSushiPlatform As Boolean?

        ''' <summary>Whether or not the access to the COUNTER SUSHI statistics API is public. If false, only users with admin or manager roles can access the API.</summary>
        Public Property IsSushiApiPublic As Boolean?

        ''' <summary>Whether to retain daily usage stats or consolidate monthly stats. Daily stats can quickly result in millions of database rows for large installs with lots of submissions.</summary>
        Public Property KeepDailyUsageStats As Boolean?

        ''' <summary>Min password length.</summary>
        Public Property MinPasswordLength As Integer?

        ''' <summary>Num announcements homepage.</summary>
        Public Property NumAnnouncementsHomepage As Integer?

        ''' <summary>ORCID api type.</summary>
        Public Property OrcidApiType As String

        ''' <summary>ORCID client ID.</summary>
        Public Property OrcidClientId As String

        ''' <summary>ORCID client secret.</summary>
        Public Property OrcidClientSecret As String

        ''' <summary>A custom base URL for ORCID redirects.</summary>
        Public Property OrcidCustomRedirectBaseUrl As String

        ''' <summary>Whether ORCID functionality is enabled</summary>
        Public Property OrcidEnabled As Boolean?

        ''' <summary>Page footer.</summary>
        Public Property PageFooter As LocalizedString

        ''' <summary>The image shown in the page header for each locale.</summary>
        Public Property PageHeaderTitleImage As Dictionary(Of String, ImageFile)

        ''' <summary>Primary locale.</summary>
        Public Property PrimaryLocale As String

        ''' <summary>Privacy statement.</summary>
        Public Property PrivacyStatement As LocalizedString

        ''' <summary>Redirect context ID.</summary>
        Public Property RedirectContextId As Integer?

        ''' <summary>Sidebar.</summary>
        Public Property Sidebar As List(Of String)

        ''' <summary>A custom stylesheet uploaded for the site.</summary>
        Public Property StyleSheet As ImageFile

        ''' <summary>Supported locales.</summary>
        Public Property SupportedLocales As List(Of String)

        ''' <summary>The platform ID for COUNTER SUSHI statistics. Required when isSiteSushiPlatform is true.</summary>
        Public Property SushiPlatformID As String

        ''' <summary>Theme plugin path.</summary>
        Public Property ThemePluginPath As String

        ''' <summary>Title.</summary>
        Public Property Title As LocalizedString

        ''' <summary>Unique site ID.</summary>
        Public Property UniqueSiteId As String

        ''' <summary>
        ''' Properties returned by the server that this class does not model, such as those added by plugins
        ''' (<c>crossrefplugin::status</c>, <c>dataciteplugin::registeredDoi</c>, …).
        ''' </summary>
        <JsonExtensionData>
        Public Property AdditionalProperties As Dictionary(Of String, JsonElement)

    End Class

    ''' <summary>The writable site settings, for &lt;c&gt;UpdateAsync&lt;/c&gt;. Only properties you set are sent.</summary>
    ''' <example>
    ''' <code>
    ''' Await ojs.Site.UpdateAsync(New SiteEdit With {.Title = LocalizedString.Create("en", "My Journals"), .MinPasswordLength = 10})
    ''' </code>
    ''' </example>
    Partial Public Class SiteEdit

        ''' <summary>About.</summary>
        Public Property About As LocalizedString

        ''' <summary>Announcements introduction.</summary>
        Public Property AnnouncementsIntroduction As LocalizedString

        ''' <summary>Whether or not to compress usage statistics log files after they have been processed and archived.</summary>
        Public Property CompressStatsLogs As Boolean?

        ''' <summary>Contact email.</summary>
        Public Property ContactEmail As LocalizedString

        ''' <summary>Contact name.</summary>
        Public Property ContactName As LocalizedString

        ''' <summary>Counter r5 start date.</summary>
        Public Property CounterR5StartDate As DateOnly?

        ''' <summary>Specify whether reviewer statistics can be shared with other contexts of the installation.</summary>
        Public Property DisableSharedReviewerStatistics As Boolean?

        ''' <summary>Enable announcements.</summary>
        Public Property EnableAnnouncements As Boolean?

        ''' <summary>The IDs of the journals whose managers may send bulk emails.</summary>
        Public Property EnableBulkEmails As List(Of Integer)

        ''' <summary>Whether to track usage statistics by location, and whether to track at the country, region or city level.</summary>
        Public Property EnableGeoUsageStats As String

        ''' <summary>Whether to track usage statistics by institutions. Requires institutional IP ranges to be configured.</summary>
        Public Property EnableInstitutionUsageStats As Boolean?

        ''' <summary>Installed locales.</summary>
        Public Property InstalledLocales As List(Of String)

        ''' <summary>Whether or not to designate the site as the platform in COUNTER SUSHI statistics. Set this to true when all of the contexts (journals, presses or servers) of this site are published, owned or operated by the same provider.</summary>
        Public Property IsSiteSushiPlatform As Boolean?

        ''' <summary>Whether or not the access to the COUNTER SUSHI statistics API is public. If false, only users with admin or manager roles can access the API.</summary>
        Public Property IsSushiApiPublic As Boolean?

        ''' <summary>Whether to retain daily usage stats or consolidate monthly stats. Daily stats can quickly result in millions of database rows for large installs with lots of submissions.</summary>
        Public Property KeepDailyUsageStats As Boolean?

        ''' <summary>Min password length.</summary>
        Public Property MinPasswordLength As Integer?

        ''' <summary>Num announcements homepage.</summary>
        Public Property NumAnnouncementsHomepage As Integer?

        ''' <summary>ORCID api type.</summary>
        Public Property OrcidApiType As String

        ''' <summary>ORCID client ID.</summary>
        Public Property OrcidClientId As String

        ''' <summary>ORCID client secret.</summary>
        Public Property OrcidClientSecret As String

        ''' <summary>Whether ORCID functionality is enabled</summary>
        Public Property OrcidEnabled As Boolean?

        ''' <summary>Page footer.</summary>
        Public Property PageFooter As LocalizedString

        ''' <summary>The page header image for each locale. To upload one, set <see cref="ImageFile.TemporaryFileId"/>.</summary>
        Public Property PageHeaderTitleImage As Dictionary(Of String, ImageFile)

        ''' <summary>Primary locale.</summary>
        Public Property PrimaryLocale As String

        ''' <summary>Privacy statement.</summary>
        Public Property PrivacyStatement As LocalizedString

        ''' <summary>Redirect context ID.</summary>
        Public Property RedirectContextId As Integer?

        ''' <summary>Sidebar.</summary>
        Public Property Sidebar As List(Of String)

        ''' <summary>A custom stylesheet. To upload one, set <see cref="ImageFile.TemporaryFileId"/>.</summary>
        Public Property StyleSheet As ImageFile

        ''' <summary>Supported locales.</summary>
        Public Property SupportedLocales As List(Of String)

        ''' <summary>The platform ID for COUNTER SUSHI statistics. Required when isSiteSushiPlatform is true.</summary>
        Public Property SushiPlatformID As String

        ''' <summary>Theme plugin path.</summary>
        Public Property ThemePluginPath As String

        ''' <summary>Title.</summary>
        Public Property Title As LocalizedString

        ''' <summary>Unique site ID.</summary>
        Public Property UniqueSiteId As String

    End Class

End Namespace
