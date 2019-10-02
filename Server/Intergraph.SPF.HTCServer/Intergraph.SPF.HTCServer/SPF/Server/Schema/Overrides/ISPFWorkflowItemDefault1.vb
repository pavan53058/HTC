
' --------------------------------------------------------------
' SmartPlant Foundation Generated Code
' 24/10/2005 17:10:05
'
' --------------------------------------------------------------
Option Explicit On
Option Strict On

Imports SPF.Server.Schema.Model
Imports SPF.Server.Schema.Collections
Imports SPF.Server.Context
Imports SPF.Server.Utilities.ComplexObjectUtilities
Imports SPF.Server.Schema.Interface.Generated

Namespace SPF.Server.Schema.Interface.Default
    Public Class ISPFWorkflowItemDefault1
        Inherits ISPFWorkflowItemDefault

#Region " Constructors "

        Public Sub New(ByVal pblnInstantiateRequiredItems As Boolean)
            MyBase.New(pblnInstantiateRequiredItems)
        End Sub

#End Region
        Public Overrides Function AttachWorkflow(pobjWorkflowTemplate As ISPFWorkflowTemplate) As ISPFObjectWorkflow
            Return AttachWorkflowInternal(pobjWorkflowTemplate)
        End Function


        Private Function AttachWorkflowInternal(ByVal pobjWorkflowTemplateOrInstantiated As Generated.IObject) As Generated.ISPFObjectWorkflow
            Dim lobjForAttach As IObject

            lobjForAttach = SPF.Server.Utilities.SchemaUtilities.BeginUpdateWithImplicitClaim(Me)
            '
            ' remove the property SPFWorkflowStatusErrorMessage if it is set.
            '
            If Me.Properties.Contains("SPFWorkflowStatusErrorMessage") Then
                Me.Properties("SPFWorkflowStatusErrorMessage").DeleteProperty()
            End If

            If Me.Properties.Contains("SPFActiveWorkflowStatus") Then
                Me.Properties("SPFActiveWorkflowStatus").DeleteProperty()
            End If

            Dim lstrConfig As String = ""
            If Not Me.Config Is Nothing Then
                lstrConfig = Me.Config
            End If

            If lstrConfig <> "" AndAlso IsObjectInCreateConfig(Me) = False Then
                Throw New SPFException(1554, "Object's config does not match the current create config")
            End If
            '
            ' Lhiscock 15/08/2013 TR52337 - Throws exception if a workflow is attached with a disabled user
            '
            Dim lobjWorkflowObj As ISPFWorkflow = CType(pobjWorkflowTemplateOrInstantiated.ToInterface("ISPFWorkflow"), ISPFWorkflow)

            If lobjWorkflowObj.GetReassigntoParticipant.Interfaces.Contains("ISPFLoginUser") Then
                If DirectCast(lobjWorkflowObj.GetReassigntoParticipant.Interfaces("ISPFLoginUser"), ISPFLoginUser).SPFDisableUser Then
                    Throw New SPFException(2500, "Workflow reassign participant $1 is disabled", New String() {lobjWorkflowObj.GetReassigntoParticipant.Name})
                End If
            End If

            Dim lobjParticipants As IObjectDictionary = lobjWorkflowObj.GetWorkflowSteps.GetEnd1Relationships.GetRels("EDG_SPFWFStepParticipant").GetEnd2s

            Dim lblnAllowAutoWorkflowStepReassignment As Boolean = SPFRequestContext.Instance.SPFOptions.SPFWFOptAllowStepAutoReassignment
            '
            ' Modified vpathi 08/12/2016 CR-AM-111700
            ' Don't throw exception for the disabled users when workflow step auto reassignment is on
            ' 
            If Not lblnAllowAutoWorkflowStepReassignment Then
                For Each lobjParticipant As IObject In lobjParticipants.Values
                    If lobjParticipant.Interfaces.Contains("ISPFLoginUser") AndAlso DirectCast(lobjParticipant.Interfaces("ISPFLoginUser"), ISPFLoginUser).SPFDisableUser Then
                        Throw New SPFException(2499, "The participant $1 on the workflow is disabled", New String() {lobjParticipant.Name})
                    End If
                Next
            End If

            'Create an instance of the workflow
            Dim lobjWorkflow As IObject = CType(pobjWorkflowTemplateOrInstantiated.ToInterface("ISPFWorkflow"), ISPFWorkflow).CreateWorkflowInstance(Me, 1)

            'Record the fact that I was the submitter
            Dim lobjIRel As IObject = SPF.Server.Utilities.GeneralUtilities.InstantiateRelation("SPFWorkflowSubmitter", lobjWorkflow, SPFRequestContext.Instance.LoginUser, False)
            '
            ' ADM-05/10/06 DI32913 have to call finish create taken out of Instantiate because events weren't being fired.
            '
            lobjIRel.GetClassDefinition.FinishCreate(lobjIRel)

            'Record the fact that we have added an extra workflow
            Dim lobjWorkflowItem As ISPFWorkflowItem = CType(lobjForAttach.Interfaces("ISPFWorkflowItem"), ISPFWorkflowItem)

            '
            ' Send the Future email to step recipients
            ' Moved send future email code after SPFWorkflowSubmitter rel creation
            ' since we have to send the email to reassign to participant when Step recipient Is disabled
            '
            CType(lobjWorkflow.Interfaces("ISPFWorkflow"), ISPFWorkflowDefault).SendFutureActionEmails(lobjWorkflow)

            '
            ' Altered MLF 02/08/10 DI 53914 - Access this through the real property accessors 
            '
            'Dim lintActiveWorkflows As Integer = CType(lobjForAttach.Interfaces("ISPFWorkflowItem").Properties("SPFActiveWorkflowCount", True).Value, Integer)
            Dim lintActiveWorkflows As Integer = CType(lobjForAttach.ToInterface("ISPFWorkflowItem"), ISPFWorkflowItem).SPFActiveWorkflowCount


            lobjWorkflowItem.SPFActiveWorkflowCount = lintActiveWorkflows + 1
            Dim lobjWorkflowOptions As ISPFWorkflowOptions = CType(SPFRequestContext.Instance.SPFOptions.ToInterface("ISPFWorkflowOptions"), ISPFWorkflowOptions)
            If Not lobjWorkflowOptions.GetInitialWorkflowStatus Is Nothing Then
                CType(lobjWorkflow.ToInterface("ISPFObjectWorkflow"), ISPFObjectWorkflow).SetStatus(lobjWorkflowOptions.GetInitialWorkflowStatus.Name)
            End If
            lobjForAttach.FinishUpdate()
            'Start the workflow off
            Dim lobjWorkflowObject As ISPFObjectWorkflow = CType(lobjWorkflow.ToInterface("ISPFObjectWorkflow"), ISPFObjectWorkflow)
            lobjWorkflowObject.StartWorkflow(Nothing, "")
            Return lobjWorkflowObject
        End Function

        Public Function IsObjectInCreateConfig(ByVal pobjObject As IObject) As Boolean
            'Altered MLF 05/11/07 the default create config is not neccesarily the one we are logged into now
            'This is due to TEF tr 40221
            'Dim lobjConfig As IObject = Me.LoginUser.GetDefaultCreateConfiguration
            Dim lobjConfig As IObject = GetCurrentConfig()

            If lobjConfig Is Nothing Then
                lobjConfig = SPFApplicationContext.Instance.ConfigurationTop
            End If

            'The config could be nothing if we are in the process of changing config
            'in this case assume the object is in the create config
            If Not lobjConfig Is Nothing Then
                Dim lstrConfig As String = lobjConfig.UID


                Dim lstrObjConfig As String = ""
                If Not pobjObject.Config Is Nothing Then
                    lstrObjConfig = pobjObject.Config()
                    'Added MLF just for completness 05/11/07 tr 40221
                Else
                    lstrObjConfig = SPFApplicationContext.Instance.ConfigurationTop.UID
                End If

                If lstrObjConfig = lstrConfig Then
                    Return True
                Else
                    Return False
                End If
            End If

            Return True
        End Function
        Public Function GetCurrentConfig() As ISPFConfigurationItem

            If SPFRequestContext.Instance.CreateConfiguration.Levels.Count = 0 Then
                Return SPFApplicationContext.Instance.ConfigurationTop
            Else
                Return SPFRequestContext.Instance.CreateConfiguration.Levels.Item(SPFRequestContext.Instance.CreateConfiguration.Levels.Count - 1)
            End If
        End Function

    End Class

End Namespace

