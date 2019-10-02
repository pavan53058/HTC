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
Imports DocumentFormat.OpenXml.Packaging
Imports org.apache.pdfbox.pdmodel
Imports org.apache.pdfbox.util
'Imports DocumentFormat.OpenXml.Packaging
'Imports Org.apache.pdfbox.pdmodel
'Imports Org.apache.pdfbox.util

Namespace SPF.Server.Components.Workflow.ProcessSteps
    Public Class HTCUpdateGraphicsMapFile
        Inherits ProcessStepBase
#Region "Constructors"
        Public Sub New(ByVal pobjStep As ISPFObjectWorkflowStep, ByVal pobjProcessStepArgs As ProcessStepArgs)
            MyBase.New(pobjStep, pobjProcessStepArgs)
        End Sub
#End Region

#Region "Constants"
        Private mDefaultPrimaryClsfn As String = "Document classifications"
#End Region

#Region "Methods"
        Public Overrides Sub Execute()
            ''UpdateGraphicsMapFile(False)
            UpdateGraphicsMapFile(True)
        End Sub


        Public Sub UpdateGraphicsMapFile(pboolIsUpdateAfterTagCreation As Boolean)
            Dim lobjFileObject As IObject = Nothing
            Dim lobjWFObject As IObject = Nothing

            Try
                Tracing.Info(TracingTypes.Custom, "Get the Workflow object (HTCDocumentversion)..........")
                'Get the starting Object(i.e HTCDocumentversion Document Version in this case)
                Dim lobjWorkflow As ISPFWorkflow = CType(CType(Me.StartingObj.ToInterface("ISPFObjectWorkflowStep"), ISPFObjectWorkflowStep).GetWorkflow(False).ToInterface("ISPFWorkflow"), ISPFWorkflow)
                If lobjWorkflow IsNot Nothing AndAlso lobjWorkflow.GetEnd2Relationships.Count <> 0 Then
                    If lobjWorkflow.GetEnd2Relationships.GetRel("SPFItemWorkflow") IsNot Nothing Then
                        lobjWFObject = lobjWorkflow.GetEnd2Relationships.GetRel("SPFItemWorkflow").GetEnd1
                    End If
                End If

                Tracing.Info(TracingTypes.Custom, "Get the Visual File..........")
                If lobjWFObject.IsTypeOf("IHTCDocumentRevision") Then
                    Dim lobjVersionDoc As IObject = lobjWFObject.GetEnd1Relationships.GetRel("SPFRevisionVersions").GetEnd2

                    Dim lobjFileComposition As ISPFFileComposition = CType(lobjVersionDoc.Interfaces("ISPFFileComposition"), ISPFFileComposition)
                    Dim lcolFiles As IObjectDictionary = lobjFileComposition.GetAllFiles
                    If lcolFiles IsNot Nothing And lcolFiles.Count > 0 Then
                        With lcolFiles.GetEnumerator
                            While .MoveNext
                                Dim lobjfile As ISPFFile = CType(.Value.Interfaces("ISPFFile"), ISPFFile)
                                Dim FileType As String = IsVisualFile(lobjfile)
                                lobjFileObject = .Value
                                If FileType = "VisualFile" Then
                                    If Not lobjFileObject Is Nothing Then

                                        Tracing.Info(TracingTypes.Custom, "Navigate and Get the Graphics Map File..........")
                                        Dim lobjISPFNavigationFileComposition As ISPFNavigationFileComposition = CType(lobjFileObject.ToInterface("ISPFNavigationFileComposition"), ISPFNavigationFileComposition)
                                        Dim lobjNavigationFile As ISPFNavigationFile = lobjISPFNavigationFileComposition.GetNavigationFile
                                        'Map File generated from Convert for navigation Method
                                        Dim lobjMapFile As ISPFGraphicsMap = lobjNavigationFile.GetGraphicsMapFile

                                        Tracing.Info(TracingTypes.Custom, "Download Graphics Map File to temp Directory..........")
                                        'Download graphics file to temp Directory
                                        Dim lstrtepDirectory As String = System.IO.Path.GetTempPath

                                        Dim lstrMapFilePath As String = lobjMapFile.DownloadToTempDir

                                        Tracing.Info(TracingTypes.Custom, "Update Graphics Map File..........")
                                        Dim GraphicMapDocument As XDocument = XDocument.Load(lstrMapFilePath)


                                        ''Dim lxmlNodelist As XmlNodeList = GraphicMapDocument.SelectNodes("//Mappings/Mapping/Maps/Map")


                                        Tracing.Info(TracingTypes.Custom, "Update UIDs for each Tag in Graphics Map File..........")
                                        Dim lstTags As List(Of String) = New List(Of String)
                                        'Create UIDs for Each Tag
                                        Tracing.Info(TracingTypes.Custom, "Collect Tags..........")
                                        For Each lxElement As XElement In GraphicMapDocument.Descendants("Map")
                                            lxElement.Attribute("IDef").Value = "IHTCTag"
                                            lstTags.Add(lxElement.Attribute("Name").Value)
                                        Next

                                        ''Get all the matching tags in the system
                                        Dim lobjTagQuery As New DynamicQuery
                                        Dim lcolTags As IObjectDictionary = Nothing
                                        If lstTags.Count > 0 Then
                                            lobjTagQuery.Query.Criteria = New ObjectCriteria(Function(x) x.HasInterface("IHTCTag") And x.Name.InList(lstTags))
                                            lcolTags = lobjTagQuery.ExecuteToIObjectDictionary
                                        End If
                                        If lcolTags IsNot Nothing AndAlso lcolTags.Count > 0 Then
                                            Tracing.Info(TracingTypes.Custom, "Looping through the found tags in the system..........")
                                            With lcolTags.GetEnumerator
                                                While .MoveNext
                                                    Dim lobjTag = .Value
                                                    Tracing.Info(TracingTypes.Custom, "Update Graphic map with the Tag UID..........")
                                                    ''Dim lxDoc As XDocument = XDocument.Load(lstrMapFilePath)
                                                    ''TODO--chances of getting multiple results with the same name??
                                                    Dim lxEleMatchingNode As IEnumerable(Of XElement) = GraphicMapDocument.Descendants("Map").Where(Function(x) CStr(x.Attribute("Name")) = lobjTag.Name)
                                                    Tracing.Info(TracingTypes.Custom, "Processing Tag:" & lobjTag.Name)
                                                    ''Dim lxmlMapNode As XmlNodeList = GraphicMapDocument.SelectNodes("//Mappings/Mapping/Maps/Map[@Name=" & lobjTag.Name & "]")
                                                    For Each lxmlElement As XElement In lxEleMatchingNode
                                                        Dim lxAttUID As XAttribute = New XAttribute("UID", lobjTag.UID)
                                                        lxmlElement.Add(lxAttUID)
                                                    Next

                                                    If (lobjWFObject.GetEnd1Relationships.GetRels("HTCTagDocument") Is Nothing) Then
                                                        Dim lobjRevisionDocToTagRel As IObject = GeneralUtilities.InstantiateRelation("HTCTagDocument", lobjWFObject, lobjTag, False)
                                                        lobjRevisionDocToTagRel.GetClassDefinition.FinishCreate(lobjRevisionDocToTagRel)

                                                    ElseIf (lobjWFObject.GetEnd1Relationships.GetRels("HTCTagDocument") IsNot Nothing AndAlso
                                                   Not lobjWFObject.GetEnd1Relationships.GetRels("HTCTagDocument").GetEnd2s.ContainsByObid(lobjTag.OBID)) Then

                                                        Dim lobjRevisionDocToTagRel As IObject = GeneralUtilities.InstantiateRelation("HTCTagDocument", lobjWFObject, lobjTag, False)
                                                        lobjRevisionDocToTagRel.GetClassDefinition.FinishCreate(lobjRevisionDocToTagRel)
                                                    End If

                                                End While
                                            End With
                                        End If


                                        Dim lstrNewMapFilePath As String = lstrtepDirectory & lobjMapFile.Name.Replace(".xml", "") & ".xml"

                                        Tracing.Info(TracingTypes.Custom, "Save Graphics Map File..........")
                                        GraphicMapDocument.Save(lstrNewMapFilePath)

                                        'Create Tags from updated Graphic Map File if tag patterns exist in SPF
                                        'If pboolIsUpdateAfterTagCreation = False Then
                                        '    MatchTagPattern(GraphicMapDocument, lobjWFObject)
                                        'End If

                                        'Delete GraphicsMap Files and Create new Map File
                                        Tracing.Info(TracingTypes.Custom, "Delete Current Graphics Map File..........")
                                        lobjMapFile.Delete()

                                        Tracing.Info(TracingTypes.Custom, "Create New Graphics Map File by using the Updated Map File..........")
                                        'Create New Graphics map File
                                        Tracing.Info(TracingTypes.Custom, "Start: Create New Graphics Map File..........")
                                        CreateGraphicsMapFile(lstrNewMapFilePath, lobjNavigationFile, lobjVersionDoc)
                                        Tracing.Info(TracingTypes.Custom, "End: Create New Graphics Map File..........")

                                    End If

                                ElseIf FileType = "Word" AndAlso pboolIsUpdateAfterTagCreation = False Then
                                    Dim lstrTagPatterns = GetSPFTagPatternList()

                                    ''Download excel file to temp Directory
                                    'Dim lstrtepDirectory As String = System.IO.Path.GetTempPath
                                    'Dim lstrMapFilePath As String = lobjfile.DownloadToTempDir

                                    'Dim docOpen = WordprocessingDocument.Open(lstrMapFilePath, True)

                                    'For Each lstrPattern In lstrTagPatterns
                                    '    Dim rx As Regex = New Regex(lstrPattern, RegexOptions.Compiled Or RegexOptions.IgnoreCase)
                                    '    Dim matches As MatchCollection = rx.Matches(docOpen.MainDocumentPart.Document.Body.InnerText)

                                    '    For Each match As Match In matches
                                    '        CheckTagAndRelateToDoc(match.ToString(), lobjWFObject)
                                    '    Next
                                    'Next

                                ElseIf FileType = "PDF" AndAlso pboolIsUpdateAfterTagCreation = False Then
                                    'Dim lstrTagPatterns = GetSPFTagPatternList()

                                    ''Download PDF file to temp Directory
                                    'Dim lstrtepDirectory As String = System.IO.Path.GetTempPath
                                    'Dim lstrMapFilePath As String = lobjfile.DownloadToTempDir

                                    'Dim ldocPDF As PDDocument = PDDocument.load(lstrMapFilePath)
                                    'Dim lpdfReader As PDFTextStripper = New PDFTextStripper()
                                    'Dim currentPageText As String = lpdfReader.getText(ldocPDF)

                                    'For Each lstrPattern In lstrTagPatterns
                                    '    Dim rx As Regex = New Regex(lstrPattern, RegexOptions.Compiled Or RegexOptions.IgnoreCase)
                                    '    Dim matches As MatchCollection = rx.Matches(currentPageText)

                                    '    For Each match As Match In matches
                                    '        CheckTagAndRelateToDoc(match.ToString(), lobjWFObject)
                                    '    Next
                                    'Next

                                End If
                            End While
                        End With
                    End If
                End If

            Catch lobjSPFException As SPFException
                Tracing.Error(TracingTypes.Custom, lobjSPFException)
                Throw lobjSPFException
            End Try

        End Sub

        Public Sub MatchTagPattern(pGraphicMapDocument As XmlDocument, pObjDocumnet As IObject)

            'Get Tag Patterns Name and Pattern Object from SPF
            Dim ldictSPFTagPattern As Dictionary(Of String, IObject) = GetSPFTagPatternAndObject()

            'Reading Updated Graphic Map Document
            Try
                Dim xmlDoc As XmlDocument = pGraphicMapDocument

                Dim lnodeMaps As XmlNode = xmlDoc.SelectSingleNode("Mappings/Mapping/Maps")
                Dim lArrNodeNames As List(Of String) = New List(Of String)

                For Each xmlNode As XmlNode In lnodeMaps.ChildNodes
                    Dim lstrTagNamefrmXML = xmlNode.Attributes("Name").Value.ToString()
                    If lArrNodeNames Is Nothing OrElse lArrNodeNames.Contains(lstrTagNamefrmXML) = False Then
                        lArrNodeNames.Add(lstrTagNamefrmXML)

                        'lstrArrTagDetails consists of Class Def UID, InterfaceDef(Optional, seperated with '~') 
                        Dim lstrArrTagDetails As String() = Split(xmlNode.Attributes("IDef").Value.ToString(), "~")
                        Dim lstrClassDefName As String = lstrArrTagDetails(0)
                        Dim ldictClassDef As IObjectDictionary = Nothing

                        'Check if ClassDef is valid or not in SPF Schema
                        If lstrClassDefName IsNot Nothing AndAlso lstrClassDefName <> "" Then
                            Dim lClassDefCriteria As New DynamicQuery
                            lClassDefCriteria.Query.Criteria = New ObjectCriteria(Function(x) x.HasInterface("IClassDef") And x.Name = lstrClassDefName)
                            ldictClassDef = lClassDefCriteria.ExecuteToIObjectDictionary()
                        End If

