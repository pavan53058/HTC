Option Explicit On
Option Strict On
Imports System
Imports SPF.Server.Schema.Interface.Generated
Imports System.Diagnostics
Imports SPF.Server.Context
Imports SPF.Server.Utilities
Imports SPF.Server.QueryClasses
Imports Intergraph.SPF.DAL.Common.Criteria.Object
Imports SPF.Server.Schema.Collections
Imports SPF.Diagnostics
Imports SPF.Server.Schema.Model

Namespace SPF.Server.Schema.Interface.Default
    ''' <summary>
    ''' Auto Sign off "Review Transmittal" step based on the "SPFWFOptNoOfDaysOverDueBeforeExpiration" from system options
    ''' get all the steps with specific name as parameter and are RS
    ''' loop through the steps and sign off
    ''' Send email to the corresponding user about the action taken
    ''' </summary>
    Public Class IHTCAutoSignOffOverdueStepsTaskDefault
        Inherits IHTCAutoSignOffOverdueStepsTaskBase
        Public Sub New(ByVal pblnInstantiateRequiredItems As Boolean)
            MyBase.New(pblnInstantiateRequiredItems)

        End Sub

        Public Overrides Sub OnProcess()

            MyBase.OnProcess()

            ''
            ''KEEP THE CODE,REQUIREMNTS MAY CHANGE AGAIN :)
            ''
            ''Mail will be handled as part of CIRCLE SMTP implementation
            'Dim lobjEmailUser As ISPFEmailUser
            'Dim lobjEmailDef As ISPFEmailDef
            Dim lcolOverdueSteps = GetOverDueSteps()
            If lcolOverdueSteps.Count > 0 Then
                With lcolOverdueSteps.GetEnumerator
                    While .MoveNext
                        Dim lobjStep = CType(.Value.Interfaces("ISPFObjectWorkflowStep"), ISPFObjectWorkflowStep)
                        '
                        ''get all the other WF steps of this WF and loop through the steps to find therequired steps to sign-off

                        Dim lobjWorkflowAttachedObj As ISPFWorkflowItem = CType(lobjStep.GetWorkflow.ToInterface("ISPFObjectWorkflow"), ISPFObjectWorkflow).GetItem

                        Dim lboolCommentsAvailableonTransmittal = HTCHelper.AreCommentsAvailableonTransmittal(lobjWorkflowAttachedObj)

                        HTCHelper.SignoffWFStepandStampTransmittalReturnCode(lobjStep, lobjWorkflowAttachedObj, lboolCommentsAvailableonTransmittal)

                        ''If it reaches 10 days the steps should rejected.This step will go to contarctor for as "System Approved with/Without comments" based on the comments attached to TR
                        ''
                        ''Get all WF steps that are RS and satisfies over due condition
                        'Dim lobjWFStepUsers = GetWFStepUsers(lobjStep)
                        ''Get the email def attached to system options
                        'lobjEmailDef = GetAutoSignoffEmailDef()
                        ''Send email notification to user who were responsible for this step
                        'With lobjWFStepUsers.GetEnumerator
                        '    While .MoveNext
                        '        lobjEmailUser = CType(.Value.Interfaces("ISPFEmailUser"), ISPFEmailUser)
                        '        lobjEmailDef.Send(lobjEmailUser, lobjStep)
                        '    End While
                        'End With
                    End While
                End With
            End If
        End Sub

        ''' <summary>
        ''' Get all the WF steps with the name "Review Transmittal" and are RS 
        ''' </summary>
        ''' <returns></returns>
        Function GetOverDueSteps() As IObjectDictionary
            Dim lobjOverDueSteps As New ObjectDictionary

            'Calculate the overduedate
            Dim lobjOverDueDate As SPF.Server.Schema.Model.PropertyTypes.YMDType
            Dim lstrIgnoreConfig = SPFRequestContext.Instance.IgnoreConfiguration
            SPFRequestContext.Instance.IgnoreConfiguration = True
            Dim lobjCurrentQueryConfig As SPFConfiguration = SPFRequestContext.Instance.QueryConfiguration
            Dim lobjCurrentCreateConfig As SPFConfiguration = SPFRequestContext.Instance.CreateConfiguration
            Try

                Dim lstrStepToSignOff As String = CType(SPFRequestContext.Instance.SPFOptions, ISPFOptions).Interfaces("ISPFWorkflowOptions").Properties("HTCOverdueStepName").Value.ToString
                Dim lintNoOfDays As Integer = 0 '' CType(SPFRequestContext.Instance.SPFOptions.Interfaces("ISPFWorkflowOptions"), ISPFWorkflowOptions).SPFWFOptNoOfDaysOverDueBeforeExpiration  
                Dim lobjProjectsQuery As New DynamicQuery
                lobjProjectsQuery.Query.Criteria = New ObjectCriteria(Function(x) x.HasInterface("ISPFProject") AndAlso
                                                                          x.RelatedItem("+SPFConfigurationConfigurationStatus", ObjectCriteria.Property("Name") = "Active"))
                Dim lcolprojects = lobjProjectsQuery.ExecuteToIObjectDictionary

                Dim lobjOverdueStepsQuery As New DynamicQuery
                lobjOverdueStepsQuery.Query.Criteria = New ObjectCriteria(Function(x) x.HasInterface("ISPFWorkflowStep") And
                                                                                          x.Name = lstrStepToSignOff And
                                                                                          x.Property("SPFStepStatus") = "RS")
                Dim lcolWFSteps As IObjectDictionary = lobjOverdueStepsQuery.ExecuteToIObjectDictionary

                If lcolprojects IsNot Nothing AndAlso lcolprojects.Count > 0 Then
                    With lcolprojects.GetEnumerator
                        While .MoveNext
                            Dim lobjproject = .Value

                            Dim lobjProjectCreateConfig As SPFConfiguration = Nothing
                            Dim lobjProjectQueryConfig As SPFConfiguration = Nothing

                            lobjProjectCreateConfig = ContainerDictionary.CreateConfiguration(CType((lobjproject.ToInterface("ISPFConfigurationItem")), ISPFConfigurationItem), SPFConfiguration.SPFConfigurationType.Create)
                            lobjProjectQueryConfig = ContainerDictionary.CreateConfiguration(CType((lobjproject.ToInterface("ISPFConfigurationItem")), ISPFConfigurationItem), SPFConfiguration.SPFConfigurationType.Query)



                            SPFRequestContext.Instance.ChangeServerConfigs(lobjProjectCreateConfig, lobjProjectQueryConfig)
                            ''
                            ''Get the sigoff duration from project object  And
                            ''x.RelatedItem("EDG_StepWorkflowItem", ObjectCriteria.Property("Name") = "TR-TO-P2")
                            ''
                            If lobjproject.Interfaces("IHTCProjectDetails").Properties("HTCAutoSignOffDuration") IsNot Nothing AndAlso
                                lobjproject.Interfaces("IHTCProjectDetails").Properties("HTCAutoSignOffDuration").Value IsNot Nothing Then
                                Dim lstrAutoSignOffDuration = lobjproject.Interfaces("IHTCProjectDetails").Properties("HTCAutoSignOffDuration").Value.ToString

                                'Now check all the steps
                                With lcolWFSteps.GetEnumerator
                                    While .MoveNext
                                        Dim lobjObjectWorkflowStep As ISPFObjectWorkflowStep = CType(.Value.Interfaces("ISPFObjectWorkflowStep"), ISPFObjectWorkflowStep)
                                        'If the step is "RS" and the target date is x amount of days, add it to the collection
                                        If Not lobjObjectWorkflowStep Is Nothing AndAlso IsWFStepOriginatedInCurrentProjectScope(.Value, lobjproject.UID) Then
                                            ''Calculate the over date from the system calender and the system options value
                                            ''Target 
                                            lobjOverDueDate = New SPF.Server.Schema.Model.PropertyTypes.YMDType(CType(SPFRequestContext.Instance.SPFOptions.Interfaces("ISPFWorkflowOptions"), ISPFWorkflowOptions).GetWorkflowCalendar.GetDate(lobjObjectWorkflowStep.SPFStepStartDate.Date.AddDays(Convert.ToDouble(lstrAutoSignOffDuration)), lintNoOfDays, True))
                                            If lobjOverDueDate.Date.CompareTo(System.DateTime.Now.Date) < 0 Then
                                                lobjOverDueSteps.Add(lobjObjectWorkflowStep)
                                            End If
                                        End If
                                    End While
                                End With
                            End If
                        End While
                    End With

                End If



            Catch ex As Exception
                Tracing.Error(TracingTypes.Custom, ex)
                Me.SPFSchTskFailureMsg = "There was an error while getting over due steps" + ex.Message
            Finally
                SPFRequestContext.Instance.ChangeServerConfigs(lobjCurrentCreateConfig, lobjCurrentQueryConfig)
                SPFRequestContext.Instance.IgnoreConfiguration = lstrIgnoreConfig
            End Try
            Return lobjOverDueSteps
        End Function
        ''' <summary>
        ''' Get all the users associated with this step.This is to notfy all the user about the system auto-signoff
        ''' </summary>
        ''' <param name="pobjObjectWorkflowStep"></param>
        ''' <returns></returns>
        Private Function GetWFStepUsers(ByVal pobjObjectWorkflowStep As ISPFObjectWorkflowStep) As IObjectDictionary

            Dim lobjRecipients As New ObjectDictionary
            Dim lobjUsers As New ObjectDictionary
            Dim lobjRecipient As ISPFRecipient

            Try
                lobjRecipients.AddRangeUniquely(pobjObjectWorkflowStep.GetRecipients)

                'Loop through all the recipients to build up a unique set of users
                With lobjRecipients.GetEnumerator
                    While .MoveNext
                        lobjRecipient = CType(.Value.ToInterface("ISPFRecipient"), ISPFRecipient)
                        If lobjRecipient.ObjectUpdateState <> ObjectUpdateStates.Terminated Then
                            Dim lobjParticipant As ISPFParticipant = lobjRecipient.GetParticipant
                            If lobjParticipant.IsTypeOf("ISPFEmailUser") Then
                                If Not lobjUsers.Contains(.Value) Then lobjUsers.Add(lobjParticipant)
                            End If
                        End If
                    End While
                End With

            Catch ex As Exception
                Tracing.Error(TracingTypes.Custom, ex)
                Me.SPFSchTskFailureMsg = "There was an error while getting workflow step users" + ex.Message
                'Throw New SPFException(1347, "There was an error while getting workflow step users", ex)
            End Try
            Return lobjUsers
        End Function
        Public Function GetAutoSignoffEmailDef() As ISPFEmailDef

            Dim lobjRel As IRel = SPFRequestContext.Instance.SPFOptions.GetEnd1Relationships.GetRel("HTCWorkflowOptionsAutoSignoffEmail")
            If Not lobjRel Is Nothing Then
                Return CType(CType(lobjRel.GetEnd2, IObject).ToInterface("ISPFEmailDef"), ISPFEmailDef)
            Else
                Return Nothing
            End If
        End Function

        Public Function IsWFStepOriginatedInCurrentProjectScope(pobjWFStep As IObject, pstrObjConfig As String) As Boolean

            Dim lobjStartingObject As IObject = Nothing
            Dim lobjWorkflow As ISPFObjectWorkflow = CType(CType(pobjWFStep.ToInterface("ISPFObjectWorkflowStep"), ISPFObjectWorkflowStep).GetWorkflow().ToInterface("ISPFWorkflow"), ISPFObjectWorkflow)
            If lobjWorkflow.GetEnd2Relationships.GetRel("SPFItemWorkflow") IsNot Nothing Then
                lobjStartingObject = lobjWorkflow.GetEnd2Relationships.GetRel("SPFItemWorkflow").GetEnd1
            End If

            If lobjStartingObject IsNot Nothing AndAlso lobjStartingObject.Config = pstrObjConfig Then
                Return True
            Else
                Return False
            End If
        End Function
    End Class
End Namespace

