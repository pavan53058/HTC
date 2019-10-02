Option Explicit On
Option Strict On

Imports SPF.Server.Schema.Collections
Imports SPF.Server.Schema.Model
Imports SPF.Server.Schema.Interface.Generated
Imports SPF.Server.Context
Imports SPF.Server.DataAccess
Imports SPF.Diagnostics
Imports SPF.Server.QueryClasses
Imports Intergraph.SPF.DAL.Common.Criteria.Object
Imports SPF.Server.Utilities

Namespace SPF.Server.Schema.Interface.Default
    ''' <summary>
    ''' This override is to handle specific behaviours on master object
    ''' </summary>
    Public Class IHTCDocumentMasterDefault
        Inherits IHTCDocumentMasterBase

#Region "Constructors"

        Public Sub New(ByVal pblnInstantiateRequiredItems As Boolean)
            MyBase.New(pblnInstantiateRequiredItems)
        End Sub
#End Region

#Region "Members"
        Private pobjRevision As IObject = Nothing
        Private pobjJob As IObject = Nothing

#End Region

#Region "Overrides"
        ''' <summary>
        '''Get the old doc from some property
        '''copy the files
        '''attach the files to the new document
        '''This logic appies only to Vendor print documents and documents with "HTCOldDocumentName" proeprty set on doc master
        ''' </summary>
        ''' <param name="e"></param>
        Public Overrides Sub OnCreate(e As CreateEventArgs)
            MyBase.OnCreate(e)

            If (Me.Interfaces.Contains("IHTCDocumentMaster") AndAlso
                                   CType(Me.Interfaces("ISPFClassifiedItem"), ISPFClassifiedItem).GetPrimaryClassification.Name = "ELVPK") Then

                If (Me.Interfaces.Contains("IHTCDocumentCommon") AndAlso Me.Interfaces("IHTCDocumentCommon").Properties("HTCOldDocumentName") IsNot Nothing AndAlso
                    Not String.IsNullOrWhiteSpace(Me.Interfaces("IHTCDocumentCommon").Properties("HTCOldDocumentName").Value.ToString)) Then

                    ''Get Latest Revision
                    ''As this is a new document,there will be only revision
                    Dim lobjCurrentDocRevision = CType(Me.Interfaces("ISPFDocumentMaster"), ISPFDocumentMaster).GetDocumentRevisions.FirstOrDefault
                    ''Get Latest Version
                    Dim lobjCurrentDocVersion = CType(lobjCurrentDocRevision.ToInterface("ISPFDocumentRevision"), ISPFDocumentRevision).GetDocumentVersions.FirstOrDefault

                    Dim lstrOldDocMaster As String = Me.Interfaces("IHTCDocumentCommon").Properties("HTCOldDocumentName").Value.ToString

                    Dim lobjDocumentQuery As New DynamicQuery
                    lobjDocumentQuery.Query.Criteria = New ObjectCriteria(Function(x) x.HasInterface("IHTCDocumentMaster") And x.Name = lstrOldDocMaster)
                    Dim lobjMasterDocToTerminate = lobjDocumentQuery.ExecuteToIObjectDictionary.FirstOrDefault


                    If lobjMasterDocToTerminate IsNot Nothing Then
                        ''Get Latest Revision
                        Dim lobjOldDocRevision = CType(lobjMasterDocToTerminate.Interfaces("ISPFDocumentMaster"), ISPFDocumentMaster).GetLatestRevisions.FirstOrDefault
                        ''Get Latest Version
                        Dim lobjOldDocVersion = CType(lobjOldDocRevision.ToInterface("ISPFDocumentRevision"), ISPFDocumentRevision).GetLatestVersion.FirstOrDefault
                        Dim lcolDocVersionFiles = CType(lobjOldDocVersion.ToInterface("ISPFDocumentVersion"), ISPFDocumentVersion).GetAllFiles

                        With lcolDocVersionFiles.GetEnumerator
                            While .MoveNext
                                Dim lobjFile = CType(.Value.ToInterface("ISPFFile"), ISPFFile)
                                '
                                'Copy the file
                                '
                                Dim lobjOldFile As ISPFFile = CType(.Value.ToInterface("ISPFFile"), ISPFFile)
                                Dim lobjNewFile As IObject = lobjOldFile.Copy()
                                '
                                ' Create SPFFileFileType rel
                                '
                                Dim lobjFileFileType As IObject = GeneralUtilities.InstantiateRelation("SPFFileFileType", lobjNewFile, lobjOldFile.GetFileTypes.Item(0), False)
                                lobjFileFileType.GetClassDefinition.FinishCreate(lobjFileFileType)
                                '
                                ' Create SPFFileComposition relation
                                '
                                Dim lobjFileFileComposition As IObject = GeneralUtilities.InstantiateRelation("SPFFileComposition", lobjNewFile, lobjCurrentDocVersion, False)
                                lobjFileFileComposition.GetClassDefinition.FinishCreate(lobjFileFileComposition)
                            End While
                        End With
                    End If
                    '$$$$$$$$$$$$$$$$$$$$$$$$-TODO-$$$$$$$$$$$$$$$$$$$$$$$$$$$
                    ''delete the old Document??? 
                    ''Or link the old to new document???
                    ''Or add old document name as attribute on new document???
                    '$$$$$$$$$$$$$$$$$$$$$$$$-TODO-$$$$$$$$$$$$$$$$$$$$$$$$$$$

                End If
            End If
        End Sub

        Public Overrides Sub OnDeleting(e As CancelEventArgs)
            MyBase.OnDeleting(e)
            Dim lobjMaster = Me
            ''collect the revision doc and EW object,Otherwise we cannot expand during OnDelete method
            ReadDocumentDependencies()
        End Sub

        ''' <summary>
        ''' If a copied master document is getting deleted the remove the relation between the EW and original Document
        ''' Before Delete
        ''' 
        ''' EW====>Doc(revision)
        ''' ||      ||
        ''' Document Master(Copied)
        ''' 
        ''' After Delete
        ''' 
        ''' EW= X Delete this rel X =>Doc(revision)
        ''' XX      XX
        ''' Document Master(Copied)--Delete
        ''' 
        ''' </summary>
        ''' <param name="e"></param>
        Public Overrides Sub OnDelete(e As SuppressibleEventArgs)
            MyBase.OnDelete(e)

            DeleteTerminateEWToDocumentRevisionRel(True)
        End Sub

        Public Overrides Sub OnTerminating(e As CancelEventArgs)
            MyBase.OnTerminating(e)

            ReadDocumentDependencies()
        End Sub
        Public Overrides Sub OnTerminate(e As SuppressibleEventArgs)
            MyBase.OnTerminate(e)

            DeleteTerminateEWToDocumentRevisionRel(False)
        End Sub

