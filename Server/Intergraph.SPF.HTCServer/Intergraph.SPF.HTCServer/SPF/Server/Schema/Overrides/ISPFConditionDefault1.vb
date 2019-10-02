Option Explicit On
Option Strict On

Imports SPF.Server.Schema.Interface.Generated
Imports SPF.Server.Schema.Collections

Namespace SPF.Server.Schema.Interface.Default
    ''' <summary>
    ''' Override to handle the VP Document EWR count case
    ''' Group object attached Vendor prints should fall under 2 cases
    ''' Either all VPs should have one EWR number or none are attached to any EWR
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ISPFConditionDefault1
        Inherits ISPFConditionDefault

#Region " Constructors "

        Public Sub New(ByVal pblnInstantiateRequiredItems As Boolean)
            MyBase.New(pblnInstantiateRequiredItems)
        End Sub

#End Region

#Region "Overridden Methods"

        Public Overrides Function IsSatisfied(pobjGroupItem As Generated.IObject) As Boolean

            If Me.Name = "HTCCIRCLEIsGroupAttachedVPDocumentsHavingPartialEWRRel" Then

                If MyBase.IsSatisfied(pobjGroupItem) Or MyBase.IsSatisfied() Then

                    If pobjGroupItem.GetEnd1Relationships.GetRels("EDG_HTCVendorPrintEWRCountFromRequest").Count > 0 AndAlso
                       Not pobjGroupItem.GetEnd1Relationships.GetRels("HTCRequestGroupDocumentRevision").Any(Function(x) x.GetEnd1Relationships.GetRel("HTCDocumentEWRNumber") Is Nothing) Then
                        Return True
                    Else
                        Return False
                    End If
                Else
                    Return False
                End If
            Else
                Return MyBase.IsSatisfied(pobjGroupItem)
            End If
        End Function

#End Region

    End Class
End Namespace

