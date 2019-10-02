Option Explicit On
Option Strict On

Imports SPF.Server.Schema.Collections
Imports SPF.Server.Schema.Model
Imports SPF.Server.Schema.Interface.Generated

Namespace SPF.Server.Schema.Interface.Default
    ''' <summary>
    ''' This override is to handle specific behaviours on Job/EW object
    ''' </summary>
    Public Class IHTCJobDefault
        Inherits IHTCJobBase

#Region "Constructors"

        Public Sub New(ByVal pblnInstantiateRequiredItems As Boolean)
            MyBase.New(pblnInstantiateRequiredItems)
        End Sub
#End Region

#Region "Overrides"
        ''' <summary>
        ''' On Update of Job/EW,if the status is set to "No" then all the related Copied documents should be deleted along with the rel between job and actual revision
        ''' Deleting the rel between Job and actual doc is already covered under "IHTCDocumentMasterDefault" interface behaviour
        ''' </summary>
        ''' <param name="e"></param>
        Public Overrides Sub OnUpdate(e As UpdateEventArgs)
            MyBase.OnUpdate(e)
            Dim lcolCopiedDocuments As IObjectDictionary = Nothing
            If (Me.Interfaces("IHTCJobDetails") IsNot Nothing AndAlso
                Me.Interfaces("IHTCJobDetails").Properties("HTCJobEWStatus") IsNot Nothing AndAlso Me.Interfaces("IHTCJobDetails").Properties("HTCJobEWStatus").Value IsNot Nothing) Then
                Dim lstEWStatus = Me.Interfaces("IHTCJobDetails").Properties("HTCJobEWStatus").Value.ToString
                If lstEWStatus = "HTCENUM_No" Then
                    If Me.GetEnd2Relationships.GetRels("HTCJobToCopiedDocument") IsNot Nothing AndAlso
                        Me.GetEnd2Relationships.GetRels("HTCJobToCopiedDocument").GetEnd1s IsNot Nothing AndAlso
                        Me.GetEnd2Relationships.GetRels("HTCJobToCopiedDocument").GetEnd1s.Count > 0 Then
                        lcolCopiedDocuments = Me.GetEnd2Relationships.GetRels("HTCJobToCopiedDocument").GetEnd1s
                        With lcolCopiedDocuments.GetEnumerator
                            While .MoveNext
                                .Value.Delete()
                            End While
                        End With
                    End If
                End If
            End If
        End Sub

#End Region

    End Class

End Namespace


