
Option Explicit On
Option Strict On

Imports SPF.Server.APIs.ConfigManagement
Imports SPF.Server.Schema.Collections
Imports SPF.Server.Schema.Errors
Imports SPF.Server.Schema.Model
Imports SPF.Server.Schema.Model.PropertyTypes
Imports System.Collections.Generic

Namespace SPF.Server.Schema.Interface.Generated

    Public Interface IHTCAutoSignOffOverdueStepsTask
        Inherits ISPFSchedulerTask, IObject

#Region " Properties "
        'Private mobjProperties As IPropertyDictionary

#End Region

#Region " Methods "


#End Region

    End Interface

    Public MustInherit Class IHTCAutoSignOffOverdueStepsTaskBase
        Inherits InterfaceBase
        Implements IHTCAutoSignOffOverdueStepsTask



#Region " Constructors "

        Protected Sub New(ByVal pblnInstantiateRequiredItems As Boolean)
            MyBase.New("IHTCAutoSignOffOverdueStepsTask", pblnInstantiateRequiredItems)
        End Sub
#End Region
#Region " Properties "

        Public Property SPFAllowRetry() As Boolean Implements ISPFSchedulerTask.SPFAllowRetry
            Get
                Dim lobjInterface As ISPFSchedulerTask = CType(Interfaces("ISPFSchedulerTask"), ISPFSchedulerTask)
                If Not lobjInterface Is Nothing Then Return lobjInterface.SPFAllowRetry Else Return Nothing
            End Get
            Set(ByVal Value As Boolean)
                CType(Interfaces("ISPFSchedulerTask"), ISPFSchedulerTask).SPFAllowRetry = Value
            End Set
        End Property

        Public Property SPFSchTskContextCreateConfig() As ObjectsType Implements ISPFSchedulerTask.SPFSchTskContextCreateConfig
            Get
                Dim lobjInterface As ISPFSchedulerTask = CType(Interfaces("ISPFSchedulerTask"), ISPFSchedulerTask)
                If Not lobjInterface Is Nothing Then Return lobjInterface.SPFSchTskContextCreateConfig Else Return Nothing
            End Get
            Set(ByVal Value As ObjectsType)
                CType(Interfaces("ISPFSchedulerTask"), ISPFSchedulerTask).SPFSchTskContextCreateConfig = Value
            End Set
        End Property

        Public Property SPFSchTskContextEffectivityDate() As DateTimeType Implements ISPFSchedulerTask.SPFSchTskContextEffectivityDate
            Get
                Dim lobjInterface As ISPFSchedulerTask = CType(Interfaces("ISPFSchedulerTask"), ISPFSchedulerTask)
                If Not lobjInterface Is Nothing Then Return lobjInterface.SPFSchTskContextEffectivityDate Else Return Nothing
            End Get
            Set(ByVal Value As DateTimeType)
                CType(Interfaces("ISPFSchedulerTask"), ISPFSchedulerTask).SPFSchTskContextEffectivityDate = Value
            End Set
        End Property

        Public Property SPFSchTskContextIncludeHigherConfig() As Boolean Implements ISPFSchedulerTask.SPFSchTskContextIncludeHigherConfig
            Get
                Dim lobjInterface As ISPFSchedulerTask = CType(Interfaces("ISPFSchedulerTask"), ISPFSchedulerTask)
                If Not lobjInterface Is Nothing Then Return lobjInterface.SPFSchTskContextIncludeHigherConfig Else Return Nothing
            End Get
            Set(ByVal Value As Boolean)
                CType(Interfaces("ISPFSchedulerTask"), ISPFSchedulerTask).SPFSchTskContextIncludeHigherConfig = Value
            End Set
        End Property

        Public Property SPFSchTskContextQueryConfig() As ObjectsType Implements ISPFSchedulerTask.SPFSchTskContextQueryConfig
            Get
                Dim lobjInterface As ISPFSchedulerTask = CType(Interfaces("ISPFSchedulerTask"), ISPFSchedulerTask)
                If Not lobjInterface Is Nothing Then Return lobjInterface.SPFSchTskContextQueryConfig Else Return Nothing
            End Get
            Set(ByVal Value As ObjectsType)
                CType(Interfaces("ISPFSchedulerTask"), ISPFSchedulerTask).SPFSchTskContextQueryConfig = Value
            End Set
        End Property

        Public Property SPFSchTskContextRoles() As ObjectsType Implements ISPFSchedulerTask.SPFSchTskContextRoles
            Get
                Dim lobjInterface As ISPFSchedulerTask = CType(Interfaces("ISPFSchedulerTask"), ISPFSchedulerTask)
                If Not lobjInterface Is Nothing Then Return lobjInterface.SPFSchTskContextRoles Else Return Nothing
            End Get
            Set(ByVal Value As ObjectsType)
                CType(Interfaces("ISPFSchedulerTask"), ISPFSchedulerTask).SPFSchTskContextRoles = Value
            End Set
        End Property

        Public Property SPFSchTskContextUser() As String Implements ISPFSchedulerTask.SPFSchTskContextUser
            Get
                Dim lobjInterface As ISPFSchedulerTask = CType(Interfaces("ISPFSchedulerTask"), ISPFSchedulerTask)
                If Not lobjInterface Is Nothing Then Return lobjInterface.SPFSchTskContextUser Else Return Nothing
            End Get
            Set(ByVal Value As String)
                CType(Interfaces("ISPFSchedulerTask"), ISPFSchedulerTask).SPFSchTskContextUser = Value
            End Set
        End Property

        Public Property SPFSchTskDoNotProcessSubTasksOnFailure() As Boolean Implements ISPFSchedulerTask.SPFSchTskDoNotProcessSubTasksOnFailure
            Get
                Dim lobjInterface As ISPFSchedulerTask = CType(Interfaces("ISPFSchedulerTask"), ISPFSchedulerTask)
                If Not lobjInterface Is Nothing Then Return lobjInterface.SPFSchTskDoNotProcessSubTasksOnFailure Else Return Nothing
            End Get
            Set(ByVal Value As Boolean)
                CType(Interfaces("ISPFSchedulerTask"), ISPFSchedulerTask).SPFSchTskDoNotProcessSubTasksOnFailure = Value
            End Set
        End Property

        Public Property SPFSchTskEnabled() As Boolean Implements ISPFSchedulerTask.SPFSchTskEnabled
            Get
                Dim lobjInterface As ISPFSchedulerTask = CType(Interfaces("ISPFSchedulerTask"), ISPFSchedulerTask)
                If Not lobjInterface Is Nothing Then Return lobjInterface.SPFSchTskEnabled Else Return Nothing
            End Get
            Set(ByVal Value As Boolean)
                CType(Interfaces("ISPFSchedulerTask"), ISPFSchedulerTask).SPFSchTskEnabled = Value
            End Set
        End Property

        Public Property SPFSchTskEndDate() As DateTimeType Implements ISPFSchedulerTask.SPFSchTskEndDate
            Get
                Dim lobjInterface As ISPFSchedulerTask = CType(Interfaces("ISPFSchedulerTask"), ISPFSchedulerTask)
                If Not lobjInterface Is Nothing Then Return lobjInterface.SPFSchTskEndDate Else Return Nothing
            End Get
            Set(ByVal Value As DateTimeType)
                CType(Interfaces("ISPFSchedulerTask"), ISPFSchedulerTask).SPFSchTskEndDate = Value
            End Set
        End Property

        Public Property SPFSchTskFailOnSubTaskFailure() As Boolean Implements ISPFSchedulerTask.SPFSchTskFailOnSubTaskFailure
            Get
                Dim lobjInterface As ISPFSchedulerTask = CType(Interfaces("ISPFSchedulerTask"), ISPFSchedulerTask)
                If Not lobjInterface Is Nothing Then Return lobjInterface.SPFSchTskFailOnSubTaskFailure Else Return Nothing
            End Get
            Set(ByVal Value As Boolean)
                CType(Interfaces("ISPFSchedulerTask"), ISPFSchedulerTask).SPFSchTskFailOnSubTaskFailure = Value
            End Set
        End Property

        Public Property SPFSchTskFailureMsg() As String Implements ISPFSchedulerTask.SPFSchTskFailureMsg
            Get
                Dim lobjInterface As ISPFSchedulerTask = CType(Interfaces("ISPFSchedulerTask"), ISPFSchedulerTask)
                If Not lobjInterface Is Nothing Then Return lobjInterface.SPFSchTskFailureMsg Else Return Nothing
            End Get
            Set(ByVal Value As String)
                CType(Interfaces("ISPFSchedulerTask"), ISPFSchedulerTask).SPFSchTskFailureMsg = Value
            End Set
        End Property

        Public Property SPFSchTskFrequency() As Integer Implements ISPFSchedulerTask.SPFSchTskFrequency
            Get
                Dim lobjInterface As ISPFSchedulerTask = CType(Interfaces("ISPFSchedulerTask"), ISPFSchedulerTask)
                If Not lobjInterface Is Nothing Then Return lobjInterface.SPFSchTskFrequency Else Return Nothing
            End Get
            Set(ByVal Value As Integer)
                CType(Interfaces("ISPFSchedulerTask"), ISPFSchedulerTask).SPFSchTskFrequency = Value
            End Set
        End Property

        Public Property SPFSchTskInterval() As Integer Implements ISPFSchedulerTask.SPFSchTskInterval
            Get
                Dim lobjInterface As ISPFSchedulerTask = CType(Interfaces("ISPFSchedulerTask"), ISPFSchedulerTask)
                If Not lobjInterface Is Nothing Then Return lobjInterface.SPFSchTskInterval Else Return Nothing
            End Get
            Set(ByVal Value As Integer)
                CType(Interfaces("ISPFSchedulerTask"), ISPFSchedulerTask).SPFSchTskInterval = Value
            End Set
        End Property

        Public Property SPFSchTskLocked() As Boolean Implements ISPFSchedulerTask.SPFSchTskLocked
            Get
                Dim lobjInterface As ISPFSchedulerTask = CType(Interfaces("ISPFSchedulerTask"), ISPFSchedulerTask)
                If Not lobjInterface Is Nothing Then Return lobjInterface.SPFSchTskLocked Else Return Nothing
            End Get
            Set(ByVal Value As Boolean)
                CType(Interfaces("ISPFSchedulerTask"), ISPFSchedulerTask).SPFSchTskLocked = Value
            End Set
        End Property

        Public Property SPFSchTskNotifyUser() As String Implements ISPFSchedulerTask.SPFSchTskNotifyUser
            Get
                Dim lobjInterface As ISPFSchedulerTask = CType(Interfaces("ISPFSchedulerTask"), ISPFSchedulerTask)
                If Not lobjInterface Is Nothing Then Return lobjInterface.SPFSchTskNotifyUser Else Return Nothing
            End Get
            Set(ByVal Value As String)
                CType(Interfaces("ISPFSchedulerTask"), ISPFSchedulerTask).SPFSchTskNotifyUser = Value
            End Set
        End Property

        Public Property SPFSchTskRetryCount() As Integer Implements ISPFSchedulerTask.SPFSchTskRetryCount
            Get
                Dim lobjInterface As ISPFSchedulerTask = CType(Interfaces("ISPFSchedulerTask"), ISPFSchedulerTask)
                If Not lobjInterface Is Nothing Then Return lobjInterface.SPFSchTskRetryCount Else Return Nothing
            End Get
            Set(ByVal Value As Integer)
                CType(Interfaces("ISPFSchedulerTask"), ISPFSchedulerTask).SPFSchTskRetryCount = Value
            End Set
        End Property

        Public Property SPFSchTskRetryLimit() As Integer Implements ISPFSchedulerTask.SPFSchTskRetryLimit
            Get
                Dim lobjInterface As ISPFSchedulerTask = CType(Interfaces("ISPFSchedulerTask"), ISPFSchedulerTask)
                If Not lobjInterface Is Nothing Then Return lobjInterface.SPFSchTskRetryLimit Else Return Nothing
            End Get
            Set(ByVal Value As Integer)
                CType(Interfaces("ISPFSchedulerTask"), ISPFSchedulerTask).SPFSchTskRetryLimit = Value
            End Set
        End Property

        Public Property SPFSchTskStartDate() As DateTimeType Implements ISPFSchedulerTask.SPFSchTskStartDate
            Get
                Dim lobjInterface As ISPFSchedulerTask = CType(Interfaces("ISPFSchedulerTask"), ISPFSchedulerTask)
                If Not lobjInterface Is Nothing Then Return lobjInterface.SPFSchTskStartDate Else Return Nothing
            End Get
            Set(ByVal Value As DateTimeType)
                CType(Interfaces("ISPFSchedulerTask"), ISPFSchedulerTask).SPFSchTskStartDate = Value
            End Set
        End Property

        Public Property SPFSchTskStatus() As String Implements ISPFSchedulerTask.SPFSchTskStatus
            Get
                Dim lobjInterface As ISPFSchedulerTask = CType(Interfaces("ISPFSchedulerTask"), ISPFSchedulerTask)
                If Not lobjInterface Is Nothing Then Return lobjInterface.SPFSchTskStatus Else Return Nothing
            End Get
            Set(ByVal Value As String)
                CType(Interfaces("ISPFSchedulerTask"), ISPFSchedulerTask).SPFSchTskStatus = Value
            End Set
        End Property

        Public Property SPFSchTskSubmitter() As String Implements ISPFSchedulerTask.SPFSchTskSubmitter
            Get
                Dim lobjInterface As ISPFSchedulerTask = CType(Interfaces("ISPFSchedulerTask"), ISPFSchedulerTask)
                If Not lobjInterface Is Nothing Then Return lobjInterface.SPFSchTskSubmitter Else Return Nothing
            End Get
            Set(ByVal Value As String)
                CType(Interfaces("ISPFSchedulerTask"), ISPFSchedulerTask).SPFSchTskSubmitter = Value
            End Set
        End Property

        Public Property SPFSchTskTag() As String Implements ISPFSchedulerTask.SPFSchTskTag
            Get
                Dim lobjInterface As ISPFSchedulerTask = CType(Interfaces("ISPFSchedulerTask"), ISPFSchedulerTask)
                If Not lobjInterface Is Nothing Then Return lobjInterface.SPFSchTskTag Else Return Nothing
            End Get
            Set(ByVal Value As String)
                CType(Interfaces("ISPFSchedulerTask"), ISPFSchedulerTask).SPFSchTskTag = Value
            End Set
        End Property

