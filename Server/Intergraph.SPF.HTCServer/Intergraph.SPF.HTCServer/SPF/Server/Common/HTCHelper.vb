Imports SPF.Diagnostics
Imports SPF.Server.Context
Imports SPF.Server.Schema.Interface.Generated

Public Class HTCHelper

    Shared Function AreCommentsAvailableonTransmittal(ByVal pobjWorkflowAttachedObj As IObject) As Boolean
        Dim lboolCommentsAvailableonTransmittal As Boolean = False
        ''Check the availability of comment on transmittal
        If pobjWorkflowAttachedObj.ClassDefinitionUID = "HTCIncomingTransmittal" And
                pobjWorkflowAttachedObj.GetEnd1Relationships IsNot Nothing And
            pobjWorkflowAttachedObj.GetEnd1Relationships.GetRels("HTCInTransmittalToComment").Count > 0 Then
            lboolCommentsAvailableonTransmittal = True

        ElseIf pobjWorkflowAttachedObj.ClassDefinitionUID = "HTCOutgoingTransmittal" And
                pobjWorkflowAttachedObj.GetEnd1Relationships IsNot Nothing And
            pobjWorkflowAttachedObj.GetEnd1Relationships.GetRels("HTCOutTransmittalToComment").Count > 0 Then
            lboolCommentsAvailableonTransmittal = True

        End If
        Return lboolCommentsAvailableonTransmittal
    End Function

    Shared Sub SignoffWFStepandStampTransmittalReturnCode(pobjWFStep As ISPFObjectWorkflowStep, pobjWFAttachedItem As ISPFWorkflowItem, pboolCommentsAvailableonTR As Boolean)
        Try
            If Not SPFRequestContext.Instance.Transaction.InTransaction Then
                SPFRequestContext.Instance.Transaction.Begin()
                ''lboolStartedTransaction = True
            End If
            ''TODO:If there is a need to change the step names then these conditions also to be modified
            If pobjWFStep.Name = ("Review Transmittal Info") And pobjWFStep.SPFStepStatus = "RS" Then
                pobjWFAttachedItem.BeginUpdate()
                If pboolCommentsAvailableonTR Then

                    pobjWFAttachedItem.Interfaces("IHTCTransmittal").Properties("HTCTransmittalStatus").SetValue("HTCENUM_Systemitically_Confirmed_with_Comments")
                Else

                    pobjWFAttachedItem.Interfaces("IHTCTransmittal").Properties("HTCTransmittalStatus").SetValue("HTCENUM_Systemtically_Confirmed")
                End If

                pobjWFAttachedItem.FinishUpdate()

                If pboolCommentsAvailableonTR Then
                    pobjWFStep.SignOff("Auto Approved by System with Comments", "Auto Approved by System with Comments", "", False)
                Else
                    pobjWFStep.SignOff("Auto Approved by System", "Auto Approved by System", "", False)
                End If

                ''TODO:If there is a need to change the step names then these conditions also to be modified
            ElseIf (pobjWFStep.Name = ("Review Transmittal")) And pobjWFStep.SPFStepStatus = "RS" Then
                pobjWFAttachedItem.BeginUpdate()
                If pboolCommentsAvailableonTR Then

                    pobjWFAttachedItem.Interfaces("IHTCTransmittal").Properties("HTCTransmittalStatus").SetValue("HTCENUM_Systemitically_Confirmed_with_Comments")
                Else

                    pobjWFAttachedItem.Interfaces("IHTCTransmittal").Properties("HTCTransmittalStatus").SetValue("HTCENUM_Systemtically_Confirmed")
                End If

                pobjWFAttachedItem.FinishUpdate()
                ''sign off the step as reject

                If pboolCommentsAvailableonTR Then
                    pobjWFStep.SignOff("Auto Approved by System with Comments", "Auto Approved by System with Comments", "", False)
                Else
                    pobjWFStep.SignOff("Auto Approved by System", "Auto Approved by System", "", False)
                End If
                ' pobjWFStep.RejectStep("Auto Approved by System", "Auto Approved by System")
            End If
            Try
                SPFRequestContext.Instance.Transaction.Commit()
            Catch ex As Exception
                SPFRequestContext.Instance.Transaction.Rollback()
                Throw ex
            End Try
        Catch ex As Exception
            Tracing.Error(TracingTypes.Custom, ex)
            Throw ex
        End Try

    End Sub

End Class
