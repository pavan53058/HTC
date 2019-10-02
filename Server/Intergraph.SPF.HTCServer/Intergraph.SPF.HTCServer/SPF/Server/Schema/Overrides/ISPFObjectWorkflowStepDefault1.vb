Imports SPF.Client
Imports SPF.Diagnostics
Imports SPF.Server.Context
Imports SPF.Server.Schema.Collections
Imports SPF.Server.Schema.Interface.Default
Imports SPF.Server.Schema.Interface.Generated

Namespace SPF.Server.Schema.Interface.Default

    Public Class ISPFObjectWorkflowStepDefault1
        Inherits ISPFObjectWorkflowStepDefault

#Region " Members "


#End Region

#Region " Properties "

#End Region

#Region " Constructor "

        Public Sub New(ByVal pblnInstantiateRequiredItems As Boolean)
            MyBase.New(pblnInstantiateRequiredItems)
        End Sub

#End Region

#Region " Methods "

        Public Overrides Sub SignOff(pstrComments As String, pstrMessageToNextStep As String, pstrStepStatus As String, pblnDontComplete As Boolean)
            If Me.GetWorkflow.Name = "HTCReviewTransmittal" Then
                Dim lobjWorkflowItem As ISPFWorkflowItem = CType(Me.GetWorkflow.ToInterface("ISPFObjectWorkflow"), ISPFObjectWorkflow).GetItem
                '
                'this logic is to stamp the duration on the step based on the project "sign-off" duration
                '
                Try
                    ''step name is already included as part of system options
                    Dim lstrStepToSignOff As String = CType(SPFRequestContext.Instance.SPFOptions, ISPFOptions).Interfaces("ISPFWorkflowOptions").Properties("HTCOverdueStepName").Value.ToString
                    Dim lcolSteps As IObjectDictionary = New ObjectDictionary
                    Dim lcolNextInteractiveSteps As IObjectDictionary = Me.GetNextInteractiveSteps(lcolSteps)

                    If lcolNextInteractiveSteps IsNot Nothing AndAlso lcolNextInteractiveSteps.Count > 0 Then

                        Dim lobjNextWorkStep As IObject = lcolNextInteractiveSteps.FirstOrDefault

                        If lobjNextWorkStep.Name = lstrStepToSignOff Then
                            Dim lstrProjectDuration = GetProjectDuration(lobjWorkflowItem)
                            If Not String.IsNullOrWhiteSpace(lstrProjectDuration) Then
                                lobjNextWorkStep.BeginUpdate()
                                lobjNextWorkStep.Interfaces("ISPFWorkflowStep").Properties("SPFStepDuration").SetValue(lstrProjectDuration)
                                lobjNextWorkStep.FinishUpdate()
                            End If
                        End If
                    End If


                Catch ex As Exception
                    Tracing.Error(TracingTypes.Custom, ex)
                    Throw ex
                End Try
                MyBase.SignOff(pstrComments, pstrMessageToNextStep, pstrStepStatus, pblnDontComplete)
            Else
                MyBase.SignOff(pstrComments, pstrMessageToNextStep, pstrStepStatus, pblnDontComplete)
            End If



        End Sub
        ''' <summary>
        ''' Gets the project duration from the attached WF attached object
        ''' </summary>
        ''' <param name="pobjStartingObject"></param>
        ''' <returns></returns>
        Private Function GetProjectDuration(pobjStartingObject As IObject) As String
            Dim lstrProjectDuration As String = String.Empty
            Dim lobjConfig = pobjStartingObject.GetConfig
            If lobjConfig IsNot Nothing Then
                If lobjConfig.Interfaces("IHTCProjectDetails") IsNot Nothing AndAlso lobjConfig.Interfaces("IHTCProjectDetails").Properties("HTCAutoSignOffDuration") IsNot Nothing AndAlso
                    lobjConfig.Interfaces("IHTCProjectDetails").Properties("HTCAutoSignOffDuration").Value IsNot Nothing AndAlso
                      Not String.IsNullOrWhiteSpace(lobjConfig.Interfaces("IHTCProjectDetails").Properties("HTCAutoSignOffDuration").Value.ToString) Then
                    lstrProjectDuration = lobjConfig.Interfaces("IHTCProjectDetails").Properties("HTCAutoSignOffDuration").Value.ToString
                End If
            End If
            Return lstrProjectDuration
        End Function


#End Region

    End Class

End Namespace