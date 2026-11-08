Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports Library.Serialization

Namespace Models

    ''' <summary>A DOI assigned to a publication, galley or issue.</summary>
    Partial Public Class Doi

        ''' <summary>The context this DOI was created by. Read-only.</summary>
        Public Property ContextId As Integer?

        ''' <summary>The DOI itself, such as <c>10.1234/5a6b-7c8d</c>.</summary>
        Public Property Doi As String

        ''' <summary>An internal id used to track the DOI. Read-only.</summary>
        Public Property Id As Integer?

        ''' <summary>The agency with which the DOI is registered. If the DOI was manually marked registered, this will be <c>null</c>.</summary>
        Public Property RegistrationAgency As String

        ''' <summary>The full URL to resolve the DOI, such as <c>https://doi.org/10.1234/5a6b-7c8d</c>. Read-only.</summary>
        Public Property ResolvingUrl As String

        ''' <summary>The status of the DOI, such as whether it has been successfully deposited. Must match the value of one of the following: <c>Doi::STATUS_UNREGISTERED</c>, <c>Doi::STATUS_SUBMITTED</c>, <c>Doi::STATUS_REGISTERED</c>, <c>Doi::STATUS_ERROR</c>, <c>Doi::STATUS_STALE</c>.</summary>
        Public Property Status As DoiStatus?

        ''' <summary>
        ''' Properties returned by the server that this class does not model, such as those added by plugins
        ''' (<c>crossrefplugin::status</c>, <c>dataciteplugin::registeredDoi</c>, …).
        ''' </summary>
        <JsonExtensionData>
        Public Property AdditionalProperties As Dictionary(Of String, JsonElement)

    End Class

End Namespace
