Imports System.Threading
Imports OJS3_5.Library.Http
Imports OJS3_5.Library.Models
Imports OJS3_5.Library.Paging
Imports OJS3_5.Library.Queries

Namespace Resources

    ''' <summary><c>/emailTemplates</c> — the journal's email templates.</summary>
    ''' <remarks>Templates are identified by their key (e.g. <c>SUBMISSION_ACK</c>), not by a numeric ID.</remarks>
    ''' <example>
    ''' <code>
    ''' Dim t = Await ojs.EmailTemplates.GetAsync("SUBMISSION_ACK")
    ''' Await ojs.EmailTemplates.UpdateAsync(t.Key, New EmailTemplateEdit With {
    '''     .Subject = LocalizedString.Create("en", "We received your submission")})
    ''' </code>
    ''' </example>
    Public NotInheritable Class EmailTemplatesResource
        Inherits ResourceBase

        Friend Sub New(http As OjsHttp)
            MyBase.New(http)
        End Sub

        ''' <summary>Gets one page of email templates.</summary>
        ''' <param name="query">Filters and paging; <c>Nothing</c> for the first page.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim page = Await ojs.EmailTemplates.ListAsync(New EmailTemplateQuery With {.IsModified = True})
        ''' Console.WriteLine($"{page.ItemsMax} in total")
        ''' For Each item In page.Items : Console.WriteLine(item.Key) : Next
        ''' </code>
        ''' </example>
        Public Function ListAsync(Optional query As EmailTemplateQuery = Nothing,
                                  Optional cancellationToken As CancellationToken = Nothing) As Task(Of PagedResult(Of EmailTemplate))
            Return Http.GetAsync(Of PagedResult(Of EmailTemplate))("emailTemplates", If(query, New EmailTemplateQuery()).ToQueryBuilder(), cancellationToken)
        End Function

        ''' <summary>Gets every email template matching <paramref name="query"/>.</summary>
        ''' <param name="query">Filters; <see cref="ListQuery.Count"/> sets the page size.</param>
        ''' <param name="maxItems">Stops after this many templates.</param>
        ''' <param name="cancellationToken">Cancels the requests.</param>
        ''' <example>
        ''' <code>
        ''' Dim all = Await ojs.EmailTemplates.ListAllAsync(New EmailTemplateQuery With {.AlternateTo = "SUBMISSION_ACK"})
        ''' For Each item In all : Console.WriteLine(item.Name.GetValue("en")) : Next
        ''' </code>
        ''' </example>
        Public Function ListAllAsync(Optional query As EmailTemplateQuery = Nothing, Optional maxItems As Integer? = Nothing,
                                     Optional cancellationToken As CancellationToken = Nothing) As Task(Of List(Of EmailTemplate))
            Return ListAllCoreAsync(Of EmailTemplate, EmailTemplateQuery)(If(query, New EmailTemplateQuery()), AddressOf ListAsync, maxItems, cancellationToken)
        End Function

        ''' <summary>Streams every email template matching <paramref name="query"/> page by page.</summary>
        ''' <param name="query">Filters; <see cref="ListQuery.Count"/> sets the page size.</param>
        ''' <example>
        ''' <code>
        ''' ' C#: await foreach (var item in ojs.EmailTemplates.Enumerate()) Console.WriteLine(item.Key);
        ''' ' VB has no Await For Each; use the enumerator (or ListAllAsync):
        ''' Dim e = ojs.EmailTemplates.Enumerate().GetAsyncEnumerator()
        ''' While Await e.MoveNextAsync()
        '''     Console.WriteLine(e.Current.Key)
        ''' End While
        ''' Await e.DisposeAsync()
        ''' </code>
        ''' </example>
        Public Function Enumerate(Optional query As EmailTemplateQuery = Nothing) As IAsyncEnumerable(Of EmailTemplate)
            Return EnumerateCore(Of EmailTemplate, EmailTemplateQuery)(If(query, New EmailTemplateQuery()), AddressOf ListAsync)
        End Function

        ''' <summary>Gets an email template.</summary>
        ''' <param name="key">The template key, e.g. <c>SUBMISSION_ACK</c>.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim t = Await ojs.EmailTemplates.GetAsync("SUBMISSION_ACK")
        ''' Console.WriteLine(t.Subject.GetValue("en"))
        ''' </code>
        ''' </example>
        Public Function GetAsync(key As String, Optional cancellationToken As CancellationToken = Nothing) As Task(Of EmailTemplate)
            ArgumentException.ThrowIfNullOrEmpty(key)
            Return Http.GetAsync(Of EmailTemplate)($"emailTemplates/{OjsHttp.Seg(key)}", Nothing, cancellationToken)
        End Function

        ''' <summary>Creates a custom email template.</summary>
        ''' <param name="template">The template. <c>Key</c>, <c>Name</c>, <c>Subject</c> and <c>Body</c> are required; set <c>AlternateTo</c> to attach it to a mailable.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <returns>The new template.</returns>
        ''' <example>
        ''' <code>
        ''' Await ojs.EmailTemplates.CreateAsync(New EmailTemplateEdit With {
        '''     .Key = "WARM_THANKS", .AlternateTo = "SUBMISSION_ACK",
        '''     .Name = LocalizedString.Create("en", "Warm thanks"), .Subject = LocalizedString.Create("en", "Thank you, {$recipientName}"),
        '''     .Body = LocalizedString.Create("en", "&lt;p&gt;We received your submission.&lt;/p&gt;")})
        ''' </code>
        ''' </example>
        Public Function CreateAsync(template As EmailTemplateEdit, Optional cancellationToken As CancellationToken = Nothing) As Task(Of EmailTemplate)
            ArgumentNullException.ThrowIfNull(template)
            Return Http.PostAsync(Of EmailTemplate)("emailTemplates", template, cancellationToken)
        End Function

        ''' <summary>Changes an email template. Only the properties you set are changed.</summary>
        ''' <param name="key">The template key.</param>
        ''' <param name="changes">The properties to change.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <returns>The updated template.</returns>
        ''' <example>
        ''' <code>
        ''' Await ojs.EmailTemplates.UpdateAsync("SUBMISSION_ACK", New EmailTemplateEdit With {.Subject = LocalizedString.Create("en", "We received your submission")})
        ''' </code>
        ''' </example>
        Public Function UpdateAsync(key As String, changes As EmailTemplateEdit,
                                    Optional cancellationToken As CancellationToken = Nothing) As Task(Of EmailTemplate)
            ArgumentException.ThrowIfNullOrEmpty(key)
            ArgumentNullException.ThrowIfNull(changes)
            Return Http.PutAsync(Of EmailTemplate)($"emailTemplates/{OjsHttp.Seg(key)}", changes, cancellationToken)
        End Function

        ''' <summary>
        ''' Deletes a custom template, or resets a default template to its original text.
        ''' </summary>
        ''' <param name="key">The template key.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <returns>The deleted (or reset) template.</returns>
        ''' <example>
        ''' <code>
        ''' Await ojs.EmailTemplates.DeleteAsync("WARM_THANKS")
        ''' </code>
        ''' </example>
        Public Function DeleteAsync(key As String, Optional cancellationToken As CancellationToken = Nothing) As Task(Of EmailTemplate)
            ArgumentException.ThrowIfNullOrEmpty(key)
            Return Http.DeleteAsync(Of EmailTemplate)($"emailTemplates/{OjsHttp.Seg(key)}", Nothing, Nothing, cancellationToken)
        End Function

        ''' <summary>Resets every default template to its original text and deletes all custom templates.</summary>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <returns>The keys of the templates that were reset or deleted.</returns>
        ''' <example>
        ''' <code>
        ''' Dim keys = Await ojs.EmailTemplates.RestoreDefaultsAsync()
        ''' Console.WriteLine($"Reset {keys.Count} templates")
        ''' </code>
        ''' </example>
        Public Function RestoreDefaultsAsync(Optional cancellationToken As CancellationToken = Nothing) As Task(Of List(Of String))
            Return Http.DeleteAsync(Of List(Of String))("emailTemplates/restoreDefaults", Nothing, Nothing, cancellationToken)
        End Function

    End Class

    ''' <summary><c>/mailables</c> — the kinds of email OJS sends, and their templates.</summary>
    ''' <example>
    ''' <code>
    ''' Dim m = Await ojs.Mailables.GetAsync("SUBMISSION_ACK")
    ''' For Each t In m.EmailTemplates
    '''     Console.WriteLine($"{t.Key}: {t.Name.GetValue("en")}")
    ''' Next
    ''' </code>
    ''' </example>
    Public NotInheritable Class MailablesResource
        Inherits ResourceBase

        Friend Sub New(http As OjsHttp)
            MyBase.New(http)
        End Sub

        ''' <summary>Gets all mailables (summary properties only).</summary>
        ''' <param name="searchPhrase">Only mailables whose name or description matches.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim mailables = Await ojs.Mailables.ListAsync("review")
        ''' For Each m In mailables : Console.WriteLine($"{m.EmailTemplateKey}: {m.Name}") : Next
        ''' </code>
        ''' </example>
        Public Function ListAsync(Optional searchPhrase As String = Nothing,
                                  Optional cancellationToken As CancellationToken = Nothing) As Task(Of List(Of Mailable))
            Return Http.GetAsync(Of List(Of Mailable))("mailables", New QueryBuilder().Add("searchPhrase", searchPhrase), cancellationToken)
        End Function

        ''' <summary>Gets a mailable with its email templates.</summary>
        ''' <param name="mailableId">The mailable's <see cref="Mailable.EmailTemplateKey"/>, e.g. <c>SUBMISSION_ACK</c>.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim m = Await ojs.Mailables.GetAsync("SUBMISSION_ACK")
        ''' For Each v In m.DataDescriptions : Console.WriteLine($"{{${v.Key}}}: {v.Value}") : Next
        ''' </code>
        ''' </example>
        Public Function GetAsync(mailableId As String, Optional cancellationToken As CancellationToken = Nothing) As Task(Of Mailable)
            ArgumentException.ThrowIfNullOrEmpty(mailableId)
            Return Http.GetAsync(Of Mailable)($"mailables/{OjsHttp.Seg(mailableId)}", Nothing, cancellationToken)
        End Function

    End Class

    ''' <summary><c>/emails</c> — the log of emails OJS has sent.</summary>
    ''' <example>
    ''' <code>
    ''' For Each e In Await ojs.Emails.ListAuthorEmailsAsync(submissionId)
    '''     Console.WriteLine($"{e.DateSent:g} {e.Subject}")
    ''' Next
    ''' </code>
    ''' </example>
    Public NotInheritable Class EmailsResource
        Inherits ResourceBase

        Friend Sub New(http As OjsHttp)
            MyBase.New(http)
        End Sub

        ''' <summary>Gets the emails sent to the authors of a submission.</summary>
        ''' <param name="submissionId">The submission ID.</param>
        ''' <param name="eventType">Only emails of this event type (<c>SubmissionEmailLogEventType</c> constant).</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim emails = Await ojs.Emails.ListAuthorEmailsAsync(12)
        ''' For Each e In emails : Console.WriteLine($"{e.DateSent:g} {e.Subject}") : Next
        ''' </code>
        ''' </example>
        Public Function ListAuthorEmailsAsync(submissionId As Integer, Optional eventType As Integer? = Nothing,
                                              Optional cancellationToken As CancellationToken = Nothing) As Task(Of List(Of EmailLogEntry))
            Dim q = New QueryBuilder().Add("submissionId", CType(submissionId, Integer?)).Add("eventType", eventType)
            Return Http.GetAsync(Of List(Of EmailLogEntry))("emails/authorEmails", q, cancellationToken)
        End Function

        ''' <summary>Gets one logged email.</summary>
        ''' <param name="emailId">The email log entry ID.</param>
        ''' <param name="cancellationToken">Cancels the request.</param>
        ''' <example>
        ''' <code>
        ''' Dim e = Await ojs.Emails.GetAsync(100)
        ''' Console.WriteLine(e.Body)
        ''' </code>
        ''' </example>
        Public Function GetAsync(emailId As Integer, Optional cancellationToken As CancellationToken = Nothing) As Task(Of EmailLogEntry)
            Return Http.GetAsync(Of EmailLogEntry)($"emails/{emailId}", Nothing, cancellationToken)
        End Function

    End Class

End Namespace
