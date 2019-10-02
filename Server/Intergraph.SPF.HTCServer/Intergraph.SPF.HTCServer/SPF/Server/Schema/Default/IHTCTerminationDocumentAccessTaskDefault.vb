Imports SPF.Diagnostics
Imports SPF.Server.Context
Imports SPF.Server.DataAccess
Imports SPF.Server.Schema.Collections
Imports SPF.Server.Schema.Interface
Imports SPF.Server.Schema.Interface.Generated
Imports SPF.Server.Schema.Interface.Default
Imports SPF.Server.Utilities
Imports SPF.Server.Utilities.GeneralUtilities
Imports SPF.Server.Schema.Model
Imports SPF.Server.Components.Core.APIs
Imports System.Windows
Imports SPF.Common.DataAccessLayer.DataRetrieval
Imports SPF.Server.QueryEngine
Imports SPF.Server.QueryClasses
Imports Intergraph.SPF.DAL.Common.Criteria.Object
Imports System.Data.SqlTypes
Imports SPF.Server.Schema.Model.PropertyTypes

Namespace SPF.Server.Schema.Interface.Default
    Public Class IHTCTerminateDocumentUserAccessTaskDefault
        Inherits IHTCTerminateDocumentUserAccessTaskBase

#Region " Constructors "
        Public Sub New(ByVal pblnInstantiateRequiredItems As Boolean)
            MyBase.New(pblnInstantiateRequiredItems)
        End Sub

#End Region
        Public Overrides Sub OnProcess()
            Dim lobjSW As New Stopwatch
            lobjSW.Start()
            Tracing.Info(TracingTypes.Custom, "START : Scheduler Task IHTCTerminationDocumentAccessTask OnProcess()" + DateTime.Now.ToString)
            'Terminate the Document User Access if difference between the creation Date and Current Date is more than
            TerminateDocumentAccessToUser()
            MyBase.OnProcess()
            lobjSW.Stop()
            Tracing.Info(TracingTypes.Custom, "Scheduler Task IHTCTerminationDocumentAccessTask finished" + DateTime.Now.ToString)
            Tracing.Info(TracingTypes.Custom, String.Format("END : Elapsed time in milliseconds : " + lobjSW.Elapsed.TotalMilliseconds.ToString()))
        End Sub

        Private Sub TerminateDocumentAccessToUser()
            Try
                If Not (SPFRequestContext.Instance.Transaction.InTransaction) Then
                    SPFRequestContext.Instance.Transaction.Begin()
                End If
                Tracing.Info(TracingTypes.Custom, "Getting the relationships between User and Documents")

                'Getting the relationships between User and Document
                SPFRequestContext.Instance.QueryRequest.AddQueryRelProperty("IRel", "DefUID", "HTCUserDownloadableDocumentRevision")
                Dim lobjRelationsBetweenUserAndDocument As IObjectDictionary = SPFRequestContext.Instance.QueryRequest.QueryForRelationship(QueryType.AdvancedRelQuery, "*", "DOC")
                Dim ObjectsToTerminate As List(Of IObject) = lobjRelationsBetweenUserAndDocument.Where(Function(x) (Date.UtcNow.Day - x.CreationDate.Date.Day > 7)).ToList()
                'looping through all relationships that has to be terminated
                With ObjectsToTerminate.GetEnumerator
                    While .MoveNext
                        Dim lobjectRel As IRel = .Current.Interfaces("IRel")
                        lobjectRel.Terminate()
                        Tracing.Info(TracingTypes.Custom, "Relationship has been terminated between an user " + lobjectRel.GetEnd1.Name + "and the document " + lobjectRel.GetEnd2.Name)
                    End While
                End With
                SPFRequestContext.Instance.Transaction.Commit()
                Tracing.Info(TracingTypes.Custom, "Transaction has been committed.")
            Catch ex As Exception
                Me.SPFSchTskFailureMsg = "Error occured while terminating the relation between an User and a document:" + ex.Message
                Throw New SPFException("9999", "Error occured while terminating the relation between an User and a document", ex)
            End Try
        End Sub


    End Class

End Namespace




