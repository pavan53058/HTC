' --------------------------------------------------------------
' SmartPlant Foundation Generated Code
' 02-01-2015 20:51:23
'
' --------------------------------------------------------------
Option Explicit On
Option Strict On

Imports SPF.Server.Schema.Collections
Imports SPF.Server.Schema.Model
Imports SPF.Server.Schema.Interface.Generated
Imports HTCHelper


Namespace SPF.Server.Schema.Interface.Default

    Public Class IHTCTransmittalDefault
        Inherits IHTCTransmittalBase

#Region "Constructors"

        Public Sub New(ByVal pblnInstantiateRequiredItems As Boolean)
            MyBase.New(pblnInstantiateRequiredItems)
        End Sub

#End Region

#Region "Members"

#End Region

#Region "Overrides"


        Public Overrides Sub OnUpdatingValidation(e As CancelEventArgs)
            MyBase.OnUpdatingValidation(e)

            ''$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
            'Dim lobjHTMLTR As GenerateTRHTML = New GenerateTRHTML(Me.OBID)
            'Dim lstrHTML = lobjHTMLTR.GenerateTRHTMLContent()
            ''$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$

            ''If there are any comments available on Transmittal then retrun code 'A' should not be updated
            ''else if the transmittal no comments then retrun code 'B' should not be updated
            If AreCommentsAvailableonTransmittal(Me) Then
                If Me.Interfaces("IHTCTransmittal") IsNot Nothing AndAlso Me.Interfaces("IHTCTransmittal").Properties("HTCReturnCode") IsNot Nothing AndAlso Me.Interfaces("IHTCTransmittal").Properties("HTCReturnCode").Value IsNot Nothing Then
                    Dim lstrTransmittalReturnCode = Me.Interfaces("IHTCTransmittal").Properties("HTCReturnCode").Value.ToString
                    If lstrTransmittalReturnCode = "HTCENUM_A_(Accepted_as_submitted)" Then
                        e.CancelException = New SPFException(99999, "Transmittal has comments,cannot update 'A' as return code.")
                        e.Cancel = True
                    End If
                End If
            Else
                If Me.Interfaces("IHTCTransmittal") IsNot Nothing AndAlso Me.Interfaces("IHTCTransmittal").Properties("HTCReturnCode") IsNot Nothing AndAlso Me.Interfaces("IHTCTransmittal").Properties("HTCReturnCode").Value IsNot Nothing Then
                    Dim lstrTransmittalReturnCode = Me.Interfaces("IHTCTransmittal").Properties("HTCReturnCode").Value.ToString
                    If lstrTransmittalReturnCode = "HTCENUM_B_(Approval_with_comments)" Then
                        e.CancelException = New SPFException(99999, "Transmittal has no comments,cannot update 'B' as return code.")
                        e.Cancel = True
                    End If
                End If

            End If

        End Sub
        ''' <summary>
        ''' Documents
        ''' </summary>
        ''' <param name="e"></param>
        Public Overrides Sub OnRelationshipAdding(e As RelEventArgs)
            MyBase.OnRelationshipAdding(e)
            If e.Rel.DefUID = "HTCIncomingDocuments" Or e.Rel.DefUID = "HTCOutgoingDocuments" Then
                Dim lobjDocRev = CType(e.Rel.GetEnd2.Interfaces("ISPFDocumentRevision"), ISPFDocumentRevision)
                If lobjDocRev.SPFRevState.ToUpper <> "E1WORKING" Then
                    e.Cancel = True
                    e.CancelException = New SPFException(99999, "Cannot relate other than WORKING documents to Transmittal")
                End If
            End If

        End Sub
        Public Overrides Sub OnUpdate(e As UpdateEventArgs)
            MyBase.OnUpdate(e)

            Dim lstofConfirmedRetCodes As List(Of String) = New List(Of String)(New String() {"HTCENUM_A_(Accepted_as_submitted)", "HTCENUM_B_(Approval_with_comments)", "HTCENUM_C(Review_Not_Required)"})
            '
            ''To set the document status as "CONFIRMED" if the return code is either A B or C
            '
            Try
                If Me.Interfaces("IHTCTransmittal") IsNot Nothing AndAlso Me.Interfaces("IHTCTransmittal").Properties("HTCReturnCode") IsNot Nothing AndAlso
                    Me.Interfaces("IHTCTransmittal").Properties("HTCReturnCode").Value IsNot Nothing Then
                    Dim lstrTransmittalReturnCode = Me.Interfaces("IHTCTransmittal").Properties("HTCReturnCode").Value.ToString
                    If lstofConfirmedRetCodes.Contains(lstrTransmittalReturnCode) Then
                        ''Get the Document objects attached to the transmittal
                        '
                        If Me.GetEnd1Relationships.GetRels("HTCTransmittalDocument") IsNot Nothing AndAlso Me.GetEnd1Relationships.GetRels("HTCTransmittalDocument").GetEnd2s.Count > 0 Then
                            Dim lcolDocuments As IObjectDictionary = Me.GetEnd1Relationships.GetRels("HTCTransmittalDocument").GetEnd2s
                            SetDocumentStatusBasedOnTRReturnCode(lcolDocuments, "HTCENUM_DS_CONFIRMED")
                        End If

                    ElseIf lstrTransmittalReturnCode = "D" Then
                        ''To set the document status as "CONFIRMED" if the return code is either A B or C
                        '
                        If Me.GetEnd1Relationships.GetRels("HTCTransmittalDocument") IsNot Nothing AndAlso Me.GetEnd1Relationships.GetRels("HTCTransmittalDocument").GetEnd2s.Count > 0 Then
                            Dim lcolDocuments As IObjectDictionary = Me.GetEnd1Relationships.GetRels("HTCTransmittalDocument").GetEnd2s
                            SetDocumentStatusBasedOnTRReturnCode(lcolDocuments, "HTCENUM_DS_REJECTED")
                        End If
                    End If
                End If
            Catch ex As Exception
                Throw New SPFException(99999, "Error setting Document review code")
            End Try
        End Sub
        Private Sub SetDocumentStatusBasedOnTRReturnCode(pcolDocRevisions As IObjectDictionary, pstrDocReviewStatus As String)
            With pcolDocRevisions.GetEnumerator
                While .MoveNext
                    Dim lobjDocumentRevision = .Value
                    lobjDocumentRevision.BeginUpdate()
                    lobjDocumentRevision.Interfaces("IHTCDocumentRevision").Properties("HTCDocumentReviewStatus", True).SetValue(pstrDocReviewStatus)
                    lobjDocumentRevision.FinishUpdate()
                End While
            End With
        End Sub
#End Region

    End Class


End Namespace


