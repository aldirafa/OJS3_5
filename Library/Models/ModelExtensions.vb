Imports System.Text.Json.Serialization

Namespace Models

    Partial Public Class Submission

        ''' <summary>
        ''' The publication matching <see cref="CurrentPublicationId"/> from <see cref="Publications"/>, or the
        ''' latest one when the ID is not set. <c>Nothing</c> when no publications were returned.
        ''' </summary>
        <JsonIgnore>
        Public ReadOnly Property CurrentPublication As Publication
            Get
                If Publications Is Nothing OrElse Publications.Count = 0 Then Return Nothing
                If CurrentPublicationId.HasValue Then
                    Dim match = Publications.FirstOrDefault(Function(p) Nullable.Equals(p.Id, CurrentPublicationId))
                    If match IsNot Nothing Then Return match
                End If
                Return Publications.OrderByDescending(Function(p) If(p.Version, 0)).First()
            End Get
        End Property

    End Class

End Namespace
