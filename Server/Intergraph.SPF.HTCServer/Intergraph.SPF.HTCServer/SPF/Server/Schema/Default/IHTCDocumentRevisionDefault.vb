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

        'Public Overrides Sub OnRelationshipAdding(ByVal e As Model.RelEventArgs)

        '    If e IsNot Nothing AndAlso (e.Rel.DefUID = "HTCJobToDocument") Then
        '        Dim lobjJob = e.Rel.GetEnd2

        '        If lobjJob.GetEnd1Relationships.GetRel("HTCJobToEWRNmber") IsNot Nothing AndAlso
        '                lobjJob.GetEnd1Relationships.GetRel("HTCJobToEWRNmber").GetEnd2 IsNot Nothing Then
        '            MyBase.OnRelationshipAdding(e)

        '        Else
        '            Throw New SPFException(99999, "No EWR Number related to EW/Job,Please relate EWR number to proceed")
        '        End If
        '        Dim lobjMaster = CType(Me.ToInterface("ISPFDocumentRevision"), ISPFDocumentRevision).GetDocumentMaster

        '        If lobjMaster.Interfaces("IHTCDocumentCommon") IsNot Nothing AndAlso
        '            lobjMaster.Interfaces("IHTCDocumentCommon").Properties("HTCDocumentStatus") IsNot Nothing AndAlso
        '            lobjMaster.Interfaces("IHTCDocumentCommon").Properties("HTCDocumentStatus").Value.ToString = "HTCENUM_Merge_in_progress" Then

        '            Throw New SPFException(99999, "Cannot relate 'Merge in Progress' document to EW")
        '        End If
        '    End If

        'End Sub
        Public Overrides Sub OnCreate(e As CreateEventArgs)
            MyBase.OnCreate(e)
            If Me.Interfaces.Contains("IHTCVendorPrintRevision") Then
                Me.Interfaces("IHTCVendorPrintRevision").Properties("HTCVendorPrintDocumentStatus").SetValue("HTCENUM_RequestCircleForApproval")
            End If
        End Sub

        'Public Overrides Sub OnRelationshipAdd(e As RelEventArgs)
        '    MyBase.OnRelationshipAdd(e)
        '    If e.Rel.DefUID = "HTCIncomingDocuments" Or e.Rel.DefUID = "HTCOutgoingDocuments" Then
        '        Dim lobjTransmittal As IObject = e.Rel.GetEnd1

        '        If lobjTransmittal IsNot Nothing Then

        '            Dim lobjPICObject As IHTCDocumentRevision = CType(Me.ToInterface("IHTCDocumentRevision"), IHTCDocumentRevision)
        '            ''Set the document status to "SUBMITTED" on attaching it to the Transmittal
        '            Me.BeginUpdate()
        '            Me.HTCDocumentReviewStatus = "HTCENUM_DS_SUBMITTED"
        '            Me.FinishUpdate()
        '        End If
        '    End If
        'End Sub


        ''TODO:review to include all the transactional information into one container or move this to seperate service to get of interface override behaviours
        ''Also to use other service instead of this interface override to avoid lot of hits on the document rel overrides
        Public Overrides Sub OnRelationshipAdded(e As RelEventArgs)
            MyBase.OnRelationshipAdded(e)

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
                    End If
                    Dim lxmlDocRequest As New Xml.XmlDocument
                    ''if there is no EWR no attached to EW then use EW name as part of the copy document name
                    If lobjEWRNumber IsNot Nothing Then
                        lxmlDocRequest = OnBuildXMLForCopyDocumentWithFile(lobjDoc, SPFRequestContext.Instance.CreateConfiguration.ToString, e.Rel.GetEnd1.Name + "-" + lobjEWRNumber.Name, lobjDoc.Version.GetAllFiles, True)
                    Else
                        lxmlDocRequest = OnBuildXMLForCopyDocumentWithFile(lobjDoc, SPFRequestContext.Instance.CreateConfiguration.ToString, e.Rel.GetEnd1.Name + "-" + lobjJob.Name, lobjDoc.Version.GetAllFiles, True)
                    End If

                    ''Create Document from the selected document


                    Dim ldomResopnse As XmlDocument = ProcessRequest(lxmlDocRequest.DocumentElement)

                    ''once document is created then get the job and related to newly created temp document
                    ''Get the name from the response xml or as object
                    Dim lstrDocMasterUID = CType(ldomResopnse.SelectSingleNode("Reply/NewItems/HTCDocumentMaster/IObject"), XmlElement).GetAttribute("UID").ToString

                    Dim lobjDocumentMaster As New DynamicQuery
                    ''is it going to return multiple master documents during concurrent engineering??kept config to control that case
                    lobjDocumentMaster.Query.Criteria = New ObjectCriteria(Function(x) x.HasInterface("IHTCDocumentMaster") And x.UID = lstrDocMasterUID And x.Config = Me.Config)

                    Dim lobjDocMaster As IObject = lobjDocumentMaster.ExecuteToIObjectDictionary.FirstOrDefault()
                    Dim lobjDocRevision As IObject = CType(lobjDocMaster.ToInterface("ISPFDocumentMaster"), ISPFDocumentMaster).GetNewestRevision()

                    If Not SPFRequestContext.Instance.Transaction.InTransaction Then
                        SPFRequestContext.Instance.Transaction.Begin()
                        ''lboolStartedTransaction = True
                    End If

                    ''rel between new copied document master and existing document revision
                    Dim lobjMasterDocToRevisionDocRel As IObject = GeneralUtilities.InstantiateRelation("HTCDocumentToCopiedDocument", CType(Me, IObject), lobjDocMaster, False)
                    lobjMasterDocToRevisionDocRel.GetClassDefinition.FinishCreate(lobjMasterDocToRevisionDocRel)
                    If (lobjEWRNumber IsNot Nothing) Then
                        Dim lobjRevisionDocToRevisionDocRel As IObject = GeneralUtilities.InstantiateRelation("HTCDocumentEWRNumber", lobjDocRevision, lobjEWRNumber, False)
                        lobjRevisionDocToRevisionDocRel.GetClassDefinition.FinishCreate(lobjRevisionDocToRevisionDocRel)
                    End If
                    ''Rel between job and new copied document master
                    Dim lobjJobMasterRel As IObject = GeneralUtilities.InstantiateRelation("HTCJobToCopiedDocument", lobjDocMaster, lobjJob, False)
                    lobjJobMasterRel.GetClassDefinition.FinishCreate(lobjJobMasterRel)
                    lobjJob.BeginUpdate()
                    lobjJob.Interfaces("IHTCJobDetails").Properties("HTCJobEWStatus").SetValue("HTCENUM_Changing")
                    lobjJob.FinishUpdate()

                    Try
                        SPFRequestContext.Instance.Transaction.Commit()
                    Catch ex As Exception
                        SPFRequestContext.Instance.Transaction.Rollback()
                        Throw ex
                    End Try
                End If

            Catch ex As Exception
                SPFRequestContext.Instance.Transaction.Rollback()
                Tracing.Error(TracingTypes.Custom, ex)
                Throw ex
            End Try

        End Sub
