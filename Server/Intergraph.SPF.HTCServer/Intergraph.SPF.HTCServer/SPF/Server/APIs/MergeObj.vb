Option Strict On
Option Explicit On

Imports System.IO
Imports SPF.Server
Imports SPF.Server.Components.Core.APIs
Imports SPF.Server.Components.Core.Serialization
Imports SPF.Server.Schema.Collections
Imports SPF.Server.DataAccess
Imports SPF.Server.Schema
Imports SPF.Utilities
Imports System.Xml
Imports System.Timers
Imports SPF.Server.Classes.Configuration
Imports SPF.Server.Context
Imports SPF.Server.Utilities
Imports SPF.Server.Schema.Model
Imports SPF.Diagnostics
Imports SPF.Server.Schema.Interface.Generated

Namespace SPF.Server.APIs.ConfigManagement.Cust

    Public Class MergeObjs
        Inherits SPF.Server.APIs.ConfigManagement.MergeObjs

#Region " Members "

        Private mblnRunAsync As Boolean = False
        Private mobjNotification As IObject = Nothing
        Private mstrNotificationOBID As String = Nothing
        Private mstrNotificationDomainUID As String = Nothing
        Private mcolIDs As New Generic.Dictionary(Of String, String)
        Private mintObjCount As Integer = 0
        Private mintRelCount As Integer = 0
        Private mblnReportItemFailures As Boolean = False
        Private mintFailedItemCount As Integer = 0
        Private mstrChunkSize As String
        Private mblnIgnoreRequiredRelIfTerminated As Boolean

#End Region

