Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports OJS3_5.Library.Serialization

Namespace Models

    ''' <summary>A Research Organization Registry record to add or update with &lt;c&gt;RorsResource.SaveAsync&lt;/c&gt;. Only properties you set are sent.</summary>
    ''' <example>
    ''' <code>
    ''' Await ojs.Rors.SaveAsync(New RorEdit With {
    '''     .Ror = "https://ror.org/05ezgz346", .DisplayLocale = "en", .IsActive = True,
    '''     .Name = LocalizedString.Create("en", "Example University")})
    ''' </code>
    ''' </example>
    Partial Public Class RorEdit

        ''' <summary>The display language on ror.org of this institution.</summary>
        Public Property DisplayLocale As String

        ''' <summary>Whether this institution is active or inactive.</summary>
        Public Property IsActive As Boolean?

        ''' <summary>Name.</summary>
        Public Property Name As LocalizedString

        ''' <summary>The ROR ID of this institution.</summary>
        Public Property Ror As String

        ''' <summary>Search phrase for lookups, which consists of ROR ID and all names.</summary>
        Public Property SearchPhrase As String

    End Class

End Namespace
