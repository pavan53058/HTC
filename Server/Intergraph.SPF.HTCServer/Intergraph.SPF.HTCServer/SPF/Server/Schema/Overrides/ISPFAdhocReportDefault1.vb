Option Explicit On
Option Strict On

Imports SPF.Server.Schema.Model
Imports SPF.Server.Schema.Model.PropertyTypes
Imports SPF.Server.Schema.Collections
Imports SPF.Server.Components.Reporting
Imports SPF.Server.Utilities.ReportingUtilities
Imports SPF.Server.Schema.Interface.Generated
Imports System.Xml
Imports System.IO
Imports SPF.Server.Components.Core.Serialization
Imports SPF.Server.Context
Imports System.Globalization
Imports SPF.Server.QueryEngine
Imports SPF.Diagnostics
Imports SPF.Server.Modules

Namespace SPF.Server.Schema.Interface.Default
    Public Class ISPFAdhocReportDefault1
        Inherits ISPFAdhocReportDefault

#Region " Constructors "

        Public Sub New(ByVal pblnInstantiateRequiredItems As Boolean)
            MyBase.New(pblnInstantiateRequiredItems)
        End Sub

#End Region

#Region " Public Overrides "

        Public Overrides Function WriteResultsToFile(pobjAdhocReportResultsArgs As SPF.Server.Schema.Model.AdhocReportResultsArgs) As String


            Select Case ExtractOuputMode(Me.SPFReportOutputTo)

                Case OutputMode.XHTMLWORD

                    Return MyBase.WriteResultsToFile(pobjAdhocReportResultsArgs)

                Case Else
                    Return MyBase.WriteResultsToFile(pobjAdhocReportResultsArgs)
            End Select
        End Function
        Public Overrides Sub SerializeReportResultsToStream(ByVal pobjReportResults As System.Collections.CollectionBase, ByRef pobjStream As System.IO.Stream)
            MyBase.SerializeReportResultsToStream(pobjReportResults, pobjStream)
        End Sub

        Public Overrides Function CreateReportResultsFile(ByVal pobjAdhocReportResultsArgs As Model.AdhocReportResultsArgs) As IObject
            MyBase.CreateReportResultsFile(pobjAdhocReportResultsArgs)
            Return Nothing
        End Function

#End Region

    End Class

End Namespace