#Region " Methods "

        ''' <remarks>
        ''' Modified daughey DI-AM-82752 25/02/2015 - Updated to extract the notification OBID and domain
        ''' Modified MProbyn CR-AM-88798 09/06/2015 - Pass through new method arg for suppressing merge of terminated rels
        ''' </remarks>
        Protected Overrides Sub OnDeSerialize()
            Dim lobjXMLRequestHelper As New XMLRequestHelper()
            ' DeSerializeObjects(lobjXMLRequestHelper.XMLSelectSingleNode("Objects"))

            For Each lnodObject As XmlNode In lobjXMLRequestHelper.XMLSelectSingleNode("Objects").ChildNodes
                mcolIDs.Add(lnodObject.Attributes("OBID").Value, lnodObject.Attributes("DomainUID").Value)
            Next
            '
            ' MProbyn CR 69241 17/04/2014 Get the obj and rel counts from the request
            '
            mintObjCount = CInt(lobjXMLRequestHelper.XMLSelectSingleNodeValue("ObjCount"))
            mintRelCount = CInt(lobjXMLRequestHelper.XMLSelectSingleNodeValue("RelCount"))

            ' mstrColumnSet = lobjXMLRequestHelper.XMLSelectSingleNodeValue("ColumnSetName")
            Dim lstrRunAsync As String = lobjXMLRequestHelper.XMLSelectSingleNodeValue("RunAsync")
            If String.IsNullOrWhiteSpace(lstrRunAsync) = False AndAlso lstrRunAsync.ToUpper = "TRUE" Then
                mblnRunAsync = True
            End If
            mstrChunkSize = lobjXMLRequestHelper.XMLSelectSingleNodeValue("ChunkSize")
            '
            ' extract the notification object's OBID and domain if they were provided
            '
            mstrNotificationOBID = lobjXMLRequestHelper.XMLSelectSingleNodeValue("NotificationOBID")
            mstrNotificationDomainUID = lobjXMLRequestHelper.XMLSelectSingleNodeValue("NotificationDomainUID")

            If lobjXMLRequestHelper.XMLSelectSingleNode("IgnoreRequiredRelIfTerminated") IsNot Nothing Then
                If lobjXMLRequestHelper.XMLSelectSingleNodeValue("IgnoreRequiredRelIfTerminated").Length > 0 Then
                    Dim lblnIgnoreRequiredRelIfTerminated As Boolean
                    If Not (Boolean.TryParse(lobjXMLRequestHelper.XMLSelectSingleNodeValue("IgnoreRequiredRelIfTerminated"), lblnIgnoreRequiredRelIfTerminated)) Then
                        Throw New SPFException(2088, "Unexpected error '$1' found in criteria.  Expecting boolean value or boolean property.", New String() {lobjXMLRequestHelper.XMLSelectSingleNodeValue("IgnoreRequiredRelIfTerminated")})
                    End If
                    mblnIgnoreRequiredRelIfTerminated = lblnIgnoreRequiredRelIfTerminated
                End If
            End If

        End Sub

        ''' <remarks>
        ''' Modified daughey DI-AM-82752 25/02/2015 - Changed asychronous merging to call back to the same server for a synchronous merge using an asynchronous request
        ''' Modified MProbyn CR-AM-88798 09/06/2015 - Pass through new method arg for suppressing merge of terminated rels
        ''' Modified jrobson DI-AM-93512 22/07/2015 - Check for ValidateUser call in Process request
        ''' Modified rhanley TR-AM-94394 21/08/2015 - Changed to ensure we are passing the same value into called from we were before
        ''' </remarks>
        Protected Overrides Sub OnHandlerBody()
            Try
                ''
                '' Extract the OBIDs and domainUIDs and store locally
                ''
                'Dim larrOBIDs As ArrayList = Me.ObjectCollection.OBIDs
                'Dim larrDomainUIDs As Specialized.StringDictionary = Me.ObjectCollection.DomainUIDs
                Dim lobjLowQueryCFG As SPFConfiguration = SPFRequestContext.Instance.QueryConfiguration
                Dim lobjLowCreateCFG As SPFConfiguration = SPFRequestContext.Instance.CreateConfiguration
                lobjLowQueryCFG.IncludeHigherConfiguration = True

                Dim lobjCreateConfig As SPFConfiguration = ContainerDictionary.CreateConfiguration(lobjLowQueryCFG.GetCurrentConfig.GetParentConfigurationItem, SPFConfiguration.SPFConfigurationType.Create)
                Dim lobjQueryConfig As SPFConfiguration = ContainerDictionary.CreateConfiguration(lobjLowQueryCFG.GetCurrentConfig.GetParentConfigurationItem, SPFConfiguration.SPFConfigurationType.Query)
                lobjQueryConfig.IncludeHigherConfiguration = True
                '
                ' Change the context of the server
                ' Altered MLF tr 42574 06/06/08 - this should use the requested roles (doesn't always match the default roles)
                ' Me.CoreModule.Server.Context.ChangeServerConfigs(lobjCreateConfig, lobjQueryConfig, Me.CoreModule.LoginUser.GetDefaultRoles)
                '
                SPFRequestContext.Instance.ChangeServerConfigs(lobjCreateConfig, lobjQueryConfig)

                '
                ' Based on the input variable run Async or non Async.
                '
                Dim lobjMergeClass As New Classes.Configuration.MergeObjSupportClass
                If mblnRunAsync = False Then
                    '
                    ' merge synchronously
                    ' check if we have the information to create a notification object for progress updates
                    '
                    If (String.IsNullOrEmpty(mstrNotificationOBID) = False AndAlso String.IsNullOrEmpty(mstrNotificationDomainUID) = False) Then
                        '
                        ' create a notification object for progress updates
                        '
                        mobjNotification = SPFRequestContext.Instance.QueryRequest.GetObjectByOBIDAndDomain(mstrNotificationOBID, mstrNotificationDomainUID)
                    End If
                    '
                    ' perform the merge
                    '
                    lobjMergeClass.MergeItemCollection(mobjNotification, lobjLowQueryCFG, lobjLowCreateCFG, mcolIDs, New Generic.Dictionary(Of String, String), False, mintObjCount, mintRelCount, CInt(mstrChunkSize), mblnIgnoreRequiredRelIfTerminated)
                    '
                    ' Check if any items failed MProbyn CR69241 23/04/2014
                    '
                    If lobjMergeClass.FailedItems.Count <> 0 Then
                        mblnReportItemFailures = True
                        mintFailedItemCount = lobjMergeClass.FailedItems.Count
                    End If

                Else
                    '
                    ' Merge asynchronously.
                    ' This is now done by initiating an asynchronous http request back to the server
                    ' for a synchronous merge operation
                    ' This is needed to overcome an issue where the application pool was recycling
                    ' while an asynchronous merge operation was being performed on a background thread.
                    ' The background thread wasn't keeping the application pool alive and the idle 
                    ' timeout for recycling killed the merge operation.  Using an http request means
                    ' the application pool will not trigger the idle threshold for recycling.
                    '
                    SPFRequestContext.Instance.Transaction.Begin()
                    Dim lstrName As String = lobjMergeClass.GenerateNotificationName(lobjLowQueryCFG.GetCurrentConfig.Name)
                    Dim lstrDescription As String = lobjMergeClass.GenerateNotificationDescription(lobjLowQueryCFG.GetCurrentConfig.Name)
                    mobjNotification = lobjMergeClass.CreateNotification(lstrName, lstrDescription)
                    '
                    ' need to provide ISPFMergeProgressNotification information on the first item 
                    ' as the client will show the internal names without it present
                    '
                    mobjNotification.Interfaces("ISPFMergeProgressNotification").Properties("SPFMergeProgressStatus").Value = UpdateMergeNotificationArgs.ProgressStatuses.Processing.ToString()
                    SPFRequestContext.Instance.Transaction.Commit()
                    '
                    ' create a simple request to the server
                    '
                    Dim lobjSimpleRequest As New SimpleRequest(SPFRequestContext.Instance.GetServerRequestURL, SPFRequestContext.Instance.SessionID, "VBCLIENT")
                    '
                    ' copy the current request into the new request
                    '
                    lobjSimpleRequest.Request.InnerXml = SPFRequestContext.Instance.Request.OuterXml
                    '
                    ' add the notification object identifiers to the request
                    '
                    lobjSimpleRequest.AddQueryElement("NotificationOBID", mobjNotification.OBID)
                    lobjSimpleRequest.AddQueryElement("NotificationDomainUID", mobjNotification.DomainUID)
                    '
                    ' change the RunAsync flag in the request to be synchronous
                    '
                    Dim lobjRunAyncElement As XmlElement = DirectCast(lobjSimpleRequest.Request.SelectSingleNode("/Query/RunAsync"), XmlElement)
                    lobjRunAyncElement.InnerText = "False"
                    '
                    ' execute the simple request asynchronously
                    '
                    lobjSimpleRequest.ExecuteAsync()
                    Tracing.Info(TracingTypes.Performance, " Merge process submitted asychronously")

                End If

            Catch ex As Exception
                '
                ' Rollback the transaction on an error
                '
                If SPFRequestContext.Instance.Transaction.InTransaction Then SPFRequestContext.Instance.Transaction.Rollback()
                Throw New SPFException(1329, "There was an error updating the database", ex)
            End Try
        End Sub

        Protected Overrides Sub OnSerialize()
            Dim lnodItem As XmlElement
            Dim lnodReply As XmlNode = SPFRequestContext.Instance.Response.AppendChild(SPFRequestContext.Instance.Response.CreateElement("Reply"))
            If Not mobjNotification Is Nothing Then
                lnodItem = SPFRequestContext.Instance.Serializer.Serialize(mobjNotification, New EFXmlSerializationWriter())
                lnodReply.AppendChild(SPFRequestContext.Instance.Response.ImportNode(lnodItem, True))
            End If
            '
            ' Modified MPRobyn CR69241 23/04/2014 - Return object failure count to client
            '
            If mintFailedItemCount > 0 Then
                Dim lelmItem As XmlElement = CType(lnodReply.AppendChild(SPFRequestContext.Instance.Response.CreateElement("FailedMerge")), XmlElement)
                lelmItem.SetAttribute("Count", mintFailedItemCount.ToString)
            End If
        End Sub

#End Region

#Region " Properties "

#End Region

    End Class

End Namespace
