'Imports System.Xml
'Imports SPF.Server.Schema.Interface.Default
'Imports SPF.Server.Schema.Collections
'Imports SPF.Server.Schema.Model
'Imports SPF.Server.Schema.Interface.Generated
'Imports SPF.Server.Modules
'Imports SPF.Diagnostics
'Imports SPF.Server.Context

'Namespace SPF.Server.Schema.Interface.Default

'    Public Class IObjectDefault1
'        Inherits IObjectDefault

'        Public Sub New(ByVal pblnInstantiateRequiredItems As Boolean)
'            MyBase.New(pblnInstantiateRequiredItems)
'        End Sub

'        Public Overrides Function GetMethodsForRHMConsideration(ByVal pobjValidRoles As Collections.IObjectDictionary, ByVal e As GetMethodsArgs) As Collections.IObjectDictionary
'            Dim lobjMethods As New ObjectDictionary
'            Dim lobjSW As New Stopwatch
'            lobjSW.Start()

'            Dim lobjClassDef As IClassDef = Me.GetClassDefinition
'            Dim lobjIDefs As IObjectDictionary = lobjClassDef.GetRealizedInterfaceDefs
'            '
'            ' Get as set of DAGs for the user and change the call to GetUsersAccessLevel below
'            '
'            Dim lobjLoginDAGS As IObjectDictionary = SPFRequestContext.Instance.LoginUser.GetOwningGroups(e.Config)

'            'Look at each interface for ownership
'            With lobjIDefs.GetEnumerator
'                While .MoveNext
'                    Dim lobjIFDef As IInterfaceDef = CType(.Value.Interfaces("IInterfaceDef"), IInterfaceDef)

'                    'Only look at interfaces that are instantiated on this object
'                    If Me.Interfaces.Contains(lobjIFDef.UID) Then
'                        'Dim lobjUserAccessLevel As UserAccessLevel = lobjIFDef.GetUsersAccessLevel(pobjconfig)
'                        Dim lobjUserAccessLevel As UserAccessLevel = lobjIFDef.GetUsersAccessLevel(lobjLoginDAGS)
'                        Dim lobjMethodsForThisIDef As IObjectDictionary = lobjIFDef.GetMethods(True) ' Expanded above so ok for True

'                        Select Case lobjUserAccessLevel
'                            Case UserAccessLevel.ReadWrite
'                                'Get the methods related to this interface def
'                                lobjMethods.AddRange(lobjMethodsForThisIDef)
'                            Case UserAccessLevel.Read
'                                'Add any methods which are not updating
'                                With lobjMethodsForThisIDef.GetEnumerator
'                                    While .MoveNext
'                                        If CType(.Value.Interfaces("ISPFMethod"), ISPFMethod).GetClientAPI.SPFIsACreateConfigurationAPI = False Then
'                                            lobjMethods.Add(.Value)
'                                        End If
'                                    End While
'                                End With
'                        End Select
'                    End If
'                End While
'            End With
'            lobjSW.Stop()
'            Tracing.Info(TracingTypes.Performance, "For Object UID: " & Me.UID & " Class: " & Me.ClassDefinitionUID & " Found: " & lobjMethods.Count.ToString & " Time: " & lobjSW.Elapsed.ToString)
'            Return CType(lobjMethods, IObjectDictionary)
'        End Function


'        Public Overrides Sub OnGetMethodsForConfig(ByVal e As GetMethodsArgs)
'            Dim lobjSW As New Stopwatch
'            lobjSW.Start()
'            Try

'                Dim lobjRoles As IObjectDictionary
'                Dim lobjMethods As IObjectDictionary

'                If (SPFRequestContext.Instance.QueryConfiguration.GetCurrentConfig IsNot Nothing) AndAlso (SPFRequestContext.Instance.QueryConfiguration.GetCurrentConfig.OBID = e.Config.OBID) Then
'                    'If this config happens to be the query config, the role filtering has been done for us already
'                    'This is true in most cases so we should save some time here
'                    lobjRoles = SPFRequestContext.Instance.ValidRoles
'                Else
'                    lobjRoles = CType(SPFRequestContext.Instance.LoginUser.ToInterface("ISPFUser"), ISPFUser).GetValidRolesForConfig(e.Config)
'                End If


