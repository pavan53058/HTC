Imports System.Text
Imports Intergraph.SPF.DAL.Common.Criteria.Object
Imports SPF.Server.Context
Imports SPF.Server.QueryClasses
Imports SPF.Server.Schema.Collections
Imports SPF.Server.Schema.Interface.Generated
Imports SPF.Server.Schema.Model
Imports SPF.Server.Utilities

Namespace SPF.Server.Components.Workflow.ProcessSteps
    ''' <summary>
    ''' This process step takes document inputs.
    ''' Navigates to Tax text,splits the tags and run query to find the available tags in the system
    ''' if a tag exist in the system then it relates to the current document 
    ''' </summary>
    Public Class HTCExtractTagTextFromDocument
        Inherits ProcessStepBase
        Public Sub New(ByVal pobjStep As ISPFObjectWorkflowStep, ByVal pobjProcessStepArgs As ProcessStepArgs)
            MyBase.New(pobjStep, pobjProcessStepArgs)
        End Sub


        Public Overrides Sub Execute()
            Try
                Dim lobjStartingObject As IObject = Nothing
                Dim lobjWorkflow As ISPFWorkflow = CType(CType(Me.StartingObj.ToInterface("ISPFObjectWorkflowStep"), ISPFObjectWorkflowStep).GetWorkflow().ToInterface("ISPFWorkflow"), ISPFWorkflow)

                lobjStartingObject = lobjWorkflow.GetEnd2Relationships.GetRel("SPFItemWorkflow").GetEnd1
                Dim lstrDocumentTagText = String.Empty
                Dim lstrLogContent As New StringBuilder
                Dim lobjNotification As IObject = Nothing
                ''
                ''Prepare a notification object to track all the tags that are being processed
                ''
                Dim lstrName As String = lobjStartingObject.Name & "-" & SPFRequestContext.Instance.RequestSettings.UserName & "-" & DateTime.Now.ToString
                Dim lstrDescription As String = "Tag extraction notification"
                Dim lobjCommonUtilities As New CommonUtilities
                lobjNotification = lobjCommonUtilities.CreateNotificationObject(lstrName, lstrDescription)

                lobjNotification.Interfaces("ISPFContextualNotification", True).Properties("SPFContextObjectOBID", True).SetValue(lobjStartingObject.OBID)
                lobjNotification.Interfaces("ISPFContextualNotification", True).Properties("SPFContextObjectDomainUID", True).SetValue(lobjStartingObject.DomainUID)

                lobjNotification.BeginUpdate()

                lstrLogContent.Append("Initiating Tag Extraction" + Environment.NewLine)
                lstrLogContent.Append(Environment.NewLine + Environment.NewLine)

                If lobjStartingObject.Interfaces("IHTCDocumentRevision") IsNot Nothing AndAlso
                            lobjStartingObject.Interfaces("IHTCDocumentRevision").Properties("HTCDocRevisionTags") IsNot Nothing AndAlso
                            lobjStartingObject.Interfaces("IHTCDocumentRevision").Properties("HTCDocRevisionTags").Value IsNot Nothing AndAlso
                            Not String.IsNullOrWhiteSpace(lobjStartingObject.Interfaces("IHTCDocumentRevision").Properties("HTCDocRevisionTags").ToDisplayValue) Then

                    lstrDocumentTagText = lobjStartingObject.Interfaces("IHTCDocumentRevision").Properties("HTCDocRevisionTags").ToDisplayValue
                    If Not String.IsNullOrEmpty(lstrDocumentTagText) Then
                        Dim lstTagsFromDocumentString As List(Of String) = lstrDocumentTagText.Split(",").ToList

                        '' lstrLogContent.Append("Extracted tag list:" & lstrDocumentTagText + Environment.NewLine)

                        Dim lobjTagQuery As New DynamicQuery
                        lobjTagQuery.Query.Criteria = New ObjectCriteria(Function(x) x.HasInterface("IHTCTag") And x.Name.InList(lstTagsFromDocumentString))
                        Dim lcolTags = lobjTagQuery.ExecuteToIObjectDictionary
                        Dim lstTagsAlreadyInSystem As New List(Of String)(lcolTags.Select(Function(o) o.Name).ToList)
                        ''
                        ''Run the except to get the tags that are missing in system
                        ''
                        Dim lstMissingTags = lstTagsFromDocumentString.Except(lstTagsAlreadyInSystem)

                        With lcolTags.GetEnumerator
                            While .MoveNext
                                Dim lobjtag = .Value
                                lstrLogContent.Append(Environment.NewLine)
                                lstrLogContent.Append("Validating the Tag to relate:" & lobjtag.Name + Environment.NewLine)

                                If Not (lobjStartingObject.GetEnd1Relationships.GetRels("HTCTagDocument") IsNot Nothing AndAlso
                                lobjStartingObject.GetEnd1Relationships.GetRels("HTCTagDocument").GetEnd2s.Contains(lobjtag)) Then
                                    Dim lobjRevisionDocToTagRel As IObject = GeneralUtilities.InstantiateRelation("HTCTagDocument", lobjStartingObject, lobjtag, False)
                                    lobjRevisionDocToTagRel.GetClassDefinition.FinishCreate(lobjRevisionDocToTagRel)
                                    lstrLogContent.Append("Relating Tag:" & lobjtag.Name + Environment.NewLine)
                                    lstrLogContent.Append(Environment.NewLine)
                                Else
                                    lstrLogContent.Append("Tag Already Attached to Document" & lobjtag.Name + Environment.NewLine)

                                End If

                            End While
                        End With

                        For Each Tagname As String In lstMissingTags
                            lstrLogContent.Append(Environment.NewLine)
                            lstrLogContent.Append("Missing Tag in system:" & Tagname + Environment.NewLine)
                        Next
                        ''
                        ''if all the tags are extracted and related to document then set the "Extracted" status orelse set "Partially Extracted" status
                        ''
                        If lstMissingTags.Count > 0 Then
                            lobjNotification.Interfaces("IHTCExtractTagNotification", True).Properties("HTCTagExtractedStatus").SetValue("HTCENUM_Partially_Extracted")
                        Else
                            lobjNotification.Interfaces("IHTCExtractTagNotification", True).Properties("HTCTagExtractedStatus").SetValue("HTCENUM_Extracted")
                        End If

                    End If
                Else
                    lstrLogContent.Append("No Tag text to extract" + Environment.NewLine)
                End If

                lobjNotification.Interfaces("IHTCExtractTagNotification", True).Properties("HTCTagExtractionDetailedLog").SetValue(lstrLogContent.ToString)

                lobjNotification.FinishUpdate()
            Catch ex As Exception
                Throw New SPFException(2194, "Error during Tag extraction process '$1'.", New String() {ex.Message}, ex.InnerException)
            End Try

        End Sub


    End Class

End Namespace