#Region "Match Pattern and Create Tag"
                        If ldictClassDef IsNot Nothing AndAlso ldictClassDef.Count > 0 Then
                            For Each lstrPattern In ldictSPFTagPattern.Keys
                                Dim rx As Regex = New Regex("^" & lstrPattern & "$")
                                Dim lboolIaMatch = rx.IsMatch(lstrTagNamefrmXML)
                                If lboolIaMatch Then

                                    'Create Tag
                                    Try
                                        Dim lboolIgnoreConfig As Boolean = SPFRequestContext.Instance.IgnoreConfiguration
                                        SPFRequestContext.Instance.IgnoreConfiguration = True

                                        'Check if Tag already exists-TODO-THIS NEED TO BE RELOOKED
                                        Dim lExistingTag As IObjectDictionary = CheckTagByName(lstrTagNamefrmXML)

                                        If lExistingTag.Count = 0 Then
                                            Dim lstrPrimaryClassificationName As String = Nothing
                                            Dim lobjPattern As IObject = ldictSPFTagPattern(lstrPattern)
                                            Dim lobjsPartItems As IObjectDictionary = lobjPattern.GetEnd1Relationships().GetRels("HTCTagPatternPartItem").GetEnd2s()
                                            'Get Primary Classfication from Name
                                            For Each lobjTagPartItem As IObject In lobjsPartItems.Values

                                                Dim lpropsPartItem As IPropertyDictionary = lobjTagPartItem.Interfaces("IHTCPartItem").Properties
                                                If lobjTagPartItem.Name = "PrimaryClassification" Then
                                                    lstrPrimaryClassificationName = Split(lstrTagNamefrmXML, lpropsPartItem("HTCPartItemSeparator").Value.ToString())(Convert.ToInt32(lpropsPartItem("HTCPartItemStartPosition").Value) - 1)
                                                End If

                                            Next

                                            If lstrPrimaryClassificationName Is Nothing Then
                                                Tracing.Info(TracingTypes.Custom, "No Primary Classification found for Tag " + lstrTagNamefrmXML + ". Tag is assigned with default classification.........")
                                                lstrPrimaryClassificationName = mDefaultPrimaryClsfn
                                            End If

                                            Dim lPrimaryClassificationCriteria As New DynamicQuery
                                            lPrimaryClassificationCriteria.Query.Criteria = New ObjectCriteria(Function(x) x.HasInterface("IHTCDocumentClassification") And x.Name = lstrPrimaryClassificationName)
                                            Dim lobjSPXTagClassification As IObject = lPrimaryClassificationCriteria.ExecuteToIObjectDictionary().Values(0)
                                            'Dim lobjSPXTagClassification As IObject = SPFRequestContext.Instance.QueryRequest.GetObject(GetObjType.ByUID, lstrPrimaryClassificationName)

                                            CheckTagAndRelateToDoc(lstrArrTagDetails(0), pObjDocumnet)

                                            ''############$$$$$$###########
                                            ''NO NEED TO CREATE TAGS,CUSTOMER EXPECTED TO ONLY LINK TO EXISTING TAGS
                                            ''############$$$$$$###########
                                            'Dim lobjSPXTag As IObject = GeneralUtilities.InstantiateObject(lstrArrTagDetails(0), lstrTagNamefrmXML, "", "", False)
                                            'CType(lobjSPXTag.Interfaces("IObject"), IObjectDefault).DontApplyENS = True
                                            'CType(lobjSPXTag.Interfaces("IObject"), IObjectDefault).UID = xmlNode.Attributes("UID").Value.ToString()

                                            'lobjSPXTag.GetClassDefinition.FinishCreate(lobjSPXTag)
                                            'Dim lobjPrimaryRel = GeneralUtilities.InstantiateRelation("SPFPrimaryClassification", lobjSPXTagClassification, lobjSPXTag, False)
                                            'lobjPrimaryRel.GetClassDefinition.FinishCreate(lobjPrimaryRel)

                                            ''Attach Tag to Documnet
                                            'If pObjDocumnet IsNot Nothing Then
                                            '    Dim lobjDocRel = GeneralUtilities.InstantiateRelation("HTCTagDocument", pObjDocumnet, lobjSPXTag, False)
                                            '    lobjDocRel.GetClassDefinition.FinishCreate(lobjDocRel)
                                            'End If

                                            ''Create Rels
                                            'For Each lobjTagPartItem As IObject In lobjPattern.GetEnd1Relationships().GetRels("HTCTagPatternPartItem").GetEnd2s().Values
                                            '    Dim lpropsPartItem As IPropertyDictionary = lobjTagPartItem.Interfaces("IHTCPartItem").Properties
                                            '    Dim lAttrName = Split(lstrTagNamefrmXML, lpropsPartItem("HTCPartItemSeparator").Value.ToString())(Convert.ToInt32(lpropsPartItem("HTCPartItemStartPosition").Value) - 1)

                                            '    'If Part Item is not a Rel it is a property
                                            '    If lpropsPartItem("HTCPartItemIsRel").Value.ToString() = "False" Then
                                            '        'Instantiate property on tag
                                            '        'Expects a valid InterfaceDef of property which can be realted to Tag created
                                            '        lobjSPXTag.Interfaces(lpropsPartItem("HTCTagAttrRelatedInterfaceDef").Value.ToString(), True).Properties(lpropsPartItem("HTCTagAttributeName").Value.ToString(), True).SetValue(lAttrName)

                                            '    ElseIf lpropsPartItem("HTCPartItemIsRel").Value.ToString() = "True" Then
                                            '        Dim lobjRelItem As IObject = SPFRequestContext.Instance.QueryRequest.GetObject(GetObjType.ByUID, Split(lpropsPartItem("HTCTagRelDefUID").Value.ToString(), "_")(0))

                                            '        If lobjRelItem.Interfaces("IRelDef").Properties("Role1").Value = lobjTagPartItem.Name Then
                                            '            Dim lInterfaceEnd1 = lobjRelItem.Interfaces("IRel").Properties("UID1").Value

                                            '            Dim lobjEnd1Criteria As New DynamicQuery
                                            '            lobjEnd1Criteria.Query.Criteria = New ObjectCriteria(Function(x) x.HasInterface(lInterfaceEnd1) And x.Name = lAttrName)
                                            '            Dim ldictobjEnd1 As IObjectDictionary = lobjEnd1Criteria.ExecuteToIObjectDictionary()

                                            '            If ldictobjEnd1.Count = 1 Then
                                            '                Dim lobjnewRel = GeneralUtilities.InstantiateRelation(Split(lpropsPartItem("HTCTagRelDefUID").Value.ToString(), "_")(0), ldictobjEnd1(0), lobjSPXTag, False)
                                            '                lobjnewRel.GetClassDefinition.FinishCreate(lobjnewRel)
                                            '            End If

                                            '        ElseIf lobjRelItem.Interfaces("IRelDef").Properties("Role2").Value = lobjTagPartItem.Name Then
                                            '            Dim lInterfaceEnd2 = lobjRelItem.Interfaces("IRel").Properties("UID2").Value.ToString

                                            '            Dim lobjEnd2Criteria As New DynamicQuery
                                            '            lobjEnd2Criteria.Query.Criteria = New ObjectCriteria(Function(x) x.HasInterface(lInterfaceEnd2) And x.Name = lAttrName)
                                            '            Dim ldictobjEnd2 As IObjectDictionary = lobjEnd2Criteria.ExecuteToIObjectDictionary()

                                            '            If ldictobjEnd2.Count = 1 Then
                                            '                Dim lobjnewRel = GeneralUtilities.InstantiateRelation(Split(lpropsPartItem("HTCTagRelDefUID").Value.ToString(), "_")(0), lobjSPXTag, ldictobjEnd2(0), False)
                                            '                lobjnewRel.GetClassDefinition.FinishCreate(lobjnewRel)
                                            '            End If

                                            '        End If
                                            '    End If
                                            'Next


                                        End If

                                        SPFRequestContext.Instance.IgnoreConfiguration = lboolIgnoreConfig
                                    Catch ex As SPFException
                                        SPFRequestContext.Instance.Transaction.Rollback()
                                        Throw ex
                                    Finally
                                    End Try
                                End If
                            Next
                        End If