#End Region

#Region "Private Methods"
        Private Sub ReadDocumentDependencies()
            Dim lobjMaster = Me
            If ValidateMasterDoc() Then
                If lobjMaster.GetEnd2Relationships.GetRel("HTCDocumentToCopiedDocument") IsNot Nothing AndAlso
                    lobjMaster.GetEnd2Relationships.GetRel("HTCDocumentToCopiedDocument").GetEnd1 IsNot Nothing Then
                    pobjRevision = lobjMaster.GetEnd2Relationships.GetRel("HTCDocumentToCopiedDocument").GetEnd1
                End If
                ''Cross verfy the the revision is actually has rel to EW!!
                If lobjMaster.GetEnd1Relationships.GetRel("HTCJobToCopiedDocument") IsNot Nothing AndAlso
                    lobjMaster.GetEnd1Relationships.GetRel("HTCJobToCopiedDocument").GetEnd2 IsNot Nothing Then
                    pobjJob = lobjMaster.GetEnd1Relationships.GetRel("HTCJobToCopiedDocument").GetEnd2
                End If
            End If
        End Sub

        Private Function ValidateMasterDoc() As Boolean
            Dim IsValidMaster = False

            If (Me.Interfaces.Contains("IHTCDocumentMaster") AndAlso
                Me.Interfaces.Contains("IHTCDocumentCommon") AndAlso
                Me.Interfaces("IHTCDocumentCommon").Properties("HTCDocumentStatus") IsNot Nothing AndAlso
                Me.Interfaces("IHTCDocumentCommon").Properties("HTCDocumentStatus").Value IsNot Nothing AndAlso
                Me.Interfaces("IHTCDocumentCommon").Properties("HTCDocumentStatus").Value.ToString = "HTCENUM_Copied") Then
                IsValidMaster = True
            End If

            Return IsValidMaster
        End Function
        ''this method can be called from either delete or terminate.
        Private Sub DeleteTerminateEWToDocumentRevisionRel(pboolIsdelete As Boolean)
            Dim lobjMaster = Me

            If ValidateMasterDoc() Then
                Try
                    ''Run the expand on the collected Doc revision and EW and delete the result
                    'Assuming the query woud result only one rel
                    If pobjRevision IsNot Nothing AndAlso pobjJob IsNot Nothing Then
                        SPFRequestContext.Instance.QueryRequest.AddQueryRelProperty("IRel", "DefUID", "HTCJobToDocument")
                        SPFRequestContext.Instance.QueryRequest.AddQueryRelProperty("IRel", "UID1", pobjRevision.UID)
                        SPFRequestContext.Instance.QueryRequest.AddQueryRelProperty("IRel", "UID2", pobjJob.UID)
                        Dim lobjRelationsBetweenJobAndRevision As IObjectDictionary = SPFRequestContext.Instance.QueryRequest.QueryForRelationship(QueryType.AdvancedRelQuery, "*", "DOC")

                        If lobjRelationsBetweenJobAndRevision.Count > 0 Then
                            If pboolIsdelete Then
                                lobjRelationsBetweenJobAndRevision.FirstOrDefault().Delete()
                            Else
                                lobjRelationsBetweenJobAndRevision.FirstOrDefault().Terminate()
                            End If

                        End If
                    Else
                        Tracing.Info(TracingTypes.Custom, "Not able to terminate the EW-->Doc(Revision),Either revision (or) EW object are missing")
                    End If

                Catch ex As Exception
                    Tracing.Error(TracingTypes.Custom, ex)
                    Throw ex
                End Try
            End If
        End Sub

#End Region


    End Class

End Namespace


