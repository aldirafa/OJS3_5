Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports Library.Serialization

Namespace Models

    ''' <summary>
    ''' A mailable: a kind of email OJS can send (e.g. the submission acknowledgement), with its default and
    ''' custom email templates.
    ''' </summary>
    ''' <remarks>
    ''' The list endpoint fills the summary properties; <c>GetAsync</c> also fills <see cref="EmailTemplates"/> (the custom alternates, if any).
    ''' The OpenAPI document does not describe this schema, so properties are mapped from what OJS 3.5 returns;
    ''' anything else ends up in <see cref="AdditionalProperties"/>.
    ''' </remarks>
    ''' <example>
    ''' <code>
    ''' Dim mailables = Await ojs.Mailables.ListAsync(searchPhrase:="review")
    ''' For Each m In mailables
    '''     Console.WriteLine($"{m.EmailTemplateKey}: {m.Name} ({String.Join(", ", m.GroupIds)})")
    ''' Next
    ''' </code>
    ''' </example>
    Partial Public Class Mailable

        ''' <summary>The URL of this mailable in the API.</summary>
        <JsonPropertyName("_href")>
        Public Property Href As String

        ''' <summary>The mailable's name, in the user's locale.</summary>
        Public Property Name As String

        ''' <summary>A description of when this email is sent, in the user's locale.</summary>
        Public Property Description As String

        ''' <summary>
        ''' The key of the default email template, e.g. <c>SUBMISSION_ACK</c>. It also identifies the mailable:
        ''' pass it to <c>MailablesResource.GetAsync</c>.
        ''' </summary>
        Public Property EmailTemplateKey As String

        ''' <summary>The groups this mailable belongs to (e.g. <c>submission</c>, <c>peerReview</c>).</summary>
        Public Property GroupIds As List(Of String)

        ''' <summary>The roles that send this email. <c>-1</c> (not a <see cref="UserRole"/> member) means OJS sends it automatically.</summary>
        Public Property FromRoleIds As List(Of UserRole)

        ''' <summary>The roles that receive this email.</summary>
        Public Property ToRoleIds As List(Of UserRole)

        ''' <summary>Whether custom (alternate) templates can be added for this mailable.</summary>
        Public Property SupportsTemplates As Boolean?

        ''' <summary>The variables available in templates, as variable name =&gt; description (e.g. <c>recipientName</c>).</summary>
        Public Property DataDescriptions As Dictionary(Of String, String)

        ''' <summary>The email templates of this mailable (only filled by <c>GetAsync</c>).</summary>
        Public Property EmailTemplates As List(Of EmailTemplate)

        ''' <summary>Other properties returned by the server.</summary>
        <JsonExtensionData>
        Public Property AdditionalProperties As Dictionary(Of String, JsonElement)

    End Class

End Namespace