#End Region
                    End If
                Next

            Catch ex As SPFException
                Throw ex
            End Try

        End Sub
#End Region

#Region "Functions"

        Private Function IsVisualFile(ByVal pobjFile As ISPFFile) As String
            Dim lstrFileType As String = Nothing

            If pobjFile IsNot Nothing Then
                Dim lobjFileType As IObject = pobjFile.GetFileTypes.Item(0)
                If lobjFileType IsNot Nothing Then
                    Select Case lobjFileType.UID
                        Case "FT_dwg", "FT_dgn", "FT_dxf", "FT_igr", "FT_pid", "FT_sha", "FT_spe", "FT_zyq"
                            lstrFileType = "VisualFile"
                        Case "FT_docx", "FT_doc"
                            lstrFileType = "Word"
                        Case "FT_pdf"
                            lstrFileType = "PDF"
                    End Select
                End If
            End If

            Return lstrFileType
        End Function

        Private Sub CreateGraphicsMapFile(pstrNewMapFilePath As String, pobjNavigationFile As ISPFNavigationFile, pobjDocumentVersion As IObject)

            Dim lobjNewFile As ISPFGraphicsMap = CType(GeneralUtilities.InstantiateObject("SPFGraphicsMap", pobjNavigationFile.Name.Replace(".igr", ".xml"), "", "", False).ToInterface("ISPFGraphicsMap"), ISPFGraphicsMap)
            CType(lobjNewFile.Interfaces("ISPFFile"), ISPFFile).SPFLocalDirectory = New IO.FileInfo(pstrNewMapFilePath).DirectoryName
            CType(lobjNewFile.Interfaces("ISPFFile"), ISPFFile).SPFLocalFileName = New IO.FileInfo(pstrNewMapFilePath).Name

            lobjNewFile.GetClassDefinition.FinishCreate(lobjNewFile)

            Dim lobjRel As IObject = GeneralUtilities.InstantiateRelation("SPFFileGraphicsMap", pobjNavigationFile, lobjNewFile, False)
            lobjRel.GetClassDefinition.FinishCreate(lobjRel)
            Dim lobjVault As ISPFVault
            lobjVault = CType(lobjNewFile.ToInterface("ISPFFile"), ISPFFile).GetNewVault(pobjDocumentVersion)
            If lobjVault Is Nothing Then
                Throw New SPFException(1570, "No vault found for object " & pobjDocumentVersion.Name, New String() {pobjDocumentVersion.Name})
            End If
            '
            ' Get the host
            '
            Dim lobjHost As ISPFHost = CType(lobjVault.ToInterface("ISPFVault"), ISPFVault).GetHost
            If lobjHost Is Nothing Then
                Throw New SPFException(123455, "There is no host object associated with vault ‘" & lobjVault.Name & "’", New String() {lobjVault.Name})
            End If
            '
            ' Upload the file to the temp area on fileservice

            Dim lobjFileService As ISPFFileService = SPFFileService.Create
            lobjFileService.UploadFileFromURL(New IO.FileInfo(pstrNewMapFilePath), lobjHost.Name)
            '
            ' Move the file to the vault
            '
            CType(lobjNewFile.ToInterface("ISPFFile"), ISPFFile).MoveFileToVault(lobjVault)
            '
            ' Create the file vault rel
            '
            Dim lobjFileVaultRel As IObject = GeneralUtilities.InstantiateRelation("SPFFileVault", lobjNewFile, lobjVault, False)
            lobjFileVaultRel.GetClassDefinition.FinishCreate(lobjFileVaultRel)

            Dim lstroutputFileType As String = "FT_xml"

            Dim lobjFileTypes As IObjectDictionary = SPFRequestContext.Instance.QueryRequest.RunByUID(lstroutputFileType)

            Dim lobjXMLFileType As ISPFFileType = Nothing
            If lobjFileTypes.Count > 0 Then
                lobjXMLFileType = CType(lobjFileTypes.Item(0).Interfaces("ISPFFileType"), ISPFFileType)
            Else
                Throw New SPFException(1654456, "Object not found", New String() {"ISPFFileType"})
            End If
            '
            ' Create the SPFFileFileType rel
            '
            Dim lobjFileFileTypeRel As IObject = GeneralUtilities.InstantiateRelation("SPFFileFileType", lobjNewFile, lobjXMLFileType, False)

            lobjFileFileTypeRel.GetClassDefinition.FinishCreate(lobjFileFileTypeRel)
        End Sub

        Private Function GetSPFTagPatternAndObject() As Dictionary(Of String, IObject)
            Dim ldictSPFTagPattern As Dictionary(Of String, IObject) = New Dictionary(Of String, IObject)

            'Get Tag Patterns from SPF
            Dim lobjPatternCriteria As New DynamicQuery
            lobjPatternCriteria.Query.Criteria = New ObjectCriteria(Function(x) x.HasInterface("IHTCMasterTagPattern"))
            Dim lobjTagPatterns As IObjectDictionary = lobjPatternCriteria.ExecuteToIObjectDictionary()

            'Creating dictionary of each spf pattern name and its Iobject...
            For Each lobjTagPattern As IObject In lobjTagPatterns.Values
                Dim lstrPattern As String = lobjTagPattern.Interfaces("IHTCTagPattern").Properties("HTCTagPattern").Value.ToString()
                lstrPattern = lstrPattern.Replace("#", "[0-9]")
                lstrPattern = lstrPattern.Replace("-[", "\-[")
                ldictSPFTagPattern.Add(lstrPattern, lobjTagPattern)
            Next

            Return ldictSPFTagPattern
        End Function

        Private Function GetSPFTagPatternList() As List(Of String)
            Dim lListSPFTagPattern As List(Of String) = New List(Of String)

            'Get Tag Patterns from SPF
            Dim lobjPatternCriteria As New DynamicQuery
            lobjPatternCriteria.Query.Criteria = New ObjectCriteria(Function(x) x.HasInterface("IHTCMasterTagPattern"))
            Dim lobjTagPatterns As IObjectDictionary = lobjPatternCriteria.ExecuteToIObjectDictionary()

            For Each lobjPattern As IObject In lobjTagPatterns.Values
                Dim lstrPattern As String = lobjPattern.Interfaces("IHTCTagPattern").Properties("HTCTagPattern").Value.ToString()
                lstrPattern = lstrPattern.Replace("#", "[0-9]")
                lstrPattern = lstrPattern.Replace("-[", "\-[")
                lListSPFTagPattern.Add(lstrPattern)
            Next

            Return lListSPFTagPattern
        End Function

        Private Sub CheckTagAndRelateToDoc(pstrTagName As String, pobjDocument As IObject)
            Try
                'Check if Tag already exists
                Dim lExistingTag As IObjectDictionary = CheckTagByName(pstrTagName)
                If lExistingTag.Count > 0 Then
                    Dim IsDocRelated = False
                    Dim lobjTag As IObject = lExistingTag.Values(0)
                    Dim lRelatedDocs As IObjectDictionary = lobjTag.GetEnd2Relationships().GetRels("HTCTagDocument").GetEnd1s()

                    'Check if tag is already related to document or not
                    For Each lobjDoc As IObject In lRelatedDocs.Values
                        If pobjDocument Is lobjDoc Then
                            IsDocRelated = True
                        End If
                    Next

                    'Attach Tag to Document
                    If pobjDocument IsNot Nothing AndAlso IsDocRelated = False Then
                        Dim lobjDocRel = GeneralUtilities.InstantiateRelation("HTCTagDocument", pobjDocument, lobjTag, False)
                        lobjDocRel.GetClassDefinition.FinishCreate(lobjDocRel)
                    End If
                End If
            Catch ex As SPFException
                Throw ex
            End Try
        End Sub

        Private Function CheckTagByName(pstrTagName As String) As IObjectDictionary
            Try
                'Check if Tag already exists
                Dim lobjTagExistsCriteria As New DynamicQuery
                lobjTagExistsCriteria.Query.Criteria = New ObjectCriteria(Function(x) x.Name = pstrTagName)
                Dim lExistingTag As IObjectDictionary = lobjTagExistsCriteria.ExecuteToIObjectDictionary()
                Return lExistingTag
            Catch ex As SPFException
                Throw ex
            End Try
        End Function

#End Region
    End Class
End Namespace