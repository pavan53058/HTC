Imports SPF.Server.Schema.Interface.Default
Imports SPF.Server.Context
Imports SPF.Server.Schema.Collections
Imports SPF.Server.QueryClasses
Imports SPF.Common.DataAccessLayer.DataRetrieval
Imports SPF.Server.Schema.Interface.Generated
Imports SPF.Common.DataAccessLayer.DBConnection
Imports SPF.Server
Namespace SPF.Server.Schema.Interface.Default
    Public Class ISPFDocumentRevisionDefailt1
        Inherits ISPFDocumentRevisionDefault

        Sub New(ByVal pblnInstantiateRequiredItems As Boolean)

            MyBase.New(pblnInstantiateRequiredItems)
        End Sub
        Overrides Function ReviseDocument(ByVal pobjReviseArgs As ReviseArgs) As IObjectDictionary
            Dim lobjRevScheme As ISPFRevisionScheme = Nothing
            Dim lstrNextMajorRev As String = ""
            Dim lstrNextMinorRev As String = ""
            If pobjReviseArgs Is Nothing Then
                lobjRevScheme = Me.GetRevisionScheme()
                lstrNextMajorRev = Me.SPFMajorRevision
                lstrNextMinorRev = Me.SPFMinorRevision
                '
                ' Get next revision from sequence
                '
                'lobjRevScheme.GetNextRevisionFromSequence(lstrNextMajorRev, lstrNextMinorRev, True, True)
                CType(lobjRevScheme.ToInterface("ISPFRevisionScheme"), ISPFRevisionScheme).GetNextRevisionFromSequence(lstrNextMajorRev, lstrNextMinorRev, True, True)
                ''Tracing.Info(TracingTypes.General, "Got next major rev and minor rev from the sequence. lstrNextMajorRev = " & lstrNextMajorRev & " lstrNextMinorRev " & lstrNextMinorRev)
            Else
                lobjRevScheme = pobjReviseArgs.GetRevisionScheme
                lstrNextMajorRev = pobjReviseArgs.SPFMajorRevision
                lstrNextMinorRev = pobjReviseArgs.SPFMinorRevision
            End If

            Dim lobjRevObj As ISPFDocumentRevision = CType(Me.ToInterface("ISPFDocumentRevision"), ISPFDocumentRevision)
            Dim lobjMasterObj As ISPFDocumentMaster = lobjRevObj.GetDocumentMaster
            Dim lobjRevisionClassDef As IClassDef = lobjRevObj.GetClassDefinition()
            Dim lobjNewRev As ISPFDocumentRevision = CType(lobjRevisionClassDef.BeginCreate(True).ToInterface("ISPFDocumentRevision"), ISPFDocumentRevision)
            '' Tracing.Warning(TracingTypes.General, "New revision object < " & lobjNewRev.Name & " > is instantiated")
            '
            ' Populate the properties on the new object attributes from selected object
            '
            lobjNewRev.Name = lobjRevObj.Name
            '
            ' jshort 15/06/2016 TR-AM-106679 - Don't add a blank description if the source revision didn't have one.
            '
            If lobjRevObj.Interfaces("IObject").Properties.Contains("Description") Then
                lobjNewRev.Description = lobjRevObj.Description
            End If
            lobjNewRev.UID = System.Guid.NewGuid.ToString
            '
            ' Set the new property values
            '
            lobjNewRev.LastUpdatedDate = CreationDate 'Set creation date
            lobjNewRev.SPFRevState = "e1WORKING" 'Set revise state
            lobjNewRev.SPFMajorRevision = lstrNextMajorRev 'Set major revision
            lobjNewRev.SPFMinorRevision = lstrNextMinorRev 'Set minor revision
            lobjNewRev.SPFExternalRevision = lstrNextMajorRev & lstrNextMinorRev 'Set external revision
            lobjNewRev.SPFRevIssueDate = Nothing ' Set to null          
            If pobjReviseArgs Is Nothing OrElse pobjReviseArgs.ValidateRevision Then
                ' Modified by Sujana - 29-Feb-2012 For TR-AM-60881 - Docs with the same name/number but with different Unique Key cannot be revised   
                ' Using new "AdvancedQuery" for querying the revision and also added the "ClassDefinitionUID" query criteria. So that revisions created
                ' using different classdefs gets filtered while query
                '
                ' Instantiate the QueryCriteria
                '
                Dim lobjQueryCriteria As New AdvancedQuery()
                '
                ' Add IObject InterfaceDef to the query criteria
                '
                lobjQueryCriteria.AddQueryCriteria("IObject", "Name", QueryProperty.OperatorType.IS_EQUAL, lobjNewRev.Name)
                lobjQueryCriteria.AddQueryCriteria("IObject", "ObjDefUID", QueryProperty.OperatorType.IS_EQUAL, Me.ClassDefinitionUID)
                '
                ' Add ISPFDocumentRevision to the query criteria
                ' Add SPFMajorRevision to ISPFDocumentRevision IDef query
                '
                lobjQueryCriteria.AddQueryCriteria("ISPFDocumentRevision", "SPFMajorRevision", QueryProperty.OperatorType.IS_EQUAL, lstrNextMajorRev)
                '
                ' Add SPFMinorRevision to ISPFDocumentRevision IDef query
                '
                lobjQueryCriteria.AddQueryCriteria("ISPFDocumentRevision", "SPFMinorRevision", QueryProperty.OperatorType.IS_EQUAL, lstrNextMinorRev)
                '
                ' TR#76276 - Add the doc master to the query so we know we are only getting revisions relevant to the master
                '
                lobjQueryCriteria.AddRelItemCriteria("SPFDocumentRevisions", ExpandRelationDirection.UID2To1, "IObject", "OBID", QueryProperty.OperatorType.IS_EQUAL, lobjMasterObj.OBID)
                '
                ' Query for the matching revisions
                '
                Dim lobjObjectsToBeIndexed As IObjectDictionary = lobjQueryCriteria.QueryObjects(-1, New ContainerDictionary())
                '
                ' Validate the number of revisions
                '
                If lobjObjectsToBeIndexed IsNot Nothing AndAlso lobjObjectsToBeIndexed.Count > 0 Then
                    '' Tracing.Warning(TracingTypes.General, "Revision already exists")
                    Throw New SPFException(1500, "Revision already exists")
                End If
            End If

            Return MyBase.ReviseDocument(pobjReviseArgs)
        End Function
    End Class
End Namespace
