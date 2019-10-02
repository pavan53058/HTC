Option Explicit On
Option Strict On

Imports SPF.Server.Schema.Collections
Imports SPF.Server.Schema.Model
Imports SPF.Server.Schema.Interface.Generated
Imports SPF.Server.Context
Imports SPF.Server.QueryEngine
Imports SPF.Server.DataAccess
Imports SPF.Server.QueryClasses
Imports Intergraph.SPF.DAL.Common.Criteria.Object
Imports SPF.Server.Components.Core.Serialization
Imports System.Xml
Imports SPF.Server.Utilities
Imports SPF.Diagnostics
Imports System.IO

Namespace SPF.Server.Schema.Interface.Default
    Public Class IHTCDocumentRevisionDefault
        Inherits IHTCDocumentRevisionBase

#Region "Constructors"

        Public Sub New(ByVal pblnInstantiateRequiredItems As Boolean)
            MyBase.New(pblnInstantiateRequiredItems)
        End Sub
#End Region

#Region "Members"
        Private mIsDraftManUpdated As Boolean = False
        Private mIsFileAttached As Boolean = False
        Private mIsSectionStamped As Boolean = False
        Private mboolJobDocRelAddedHitAlready As Boolean = False
#End Region

#Region "Overrides"
        Public Overrides Sub OnRelationshipAdding(ByVal e As Model.RelEventArgs)

            If e IsNot Nothing AndAlso (e.Rel.DefUID = "HTCJobToDocument") Then
                Dim lobjJob = e.Rel.GetEnd2
                If lobjJob.GetEnd1Relationships.GetRel("HTCJobToEWRNmber") IsNot Nothing AndAlso
                        lobjJob.GetEnd1Relationships.GetRel("HTCJobToEWRNmber").GetEnd2 IsNot Nothing Then
                    MyBase.OnRelationshipAdding(e)

                Else
                    Throw New SPFException(99999, "No EWR Number related to EW/Job,Please relate EWR number to proceed")
                End If
            End If

        End Sub

        ''Also to use other service instead of this interface override to avoid lot of hits on the document rel overrides
        Public Overrides Sub OnRelationshipAdd(e As RelEventArgs)
            MyBase.OnRelationshipAdd(e)
            If e.Rel.DefUID = "HTCIncomingDocuments" Or e.Rel.DefUID = "HTCOutgoingDocuments" Then
                Dim lobjTransmittal As IObject = e.Rel.GetEnd1
                If lobjTransmittal IsNot Nothing Then
                    Dim lobjPICObject As IHTCDocumentRevision = CType(Me.ToInterface("IHTCDocumentRevision"), IHTCDocumentRevision)
                    ''Set the document status to "SUBMITTED" on attaching it to the Transmittal
                    Me.BeginUpdate()
                    Me.HTCDocumentReviewStatus = "HTCENUM_DS_SUBMITTED"
                    Me.FinishUpdate()
                End If
            End If
            Try
                If e.Rel.DefUID = "HTCJobToDocument" And Not mboolJobDocRelAddedHitAlready Then
                    mboolJobDocRelAddedHitAlready = True
                    ''Build Document object
                    Dim lobjDoc = BuildDocument(CType(Me, IObject))
                    Dim lobjEWRNumber As IObject = Nothing
                    Dim lobjJob = e.Rel.GetEnd2
                    If lobjJob.GetEnd1Relationships.GetRel("HTCJobToEWRNmber") IsNot Nothing AndAlso
                        lobjJob.GetEnd1Relationships.GetRel("HTCJobToEWRNmber").GetEnd2 IsNot Nothing Then
                        lobjEWRNumber = lobjJob.GetEnd1Relationships.GetRel("HTCJobToEWRNmber").GetEnd2

                        Dim lobjMasterObj = OnCreateDocument(lobjDoc, lobjEWRNumber.Name)

                        ''rel between new copied document master and existing document revision
                        Dim lobjMasterDocToRevisionDocRel As IObject = GeneralUtilities.InstantiateRelation("HTCDocumentToCopiedDocument", CType(Me, IObject), lobjMasterObj, False)
                        lobjMasterDocToRevisionDocRel.GetClassDefinition.FinishCreate(lobjMasterDocToRevisionDocRel)

                        ''Rel between job and new copied document master
                        Dim lobjJobMasterRel As IObject = GeneralUtilities.InstantiateRelation("HTCJobToCopiedDocument", lobjMasterObj, lobjJob, False)
                        lobjJobMasterRel.GetClassDefinition.FinishCreate(lobjJobMasterRel)
                        lobjJob.BeginUpdate()
                        lobjJob.Interfaces("IHTCJobDetails").Properties("HTCJobEWStatus").SetValue("HTCENUM_Changing")
                        lobjJob.FinishUpdate()
                    End If
                    ''Create Document from the selected document



                End If

            Catch ex As Exception
                ''SPFRequestContext.Instance.Transaction.Rollback()
                Tracing.Error(TracingTypes.Custom, ex)
                Throw ex
            End Try

        End Sub