'                ' There is a special requirement needed for items with the ISPFConfigurationItem interface on them. 
'                'The requirement is that a project manager must be able to do actions like set the status of the project. 
'                'The problem here is that the project is a configuration  controlled item. 
'                'The user must therefore be able to set his create configuration to plant to enable him to make changes to the project object.
'                ' This implies the fact that the user must have a role at the plant level. If the role exists at the plant level, 
'                'it must be possible to narrow down which projects can be operated on eg. The administrator of project 1 should not have access to change project 2 and 3 etc.

'                'To solve this, it has been decided that for a role to be valid for rhm on an object that instantiates ISPFConfigurationItem. 
'                'The user must be assigned to the role in current query config, and the configuration item itself.

'                'Altered MLF 11/06/08  tr 43683
'                'If TypeOf Me Is ISPFConfigurationItem Then
'                '    Dim lobjCfgRoles As IObjectDictionary = CType(CoreModule.LoginUser.ToInterface("ISPFUser"), ISPFUser).GetValidRolesForConfig(pobjConfig, Me.CoreModule.LoginUser.GetDefaultRoles)
'                If Me.Interfaces.Contains("ISPFConfigurationItem") Then
'                    Dim lobjInterface As ISPFConfigurationItem = CType(Me.ToInterface("ISPFConfigurationItem"), ISPFConfigurationItem)
'                    lobjRoles = lobjInterface.GetValidRoles(lobjRoles)
'                End If

'                'If we have identified that there asre some roles available, carry on
'                If lobjRoles.Count > 0 Then
'                    'Get the Methods that we need to test for access
'                    lobjMethods = CType(Me.ToInterface("IObject"), IObject).GetMethodsForRHMConsideration(lobjRoles, e)


'                    'Added MLF 10/10/07 - this will get populated if it is needed and
'                    Dim lobjComplexItems As New ObjectDictionary
'                    If e.ComplexObject IsNot Nothing Then
'                        lobjComplexItems = e.ComplexObject
'                    End If

'                    '
'                    ' rzargham 13/12/11 - DI60637 - Add results to a dictionary to avoid processing duplicates
'                    '
'                    Dim ldicConditionsResults As New Generic.Dictionary(Of String, Generic.Dictionary(Of String, Boolean))

'                    'Look at each method in turn
'                    With lobjMethods.GetEnumerator
'                        While .MoveNext
'                            Dim lobjObj As IObject = .Value
'                            Dim lobjMethod As ISPFMethod = CType(lobjObj.ToInterface("ISPFMethod"), ISPFMethod)
'                            'Test Access to the method
'                            ' DI-35259 MPH 1-3-2007 Method was being returned as nothing
'                            If lobjMethod Is Nothing Then
'                                ' This turned out to be a PrimarySPFMethodDef having the same key as 
'                                ' a method (so instead of the SPFMethod being returned here the PrimarySPFMethodDef was returned instead) 
'                                Dim lstrMethodDef As String = lobjObj.Name
'                                Throw New SPFException(1844, "ISPFMethod missing from object with UID $1 - check the object is a SPFMethod", New String() {lstrMethodDef})
'                            Else
'                                If e.ClientAPIs Is Nothing OrElse (e.ClientAPIs IsNot Nothing _
'                                    AndAlso e.ClientAPIs.Contains(lobjMethod.GetClientAPI().Name)) Then
'                                    '
'                                    ' rzargham 13/12/11 - DI60637 - Pass in dictionary to add results to and avoid processing duplicates
'                                    '
'                                    If lobjMethod.IsAvailable(Me, CType(lobjComplexItems, IObjectDictionary), lobjRoles, e.MenuMode, True, ldicConditionsResults) Then
'                                        'If there is access, add it to the return collection
'                                        e.Methods.Add(lobjMethod)
'                                    End If
'                                End If
'                            End If
'                        End While
'                    End With
'                End If

'            Catch ex As Exception
'                '
'                ' ADM-7/6/2011 DI58352 Improve the tracing with information about the config and interface.
'                '
'                Dim lstrErrMsg As String = "Cannot get object methods: ConfigUID: " & e.Config.UID & " Interface: " & Me.UID & " InterfaceDef: " & Me.GetInterfaceDefinition.UID
'                Tracing.Error(TracingTypes.General, lstrErrMsg, ex)
'                Throw New SPFException(1343, lstrErrMsg, ex)
'            Finally
'                lobjSW.Stop()
'                Tracing.Warning(TracingTypes.Performance, "OnGetMethodsForConfig UID: " & Me.UID & " ClassDef: " & Me.ClassDefinitionUID & " Time: " & lobjSW.Elapsed.ToString)
'            End Try
'        End Sub
'    End Class

'End Namespace


