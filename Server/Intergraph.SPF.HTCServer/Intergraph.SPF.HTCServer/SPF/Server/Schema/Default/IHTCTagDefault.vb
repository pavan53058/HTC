' --------------------------------------------------------------
' SmartPlant Foundation Generated Code
' 02-01-2015 20:51:23
'
' --------------------------------------------------------------
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


Namespace SPF.Server.Schema.Interface.Default

    Public Class IHTCTagDefault
        Inherits IHTCTagBase

#Region "Constructors"

        Public Sub New(ByVal pblnInstantiateRequiredItems As Boolean)
            MyBase.New(pblnInstantiateRequiredItems)
        End Sub

#End Region

#Region "Members"

#End Region

#Region "Overrides"
        Public Overrides Sub OnCreatingValidation(e As CancelEventArgs)
            MyBase.OnCreatingValidation(e)
            ''Dim str = Me.Name
            If Me.GetPrimaryClassification.UID <> "HTCTAGCLS_piping_1" Then


                Dim lobjDynamicQuery As New DynamicQuery()
                SPFRequestContext.Instance.ProcessCache.Contains(Me.UID)
                lobjDynamicQuery.Query.Criteria = New ObjectCriteria(Function(x) x.ClassDef = "HTCTag" AndAlso x.Name = Me.Name)

                If lobjDynamicQuery.ExecuteCount() > 0 Then
                    e.CancelException = New SPFException(99999, Me.Name & ":Tag already exist,cannot create tag with duplicate name")
                    e.Cancel = True
                End If


            End If

        End Sub

#End Region

    End Class

End Namespace


