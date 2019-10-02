Imports System.Xml
Imports SPF.Diagnostics
Imports SPF.Server.Context
Imports SPF.Server.Schema.Collections
Imports SPF.Server.Schema.Interface.Generated
Imports SPF.Server.Schema.Model
Imports SPF.Server.Schema.Interface.Default
Imports SPF.Server.Utilities
Imports SPF.Server.DataAccess
Imports SPF.Server.QueryClasses
Imports Intergraph.SPF.DAL.Common.Criteria.Object
Imports System.Text.RegularExpressions
Imports SPF.Server.Components.FileService
Imports SPF.Server.Classes.SecurityRules
Imports SPF.Server.Schema.Model.PropertyTypes
Imports System.Text

Namespace SPF.Server.Components.Workflow.ProcessSteps
    ''' <summary>
    ''' This process step validated the objects in the projects before submitting the project to the WF
    ''' </summary>
    Public Class HTCValidateProjectForReview
        Inherits ProcessStepBase
#Region "Constructors"
        Public Sub New(ByVal pobjStep As ISPFObjectWorkflowStep, ByVal pobjProcessStepArgs As ProcessStepArgs)
            MyBase.New(pobjStep, pobjProcessStepArgs)
        End Sub
#End Region

#Region "Methods"
        Public Overrides Sub Execute()

            Dim lobjFileObject As IObject = Nothing
            Dim lobjProjectObject As IObject = Nothing
            Dim lboolProceedToNextStep As Boolean = True
            Dim lobjNotification As IObject = Nothing
            Dim lstrLogContent As New StringBuilder
            Dim lcolOpenDocuments As IObjectDictionary = New ObjectDictionary()
            Dim lcolOpenTranmittals As IObjectDictionary = New ObjectDictionary()
            Dim lcolOpenMarkups As IObjectDictionary = New ObjectDictionary()
            Dim lstrDefaultTRStatus = String.Empty
            Dim lstrDefaultDocStatus = String.Empty
            Dim lboolValidateMarkups As Boolean
            Dim lboolCreateprojectInfo As Boolean
            ''read default arguments from process step
            If Me.ProcessStepArgs.InputArgs.Count > 3 Then
                lstrDefaultTRStatus = CType(Me.ProcessStepArgs.InputArgs.Item(0), String)
                lstrDefaultDocStatus = CType(Me.ProcessStepArgs.InputArgs.Item(1), String)
                lboolValidateMarkups = CType(Me.ProcessStepArgs.InputArgs.Item(2), String)
                lboolCreateprojectInfo = CType(Me.ProcessStepArgs.InputArgs.Item(3), String)
            Else
                Throw New SPFException(99999, "Workflow step arguments are not supplied")
            End If

            Dim lobjCurrentQueryConfig As SPFConfiguration = SPFRequestContext.Instance.QueryConfiguration
            Dim lobjCurrentCreateConfig As SPFConfiguration = SPFRequestContext.Instance.CreateConfiguration
            Try
                Tracing.Info(TracingTypes.Custom, "Initiate Project Validate process Step")
                lstrLogContent.Append("Initiating Project Validation" + Environment.NewLine)
                'Get the starting Object(i.e Document Version in this case)
                Dim lobjWorkflow As ISPFWorkflow = CType(CType(Me.StartingObj.ToInterface("ISPFObjectWorkflowStep"), ISPFObjectWorkflowStep).GetWorkflow(False).ToInterface("ISPFWorkflow"), ISPFWorkflow)
                If lobjWorkflow IsNot Nothing AndAlso lobjWorkflow.GetEnd2Relationships.Count <> 0 Then
                    If lobjWorkflow.GetEnd2Relationships.GetRel("SPFItemWorkflow") IsNot Nothing Then
                        lobjProjectObject = lobjWorkflow.GetEnd2Relationships.GetRel("SPFItemWorkflow").GetEnd1
                    End If
                End If
                ''
                ''Switch The config to the current project object
                ''
                lstrLogContent.Append("Project Attached to the WF:" + lobjProjectObject.Name + Environment.NewLine)
                Dim lobjProjectCreateConfig As SPFConfiguration = Nothing
                Dim lobjProjectQueryConfig As SPFConfiguration = Nothing

                lobjProjectCreateConfig = ContainerDictionary.CreateConfiguration(CType((lobjProjectObject.ToInterface("ISPFConfigurationItem")), ISPFConfigurationItem), SPFConfiguration.SPFConfigurationType.Create)
                lobjProjectQueryConfig = ContainerDictionary.CreateConfiguration(CType((lobjProjectObject.ToInterface("ISPFConfigurationItem")), ISPFConfigurationItem), SPFConfiguration.SPFConfigurationType.Query)

                lstrLogContent.Append("Switching the configuration to project" + Environment.NewLine)
                Tracing.Info(TracingTypes.Custom, "Switching the configuration to project")

                SPFRequestContext.Instance.ChangeServerConfigs(lobjProjectCreateConfig, lobjProjectQueryConfig)
                lstrLogContent.Append("Creating notification object" + Environment.NewLine)
                If lobjProjectObject Is Nothing Then Throw New SPFException(99999, "Failed to find workflow object")
                Dim lstrName As String = lobjProjectObject.Name & "-" & SPFRequestContext.Instance.RequestSettings.UserName & "-" & DateTime.Now.ToString
                Dim lstrDescription As String = "Project handover notification"
                Dim lobjCommonUtilities As New CommonUtilities
                lobjNotification = lobjCommonUtilities.CreateNotificationObject(lstrName, lstrDescription)

                lobjNotification.Interfaces("ISPFContextualNotification", True).Properties("SPFContextObjectOBID", True).SetValue(lobjProjectObject.OBID)
                lobjNotification.Interfaces("ISPFContextualNotification", True).Properties("SPFContextObjectDomainUID", True).SetValue(lobjProjectObject.DomainUID)

                lobjNotification.BeginUpdate()

                lstrLogContent.Append("Notification object created" + Environment.NewLine)
                Tracing.Info(TracingTypes.Custom, "Notification object created")

                ''
                ''Get Open Transmittals
                ''
                lstrLogContent.Append("Get open Transmittals" + Environment.NewLine)
                Tracing.Info(TracingTypes.Custom, "Get open Transmittals")

                lcolOpenTranmittals = GetOpenTransmittals(lstrDefaultTRStatus)
                If lcolOpenTranmittals.Count > 0 Then
                    lobjNotification.Interfaces("IHTCProjectHandoverNotificationDetails", True).Properties("HTCProjectFailedTransmittalsWithStatus").SetValue(String.Join("|", lcolOpenTranmittals.Select(Function(o) o.Name).ToList))
                    lboolProceedToNextStep = False
                End If
                lstrLogContent.Append("Open Transmittals Count:" + lcolOpenTranmittals.Count.ToString + Environment.NewLine)
                ''
                ''Get Open Documents
                ''
                lstrLogContent.Append("Get open Documents" + Environment.NewLine)
                Tracing.Info(TracingTypes.Custom, "Get open Documents")

                lcolOpenDocuments = GetOpenDocuments(lstrDefaultDocStatus)
                If lcolOpenDocuments.Count > 0 Then
                    lobjNotification.Interfaces("IHTCProjectHandoverNotificationDetails", True).Properties("HTCProjectFailedDocumentsWithStatus").SetValue(String.Join("|", lcolOpenDocuments.Select(Function(o) o.Name).ToList))
                    lboolProceedToNextStep = False
                End If
                lstrLogContent.Append("Open Documents Count:" + lcolOpenDocuments.Count.ToString + Environment.NewLine)

                ''
                ''Get Open Markups from Document Version
                ''
                lstrLogContent.Append("Get open Document Markups" + Environment.NewLine)
                Tracing.Info(TracingTypes.Custom, "Get open Document Markups")

                If lboolValidateMarkups Then
                    lcolOpenMarkups = GetMarkupsFromLatestVersion()
                End If

                If lcolOpenMarkups.Count > 0 Then
                    lobjNotification.Interfaces("IHTCProjectHandoverNotificationDetails", True).Properties("HTCFailedDocumentsWithMarkups").SetValue(String.Join("|", lcolOpenMarkups.Select(Function(o) o.Name).ToList))
                    lboolProceedToNextStep = False
                End If
                lstrLogContent.Append("Open Document Markups Count:" + lcolOpenMarkups.Count.ToString + Environment.NewLine)



                ''
                ''If the performace is slow to commit the objects and rels,then the WF paramter can be passed as False to improve the project handover performace
                ''User can get the necessary details of all the Open Items from the notification object
                ''
                If lboolCreateprojectInfo Then

                    ''
                    ''Create Project Request Collection if it not exist already 
                    ''
                    Dim lobjProjectRequestCollection As IObject = Nothing
                    Dim lobjProjectRequestCount As New DynamicQuery()

                    lobjProjectRequestCount.Query.Criteria = New ObjectCriteria(Function(x) x.HasInterface("IHTCProjectRequestCollection"))

                    Dim lobjProjectRequestObject = lobjProjectRequestCount.ExecuteToIObjectDictionary
                    lstrLogContent.Append("Creating Project collection object" + Environment.NewLine)
                    Tracing.Info(TracingTypes.Custom, "Creating Project collection object")

                    If lobjProjectRequestObject IsNot Nothing AndAlso lobjProjectRequestObject.Count = 0 Then
                        lobjProjectRequestCollection = GeneralUtilities.InstantiateObject("HTCProjectRequestCollection", lobjProjectObject.Name, "", "", False)
                        lobjProjectRequestCollection.GetClassDefinition.FinishCreate(lobjProjectRequestCollection)
                        ''
                        ''Rel to Project
                        Dim lobjProjectToProjectCollectionRel = GeneralUtilities.InstantiateRelation("HTCProjectToProjectCollection", lobjProjectObject, lobjProjectRequestCollection, False)
                        lobjProjectToProjectCollectionRel.GetClassDefinition.FinishCreate(lobjProjectToProjectCollectionRel)
                    Else
                        lobjProjectRequestCollection = lobjProjectRequestObject.FirstOrDefault
                        lobjProjectRequestCollection.BeginUpdate()
                    End If
                    lstrLogContent.Append("Delete existing items from the project collection" + Environment.NewLine)
                    Tracing.Info(TracingTypes.Custom, "Delete existing items from the project collection")

                    ''
                    ''delete all the existing rels
                    DeleteExistingProjectCollectionRels(lobjProjectRequestCollection)
                    ''
                    ''Rel to Open TRs
                    lstrLogContent.Append("Creating rels between the open items" + Environment.NewLine)
                    Tracing.Info(TracingTypes.Custom, "Creating rels between the open items")
                    lstrLogContent.Append("Creating rels between the Open TRs to Project Collection" + Environment.NewLine)
                    With lcolOpenTranmittals.GetEnumerator
                        While .MoveNext
                            Dim lobjDocRel = GeneralUtilities.InstantiateRelation("HTCProjectCollectionToOpenTRsInProject", lobjProjectRequestCollection, .Value, False)
                            lobjDocRel.GetClassDefinition.FinishCreate(lobjDocRel)
                        End While
                    End With
                    ''
                    ''Rel to Open Docs
                    lstrLogContent.Append("Creating rels between the Open Documents and Project Collection" + Environment.NewLine)
                    With lcolOpenDocuments.GetEnumerator
                        While .MoveNext

                            Dim lobjDocRel = GeneralUtilities.InstantiateRelation("HTCProjectCollectionToOpenDocumentsInProject", lobjProjectRequestCollection, .Value, False)
                            lobjDocRel.GetClassDefinition.FinishCreate(lobjDocRel)
                        End While
                    End With
                    ''
                    ''Rel to Docs with Markups
                    lstrLogContent.Append("Creating rels between the Markups and Project Collection" + Environment.NewLine)
                    With lcolOpenMarkups.GetEnumerator
                        While .MoveNext
                            Dim lobjDocRel = GeneralUtilities.InstantiateRelation("HTCProjectCollectionToLatestDocumentRevisionsWithMarkups", lobjProjectRequestCollection, .Value, False)
                            lobjDocRel.GetClassDefinition.FinishCreate(lobjDocRel)
                        End While
                    End With

                    lobjProjectRequestCollection.FinishUpdate()
                End If



                lobjNotification.Interfaces("IHTCProjectHandoverNotificationDetails", True).Properties("HTCProjectHandoverLog").SetValue(lstrLogContent.ToString)
                lobjNotification.FinishUpdate()
                If Not lboolProceedToNextStep Then
                    Me.ProcessStepArgs.RejectStep = True
                End If
            Catch ex As Exception
                Tracing.Info(TracingTypes.Custom, "Error during project validation process with error:" + ex.InnerException.ToString)

                Throw New SPFException(2194, "Error during project validation process '$1'.", New String() {ex.Message}, ex.InnerException)

            Finally
                SPFRequestContext.Instance.ChangeServerConfigs(lobjCurrentCreateConfig, lobjCurrentQueryConfig)

            End Try

        End Sub

#End Region

#Region "Helper Methods"
        ''' <summary>
        ''' Gets all the open transmittals
        ''' </summary>
        ''' <returns></returns>
        Private Function GetOpenTransmittals(pstrdefaultTrStatus As String) As IObjectDictionary
            Dim lobjDynamicQueryForOpenTransmittals As New DynamicQuery()
            If Not String.IsNullOrWhiteSpace(pstrdefaultTrStatus) Then
                lobjDynamicQueryForOpenTransmittals.Query.Criteria = New ObjectCriteria(Function(x) x.HasInterface("IHTCTransmittal") AndAlso
                                                                                                x.Property("HTCTransmittalStatus") <> pstrdefaultTrStatus)
            Else
                lobjDynamicQueryForOpenTransmittals.Query.Criteria = New ObjectCriteria(Function(x) x.HasInterface("IHTCTransmittal") AndAlso
                                                                                                x.Property("HTCTransmittalStatus") <> "HTCENUM_Complete")
            End If


            Dim lcolOpenTranmittals = lobjDynamicQueryForOpenTransmittals.ExecuteToIObjectDictionary()
            Return lcolOpenTranmittals
        End Function
        ''' <summary>
        ''' Gets all the open documents
        ''' </summary>
        ''' <returns></returns>
        Private Function GetOpenDocuments(pstrDefaultDocStatus As String) As IObjectDictionary
            Dim lobjDynamicQueryForOpenDocuments As New DynamicQuery()
            If (Not String.IsNullOrWhiteSpace(pstrDefaultDocStatus)) Then

                lobjDynamicQueryForOpenDocuments.Query.Criteria = New ObjectCriteria(Function(x) x.HasInterface("IHTCDocumentRevision") AndAlso
                                                                                             (x.Property("SPFRevState") = "e1WORKING" OrElse x.Property("SPFRevState") = "e1CURRENT") AndAlso
                                                                                             x.Property("HTCDocumentReviewStatus") <> pstrDefaultDocStatus)
            Else
                lobjDynamicQueryForOpenDocuments.Query.Criteria = New ObjectCriteria(Function(x) x.HasInterface("IHTCDocumentRevision") AndAlso
                                                                                             (x.Property("SPFRevState") = "e1WORKING" OrElse x.Property("SPFRevState") = "e1CURRENT") AndAlso
                                                                                             x.Property("HTCDocumentReviewStatus") <> "HTCENUM_DS_CONFIRMED")
            End If

            Dim lcolOpenDocuments = lobjDynamicQueryForOpenDocuments.ExecuteToIObjectDictionary()
            Return lcolOpenDocuments
        End Function
        ''' <summary>
        ''' Gets all the pending markups on the latest document versions
        ''' </summary>
        ''' <returns></returns>

        Private Function GetMarkupsFromLatestVersion() As IObjectDictionary
            Dim lcolLatestDocRevisionsWithMarkups As IObjectDictionary = New ObjectDictionary
            Dim lobjDynamicQueryForOpenMarkupsOnLatestVersion As New DynamicQuery()

            lobjDynamicQueryForOpenMarkupsOnLatestVersion.Query.Criteria = New ObjectCriteria(Function(x) x.HasInterface("ISPFMarkupFile") AndAlso
                                                                                                          x.RelatedItem("EDG_HTCMarkupToDocumentRevision",
                                                                                                                      (Not ObjectCriteria.Property("SPFRevIssueDate").Exists OrElse ObjectCriteria.Property("SPFRevIssueDate").NullOrEmpty)))


            'lobjDynamicQueryForOpenMarkupsOnLatestVersion.Query.Criteria = New ObjectCriteria(Function(x) x.HasInterface("IHTCDocumentRevision") AndAlso ObjectCriteria.Property("SPFRevState").InList("e1CURRENT", "e1WORKING") AndAlso
            '                                                                                      Not ObjectCriteria.Property("SPFRevIssueDate").Exists OrElse ObjectCriteria.Property("SPFRevIssueDate").NullOrEmpty AndAlso
            '                                                                                              x.RelatedItem("EDG_HTCLatestDocumentRevisionWithMarkups", Function(ri1) ri1.HasInterface("ISPFObjClass")))
            Dim lcolOpenMarkups = lobjDynamicQueryForOpenMarkupsOnLatestVersion.ExecuteToIObjectDictionary()
            If lcolOpenMarkups IsNot Nothing AndAlso lcolOpenMarkups.Count > 0 Then
                lcolLatestDocRevisionsWithMarkups = lcolOpenMarkups.GetEnd1Relationships.GetRels("EDG_HTCMarkupToDocumentRevision").GetEnd2s()
            End If

            Return lcolLatestDocRevisionsWithMarkups
        End Function
        ''' <summary>
        ''' Removes the existing open items from the project collection
        ''' </summary>
        ''' <param name="pobjProjectCollection"></param>
        Private Sub DeleteExistingProjectCollectionRels(pobjProjectCollection As IObject)

            If pobjProjectCollection.GetEnd1Relationships.GetRels("HTCProjectCollectionToOpenTRsInProject").Count > 0 Then
                With pobjProjectCollection.GetEnd1Relationships.GetRels("HTCProjectCollectionToOpenTRsInProject").GetEnumerator
                    While .MoveNext
                        .Value.Delete()
                    End While
                End With
            End If

            If pobjProjectCollection.GetEnd1Relationships.GetRels("HTCProjectCollectionToOpenDocumentsInProject").Count > 0 Then
                With pobjProjectCollection.GetEnd1Relationships.GetRels("HTCProjectCollectionToOpenDocumentsInProject").GetEnumerator
                    While .MoveNext
                        .Value.Delete()
                    End While
                End With
            End If

            If pobjProjectCollection.GetEnd1Relationships.GetRels("HTCProjectCollectionToLatestDocumentRevisionsWithMarkups").Count > 0 Then
                With pobjProjectCollection.GetEnd1Relationships.GetRels("HTCProjectCollectionToLatestDocumentRevisionsWithMarkups").GetEnumerator
                    While .MoveNext
                        .Value.Delete()
                    End While
                End With
            End If
        End Sub
#End Region

    End Class
End Namespace