#End Region

#Region "Private Methods"
        ''' <summary>
        ''' Prepare document contain for further use
        ''' </summary>
        ''' <param name="pobjDoument"></param>
        ''' <returns></returns>
        Private Function BuildDocument(pobjDoument As IObject) As DocumentObject
            Dim lobjDocumentObjectToReturn As New DocumentObject
            ''Revision
            lobjDocumentObjectToReturn.Revision = CType(pobjDoument.ToInterface("ISPFDocumentRevision"), ISPFDocumentRevision)
            ''set the revision state to working
            'lobjDocumentObjectToReturn.Revision.SPFRevState = "e1WORKING"
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
        ''' <summary>
        ''' Build the new document container to copy
        ''' </summary>
        ''' <param name="pobjDocument"></param>
        ''' <param name="pstrPlantUID"></param>
        ''' <param name="pstrNewDocName"></param>
        ''' <param name="pcolAttachedFiles"></param>
        ''' <param name="pblnCopyMarkups"></param>
        ''' <returns></returns>
        Protected Function OnBuildXMLForCopyDocumentWithFile(ByVal pobjDocument As DocumentObject, ByVal pstrPlantUID As String, ByVal pstrNewDocName As String,
                                                             ByVal pcolAttachedFiles As IObjectDictionary, ByVal pblnCopyMarkups As Boolean) As XmlDocument
            '
            ' Create the XML dom to copy the above created design document
            '
            Dim lxmlDocRequest As New Xml.XmlDocument
            '
            ' Build xml for copy request
            '
            lxmlDocRequest.InnerXml = "<Query>" & QueryHeader("CopyObj", pstrPlantUID) & "</Query>"
            '
            ' Find the query node
            '
            Dim lnodQuery As XmlNode = lxmlDocRequest.SelectSingleNode("//Query")
            'SessionInfo
            ' Add the MethodUID
            '
            Dim lnodMethodUID As XmlNode = lnodQuery.AppendChild(lxmlDocRequest.CreateElement("MethodUID"))
            lnodMethodUID.InnerText = "MTH_DocumentCopy"
            '
            ' Add the object node
            '
            Dim lnodObject As XmlNode = lnodQuery.AppendChild(lxmlDocRequest.CreateElement("Object"))
            CType(lnodObject, XmlElement).SetAttribute("OBID", pobjDocument.Master.OBID)
            CType(lnodObject, XmlElement).SetAttribute("DomainUID", pobjDocument.Master.DomainUID)
            If pcolAttachedFiles IsNot Nothing AndAlso pcolAttachedFiles.Count > 0 Then
                With pcolAttachedFiles.GetEnumerator
                    While .MoveNext
                        Dim lobjAttachedfile = CType(.Value.ToInterface("ISPFBusinessFile"), ISPFBusinessFile)

                        '
                        ' Add the copy markups
                        '
                        Dim lnodCopyMarkups As XmlNode = lnodQuery.AppendChild(lxmlDocRequest.CreateElement("CopyMarkups"))
                        lnodCopyMarkups.InnerText = pblnCopyMarkups.ToString
                        '
                        ' Add the copy files node
                        '
                        Dim lnodCopyFiles As XmlNode = lnodQuery.AppendChild(lxmlDocRequest.CreateElement("CopyFiles"))
                        CType(lnodCopyFiles, XmlElement).SetAttribute("CompSchema", "SPFComponent")
                        CType(lnodCopyFiles, XmlElement).SetAttribute("Scope", "Data")
                        CType(lnodCopyFiles, XmlElement).SetAttribute("SoftwareVersion", "04.00.00.01")
                        '
                        ' Add the container node for file composition rel
                        '
                        Dim lnodFileCompositionContainer As XmlNode = lnodCopyFiles.AppendChild(lxmlDocRequest.CreateElement("Container"))
                        CType(lnodFileCompositionContainer, XmlElement).SetAttribute("CreatesAndInstructionsOnly", "True")
                        '
                        ' Serialize the file composition rel
                        '
                        Dim lelemFileCompositionRel As XmlElement = SPFRequestContext.Instance.Serializer.Serialize(lobjAttachedfile.GetEnd1Relationships.GetRel("SPFFileComposition"), New EFFlatXmlSerializationWriter)
                        lnodFileCompositionContainer.AppendChild(lxmlDocRequest.ImportNode(lelemFileCompositionRel.SelectSingleNode("//Rel"), True))
                        '
                        ' Add the container node for design file object and file type rel
                        '
                        Dim lnodDesignFileContainer As XmlNode = lnodCopyFiles.AppendChild(lxmlDocRequest.CreateElement("Container"))
                        CType(lnodDesignFileContainer, XmlElement).SetAttribute("UpdatesOnly", "True")
                        '
                        ' Serialize the design file object
                        '
                        Dim lelemAttachedFile As XmlElement = SPFRequestContext.Instance.Serializer.Serialize(lobjAttachedfile, New EFFlatXmlSerializationWriter)
                        lnodDesignFileContainer.AppendChild(lxmlDocRequest.ImportNode(lelemAttachedFile.SelectSingleNode("//SPFDesignFile"), True))
                        '
                        ' Serialize the File -> FileType rel
                        '
                        Dim lelemFileTypeRel As XmlElement = SPFRequestContext.Instance.Serializer.Serialize(lobjAttachedfile.GetEnd1Relationships.GetRel("SPFFileFileType"), New EFFlatXmlSerializationWriter)
                        lnodDesignFileContainer.AppendChild(lxmlDocRequest.ImportNode(lelemFileTypeRel.SelectSingleNode("//Rel"), True))

                    End While
                End With
            End If

            '
            ' Add the Container node
            '
            Dim lnodContainer As XmlNode = lnodQuery.AppendChild(lxmlDocRequest.CreateElement("Container"))
            CType(lnodContainer, XmlElement).SetAttribute("CompSchema", "SPFComponent")
            CType(lnodContainer, XmlElement).SetAttribute("Scope", "Data")
            CType(lnodContainer, XmlElement).SetAttribute("SoftwareVersion", "04.00.00.01")
            '
            ' Extract the source name
            '
            Dim lstrDocName As String = pobjDocument.Master.Name
            '
            ' Serialize the master object
            '
            Dim lelemMaster As XmlElement = SPFRequestContext.Instance.Serializer.Serialize(pobjDocument.Master, New EFFlatXmlSerializationWriter)
            lelemMaster.InnerXml = lelemMaster.InnerXml.Replace(lstrDocName, pstrNewDocName)
            lelemMaster.InnerXml = lelemMaster.InnerXml.Replace("e1DocStateISSUED:ISSUED", "e1DocStateRESERVED:RESERVED")
            If lelemMaster.SelectSingleNode("//HTCDocumentMaster/IHTCDocumentCommon") IsNot Nothing Then
                lelemMaster.SelectSingleNode("//HTCDocumentMaster/IHTCDocumentCommon").Attributes("HTCDocumentStatus").Value = "HTCENUM_Copied"
            End If
            ''lelemMaster.InnerXml = lelemMaster.InnerXml.Replace("HTCENUM_Available:Available", "HTCENUM_Copied:Copied")
            lnodContainer.AppendChild(lxmlDocRequest.ImportNode(lelemMaster.SelectSingleNode("//HTCDocumentMaster"), True))
            '
            ' Serialize the revision object
            '
            Dim lelemRev As XmlElement = SPFRequestContext.Instance.Serializer.Serialize(pobjDocument.Revision, New EFFlatXmlSerializationWriter)
            lelemRev.InnerXml = lelemRev.InnerXml.Replace(lstrDocName, pstrNewDocName)
            ''All the new temporary documents should have revision state as "working"
            lelemRev.InnerXml = lelemRev.InnerXml.Replace("e1CURRENT:Current", "e1WORKING:Working")
            ''Check::Is this needed ??-delete document has condition to to show only if doc is RESERVED.

            lnodContainer.AppendChild(lxmlDocRequest.ImportNode(lelemRev.SelectSingleNode("//HTCDocumentRevision"), True))

            '
            ' Serialize the version object
            '
            Dim lelemVer As XmlElement = SPFRequestContext.Instance.Serializer.Serialize(pobjDocument.Version, New EFFlatXmlSerializationWriter)
            lelemVer.InnerXml = lelemVer.InnerXml.Replace(lstrDocName, pstrNewDocName)
            lnodContainer.AppendChild(lxmlDocRequest.ImportNode(lelemVer.SelectSingleNode("//HTCDocumentVersion"), True))
            '
            ' Serialize the Master -> Revision Rel
            '
            Dim lelemMasterRev As XmlElement = SPFRequestContext.Instance.Serializer.Serialize(pobjDocument.MasterRevisionRel, New EFFlatXmlSerializationWriter)
            lnodContainer.AppendChild(lxmlDocRequest.ImportNode(lelemMasterRev.SelectSingleNode("//Rel"), True))
            '
            ' Serialize the Revision -> Version Rel
            '
            Dim lelemRevVer As XmlElement = SPFRequestContext.Instance.Serializer.Serialize(pobjDocument.RevisionVersionRel, New EFFlatXmlSerializationWriter)
            lnodContainer.AppendChild(lxmlDocRequest.ImportNode(lelemRevVer.SelectSingleNode("//Rel"), True))
            '
            ' Serialize the RevScheme rel
            '
            Dim lobjRevSchemeRel As IRel = pobjDocument.Revision.GetEnd1Relationships.GetRel("SPFDocRevisionRevisionScheme")
            Dim lelemRevSchemeRel As XmlElement = SPFRequestContext.Instance.Serializer.Serialize(lobjRevSchemeRel, New EFFlatXmlSerializationWriter)
            lnodContainer.AppendChild(lxmlDocRequest.ImportNode(lelemRevSchemeRel.SelectSingleNode("//Rel"), True))
            '
            ' Serialize the OwningGroup rel
            '
            Dim lobjOwningGroupRel As IRel = pobjDocument.Version.GetEnd1Relationships.GetRel("SPFItemOwningGroup")
            Dim lelemOwningGroupRel As XmlElement = SPFRequestContext.Instance.Serializer.Serialize(lobjOwningGroupRel, New EFFlatXmlSerializationWriter)
            lnodContainer.AppendChild(lxmlDocRequest.ImportNode(lelemOwningGroupRel.SelectSingleNode("//Rel"), True))
            '
            ' Serialize the Plant-Document rel
            '
            Dim lobjPlantDocument As IRel = pobjDocument.Master.GetEnd2Relationships.GetRel("HTCPlantDocument")
            If lobjPlantDocument IsNot Nothing Then
                Dim lelemPlantDocument As XmlElement = SPFRequestContext.Instance.Serializer.Serialize(lobjPlantDocument, New EFFlatXmlSerializationWriter)
                lnodContainer.AppendChild(lxmlDocRequest.ImportNode(lelemPlantDocument.SelectSingleNode("//Rel"), True))
            End If

            '            '
            ' Serialize the Document-PlantCode rel
            '
            Dim lobjDocumentPlantCode As IRel = pobjDocument.Revision.GetEnd1Relationships.GetRel("HTCDocumentPlantCode")
            If lobjDocumentPlantCode IsNot Nothing Then
                Dim lelemDocumentPlantCode As XmlElement = SPFRequestContext.Instance.Serializer.Serialize(lobjDocumentPlantCode, New EFFlatXmlSerializationWriter)
                lnodContainer.AppendChild(lxmlDocRequest.ImportNode(lelemDocumentPlantCode.SelectSingleNode("//Rel"), True))
            End If

            '
            ' Serialize the Document-CostCenter rel
            '
            Dim lobjDocumentCostcenter As IRel = pobjDocument.Revision.GetEnd1Relationships.GetRel("HTCDocumentCostCenter")
            If lobjDocumentCostcenter IsNot Nothing Then
                Dim lelemDocumentCostcenter As XmlElement = SPFRequestContext.Instance.Serializer.Serialize(lobjDocumentCostcenter, New EFFlatXmlSerializationWriter)
                lnodContainer.AppendChild(lxmlDocRequest.ImportNode(lelemDocumentCostcenter.SelectSingleNode("//Rel"), True))
            End If

            '
            ' Serialize the Document-EWR rel
            'For EWR ,the requirement is to get the EWR number from the Job instead of getting it from the document
            'Dim lobjDocumentEWR As IRel = pobjDocument.Revision.GetEnd1Relationships.GetRel("HTCDocumentEWRNumber")
            'Dim lobjDocumentEWR As IRel = pobjJob.GetEnd1Relationships.GetRel("HTCJobToEWRNmber")
            'Dim lobjDocEWRRel = GeneralUtilities.InstantiateRelation("HTCDocumentEWRNumber",)
            'If lobjDocumentEWR IsNot Nothing Then
            '    Dim lelemDocumentEWR As XmlElement = SPFRequestContext.Instance.Serializer.Serialize(lobjDocumentEWR, New EFFlatXmlSerializationWriter)

            '    lnodContainer.AppendChild(lxmlDocRequest.ImportNode(lelemDocumentEWR.SelectSingleNode("//Rel"), True))
            'End If

            Return lxmlDocRequest

        End Function
        Protected Function QueryHeader(ByVal pstrmethod As String, ByVal pstrPlantConfigName As String) As String

            Return QueryHeader(pstrmethod, pstrPlantConfigName, SPFRequestContext.Instance.LoginUser.Name)
        End Function
        Protected Function QueryHeader(ByVal pstrmethod As String, ByVal pstrPlantConfigName As String, ByVal pstrLoginUserName As String) As String

            Dim lcolRoles As IObjectDictionary = SPFRequestContext.Instance.LoginUser.GetDefaultRoles
            Return QueryHeader(pstrmethod, pstrPlantConfigName, pstrLoginUserName, lcolRoles)
        End Function

        Protected Function QueryHeader(ByVal pstrmethod As String, ByVal pstrPlantConfigName As String, ByVal pstrLoginUserName As String, pcolRoles As IObjectDictionary) As String
            Dim ldomSession As New Xml.XmlDocument
            Dim lobjIgnoreConfig = SPFRequestContext.Instance.IgnoreConfiguration
            Dim lnodQuery As Xml.XmlNode = ldomSession.CreateElement("Query")
            Try

                lnodQuery.AppendChild(ldomSession.CreateElement("Method")).InnerText = pstrmethod

                Dim lnodSessionInfo As Xml.XmlNode = lnodQuery.AppendChild(ldomSession.CreateElement("SessionInfo"))

                lnodSessionInfo.AppendChild(ldomSession.CreateElement("SessionID")).InnerText = SPFRequestContext.Instance.SessionID

                lnodSessionInfo.AppendChild(ldomSession.CreateElement("ApplyEffectivity")).InnerText = "False"
                lnodSessionInfo.AppendChild(ldomSession.CreateElement("CurEffectivityDate"))
                lnodSessionInfo.AppendChild(ldomSession.CreateElement("EffectivityDate"))
                lnodSessionInfo.AppendChild(ldomSession.CreateElement("ChgOBID")).InnerText = ""
                lnodSessionInfo.AppendChild(ldomSession.CreateElement("UseCreateConfigForQuery")).InnerText = "False"
                lnodSessionInfo.AppendChild(ldomSession.CreateElement("IgnoreConfig")).InnerText = "False"

                Dim lobjConfigItem As IObject

                If Not String.IsNullOrEmpty(pstrPlantConfigName) Then


                    lobjConfigItem = CType(SPFRequestContext.Instance.CreateConfiguration.GetCurrentConfig, IObject) '' SPFRequestContext.Instance.QueryRequest.RunByUID(pstrPlantConfigName).Item(0)

                Else
                    lobjConfigItem = SPFApplicationContext.Instance.ConfigurationTop
                End If

                Dim mnodQueryConfig As Xml.XmlNode = ldomSession.CreateElement("ConfigurationQuery")
                Dim lobjXMLAttributeHC As Xml.XmlAttribute = ldomSession.CreateAttribute("IncludeHigherConfiguration")
                lobjXMLAttributeHC.Value = "True"
                Dim llevel As Xml.XmlNode = ldomSession.CreateElement("Level")
                Dim lobjXMLAttribute As Xml.XmlAttribute = ldomSession.CreateAttribute("DomainUID")
                lobjXMLAttribute.Value = "ADMIN"
                llevel.Attributes.Append(lobjXMLAttribute)
                lobjXMLAttribute = ldomSession.CreateAttribute("UID")
                lobjXMLAttribute.Value = lobjConfigItem.UID
                llevel.Attributes.Append(lobjXMLAttribute)
                lobjXMLAttribute = ldomSession.CreateAttribute("OBID")
                lobjXMLAttribute.Value = lobjConfigItem.OBID
                llevel.Attributes.Append(lobjXMLAttribute)
                lobjXMLAttribute = ldomSession.CreateAttribute("ClassDefinitionUID")
                lobjXMLAttribute.Value = lobjConfigItem.ClassDefinitionUID
                llevel.Attributes.Append(lobjXMLAttribute)
                lobjXMLAttribute = ldomSession.CreateAttribute("Name")
                lobjXMLAttribute.Value = lobjConfigItem.Name
                llevel.Attributes.Append(lobjXMLAttribute)
                mnodQueryConfig.AppendChild(llevel)
                lnodSessionInfo.AppendChild(mnodQueryConfig)
                Dim mnodCreateConfig As Xml.XmlNode = ldomSession.CreateElement("ConfigurationCreate")
                Dim llevelCreate As Xml.XmlNode = ldomSession.CreateElement("Level")
                lobjXMLAttribute = ldomSession.CreateAttribute("DomainUID")
                lobjXMLAttribute.Value = "ADMIN"
                llevelCreate.Attributes.Append(lobjXMLAttribute)
                lobjXMLAttribute = ldomSession.CreateAttribute("UID")
                lobjXMLAttribute.Value = lobjConfigItem.UID
                llevelCreate.Attributes.Append(lobjXMLAttribute)
                lobjXMLAttribute = ldomSession.CreateAttribute("OBID")
                lobjXMLAttribute.Value = lobjConfigItem.OBID
                llevelCreate.Attributes.Append(lobjXMLAttribute)
                lobjXMLAttribute = ldomSession.CreateAttribute("ClassDefinitionUID")
                lobjXMLAttribute.Value = lobjConfigItem.ClassDefinitionUID
                llevelCreate.Attributes.Append(lobjXMLAttribute)
                lobjXMLAttribute = ldomSession.CreateAttribute("Name")
                lobjXMLAttribute.Value = lobjConfigItem.Name
                llevelCreate.Attributes.Append(lobjXMLAttribute)
                mnodCreateConfig.AppendChild(llevelCreate)
                lnodSessionInfo.AppendChild(mnodCreateConfig)
                '
                'Set up the roles node
                '
                Dim lnodRoles As Xml.XmlNode = ldomSession.CreateElement("Roles")
                With pcolRoles.GetEnumerator
                    Do While .MoveNext
                        Dim lObjRole As System.Collections.DictionaryEntry = CType(.Current, System.Collections.DictionaryEntry)
                        Dim lnodRole As Xml.XmlNode = ldomSession.CreateElement("Object")
                        CType(lnodRole, Xml.XmlElement).SetAttribute("OBID", lObjRole.Key.ToString)
                        CType(lnodRole, Xml.XmlElement).SetAttribute("DomainUID", CType(lObjRole.Value, IObject).DomainUID)
                        lnodRoles.AppendChild(lnodRole)
                    Loop
                End With
                lnodSessionInfo.AppendChild(lnodRoles)
                lnodSessionInfo.AppendChild(ldomSession.CreateElement("ClientLocale")).InnerText = System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName
                lnodSessionInfo.AppendChild(ldomSession.CreateElement("ClientMachineName")).InnerText = Environment.MachineName
                lnodSessionInfo.AppendChild(ldomSession.CreateElement("OrigMethodName")).InnerText = pstrmethod
                lnodSessionInfo.AppendChild(ldomSession.CreateElement("CalledFrom")).InnerText = "WEBCLIENT"
                lnodSessionInfo.AppendChild(ldomSession.CreateElement("UserName")).InnerText = SPFRequestContext.Instance.UserName
                lnodSessionInfo.AppendChild(ldomSession.CreateElement("CMaxSQLLimit")).InnerText = "200"
                lnodSessionInfo.AppendChild(ldomSession.CreateElement("CalledFromDialog")).InnerText = "None"
            Catch ex As Exception
                Throw ex

            End Try

            Return lnodQuery.InnerXml
        End Function
        Protected Function ProcessRequest(ByVal pnodRequest As XmlNode) As XmlDocument
            Dim ldomReply As XmlDocument = Nothing
            '
            ' Get the server and invoke the request
            '
            SPFRequestContext.Instance.Request = pnodRequest
            SPFRequestContext.Instance.SetRequestSettings(New SPFRequestSettings(pnodRequest))
            SPFRequestContext.Instance.ResetValidRolesForCurrentConfig()
            '
            ' Set the response up
            '
            SPFRequestContext.Instance.Response = New XmlDocument
            Dim lobjSPFHandler As New SPFHandler
            lobjSPFHandler.ProcessRequest()
            If SPFRequestContext.Instance.Response IsNot Nothing Then
                ldomReply = SPFRequestContext.Instance.Response
            ElseIf SPFRequestContext.Instance.ResponseStream IsNot Nothing Then
                Dim lobjXmlReader As XmlReader = Nothing
                SPFRequestContext.Instance.EndResponseStream()
                Dim llngXMLResponseStreamBytes As Long = SPFRequestContext.Instance.ResponseStream.Length
                lobjXmlReader = XmlReader.Create(SPFRequestContext.Instance.ResponseStream, New XmlReaderSettings)
                'ldomReply = XmlNodeFactory.Create(lobjXmlReader)
            End If

            Return ldomReply
        End Function

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


