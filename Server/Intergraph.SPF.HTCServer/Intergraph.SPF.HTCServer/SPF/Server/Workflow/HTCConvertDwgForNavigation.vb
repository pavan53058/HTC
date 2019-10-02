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

Namespace SPF.Server.Components.Workflow.ProcessSteps
    Public Class HTCConvertDwgForNavigation
        Inherits ProcessStepBase
#Region "Constructors"
        Public Sub New(ByVal pobjStep As ISPFObjectWorkflowStep, ByVal pobjProcessStepArgs As ProcessStepArgs)
            MyBase.New(pobjStep, pobjProcessStepArgs)
        End Sub
#End Region

#Region "Methods"
        Public Overrides Sub Execute()
            ConvertFileForNavigation()
        End Sub


        Public Sub ConvertFileForNavigation()

            Dim lobjFileObject As IObject = Nothing
            Dim lobjWFObject As IObject = Nothing
            Try
                Tracing.Info(TracingTypes.Custom, "Get the Workflow object (HTCDocumentversion)..........")
                'Get the starting Object(i.e Document Version in this case)
                Dim lobjWorkflow As ISPFWorkflow = CType(CType(Me.StartingObj.ToInterface("ISPFObjectWorkflowStep"), ISPFObjectWorkflowStep).GetWorkflow(False).ToInterface("ISPFWorkflow"), ISPFWorkflow)
                If lobjWorkflow IsNot Nothing AndAlso lobjWorkflow.GetEnd2Relationships.Count <> 0 Then
                    If lobjWorkflow.GetEnd2Relationships.GetRel("SPFItemWorkflow") IsNot Nothing Then
                        lobjWFObject = lobjWorkflow.GetEnd2Relationships.GetRel("SPFItemWorkflow").GetEnd1
                    End If
                End If

                Tracing.Info(TracingTypes.Custom, "Get the Smart Convertable View File..........")
                If lobjWFObject.IsTypeOf("IHTCDocumentRevision") Then
                    Dim lobjDoc As IObject = lobjWFObject.GetEnd1Relationships.GetRel("SPFRevisionVersions").GetEnd2
                    Dim lobjFileComposition As ISPFFileComposition = CType(lobjDoc.Interfaces("ISPFFileComposition"), ISPFFileComposition)

                    Dim lcolFiles As IObjectDictionary = lobjFileComposition.GetAllFiles
                    If lcolFiles IsNot Nothing And lcolFiles.Count > 0 Then
                        With lcolFiles.GetEnumerator
                            While .MoveNext
                                Dim lobjfile As ISPFFile = CType(.Value.Interfaces("ISPFFile"), ISPFFile)

                                Dim isfileConvertable As Boolean = IsFileSmartConvertable(lobjfile)
                                If isfileConvertable Then
                                    lobjFileObject = .Value

                                    If Not lobjFileObject Is Nothing Then
                                        Tracing.Info(TracingTypes.Custom, "Add ISPFNavigationFileComposition interface if missing from Publish File Object..........")
                                        If Not lobjFileObject.IsTypeOf("ISPFNavigationFileComposition") Then
                                            Dim lstrInterfaceName As String = "ISPFNavigationFileComposition"

                                            ' Add ISPFNavigationFileComposition interface if necessary                    
                                            If lobjFileObject.GetClassDefinition.GetRealizedInterfaceDefs.Contains(lstrInterfaceName) Then
                                                If lobjFileObject.Interfaces.Contains(lstrInterfaceName) = False Then
                                                    Dim lobjClaimObject As IObject = SchemaUtilities.BeginUpdateWithImplicitClaim(lobjFileObject)
                                                    Dim lobjInterface As IInterface = lobjClaimObject.Interfaces(lstrInterfaceName)
                                                    If lobjFileObject.IsTypeOf("ISPFFile") Then
                                                        CType(lobjFileObject.Interfaces("ISPFFile"), ISPFFileDefault).SPFDoNotRevault = True
                                                    End If
                                                    lobjClaimObject.FinishUpdate()
                                                    Tracing.Info(TracingTypes.Custom, "Added ISPFNavigationFileComposition interface on Publish File Object..........")
                                                End If
                                            End If
                                        End If

                                        Dim lobjISPFNavigationFileComposition As ISPFNavigationFileComposition = CType(lobjFileObject.ToInterface("ISPFNavigationFileComposition"), ISPFNavigationFileComposition)

                                        '
                                        ' Call GenerateNavigationFile for this file object. 
                                        '
                                        Tracing.Info(TracingTypes.Custom, "Start : Generate Navigation File..........")
                                        Dim lblnSts As Boolean = lobjISPFNavigationFileComposition.GenerateNavigationFile(True)
                                        'UpdateGraphicsMapFile(False)

                                        If Not lblnSts Then
                                            Tracing.Error(TracingTypes.Custom, New Exception("SmartConverter failed to convert the file for navigation.  Verify that SmartSketch is installed and configured properly on the SmartPlant Foundation server."))
                                            Throw New SPFException(1703, "SmartConverter failed to convert the file for navigation.  Verify that SmartSketch is installed and configured properly on the SmartPlant Foundation server.")
                                        End If
                                        Tracing.Info(TracingTypes.Custom, "End : Generated Navigation File..........")
                                    End If
                                End If
                            End While
                        End With
                    End If
                End If


            Catch lobjSPFException As SPFException
                If (lobjSPFException.Number = 1665) AndAlso
                lobjSPFException.Message.Contains("ISPFNavigationFileComposition") Then
                    Tracing.Error(TracingTypes.Custom, New Exception("The file '" & lobjFileObject.Name & "' is being converted by another user. Please try Convert for Navigation again"))
                    Throw New SPFException(415713465, "The file '$1' is being converted by another user. Please try Convert for Navigation again", New String() {lobjFileObject.Name})
                Else
                    Throw lobjSPFException
                End If
            Catch lobjException As Exception
                Tracing.Error(TracingTypes.Custom, lobjException)
                Throw lobjException
            End Try
        End Sub

        Private Function IsFileSmartConvertable(ByVal pobjFile As ISPFFile) As Boolean
            Dim lblnStatus As Boolean = True

            If pobjFile IsNot Nothing Then
                Dim lobjFileType As IObject = pobjFile.GetFileTypes.Item(0)
                If lobjFileType IsNot Nothing Then
                    Select Case lobjFileType.UID
                        Case "FT_dwg", "FT_dgn", "FT_dxf", "FT_igr", "FT_pid", "FT_sha", "FT_spe", "FT_zyq"
                            lblnStatus = True
                        Case Else
                            lblnStatus = False
                    End Select
                End If
            End If

            Return lblnStatus
        End Function

#End Region

#Region "Functions"

        Private Function IsVisualFile(ByVal pobjFile As ISPFFile) As Boolean
            Dim lblnStatus As Boolean = True

            If pobjFile IsNot Nothing Then
                Dim lobjFileType As IObject = pobjFile.GetFileTypes.Item(0)
                If lobjFileType IsNot Nothing Then
                    Select Case lobjFileType.UID
                        Case "FT_dwg", "FT_dgn", "FT_dxf", "FT_igr", "FT_pid", "FT_sha", "FT_spe", "FT_zyq"
                            lblnStatus = True
                        Case Else
                            lblnStatus = False
                    End Select
                End If
            End If

            Return lblnStatus
        End Function

#End Region
    End Class
End Namespace