Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports Library.Serialization

Namespace Models

    ''' <summary>An email template. A mailable has one default template (whose key matches the mailable) and may have custom alternates.</summary>
    ''' <example>
    ''' <code>
    ''' Dim t = Await ojs.EmailTemplates.GetAsync("SUBMISSION_ACK")
    ''' Console.WriteLine(t.Subject.GetValue("en"))
    ''' </code>
    ''' </example>
    Partial Public Class EmailTemplate

        ''' <summary>The <c>key</c> of another email template. This is used to assign a custom template to a <c>Mailable</c>. The value of <c>alternateTo</c> must match the email template key of a <c>Mailable</c>.</summary>
        Public Property AlternateTo As String

        ''' <summary>Body.</summary>
        Public Property Body As LocalizedString

        ''' <summary>The ID of the context (journal/press) this email is related to. Only used for custom email templates. Default is <c>null</c>. Read-only.</summary>
        Public Property ContextId As Integer?

        ''' <summary>Href. Read-only.</summary>
        <JsonPropertyName("_href")>
        Public Property Href As String

        ''' <summary>The email template ID. Read-only.</summary>
        Public Property Id As Integer?

        ''' <summary>A unique key for this email template.</summary>
        Public Property Key As String

        ''' <summary>Name.</summary>
        Public Property Name As LocalizedString

        ''' <summary>Subject.</summary>
        Public Property Subject As LocalizedString

        ''' <summary>
        ''' Properties returned by the server that this class does not model, such as those added by plugins
        ''' (<c>crossrefplugin::status</c>, <c>dataciteplugin::registeredDoi</c>, …).
        ''' </summary>
        <JsonExtensionData>
        Public Property AdditionalProperties As Dictionary(Of String, JsonElement)

    End Class

    ''' <summary>The writable properties of an email template. Only properties you set are sent.</summary>
    ''' <example>
    ''' <code>
    ''' Await ojs.EmailTemplates.CreateAsync(New EmailTemplateEdit With {
    '''     .Key = "MY_THANK_YOU", .AlternateTo = "SUBMISSION_ACK",
    '''     .Name = LocalizedString.Create("en", "Warm thank you"),
    '''     .Subject = LocalizedString.Create("en", "Thanks, {$recipientName}"),
    '''     .Body = LocalizedString.Create("en", "&lt;p&gt;We received your submission.&lt;/p&gt;")})
    ''' </code>
    ''' </example>
    Partial Public Class EmailTemplateEdit

        ''' <summary>The <c>key</c> of another email template. This is used to assign a custom template to a <c>Mailable</c>. The value of <c>alternateTo</c> must match the email template key of a <c>Mailable</c>.</summary>
        Public Property AlternateTo As String

        ''' <summary>Body.</summary>
        Public Property Body As LocalizedString

        ''' <summary>A unique key for this email template.</summary>
        Public Property Key As String

        ''' <summary>Name.</summary>
        Public Property Name As LocalizedString

        ''' <summary>Subject.</summary>
        Public Property Subject As LocalizedString

    End Class

    ''' <summary>An email that OJS sent and recorded in its email log.</summary>
    Partial Public Class EmailLogEntry

        ''' <summary>The ID of the submission associated with the logged email. Read-only.</summary>
        Public Property AssocId As Integer?

        ''' <summary>The assoc object. Typically <c>ASSOC_TYPE_SUBMISSION</c>. Read-only.</summary>
        Public Property AssocType As Integer?

        ''' <summary>The full body of the email. Read-only.</summary>
        Public Property Body As String

        ''' <summary>The date the email was logged. Read-only.</summary>
        Public Property DateSent As DateTime?

        ''' <summary>The type of event resulting in the email being sent. One of the <c>SubmissionEmailLogEventType</c> values. Read-only.</summary>
        Public Property EventType As Integer?

        ''' <summary>The email address of the sender. Read-only.</summary>
        Public Property FromAddress As String

        ''' <summary>A URL to this object in the REST API. Read-only.</summary>
        <JsonPropertyName("_href")>
        Public Property Href As String

        ''' <summary>The ID of the email log. Read-only.</summary>
        Public Property Id As Integer?

        ''' <summary>The recipients' names and email addresses. Read-only.</summary>
        Public Property Recipients As String

        ''' <summary>The user ID of the email sender. Null for automated emails. Read-only.</summary>
        Public Property SenderId As Integer?

        ''' <summary>The subject of the email. Read-only.</summary>
        Public Property Subject As String

        ''' <summary>
        ''' Properties returned by the server that this class does not model, such as those added by plugins
        ''' (<c>crossrefplugin::status</c>, <c>dataciteplugin::registeredDoi</c>, …).
        ''' </summary>
        <JsonExtensionData>
        Public Property AdditionalProperties As Dictionary(Of String, JsonElement)

    End Class

End Namespace
