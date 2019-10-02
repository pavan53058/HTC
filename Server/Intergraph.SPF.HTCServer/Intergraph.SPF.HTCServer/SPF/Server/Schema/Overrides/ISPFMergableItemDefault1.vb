
'Imports SPF.Server.Configuration

'Imports SPF.Server.Schema.Collections
'Imports SPF.Server.Schema.Interface.Generated
'Imports SPF.Server.Schema.Model


'Namespace SPF.Server.Schema.Interface.Default


'    Public Class ISPFMergableItemDefault1
'        Inherits ISPFMergableItemDefault
'        Public Sub New(ByVal pblnInstantiateRequiredItems As Boolean)
'            MyBase.New(pblnInstantiateRequiredItems)

'        End Sub

'        Public Overrides Sub OnMergingObject(pobjMergeToObject As IObject)

'            MyBase.OnMergingObject(pobjMergeToObject)

'            ''Set the Master document status to "AsBuilt" before merging the project
'            If pobjMergeToObject.IsTypeOf("IHTCDocumentRevision") Then
'                Dim lobjMaster = CType(pobjMergeToObject.ToInterface("ISPFDocumentRevision"), ISPFDocumentRevision).GetDocumentMaster
'                lobjMaster.BeginUpdate()
'                lobjMaster.ToInterface("IHTCDocumentCommon").Properties("HTCDocumentStatus").SetValue("HTCENUM_AsBuilt")
'                lobjMaster.FinishUpdate()
'            End If

'        End Sub

'    End Class
'End Namespace