#End Region

#Region "Private Methods"
        Private Function BuildDocument(pobjDoument As IObject) As DocumentObject
            Dim lobjDocumentObjectToReturn As New DocumentObject
            ''Revision
            lobjDocumentObjectToReturn.Revision = CType(pobjDoument.ToInterface("ISPFDocumentRevision"), ISPFDocumentRevision)

            ''Revision Scheme
            lobjDocumentObjectToReturn.RevScheme = CType(pobjDoument.ToInterface("ISPFDocumentRevision"), ISPFDocumentRevision).GetRevisionScheme
            ''Version
            lobjDocumentObjectToReturn.Version = CType(pobjDoument.ToInterface("ISPFDocumentRevision"), ISPFDocumentRevision).GetNewestVersion
            ''Master
            lobjDocumentObjectToReturn.Master = CType(pobjDoument.ToInterface("ISPFDocumentRevision"), ISPFDocumentRevision).GetDocumentMaster
            ''Master-Rev rel
            lobjDocumentObjectToReturn.MasterRevisionRel = CType(pobjDoument.ToInterface("ISPFDocumentRevision"), ISPFDocumentRevision).GetEnd2Relationships.GetRel("SPFDocumentRevisions")
            ''Revison-Version rel
            lobjDocumentObjectToReturn.RevisionVersionRel = lobjDocumentObjectToReturn.Version.GetEnd2Relationships.GetRel("SPFRevisionVersions")
            ''Primary Classification
            lobjDocumentObjectToReturn.MasterPrimaryClassification = lobjDocumentObjectToReturn.Master.GetEnd2Relationships.GetRel("SPFPrimaryClassification")
            ''Owning group rel
            lobjDocumentObjectToReturn.VersionOwningGroup = lobjDocumentObjectToReturn.Version.GetEnd1Relationships.GetRel("SPFItemOwningGroup")
            ''Master-Plant rel
            lobjDocumentObjectToReturn.DocMasterPlantRel = lobjDocumentObjectToReturn.Master.GetEnd2Relationships.GetRel("HTCPlantDocument")
            ''Master-Area Rel
            'lobjDocumentObjectToReturn.DocMasterAreaRel = lobjDocumentObjectToReturn.Master.GetEnd1Relationships.GetRel("HTCDocumentToArea")
            ''Revision-Plant Code Rel
            lobjDocumentObjectToReturn.DocRevisionPlantCodeRel = lobjDocumentObjectToReturn.Master.GetEnd1Relationships.GetRel("HTCDocumentPlantCode")
            ''Revision-Cost Center Rel
            lobjDocumentObjectToReturn.DocRevisionCostCenterRel = lobjDocumentObjectToReturn.Master.GetEnd1Relationships.GetRel("HTCDocumentCostCenter")
            ''Master-Unit Rel
            ''lobjDocumentObjectToReturn.DocMasterUnitRel = lobjDocumentObjectToReturn.Master.GetEnd1Relationships.GetRel("HTCDocumentToUnit")

            Return lobjDocumentObjectToReturn

        End Function

        Private Function OnCreateDocument(ByVal pobjDocObj As DocumentObject, pstrEWRNumber As String) As IObject
            Dim lobjDocumentObjectToReturn As New DocumentObject
            '
            ' Create the document revision
            '
            Dim lobjRevision As IObject = Nothing

            lobjRevision = GeneralUtilities.InstantiateObject("HTCDocumentRevision", pobjDocObj.Revision.Name & "-" & pstrEWRNumber, pobjDocObj.Revision.Description, "", False)

            lobjRevision.Interfaces("ISPFDocumentRevision").Properties("SPFMajorRevision").SetValue(pobjDocObj.Revision.SPFMajorRevision)
            ''lobjRevision.Interfaces("ISPFDocumentRevision").Properties("SPFMinorRevision").SetValue(pstrMinorRev)
            lobjRevision.GetClassDefinition.FinishCreate(lobjRevision)
            lobjDocumentObjectToReturn.Revision = CType(lobjRevision.ToInterface("ISPFDocumentRevision"), ISPFDocumentRevision)

            Dim lobjRevScheme As IObject = Nothing
            lobjRevScheme = pobjDocObj.Revision.GetEnd1Relationships.GetRel("SPFDocRevisionRevisionScheme").GetEnd2

            Dim lobjRevSchemeRel As IObject = Nothing
            lobjRevSchemeRel = GeneralUtilities.InstantiateRelation("SPFDocRevisionRevisionScheme", lobjRevision, lobjRevScheme, False)
            lobjRevSchemeRel.GetClassDefinition.FinishCreate(lobjRevSchemeRel)
            lobjDocumentObjectToReturn.RevScheme = CType(lobjRevScheme.Interfaces("ISPFRevisionScheme"), ISPFRevisionScheme)
            '
            ' Create the version
            '
            Dim lobjVersion As IObject = Nothing

            lobjVersion = GeneralUtilities.InstantiateObject("HTCDocumentVersion", pobjDocObj.Revision.Name & "-" & pstrEWRNumber, pobjDocObj.Revision.Description, "DOC", False)

            lobjVersion.Interfaces("ISPFDocumentVersion").Properties("SPFDocVersion").SetValue("1")
            lobjVersion.Interfaces("ISPFDocumentVersion").Properties("SPFIsDocVersionSuperseded").SetValue("False")
            lobjVersion.GetClassDefinition.FinishCreate(lobjVersion)
            lobjDocumentObjectToReturn.Version = CType(lobjVersion.ToInterface("ISPFDocumentVersion"), ISPFDocumentVersion)
            '
            ' Create the access group relationship
            '
            Dim lobjOwningGroup As IObject = Nothing
            lobjOwningGroup = pobjDocObj.Version.GetEnd1Relationships.GetRel("SPFItemOwningGroup").GetEnd2
            Dim lobjOwningGroupRel As IObject = Nothing
            lobjOwningGroupRel = GeneralUtilities.InstantiateRelation("SPFItemOwningGroup", lobjVersion, lobjOwningGroup, False)
            lobjOwningGroupRel.GetClassDefinition.FinishCreate(lobjOwningGroupRel)
            '
            ' Create the master
            '
            Dim lobjMaster As IObject = Nothing

            lobjMaster = GeneralUtilities.InstantiateObject("HTCDocumentMaster", pobjDocObj.Revision.Name & "-" & pstrEWRNumber, pobjDocObj.Revision.Description, "DOC", False)

            lobjMaster.Interfaces("ISPFDocument").Properties("SPFDocState").SetValue("e1DocStateRESERVED")
            lobjMaster.GetClassDefinition.FinishCreate(lobjMaster)
            lobjDocumentObjectToReturn.Master = CType(lobjMaster.ToInterface("ISPFDocumentMaster"), ISPFDocumentMaster)
            '
            ' Find and related the classification
            '
            Dim lobjClassification As IObject = Nothing
            lobjClassification = CType(pobjDocObj.Master.ToInterface("ISPFClassifiedItem"), ISPFClassifiedItem).GetPrimaryClassification
            Dim lobjClassRel As IObject = Nothing
            lobjClassRel = GeneralUtilities.InstantiateRelation("SPFPrimaryClassification", lobjClassification, lobjMaster, False)
            lobjClassRel.GetClassDefinition.FinishCreate(lobjClassRel)

            '
            ' Relate the Master to the revision
            '
            Dim lobjMasterRevRel As IObject = Nothing
            lobjMasterRevRel = GeneralUtilities.InstantiateRelation("SPFDocumentRevisions", lobjMaster, lobjRevision, False)
            lobjMasterRevRel.GetClassDefinition.FinishCreate(lobjMasterRevRel)
            lobjDocumentObjectToReturn.MasterRevisionRel = CType(lobjMasterRevRel.Interfaces("IRel"), IRel)
            '
            ' Relate the revision to the version
            '
            Dim lobjRevVerRel As IObject = Nothing
            lobjRevVerRel = GeneralUtilities.InstantiateRelation("SPFRevisionVersions", lobjRevision, lobjVersion, False)
            lobjRevVerRel.GetClassDefinition.FinishCreate(lobjRevVerRel)
            lobjDocumentObjectToReturn.RevisionVersionRel = CType(lobjRevVerRel.Interfaces("IRel"), IRel)
            '
            'Create Cost Center rel
            '
            Dim lobjCostCenter As IObject = Nothing
            If pobjDocObj.Revision.GetEnd1Relationships.GetRel("HTCDocumentCostCenter") IsNot Nothing Then
                lobjCostCenter = pobjDocObj.Revision.GetEnd1Relationships.GetRel("HTCDocumentCostCenter").GetEnd2
                Dim lobjlobjCostCenterRel As IObject = Nothing
                lobjlobjCostCenterRel = GeneralUtilities.InstantiateRelation("HTCDocumentCostCenter", lobjRevision, lobjCostCenter, False)
                lobjlobjCostCenterRel.GetClassDefinition.FinishCreate(lobjlobjCostCenterRel)
            End If

            '
            'Create Plant code rel
            '
            Dim lobjPlantCode As IObject = Nothing
            If pobjDocObj.Revision.GetEnd1Relationships.GetRel("HTCDocumentPlantCode") IsNot Nothing Then
                lobjPlantCode = pobjDocObj.Revision.GetEnd1Relationships.GetRel("HTCDocumentPlantCode").GetEnd2
                Dim lobjlobjlobjPlantCodeRel As IObject = Nothing
                lobjlobjlobjPlantCodeRel = GeneralUtilities.InstantiateRelation("HTCDocumentPlantCode", lobjRevision, lobjPlantCode, False)
                lobjlobjlobjPlantCodeRel.GetClassDefinition.FinishCreate(lobjlobjlobjPlantCodeRel)
            End If

            '
            'Create EWR rel
            '
            Dim lobjEWR As IObject = Nothing
            If pobjDocObj.Revision.GetEnd1Relationships.GetRel("HTCDocumentEWRNumber") IsNot Nothing Then
                lobjEWR = pobjDocObj.Revision.GetEnd1Relationships.GetRel("HTCDocumentEWRNumber").GetEnd2
                Dim lobjlobjlobjEWRRel As IObject = Nothing
                lobjlobjlobjEWRRel = GeneralUtilities.InstantiateRelation("HTCDocumentEWRNumber", lobjRevision, lobjEWR, False)
                lobjlobjlobjEWRRel.GetClassDefinition.FinishCreate(lobjlobjlobjEWRRel)
            End If
            '
            'loop through the available files and attach them to the newly created document
            '
            If pobjDocObj.Version.GetEnd2Relationships.GetRels("SPFFileComposition") IsNot Nothing AndAlso pobjDocObj.Version.GetEnd2Relationships.GetRels("SPFFileComposition").GetEnd1s.Count > 0 Then
                Dim lcolFiles = pobjDocObj.Version.GetEnd2Relationships.GetRels("SPFFileComposition").GetEnd1s

                CreateAndAttachInfoFile(lobjVersion, lcolFiles)

            End If
            '
            ' Return the document object
            '
            Return lobjMaster
        End Function

        Private Sub CreateAndAttachInfoFile(pobjVersion As IObject, pobjFiles As IObjectDictionary)
            '
            ' Get the local file name and directory
            '
            With pobjFiles.GetEnumerator
                While .MoveNext
                    Dim lobjFile = CType(.Value.Interfaces("ISPFFile"), ISPFFile)
                    Dim lobjFileType As ISPFFileType = CType(lobjFile.GetFileTypes.Item(0).ToInterface("ISPFFileType"), ISPFFileType)
                    Dim lobjFileInfo As FileInfo = New FileInfo(lobjFile.DownloadToTempDir())

                    ''Dim lstrInfoFileDir As String = System.IO.Path.GetTempPath
                    Dim lstrInfoFileName As String = .Value.Name

                    Dim lstrOutputFilePath As String = System.IO.Path.Combine(lobjFileInfo.DirectoryName, lstrInfoFileName)
                    '
                    ' Instantiate an fileinfo Object
                    '
                    Tracing.Info(TracingTypes.Custom, "Creating the info file with name : " & lstrInfoFileName)
                    Dim lobjNewFile As IObject = SPF.Server.Utilities.GeneralUtilities.InstantiateObject("SPFDesignFile", lobjFile.Name, lobjFile.Description, "", False)
                    CType(lobjNewFile.Interfaces("ISPFFile"), ISPFFile).SPFLocalDirectory = lobjFileInfo.DirectoryName
                    CType(lobjNewFile.Interfaces("ISPFFile"), ISPFFile).SPFLocalFileName = lstrInfoFileName
                    CType(lobjNewFile.Interfaces("ISPFBusinessFile"), ISPFBusinessFile).SPFEditInd = False
                    CType(lobjNewFile.Interfaces("ISPFBusinessFile"), ISPFBusinessFile).SPFViewInd = True
                    lobjNewFile.GetClassDefinition.FinishCreate(lobjNewFile)
                    Tracing.Info(TracingTypes.Custom, "Created the info file")
                    '
                    ' Create a rel between the owner object and the  file
                    '
                    Dim lobjRel As IObject = SPF.Server.Utilities.GeneralUtilities.InstantiateRelation("SPFFileComposition", lobjNewFile, pobjVersion, False)
                    lobjRel.GetClassDefinition.FinishCreate(lobjRel)
                    Tracing.Info(TracingTypes.Custom, "Instantiated the relationship SPFFileComposition between the  document and the  info file")
                    '
                    ' Save the info as a file in the server view dir
                    '
                    Tracing.Info(TracingTypes.Custom, "Getting vault for the info file")
                    '
                    ' Get the vault from the file attaching object
                    '
                    Dim lobjVault As ISPFVault = CType(pobjVersion.ToInterface("ISPFFileComposition"), ISPFFileComposition).GetVault()

                    If Not lobjVault Is Nothing Then
                        '
                        ' Get the host
                        '
                        Dim lobjHost As ISPFHost = CType(lobjVault.ToInterface("ISPFVault"), ISPFVault).GetHost

                        Dim lobjFileService As SPF.Server.Components.FileService.ISPFFileService = SPF.Server.Components.FileService.SPFFileService.Create
                        Tracing.Info(TracingTypes.Custom, "Uploading the file...")
                        lobjFileService.UploadFileFromURL(New IO.FileInfo(lstrOutputFilePath), lobjHost.Name)
                        Tracing.Info(TracingTypes.Custom, "Uploaded the file")
                        '
                        ' Move the file to the vault
                        '
                        CType(lobjNewFile.ToInterface("ISPFFile"), ISPFFile).MoveFileToVault(lobjVault)
                        Tracing.Info(TracingTypes.Custom, "File " & lobjNewFile.Name & " is moved to vault")
                        '
                        ' Create the file vault rel
                        '
                        Dim lobjFileVaultRel As IObject = SPF.Server.Utilities.GeneralUtilities.InstantiateRelation("SPFFileVault", lobjNewFile, lobjVault, False)
                        lobjFileVaultRel.GetClassDefinition.FinishCreate(lobjFileVaultRel)
                        Tracing.Info(TracingTypes.Custom, "SPFFileVault relationship has been created")
                        '
                        ' Get the SPFFileType
                        '

                        Dim lobjFileFileTypeRel As IObject = SPF.Server.Utilities.GeneralUtilities.InstantiateRelation("SPFFileFileType", lobjNewFile, lobjFileType, False)
                        lobjFileFileTypeRel.GetClassDefinition.FinishCreate(lobjFileFileTypeRel)

                        Tracing.Info(TracingTypes.Custom, "SPFFileFileType has been created")
                    End If
                End While
            End With
        End Sub
#End Region

    End Class
    Public Class DocumentObject
        Public Master As ISPFDocumentMaster
        Public Revision As ISPFDocumentRevision
        Public Version As ISPFDocumentVersion

        Public MasterRevisionRel As IRel
        Public DocMasterUnitRel As IRel
        Public MasterPrimaryClassification As IRel
        Public DocMasterPlantRel As IRel
        Public DocMasterAreaRel As IRel
        Public RevScheme As ISPFRevisionScheme

        Public RevisionVersionRel As IRel
        Public DocRevisionPlantCodeRel As IRel
        Public DocRevisionCostCenterRel As IRel
        Public DocRevisionJobRel As IRel
        Public VersionOwningGroup As IRel

        Public Iterator Function AllObjectsAndRels() As IEnumerable(Of IObject)
            If Master IsNot Nothing Then
                Yield Master
            End If
            If Revision IsNot Nothing Then
                Yield Revision
            End If
            If Version IsNot Nothing Then
                Yield Version
            End If
            If MasterRevisionRel IsNot Nothing Then
                Yield MasterRevisionRel
            End If
            If RevisionVersionRel IsNot Nothing Then
                Yield RevisionVersionRel
            End If
        End Function
    End Class
End Namespace