#End Region


#Region " Methods "


        Public Overloads Function GetChildTasks() As IObjectDictionary Implements ISPFSchedulerTask.GetChildTasks
            Return GetChildTasks(False)
        End Function
        Public Overloads Function GetChildTasks(ByVal pblnCacheOnly As Boolean) As IObjectDictionary Implements ISPFSchedulerTask.GetChildTasks
            Return CType(GetEnd1Relationships.GetRels("SPFSchedulerTaskTasks", pblnCacheOnly).GetEnd2s, IObjectDictionary)
        End Function

        Public Function GetParentTask() As ISPFSchedulerTask Implements ISPFSchedulerTask.GetParentTask
            Return GetParentTask(False)
        End Function
        Public Function GetParentTask(ByVal pblnCacheOnly As Boolean) As ISPFSchedulerTask Implements ISPFSchedulerTask.GetParentTask
            Dim lobjRel As IRel = GetEnd2Relationships.GetRel("SPFSchedulerTaskTasks", pblnCacheOnly)
            If Not lobjRel Is Nothing Then
                Return CType(CType(lobjRel.GetEnd1, IObject).ToInterface("ISPFSchedulerTask"), ISPFSchedulerTask)
            Else
                Return Nothing
            End If
        End Function

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overridable Function DeleteMeWhenSuccessfullyProcessed() As Boolean Implements ISPFSchedulerTask.DeleteMeWhenSuccessfullyProcessed 'Special MethodDef
            Return CType(MyNext("ISPFSchedulerTask"), ISPFSchedulerTask).DeleteMeWhenSuccessfullyProcessed()
        End Function

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overridable Sub FailTaskAfterServerFailure() Implements ISPFSchedulerTask.FailTaskAfterServerFailure 'Special MethodDef
            CType(MyNext("ISPFSchedulerTask"), ISPFSchedulerTask).FailTaskAfterServerFailure()
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overridable Function GetAdditionalDomains() As Generic.List(Of String) Implements ISPFSchedulerTask.GetAdditionalDomains 'Special MethodDef
            Return CType(MyNext("ISPFSchedulerTask"), ISPFSchedulerTask).GetAdditionalDomains()
        End Function

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overridable Function IsAsynchronous() As Boolean Implements ISPFSchedulerTask.IsAsynchronous 'Special MethodDef
            Return CType(MyNext("ISPFSchedulerTask"), ISPFSchedulerTask).IsAsynchronous()
        End Function

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overridable Function IsReady() As Boolean Implements ISPFSchedulerTask.IsReady 'Special MethodDef
            Return CType(MyNext("ISPFSchedulerTask"), ISPFSchedulerTask).IsReady()
        End Function

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overridable Sub Process() Implements ISPFSchedulerTask.Process 'Special MethodDef
            CType(MyNext("ISPFSchedulerTask"), ISPFSchedulerTask).Process()
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overridable Sub OnProcess() Implements ISPFSchedulerTask.OnProcess 'Special MethodDef
            CType(MyNext("ISPFSchedulerTask"), ISPFSchedulerTask).OnProcess()
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overridable Sub OnProcessAsync() Implements ISPFSchedulerTask.OnProcessAsync 'Special MethodDef
            CType(MyNext("ISPFSchedulerTask"), ISPFSchedulerTask).OnProcessAsync()
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overridable Sub OnProcessAsyncFailed() Implements ISPFSchedulerTask.OnProcessAsyncFailed 'Special MethodDef
            CType(MyNext("ISPFSchedulerTask"), ISPFSchedulerTask).OnProcessAsyncFailed()
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overridable Sub OnProcessed() Implements ISPFSchedulerTask.OnProcessed 'Special MethodDef
            CType(MyNext("ISPFSchedulerTask"), ISPFSchedulerTask).OnProcessed()
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overridable Sub OnProcessedAsync() Implements ISPFSchedulerTask.OnProcessedAsync 'Special MethodDef
            CType(MyNext("ISPFSchedulerTask"), ISPFSchedulerTask).OnProcessedAsync()
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overridable Sub OnProcessedSubTasks() Implements ISPFSchedulerTask.OnProcessedSubTasks 'Special MethodDef
            CType(MyNext("ISPFSchedulerTask"), ISPFSchedulerTask).OnProcessedSubTasks()
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overridable Sub OnProcessFailed() Implements ISPFSchedulerTask.OnProcessFailed 'Special MethodDef
            CType(MyNext("ISPFSchedulerTask"), ISPFSchedulerTask).OnProcessFailed()
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overridable Sub OnProcessFailed(ByVal pblnAfterServerFailure As Boolean) Implements ISPFSchedulerTask.OnProcessFailed 'Special MethodDef
            CType(MyNext("ISPFSchedulerTask"), ISPFSchedulerTask).OnProcessFailed(pblnAfterServerFailure)
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overridable Sub OnReInserting() Implements ISPFSchedulerTask.OnReInserting 'Special MethodDef
            CType(MyNext("ISPFSchedulerTask"), ISPFSchedulerTask).OnReInserting()
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overridable Function SendPollRequestAfterCreation() As Boolean Implements ISPFSchedulerTask.SendPollRequestAfterCreation 'Special MethodDef
            Return CType(MyNext("ISPFSchedulerTask"), ISPFSchedulerTask).SendPollRequestAfterCreation()
        End Function

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Sub Commit() 'Special MethodDef
            CType(MyNext("IObject"), IObject).Commit()
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Function Validate() As Boolean 'Special MethodDef
            Return CType(MyNext("IObject"), IObject).Validate()
        End Function

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Sub OnValidate(ByVal e As ValidateEventArgs) 'Special MethodDef
            CType(MyNext("IObject"), IObject).OnValidate(e)
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Sub Delete() 'Special MethodDef
            CType(MyNext("IObject"), IObject).Delete()
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Sub OnDeleting(ByVal e As CancelEventArgs) 'Special MethodDef
            CType(MyNext("IObject"), IObject).OnDeleting(e)
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Sub OnDeleted(ByVal e As SuppressibleEventArgs) 'Special MethodDef
            CType(MyNext("IObject"), IObject).OnDeleted(e)
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Sub OnDelete(ByVal e As SuppressibleEventArgs) 'Special MethodDef
            CType(MyNext("IObject"), IObject).OnDelete(e)
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Sub Delete(ByVal pblnSuppressEvents As Boolean) 'Special MethodDef
            CType(MyNext("IObject"), IObject).Delete(pblnSuppressEvents)
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Sub Terminate() 'Special MethodDef
            CType(MyNext("IObject"), IObject).Terminate()
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Sub OnTerminated(ByVal e As SuppressibleEventArgs) 'Special MethodDef
            CType(MyNext("IObject"), IObject).OnTerminated(e)
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Sub OnTerminate(ByVal e As SuppressibleEventArgs) 'Special MethodDef
            CType(MyNext("IObject"), IObject).OnTerminate(e)
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Sub OnTerminating(ByVal e As CancelEventArgs) 'Special MethodDef
            CType(MyNext("IObject"), IObject).OnTerminating(e)
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Sub Terminate(ByVal pblnSuppressEvents As Boolean) 'Special MethodDef
            CType(MyNext("IObject"), IObject).Terminate(pblnSuppressEvents)
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Sub DoDelete() 'Special MethodDef
            CType(MyNext("IObject"), IObject).DoDelete()
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overloads Overrides Sub BeginUpdate() 'Special MethodDef
            CType(MyNext("IObject"), IObject).BeginUpdate()
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Sub OnUpdated(ByVal e As SuppressibleEventArgs) 'Special MethodDef
            CType(MyNext("IObject"), IObject).OnUpdated(e)
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Sub OnUpdating(ByVal e As CancelEventArgs) 'Special MethodDef
            CType(MyNext("IObject"), IObject).OnUpdating(e)
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Sub OnUpdatingValidation(ByVal e As CancelEventArgs) 'Special MethodDef
            CType(MyNext("IObject"), IObject).OnUpdatingValidation(e)
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Sub OnUpdate(ByVal e As UpdateEventArgs) 'Special MethodDef
            CType(MyNext("IObject"), IObject).OnUpdate(e)
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Sub OnCreated(ByVal e As SuppressibleEventArgs) 'Special MethodDef
            CType(MyNext("IObject"), IObject).OnCreated(e)
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Function GetMethodsForConfig(ByVal e As GetMethodsArgs) As IObjectDictionary 'Special MethodDef
            Return CType(MyNext("IObject"), IObject).GetMethodsForConfig(e)
        End Function

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Sub OnGetMethodsForConfig(ByVal e As GetMethodsArgs) 'Special MethodDef
            CType(MyNext("IObject"), IObject).OnGetMethodsForConfig(e)
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Sub RollBack() 'Special MethodDef
            CType(MyNext("IObject"), IObject).RollBack()
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Function GetEdgeDefsForRealizedInterfacesForRole(ByVal pobjConfig As ISPFConfigurationItem) As IObjectDictionary 'Special MethodDef
            Return CType(MyNext("IObject"), IObject).GetEdgeDefsForRealizedInterfacesForRole(pobjConfig)
        End Function

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Function GetEdgeDefsForRealizedInterfaces() As IObjectDictionary 'Special MethodDef
            Return CType(MyNext("IObject"), IObject).GetEdgeDefsForRealizedInterfaces()
        End Function

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Function GetEdgeDefsForRealizedInterfaces(ByVal pblnForRole As Boolean, ByVal pobjConfig As ISPFConfigurationItem) As IObjectDictionary 'Special MethodDef
            Return CType(MyNext("IObject"), IObject).GetEdgeDefsForRealizedInterfaces(pblnForRole, pobjConfig)
        End Function

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Sub OnCreatingValidation(ByVal e As CancelEventArgs) 'Special MethodDef
            CType(MyNext("IObject"), IObject).OnCreatingValidation(e)
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Sub OnCreating(ByVal e As CancelEventArgs) 'Special MethodDef
            CType(MyNext("IObject"), IObject).OnCreating(e)
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overloads Overrides Function Copy(ByVal pobjObjectGraph As IObjectDictionary) As IObject 'Special MethodDef
            Return CType(MyNext("IObject"), IObject).Copy(pobjObjectGraph)
        End Function

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overloads Overrides Sub FinishUpdate() 'Special MethodDef
            CType(MyNext("IObject"), IObject).FinishUpdate()
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overloads Overrides Function Copy() As IObject 'Special MethodDef
            Return CType(MyNext("IObject"), IObject).Copy()
        End Function

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Sub OnCopying(ByVal e As CancelEventArgs) 'Special MethodDef
            CType(MyNext("IObject"), IObject).OnCopying(e)
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Sub OnCopied(ByVal e As CopyEventArgs) 'Special MethodDef
            CType(MyNext("IObject"), IObject).OnCopied(e)
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Sub OnCopy(ByVal e As CopyEventArgs) 'Special MethodDef
            CType(MyNext("IObject"), IObject).OnCopy(e)
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overloads Overrides Sub BeginUpdate(ByVal pblnValidateClaim As Boolean) 'Special MethodDef
            CType(MyNext("IObject"), IObject).BeginUpdate(pblnValidateClaim)
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overloads Overrides Sub BeginUpdate(ByVal pblnValidateClaim As Boolean, ByVal pblnSuppressEvents As Boolean) 'Special MethodDef
            CType(MyNext("IObject"), IObject).BeginUpdate(pblnValidateClaim, pblnSuppressEvents)
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Function IsInterfaceToBeCopied(ByVal pstrInterfaceDefUID As String) As Boolean 'Special MethodDef
            Return CType(MyNext("IObject"), IObject).IsInterfaceToBeCopied(pstrInterfaceDefUID)
        End Function

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Sub OnCreate(ByVal e As CreateEventArgs) 'Special MethodDef
            CType(MyNext("IObject"), IObject).OnCreate(e)
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Sub OnPreProcess(ByVal e As CreateEventArgs) 'Special MethodDef
            CType(MyNext("IObject"), IObject).OnPreProcess(e)
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Function GetRels(ByVal pstrEdgeDefUID As String) As IRelDictionary 'Special MethodDef
            Return CType(MyNext("IObject"), IObject).GetRels(pstrEdgeDefUID)
        End Function

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Function IsPropertyToBeCopied(ByVal pstrPropertyDefUID As String) As Boolean 'Special MethodDef
            Return CType(MyNext("IObject"), IObject).IsPropertyToBeCopied(pstrPropertyDefUID)
        End Function

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Sub SetValueOnCopy(ByVal pobjNewProperty As IProperty, ByVal pobjProperty As IProperty) 'Special MethodDef
            CType(MyNext("IObject"), IObject).SetValueOnCopy(pobjNewProperty, pobjProperty)
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Sub SetUIDOnCopy(ByVal pobjNewObject As IObject, ByVal pobjObject As IObject) 'Special MethodDef
            CType(MyNext("IObject"), IObject).SetUIDOnCopy(pobjNewObject, pobjObject)
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Sub InstantiateMergeInterfaces() 'Special MethodDef
            CType(MyNext("IObject"), IObject).InstantiateMergeInterfaces()
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Function GetRelDefsForDragDrop(ByVal pobjSourceObject As IObject, ByVal pobjUserAccessGroups As IObjectDictionary, ByRef pobjGRDDArgs As GetRelDefsForDragDropArgs) As StructuredObjectCollection 'Special MethodDef
            Return CType(MyNext("IObject"), IObject).GetRelDefsForDragDrop(pobjSourceObject, pobjUserAccessGroups, pobjGRDDArgs)
        End Function
        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Function GetRelDefsForDragDrop(ByVal pobjSourceObject As IObject, ByVal pobjUserAccessGroups As IObjectDictionary, ByVal pcolRelOverrideUIDs As System.Collections.Generic.List(Of String), ByRef pobjGRDDArgs As GetRelDefsForDragDropArgs) As StructuredObjectCollection 'Special MethodDef
            Return CType(MyNext("IObject"), IObject).GetRelDefsForDragDrop(pobjSourceObject, pobjUserAccessGroups, pcolRelOverrideUIDs, pobjGRDDArgs)
        End Function

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Sub OnRelationshipRemoved(ByVal e As RelEventArgs) 'Special MethodDef
            CType(MyNext("IObject"), IObject).OnRelationshipRemoved(e)
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Sub OnRelationshipRemoving(ByVal e As RelEventArgs) 'Special MethodDef
            CType(MyNext("IObject"), IObject).OnRelationshipRemoving(e)
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Sub OnRelationshipAdded(ByVal e As RelEventArgs) 'Special MethodDef
            CType(MyNext("IObject"), IObject).OnRelationshipAdded(e)
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Function GetMethodsForRHMConsideration(ByVal pobjValidRoles As Collections.IObjectDictionary, ByVal e As GetMethodsArgs) As IObjectDictionary 'Special MethodDef
            Return CType(MyNext("IObject"), IObject).GetMethodsForRHMConsideration(pobjValidRoles, e)
        End Function

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Function IsDeletedObj() As Boolean 'Special MethodDef
            Return CType(MyNext("IObject"), IObject).IsDeletedObj()
        End Function

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Function IsTerminated() As Boolean 'Special MethodDef
            Return CType(MyNext("IObject"), IObject).IsTerminated()
        End Function

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Sub OnRelationshipAdd(ByVal e As RelEventArgs) 'Special MethodDef
            CType(MyNext("IObject"), IObject).OnRelationshipAdd(e)
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Sub OnRelationshipAdding(ByVal e As RelEventArgs) 'Special MethodDef
            CType(MyNext("IObject"), IObject).OnRelationshipAdding(e)
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Function GetEdgeCount(ByVal pstrDefUID As String) As Integer 'Special MethodDef
            Return CType(MyNext("IObject"), IObject).GetEdgeCount(pstrDefUID)
        End Function

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overloads Overrides Function Copy(ByVal pobjContainer As IContainer) As IObject 'Special MethodDef
            Return CType(MyNext("IObject"), IObject).Copy(pobjContainer)
        End Function

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overloads Overrides Function Copy(ByVal pobjObjectGraph As IObjectDictionary, ByVal pobjContainer As IContainer, ByVal pblnSuppressEvents As Boolean) As IObject 'Special MethodDef
            Return CType(MyNext("IObject"), IObject).Copy(pobjObjectGraph, pobjContainer, pblnSuppressEvents)
        End Function

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Sub OnRelationshipUpdating(ByVal e As RelEventArgs) 'Special MethodDef
            CType(MyNext("IObject"), IObject).OnRelationshipUpdating(e)
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Function GetIconName() As String 'Special MethodDef
            Return CType(MyNext("IObject"), IObject).GetIconName()
        End Function

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Function OnGetIconNamePrefix() As String 'Special MethodDef
            Return CType(MyNext("IObject"), IObject).OnGetIconNamePrefix()
        End Function

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Function OnGetIconNameSuffix() As String 'Special MethodDef
            Return CType(MyNext("IObject"), IObject).OnGetIconNameSuffix()
        End Function

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Function ResolveConflicts(ByVal pobjChanges As CompareObjectsResult) As IObject 'Special MethodDef
            Return CType(MyNext("IObject"), IObject).ResolveConflicts(pobjChanges)
        End Function

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Function GetConfig() As ISPFConfigurationItem 'Special MethodDef
            Return CType(MyNext("IObject"), IObject).GetConfig()
        End Function

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Function GetDomain() As ISPFDomain 'Special MethodDef
            Return CType(MyNext("IObject"), IObject).GetDomain()
        End Function

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Function GetEquivalentUIDInParentConfig() As String 'Special MethodDef
            Return CType(MyNext("IObject"), IObject).GetEquivalentUIDInParentConfig()
        End Function

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Sub CompareObjectToParentConfig(ByVal pobjRefObj As Generated.IObject, ByVal pobjHighObj As Generated.IObject, ByRef phshprocessedObjects As System.Collections.Hashtable, ByRef pobjResult As CompareObjectsResult, ByVal pobjMode As SPF.Server.APIs.ConfigManagement.CompareObjsToHigherConfig.Mode) 'Special MethodDef
            CType(MyNext("IObject"), IObject).CompareObjectToParentConfig(pobjRefObj, pobjHighObj, phshprocessedObjects, pobjResult, pobjMode)
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Function GetMostSpecializedViewDefs() As IObjectDictionary 'Special MethodDef
            Return CType(MyNext("IObject"), IObject).GetMostSpecializedViewDefs()
        End Function

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Function GetIncludesObjects() As IObjectDictionary 'Special MethodDef
            Return CType(MyNext("IObject"), IObject).GetIncludesObjects()
        End Function

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Sub UniqueKeyValidation(ByRef e As CancelEventArgs) 'Special MethodDef
            CType(MyNext("IObject"), IObject).UniqueKeyValidation(e)
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Function IsUniqueKeyUniqueInConfig() As Boolean 'Special MethodDef
            Return CType(MyNext("IObject"), IObject).IsUniqueKeyUniqueInConfig()
        End Function

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Function IsUniqueKeyUnique() As Boolean 'Special MethodDef
            Return CType(MyNext("IObject"), IObject).IsUniqueKeyUnique()
        End Function

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Function IsUniqueChecksOnOBIDAndUpdateState(ByVal parrOBIDs As System.Collections.ArrayList) As Boolean 'Special MethodDef
            Return CType(MyNext("IObject"), IObject).IsUniqueChecksOnOBIDAndUpdateState(parrOBIDs)
        End Function

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Function IsObjectBeingClaimedOrPreviouslyClaimed(ByRef e As ValidateForClaimEventArgs) As Boolean 'Special MethodDef
            Return CType(MyNext("IObject"), IObject).IsObjectBeingClaimedOrPreviouslyClaimed(e)
        End Function

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Function ChangeClassDefinition(ByVal pstrTargetClassDefName As String, ByVal pstrDomainUID As String, ByVal pstrRemovedInterfaceUIDs As System.Collections.Generic.IEnumerable(Of String), ByVal pstrRemovedPropertyUIDs As System.Collections.Generic.IEnumerable(Of String), ByVal pcolRelUidsToExclude As System.Collections.Generic.IEnumerable(Of String)) As IObject 'Special MethodDef
            Return CType(MyNext("IObject"), IObject).ChangeClassDefinition(pstrTargetClassDefName, pstrDomainUID, pstrRemovedInterfaceUIDs, pstrRemovedPropertyUIDs, pcolRelUidsToExclude)
        End Function

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Function ChangeClassDefinition(ByVal pstrTargetClassDefName As String, ByVal pstrDomainUID As String, ByVal pcolRelUidsToExclude As System.Collections.Generic.IEnumerable(Of String)) As IObject 'Special MethodDef
            Return CType(MyNext("IObject"), IObject).ChangeClassDefinition(pstrTargetClassDefName, pstrDomainUID, pcolRelUidsToExclude)
        End Function

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Sub PropagateRelation(ByVal pstrPropagatesToRelDefUID As String, ByVal pobjEndObject As IObject, ByVal pstrInterfaceUID As String, ByVal pstrPropertyUID As String) 'Special MethodDef
            CType(MyNext("IObject"), IObject).PropagateRelation(pstrPropagatesToRelDefUID, pobjEndObject, pstrInterfaceUID, pstrPropertyUID)
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Sub PropagateProperty(ByVal pstrInterfaceUID As String, ByVal pstrPropertyUID As String, Optional ByVal pobjRelatedObj As IObject = Nothing) 'Special MethodDef
            CType(MyNext("IObject"), IObject).PropagateProperty(pstrInterfaceUID, pstrPropertyUID, pobjRelatedObj)
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Sub ClearPropagatedProperty(ByVal pstrInterfaceUID As String, ByVal pstrPropertyUID As String, ByVal pobjRelatedObj As IObject) 'Special MethodDef
            CType(MyNext("IObject"), IObject).ClearPropagatedProperty(pstrInterfaceUID, pstrPropertyUID, pobjRelatedObj)
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Overrides Sub TerminatePropagatedRelation(ByVal pstrPropagatesToRelDefUID As String, ByVal pobjEndObject As IObject, ByVal pstrInterfaceUID As String, ByVal pstrPropertyUID As String) 'Special MethodDef
            CType(MyNext("IObject"), IObject).TerminatePropagatedRelation(pstrPropagatesToRelDefUID, pobjEndObject, pstrInterfaceUID, pstrPropertyUID)
        End Sub

#End Region

    End Class

End Namespace
