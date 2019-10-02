Imports System.Configuration
Imports System.Globalization
Imports System.IO
Imports System.Xml
Imports System.Xml.Xsl
Imports Intergraph.SPF.DAL.Common.Criteria.Object
Imports SPF.Client
Imports SPF.Diagnostics
Imports SPF.Server.Context
Imports SPF.Server.QueryClasses
Imports SPF.Server.Schema.Collections
Imports SPF.Server.Schema.Interface.Generated

Public Class GenerateTRHTML
    Private mstrTROBID As String = String.Empty
    Private mstrTRTemplateMasterUID As String = String.Empty
    Private mstrWebClientURL As String = String.Empty

#Region "Constructors"

    Public Sub New(ByVal pstrTROBID As String)
        mstrTROBID = pstrTROBID
    End Sub

#End Region

    Public Function GenerateTRHTMLContent() As String
        Dim lrtnHTMLContent As String = String.Empty
        Dim lobjectTransmittal As IObject = GetTransmitalObject(mstrTROBID)

        If lobjectTransmittal Is Nothing Then Throw New SPFException(String.Format("Could not find Transmittal OBID {0}", mstrTROBID))

        Dim lobjMethod As ISPFMethod = Nothing
        Dim pstrCoversheetTemplateDocMasUID As String = String.Empty
        If lobjectTransmittal.ClassDefinitionUID Is "HTCOutgoingTransmittal" Then

            pstrCoversheetTemplateDocMasUID = "HTCTransmittalDocumentHTMLTemplateFileFromHTC.Master"
        Else
            pstrCoversheetTemplateDocMasUID = "HTCTransmittalDocumentHTMLTemplateFileToHTC.Master"
        End If

        Dim lstrURL As String = ConfigurationManager.AppSettings.Item("SDAClientURL")

        If String.IsNullOrEmpty(lstrURL) Then Throw New ArgumentNullException("Web Client URL", "There is no value for 'SDAClientURL' in the server manager")

        Dim lstrxmlPathToHTC As String
        Dim lstrxmlPathFromHTC As String
        Dim currentConfigName As String = SPFRequestContext.Instance.CreateConfiguration.GetCurrentConfig.Name
        Dim CurrentConfigUID As String = SPFRequestContext.Instance.CreateConfiguration.GetCurrentConfig.UID
        If (lobjectTransmittal.ClassDefinitionUID = "HTCIncomingTransmittal") Then

            lstrxmlPathToHTC = CreateHTMLTOHTCXML(lobjectTransmittal, lstrURL, currentConfigName, CurrentConfigUID)

            lrtnHTMLContent = GetHTMLContent(lstrxmlPathToHTC, pstrCoversheetTemplateDocMasUID)

        ElseIf (lobjectTransmittal.ClassDefinitionUID = "HTCOutgoingTransmittal") Then

            lstrxmlPathFromHTC = CreateHTMLFromHTCXML(lobjectTransmittal, lstrURL, currentConfigName, CurrentConfigUID)

            lrtnHTMLContent = GetHTMLContent(lstrxmlPathFromHTC, pstrCoversheetTemplateDocMasUID)
        End If

        Return lrtnHTMLContent
    End Function

    Private Function GetTransmitalObject(pstrOBID As String) As IObject
        Dim lobjTransmittal As IObject = Nothing
        SPFRequestContext.Instance.IgnoreConfiguration = True
        SPFRequestContext.Instance.QueryRequest.AddQueryInterface("IHTCTransmittal")
        SPFRequestContext.Instance.QueryRequest.AddQueryPropComparisons("IObject~OBID~=~" + pstrOBID)

        Dim lobjDIc As IObjectDictionary = SPFRequestContext.Instance.QueryRequest.RunByAdvancedQuery()
        If (lobjDIc.Count > 0) Then
            lobjTransmittal = lobjDIc(0)
        End If
        Return lobjTransmittal
    End Function

    Private Function CreateHTMLTOHTCXML(ByVal pobjTransmital As IObject, ByVal pstrURL As String, ByVal pstrCurrentConfigName As String, ByVal pstrCurrentConfigUID As String) As String
        Dim lstrCoversheetXMLFilePath As String = Path.GetTempPath() + Guid.NewGuid().ToString() + ".xml"
        Dim TransmittalCreationDate As DateTime = pobjTransmital.CreationDate.Date
        Dim dateString1 = TransmittalCreationDate.ToString("yyyy'년' M'월' d'일'", CultureInfo.CurrentCulture)
        Dim Receiver As String = String.Empty
        Dim Sender As String = String.Empty
        Dim CopyingUser As String = String.Empty
        Dim IssueUser As String = String.Empty
        Dim Notes As String = String.Empty
        Dim IssueDate As DateTime
        Dim IssueDateConvertedSTring As String = String.Empty
        Dim Note1 As String = String.Empty
        Dim Note2 As String = String.Empty
        Dim Note3 As String = String.Empty
        Dim Note4 As String = String.Empty
        Dim Note5 As String = String.Empty
        Dim Note6 As String = String.Empty
        Dim Title As String = String.Empty
        If pobjTransmital.Interfaces("IHTCTransmittal").Properties("HTCTransmittalReceiver", False) IsNot Nothing Then
            Receiver = pobjTransmital.Interfaces("IHTCTransmittal").Properties("HTCTransmittalReceiver").Value
        End If

        If pobjTransmital.Interfaces("IHTCTransmittal").Properties("HTCTransmittalSender", False) IsNot Nothing Then
            Sender = pobjTransmital.Interfaces("IHTCTransmittal").Properties("HTCTransmittalSender").Value
        End If

        If pobjTransmital.Interfaces("IHTCTransmittal").Properties("HTCTransmittalCopyingUser", False) IsNot Nothing Then
            CopyingUser = pobjTransmital.Interfaces("IHTCTransmittal").Properties("HTCTransmittalCopyingUser").Value
        End If
        If pobjTransmital.Interfaces("IHTCTransmittal").Properties("HTCTranmittalIssuer", False) IsNot Nothing Then
            IssueUser = pobjTransmital.Interfaces("IHTCTransmittal").Properties("HTCTranmittalIssuer").Value
        End If
        If pobjTransmital.Interfaces("IHTCTransmittal").Properties("HTCTransmittalTitle", False) IsNot Nothing Then
            Title = pobjTransmital.Interfaces("IHTCTransmittal").Properties("HTCTransmittalTitle").Value
        End If

        If (pobjTransmital.Interfaces("ISPFTransmittal").Properties("SPFIssueDate") IsNot Nothing) Then
            IssueDate = pobjTransmital.Interfaces("ISPFTransmittal").Properties("SPFIssueDate").Value.ToString
            IssueDateConvertedSTring = IssueDate.ToString("MMM d, yyyy", CultureInfo.CurrentCulture)
        End If
        If pobjTransmital.Interfaces("IHTCTransmittal").Properties("HTCTransmittalNotes", False) IsNot Nothing Then
            Notes = pobjTransmital.Interfaces("IHTCTransmittal").Properties("HTCTransmittalNotes").Value
            Dim NoteValues As String() = Notes.Split(vbLf)
            If (NoteValues.Count > 0) Then
                If (NoteValues.Count > 0) Then
                    Note1 = NoteValues(0)
                End If
                If (NoteValues.Count > 1) Then
                    Note2 = NoteValues(1)
                End If
                If (NoteValues.Count > 2) Then
                    Note3 = NoteValues(2)
                End If
                If (NoteValues.Count > 3) Then
                    Note4 = NoteValues(3)
                End If
                If (NoteValues.Count > 4) Then
                    Note5 = NoteValues(4)
                End If
                If (NoteValues.Count > 5) Then
                    Note6 = NoteValues(5)
                End If
            End If
        End If
        Dim RelatedDiscplines As IObjectDictionary = Nothing
        Dim UnRelatedDisciplines As List(Of IObject) = Nothing
        Dim RelatedDocuments As IObjectDictionary = Nothing
        Dim RelatedReasonForIssue As IObjectDictionary = Nothing
        Dim UnRelatedReasonForIssue As List(Of IObject) = Nothing
        Dim ALLDisciplineObjects As IObjectDictionary = GetObjects("IHTCTransmittalDiscipline")
        If (pobjTransmital.GetEnd1Relationships.GetRels("HTCTransmittalToDiscipline").Count > 0) Then
            RelatedDiscplines = pobjTransmital.GetEnd1Relationships.GetRels("HTCTransmittalToDiscipline").GetEnd2s
        End If
        If (RelatedDiscplines.Count > 0 AndAlso ALLDisciplineObjects.Count > 0) Then
            UnRelatedDisciplines = ALLDisciplineObjects.Where(Function(a) Not RelatedDiscplines.Contains(a)).ToList
        End If
        If (pobjTransmital.GetEnd1Relationships.GetRels("HTCTransmittalDocument").Count > 0) Then
            RelatedDocuments = pobjTransmital.GetEnd1Relationships.GetRels("HTCTransmittalDocument").GetEnd2s
        End If
        Dim ALLReasonForIssue As IObjectDictionary = GetObjects("IHTCIssuePurpose")
        If (pobjTransmital.GetEnd1Relationships.GetRels("HTCTransmittalToIssuePurpose").Count > 0) Then
            RelatedReasonForIssue = pobjTransmital.GetEnd1Relationships.GetRels("HTCTransmittalToIssuePurpose").GetEnd2s
        End If
        If (RelatedReasonForIssue.Count > 0 AndAlso ALLReasonForIssue.Count > 0) Then
            UnRelatedReasonForIssue = ALLReasonForIssue.Where(Function(a) Not RelatedReasonForIssue.Contains(a)).ToList
        End If


        Dim doc As XmlDocument = New XmlDocument()
        Dim xmlDeclaration As XmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", Nothing)
        Dim root As XmlElement = doc.DocumentElement
        doc.InsertBefore(xmlDeclaration, root)

        Dim lnodTransmittal, lnodDisicplines, lnodReasonForIssue As XmlNode
        Dim lnodMainDetails, lnodIssueDetails, lnodlink, lnodRelatedDisicplines, lnodNotes, lnodDocuments, lnodDocument, lnodNonRelatedDisicplines, lnodSelectedReasonForIssue, lnodUnSelectedReasonForIssue As Xml.XmlElement
        lnodTransmittal = doc.CreateElement("Transmittal")
        lnodMainDetails = doc.CreateElement("MainDetails")
        lnodMainDetails.SetAttribute("Name", pobjTransmital.Name)
        lnodMainDetails.SetAttribute("Title", Title)
        lnodMainDetails.SetAttribute("Receiver", Receiver)
        lnodMainDetails.SetAttribute("Sender", Sender)
        lnodMainDetails.SetAttribute("CopyUser", CopyingUser)
        lnodMainDetails.SetAttribute("Date", dateString1)
        lnodTransmittal.AppendChild(lnodMainDetails)

        lnodDisicplines = doc.CreateElement("Disciplines")
        If (RelatedDiscplines IsNot Nothing) Then
            With RelatedDiscplines.GetEnumerator
                While .MoveNext
                    lnodRelatedDisicplines = doc.CreateElement("RelatedDisciplines")
                    lnodRelatedDisicplines.SetAttribute("Name", .Value.Name)
                    lnodDisicplines.AppendChild(lnodRelatedDisicplines)
                End While
            End With
        End If
        If (UnRelatedDisciplines IsNot Nothing) Then
            With UnRelatedDisciplines.GetEnumerator
                While .MoveNext
                    lnodNonRelatedDisicplines = doc.CreateElement("NonRelatedDisciplines")
                    lnodNonRelatedDisicplines.SetAttribute("Name", .Current.Name)
                    lnodDisicplines.AppendChild(lnodNonRelatedDisicplines)
                End While
            End With
        End If

        lnodTransmittal.AppendChild(lnodDisicplines)
        lnodReasonForIssue = doc.CreateElement("ReasonForIssue")
        If (RelatedReasonForIssue IsNot Nothing) Then
            With RelatedReasonForIssue.GetEnumerator
                While .MoveNext
                    lnodSelectedReasonForIssue = doc.CreateElement("SelectedRFI")
                    lnodSelectedReasonForIssue.SetAttribute("Name", .Value.Description)
                    lnodReasonForIssue.AppendChild(lnodSelectedReasonForIssue)
                End While
            End With
        End If
        If (UnRelatedReasonForIssue IsNot Nothing) Then
            With UnRelatedReasonForIssue.GetEnumerator
                While .MoveNext
                    lnodUnSelectedReasonForIssue = doc.CreateElement("UnSelectedRFI")
                    lnodUnSelectedReasonForIssue.SetAttribute("Name", .Current.Description)
                    lnodReasonForIssue.AppendChild(lnodUnSelectedReasonForIssue)
                End While
            End With
        End If

        lnodTransmittal.AppendChild(lnodReasonForIssue)
        lnodNotes = doc.CreateElement("Notes")
        lnodNotes.SetAttribute("Note1", Note1)
        lnodNotes.SetAttribute("Note2", Note2)
        lnodNotes.SetAttribute("Note3", Note3)
        lnodNotes.SetAttribute("Note4", Note4)
        lnodNotes.SetAttribute("Note5", Note5)
        lnodNotes.SetAttribute("Note6", Note6)
        lnodTransmittal.AppendChild(lnodNotes)
        lnodDocuments = doc.CreateElement("Documents")
        If (RelatedDocuments IsNot Nothing) Then
            lnodDocuments.SetAttribute("Count", RelatedDocuments.Count.ToString)

            With RelatedDocuments.GetEnumerator
                While .MoveNext
                    Dim Document As IObject = .Value
                    Dim REVNO As String = String.Empty
                    If (Document.Interfaces("ISPFDocumentRevision").Properties("SPFExternalRevision") IsNot Nothing) Then
                        REVNO = Document.Interfaces("ISPFDocumentRevision").Properties("SPFExternalRevision").Value.ToString
                    End If

                    Dim ReasonForIssue As String = String.Empty
                    If Document.GetEnd1Relationships.GetRel("HTCDocumentToIssuePurpose") IsNot Nothing Then
                        ReasonForIssue = Document.GetEnd1Relationships.GetRel("HTCDocumentToIssuePurpose").GetEnd2.Name.ToString
                    End If

                    lnodDocument = doc.CreateElement("Document")
                    lnodDocument.SetAttribute("Name", Document.Name)
                    lnodDocument.SetAttribute("Description", Document.Description)
                    lnodDocument.SetAttribute("RevNo", REVNO)
                    lnodDocument.SetAttribute("ReasonForIssue", ReasonForIssue)
                    lnodDocuments.AppendChild(lnodDocument)
                End While
            End With
        Else
            lnodDocuments.SetAttribute("Count", "0")
            'lnodDocument = doc.CreateElement("Document")
            'lnodDocument.SetAttribute("Name", "")
            'lnodDocument.SetAttribute("Description", "")
            'lnodDocument.SetAttribute("RevNo", "")
            'lnodDocument.SetAttribute("ReasonForIssue", "")
            'lnodDocuments.AppendChild(lnodDocument)

        End If
        'Dim myDelims As String() = New String() {"//"}
        'Dim HostSite As String = String.Empty
        'If (pstrURL.Contains("//")) Then
        '    HostSite = pstrURL.Split(myDelims, StringSplitOptions.None)(1)
        'End If


        lnodTransmittal.AppendChild(lnodDocuments)
        lnodlink = doc.CreateElement("Link")

        If (Not String.IsNullOrWhiteSpace(pstrURL) AndAlso Not String.IsNullOrWhiteSpace(pstrCurrentConfigUID) AndAlso Not String.IsNullOrWhiteSpace(pstrCurrentConfigName)) Then
            Dim lstrURL As String = pstrURL + "/#/results;queryFilter=%7B%22filters%22:%5B%5D,%22pageSize%22:25,%22page%22:1,%22ignoreQueryBySelectedConfig%22:true,%22relatedItemFilters%22:%5B%5D,%22columnSet%22:%22CS_HTCDocumentVersion%22,%22endInterface%22:%22IHTCDocumentRevision%22,%22expandFromObjectId%22:%22" + pobjTransmital.OBID + "%22,%22navigation%22:%22HTCIncomingDocuments_12%22,%22hasLink%22:true%7D;config=%5B%22" + pstrCurrentConfigUID + "%22%5D;title=" + pobjTransmital.Name + "%20Document?selectedConfig=" + pstrCurrentConfigName
            lnodlink.SetAttribute("url", lstrURL)
            lnodlink.SetAttribute("value", "Please click here")
            lnodlink.SetAttribute("text", "to check all documents issued.")
            lnodTransmittal.AppendChild(lnodlink)
        End If
        lnodIssueDetails = doc.CreateElement("IssueDetails")
        lnodIssueDetails.SetAttribute("IssueDate", IssueDateConvertedSTring.ToString)
        lnodIssueDetails.SetAttribute("IssueUser", IssueUser.ToString)
        lnodTransmittal.AppendChild(lnodIssueDetails)
        doc.AppendChild(lnodTransmittal)
        doc.Save(lstrCoversheetXMLFilePath)
        Return lstrCoversheetXMLFilePath
    End Function
    Private Function GetObjects(ByVal pstrInterface As String) As IObjectDictionary
        SPFRequestContext.Instance.QueryRequest.AddQueryInterface(pstrInterface)
        Dim lobjDIc As IObjectDictionary = SPFRequestContext.Instance.QueryRequest.RunByAdvancedQuery()
        Return lobjDIc
    End Function

    Private Function CreateHTMLFromHTCXML(ByVal pobjTransmital As IObject, ByVal pstrURL As String, ByVal pstrCurrentConfigName As String, ByVal pstrCurrentConfigUID As String) As String
        Dim lstrCoversheetXMLFilePath As String = Path.GetTempPath() + Guid.NewGuid().ToString() + ".xml"
        Dim TransmittalCreationDate As DateTime = pobjTransmital.CreationDate.Date
        Dim dateString1 = TransmittalCreationDate.ToString("yyyy'년' M'월' d'일'", CultureInfo.CurrentCulture)
        Dim Receiver As String = String.Empty
        Dim Sender As String = String.Empty
        Dim CopyingUser As String = String.Empty
        Dim IssueUser As String = String.Empty
        Dim Notes As String = String.Empty
        Dim Note1 As String = String.Empty
        Dim Note2 As String = String.Empty
        Dim Note3 As String = String.Empty
        Dim Note4 As String = String.Empty
        Dim Note5 As String = String.Empty
        Dim Note6 As String = String.Empty
        Dim Note7 As String = String.Empty
        Dim Note8 As String = String.Empty
        Dim Title As String = String.Empty
        If pobjTransmital.Interfaces("IHTCTransmittal").Properties("HTCTransmittalReceiver", False) IsNot Nothing Then
            Receiver = pobjTransmital.Interfaces("IHTCTransmittal").Properties("HTCTransmittalReceiver").Value
        End If

        If pobjTransmital.Interfaces("IHTCTransmittal").Properties("HTCTransmittalSender", False) IsNot Nothing Then
            Sender = pobjTransmital.Interfaces("IHTCTransmittal").Properties("HTCTransmittalSender").Value
        End If

        If pobjTransmital.Interfaces("IHTCTransmittal").Properties("HTCTransmittalCopyingUser", False) IsNot Nothing Then
            CopyingUser = pobjTransmital.Interfaces("IHTCTransmittal").Properties("HTCTransmittalCopyingUser").Value
        End If
        If pobjTransmital.Interfaces("IHTCTransmittal").Properties("HTCTranmittalIssuer", False) IsNot Nothing Then
            IssueUser = pobjTransmital.Interfaces("IHTCTransmittal").Properties("HTCTranmittalIssuer").Value
        End If
        If pobjTransmital.Interfaces("IHTCTransmittal").Properties("HTCTransmittalTitle", False) IsNot Nothing Then
            Title = pobjTransmital.Interfaces("IHTCTransmittal").Properties("HTCTransmittalTitle").Value
        End If
        If pobjTransmital.Interfaces("IHTCTransmittal").Properties("HTCTransmittalNotes", False) IsNot Nothing Then
            Notes = pobjTransmital.Interfaces("IHTCTransmittal").Properties("HTCTransmittalNotes").Value

            Dim NoteValues As String() = Notes.Split(vbLf)
            If (NoteValues.Count > 0) Then

                If (NoteValues.Count > 0) Then
                    Note1 = NoteValues(0)
                End If
                If (NoteValues.Count > 1) Then
                    Note2 = NoteValues(1)
                End If
                If (NoteValues.Count > 2) Then
                    Note3 = NoteValues(2)
                End If
                If (NoteValues.Count > 3) Then
                    Note4 = NoteValues(3)
                End If
                If (NoteValues.Count > 4) Then
                    Note5 = NoteValues(4)
                End If
                If (NoteValues.Count > 5) Then
                    Note6 = NoteValues(5)
                End If
                If (NoteValues.Count > 6) Then
                    Note7 = NoteValues(6)
                End If
                If (NoteValues.Count > 7) Then
                    Note8 = NoteValues(7)
                End If
            End If
        End If
        Dim RelatedDiscplines As IObjectDictionary = Nothing
        Dim UnRelatedDisciplines As List(Of IObject) = Nothing
        Dim RelatedDocuments As IObjectDictionary = Nothing
        Dim AttachedFiles As IObjectDictionary = Nothing
        Dim TransmitalType As String = String.Empty
        Dim lobjScopedByEnumList As IObject = Nothing
        Dim lobjentries As IObjectDictionary = Nothing
        Dim UnRelatedEntries As List(Of IObject) = Nothing
        Dim ALLDisciplineObjects As IObjectDictionary = GetObjects("IHTCTransmittalDiscipline")
        If (pobjTransmital.GetEnd1Relationships.GetRels("HTCTransmittalToDiscipline").Count > 0) Then
            RelatedDiscplines = pobjTransmital.GetEnd1Relationships.GetRels("HTCTransmittalToDiscipline").GetEnd2s
        End If
        If (RelatedDiscplines.Count > 0 AndAlso ALLDisciplineObjects.Count > 0) Then
            UnRelatedDisciplines = ALLDisciplineObjects.Where(Function(a) Not RelatedDiscplines.Contains(a)).ToList
        End If
        If (pobjTransmital.GetEnd2Relationships.GetRels("SPFFileComposition").Count > 0) Then
            AttachedFiles = pobjTransmital.GetEnd2Relationships.GetRels("SPFFileComposition").GetEnd1s()
        End If
        If (pobjTransmital.Interfaces("IHTCTransmittal").Properties("HTCTransmittalType") IsNot Nothing) Then
            TransmitalType = pobjTransmital.Interfaces("IHTCTransmittal").Properties("HTCTransmittalType").ToDisplayValue
        End If

        Dim lobjTransmitalTypeProperty As IObject = GetPropertyDef("HTCTransmittalType")
        If (lobjTransmitalTypeProperty IsNot Nothing AndAlso Not String.IsNullOrEmpty(TransmitalType)) Then
            lobjScopedByEnumList = lobjTransmitalTypeProperty.GetEnd1Relationships.GetRel("ScopedBy").GetEnd2
            lobjentries = lobjScopedByEnumList.GetEnd1Relationships.GetRels("Contains").GetEnd2s
            UnRelatedEntries = lobjentries.Where(Function(a) (a.Name <> TransmitalType)).ToList
        End If

        Dim doc As XmlDocument = New XmlDocument()
        Dim xmlDeclaration As XmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", Nothing)
        Dim root As XmlElement = doc.DocumentElement
        doc.InsertBefore(xmlDeclaration, root)

        Dim lnodTransmittal, lnodDisicplines, lnodTransmittalType As XmlNode
        Dim lnodMainDetails, lnodRelatedDisicplines, lnodNotes, lnodlink, lnodAttachedFiles, lnodAttachedFile, lnodNonRelatedDisicplines, lnodSelectedTransmitalType, lnodUnSelectedTransmitalType As Xml.XmlElement
        lnodTransmittal = doc.CreateElement("Transmittal")
        lnodMainDetails = doc.CreateElement("MainDetails")
        lnodMainDetails.SetAttribute("Name", pobjTransmital.Name)
        lnodMainDetails.SetAttribute("Title", Title)
        lnodMainDetails.SetAttribute("Receiver", Receiver)
        lnodMainDetails.SetAttribute("Sender", Sender)
        lnodMainDetails.SetAttribute("CopyUser", CopyingUser)
        lnodMainDetails.SetAttribute("Date", dateString1)
        lnodTransmittal.AppendChild(lnodMainDetails)

        lnodDisicplines = doc.CreateElement("Disciplines")
        If (RelatedDiscplines IsNot Nothing) Then
            With RelatedDiscplines.GetEnumerator
                While .MoveNext
                    lnodRelatedDisicplines = doc.CreateElement("RelatedDisciplines")
                    lnodRelatedDisicplines.SetAttribute("Name", .Value.Name)
                    lnodDisicplines.AppendChild(lnodRelatedDisicplines)
                End While
            End With
        End If

        If (UnRelatedDisciplines IsNot Nothing) Then
            With UnRelatedDisciplines.GetEnumerator
                While .MoveNext
                    lnodNonRelatedDisicplines = doc.CreateElement("NonRelatedDisciplines")
                    lnodNonRelatedDisicplines.SetAttribute("Name", .Current.Name)
                    lnodDisicplines.AppendChild(lnodNonRelatedDisicplines)
                End While
            End With
        End If
        lnodTransmittal.AppendChild(lnodDisicplines)

        lnodTransmittalType = doc.CreateElement("TransmittalType")
        lnodSelectedTransmitalType = doc.CreateElement("SelectedTransmittalType")
        lnodSelectedTransmitalType.SetAttribute("Name", TransmitalType)
        lnodTransmittalType.AppendChild(lnodSelectedTransmitalType)
        If (UnRelatedEntries IsNot Nothing) Then
            With UnRelatedEntries.GetEnumerator
                While .MoveNext
                    lnodUnSelectedTransmitalType = doc.CreateElement("UnSelectedTransmittalType")
                    lnodUnSelectedTransmitalType.SetAttribute("Name", .Current.Name)
                    lnodTransmittalType.AppendChild(lnodUnSelectedTransmitalType)
                End While
            End With
        End If
        lnodTransmittal.AppendChild(lnodTransmittalType)
        lnodNotes = doc.CreateElement("Notes")
        lnodNotes.SetAttribute("Note1", Note1)
        lnodNotes.SetAttribute("Note2", Note2)
        lnodNotes.SetAttribute("Note3", Note3)
        lnodNotes.SetAttribute("Note4", Note4)
        lnodNotes.SetAttribute("Note5", Note5)
        lnodNotes.SetAttribute("Note6", Note6)
        lnodNotes.SetAttribute("Note7", Note7)
        lnodNotes.SetAttribute("Note8", Note8)
        lnodTransmittal.AppendChild(lnodNotes)
        lnodAttachedFiles = doc.CreateElement("AttachedFiles")
        Dim count As Integer = 0
        If (AttachedFiles IsNot Nothing) Then
            With AttachedFiles.GetEnumerator
                While .MoveNext
                    count += 1
                    Dim AttachedFile As IObject = .Value
                    lnodAttachedFile = doc.CreateElement("File")
                    lnodAttachedFile.SetAttribute("FileNo", count.ToString)
                    lnodAttachedFile.SetAttribute("FileName", AttachedFile.Name)
                    lnodAttachedFiles.AppendChild(lnodAttachedFile)
                End While
            End With
            'Else
            '    lnodAttachedFile = doc.CreateElement("File")
            '    lnodAttachedFile.SetAttribute("FileNo", "")
            '    lnodAttachedFile.SetAttribute("FileName", "")
            '    lnodAttachedFiles.AppendChild(lnodAttachedFile)
        End If
        lnodTransmittal.AppendChild(lnodAttachedFiles)

        'Dim myDelims As String() = New String() {"//"}
        'Dim HostSite As String = String.Empty
        'If (pstrURL.Contains("//")) Then

        '    HostSite = pstrURL.Split(myDelims, StringSplitOptions.None)(1)
        'End If

        If (Not String.IsNullOrWhiteSpace(pstrURL) AndAlso Not String.IsNullOrWhiteSpace(pstrCurrentConfigUID) AndAlso Not String.IsNullOrWhiteSpace(pstrCurrentConfigName)) Then
            Dim lstrURL As String = pstrURL + "/#/results;queryFilter=%7B%22filters%22:%5B%5D,%22pageSize%22:25,%22page%22:1,%22ignoreQueryBySelectedConfig%22:true,%22relatedItemFilters%22:%5B%5D,%22columnSet%22:%22CS_SPFBusinessFile%22,%22endInterface%22:%22ISPFBusinessFile%22,%22expandFromObjectId%22:%22" + pobjTransmital.OBID + "%22,%22navigation%22:%22SPFFileComposition_21%22,%22hasLink%22:true%7D;config=%5B%22" + pstrCurrentConfigUID.ToString() + "%22%5D;title=" + pobjTransmital.Name + "%20All%20Files?selectedConfig=" + pstrCurrentConfigName.ToString()
            lnodlink = doc.CreateElement("Link")
            lnodlink.SetAttribute("url", lstrURL)
            lnodlink.SetAttribute("value", "Please click here")
            lnodlink.SetAttribute("text", "to check detail information of this Transmittal.")
            lnodTransmittal.AppendChild(lnodlink)
        End If
        doc.AppendChild(lnodTransmittal)
        doc.Save(lstrCoversheetXMLFilePath)
        Return lstrCoversheetXMLFilePath
    End Function


    Private Function GetPropertyDef(PropertyName As String) As IObject
        Dim lobjProperty As IObject = Nothing
        SPFRequestContext.Instance.IgnoreConfiguration = True
        SPFRequestContext.Instance.QueryRequest.AddQueryInterface("IPropertyDef")
        SPFRequestContext.Instance.QueryRequest.AddQueryPropComparisons("IObject~Name~=~" + PropertyName)

        Dim lobjDIc As IObjectDictionary = SPFRequestContext.Instance.QueryRequest.RunByAdvancedQuery()
        If (lobjDIc.Count > 0) Then
            lobjProperty = lobjDIc(0)
        End If
        Return lobjProperty
    End Function

    Private Function GetHTMLContent(ByVal pstrCoversheetXMLDataFilePath As String, ByVal TemplateDocMasUID As String) As String
        Dim lrtnHTMLContent As String = String.Empty

        Try
            Dim lstrXSLTemplatePath As String = Path.GetTempPath() + Guid.NewGuid().ToString() & ".xsl"

            Try
                Dim ldicAttachedFiles As IObjectDictionary = GetAttachedFilesFromDocMas(TemplateDocMasUID)
                Dim lobjFileEnumerator As IObjectEnumerator = ldicAttachedFiles.GetEnumerator()

                While lobjFileEnumerator.MoveNext()
                    Dim lobjFile As IObject = lobjFileEnumerator.Value
                    Dim lobjSPFFile As ISPFFile = TryCast(lobjFile.Interfaces("ISPFFile"), ISPFFile)
                    Dim lobjFileType As IObject = lobjFile.GetEnd1Relationships().GetRel("SPFFileFileType").GetEnd2()

                    If lobjFileType IsNot Nothing Then

                        If lobjFileType.Name.StartsWith("xsl") Then
                            lobjSPFFile.Download(lstrXSLTemplatePath, False)
                        End If
                    End If
                End While

            Catch ex As Exception
            End Try

            Dim lobjXSLTransform As XslCompiledTransform = New XslCompiledTransform()

            Using sr As StreamReader = New StreamReader(lstrXSLTemplatePath)

                Using xr As XmlReader = XmlReader.Create(sr)
                    lobjXSLTransform.Load(xr)
                End Using
            End Using

            Using sr As StreamReader = New StreamReader(pstrCoversheetXMLDataFilePath)

                Using xr As XmlReader = XmlReader.Create(sr)

                    Using sw As StringWriter = New StringWriter()
                        lobjXSLTransform.Transform(xr, Nothing, sw)
                        lrtnHTMLContent = sw.ToString()
                    End Using
                End Using
            End Using

        Catch ex As Exception
        End Try

        Return lrtnHTMLContent
    End Function

    Private Function GetAttachedFilesFromDocMas(ByVal pstrDocMasKey As String) As IObjectDictionary
        Dim lrtnAttachedFiles As IObjectDictionary = New ObjectDictionary()
        Tracing.Info(TracingTypes.Custom, String.Format("Searching document master with key - {0}", pstrDocMasKey))
        Dim lobjAdvancedQueryForFilesByOBID As DynamicQuery = New DynamicQuery()
        lobjAdvancedQueryForFilesByOBID.Query.Criteria = ObjectCriteria.HasInterface("ISPFDocumentMaster")
        lobjAdvancedQueryForFilesByOBID.Query.Criteria = lobjAdvancedQueryForFilesByOBID.Query.Criteria And (ObjectCriteria.[Property]("UID").InList(pstrDocMasKey) OrElse ObjectCriteria.[Property]("OBID").InList(pstrDocMasKey) OrElse ObjectCriteria.[Property]("Name").InList(pstrDocMasKey))
        Dim ldicDocTemplateMas As IObjectDictionary = lobjAdvancedQueryForFilesByOBID.ExecuteToIObjectDictionary()
        Tracing.Info(TracingTypes.Custom, String.Format("No of document masters found - {0}", ldicDocTemplateMas.Count))
        If ldicDocTemplateMas.Any() = False Then Return lrtnAttachedFiles
        Dim lobjDocTemplateMas As IObject = ldicDocTemplateMas.FirstOrDefault()

        Try
            Dim lobjDocMas As ISPFDocumentMaster = TryCast(lobjDocTemplateMas.Interfaces("ISPFDocumentMaster"), ISPFDocumentMaster)
            Dim lobjDocRev As IObject = lobjDocMas.GetNewestRevision()
            Dim lobjSPFDocRev As ISPFDocumentRevision = TryCast(lobjDocRev.Interfaces("ISPFDocumentRevision"), ISPFDocumentRevision)
            Dim lobjDocVer As IObject = lobjSPFDocRev.GetNewestVersion()
            Dim lobjSPFDocVer As ISPFFileComposition = TryCast(lobjDocVer.Interfaces("ISPFFileComposition"), ISPFFileComposition)
            lrtnAttachedFiles = lobjSPFDocVer.GetAllFiles()
            Tracing.Info(TracingTypes.Custom, String.Format("No of files attached - {0}", lrtnAttachedFiles.Count))
        Catch ex As Exception
            Tracing.[Error](TracingTypes.Custom, ex)
        End Try

        Return lrtnAttachedFiles
    End Function

    Private Function GetMethodObject(ByVal MethodName As String) As ISPFMethod
        Dim lobjMethod As ISPFMethod = Nothing
        SPFRequestContext.Instance.IgnoreConfiguration = True
        SPFRequestContext.Instance.QueryRequest.AddQueryInterface("ISPFMethod")
        SPFRequestContext.Instance.QueryRequest.AddQueryPropComparisons("IObject~Name~=~" & MethodName)
        Dim lobjDIc As IObjectDictionary = SPFRequestContext.Instance.QueryRequest.RunByAdvancedQuery()
        If (lobjDIc.Count > 0) Then lobjMethod = CType(lobjDIc(0).ToInterface("ISPFMethod"), ISPFMethod)
        Return lobjMethod
    End Function
End Class
