Imports System.ComponentModel.Composition
Imports Intergraph.SPF.Services.RequestContextService.Implementation
Imports Intergraph.VTL.Server.Services.Common.Classes.Interfaces
Imports Intergraph.VTL.Server.Services.Validation.Classes.Classes
Imports Intergraph.VTL.Server.Services.Validation.Classes.Interfaces.ErrorTable
Imports Intergraph.VTL.Server.Services.Validation.Classes.Interfaces.RuleExecution
Imports SPF.Common.DataAccessLayer.DataRetrieval
Imports SPF.Server.Schema.[Interface].Generated
Imports Intergraph.VTL.Server.Services.TargetSystem.Contracts.Types.Requests
Imports Intergraph.VTL.Server.Services.TargetSystem.Contracts.Types.Responses

Namespace Intergraph.VTL.Server.CustomRule

    ''' <summary>
    ''' Validate that the selected property valud is using the default SI UoM
    ''' </summary>
    <Export(GetType(IVTLValidationRuleExecution)), PartCreationPolicy(CreationPolicy.NonShared)>
    Public Class VTLDefaultSiUoMValidationRule
        Implements IVTLValidationRuleExecution, IVTLValidationRuleExecution(Of IVTLDefaultSiUoMValidationRule)

#Region " MEF Imports "

        <Import(GetType(IVTLTargetSystemServiceClient))>
        Private mobjTargetSystemServiceClient As Lazy(Of IVTLTargetSystemServiceClient) = Nothing

        Private ReadOnly Property TargetSystemServiceClient As IVTLTargetSystemServiceClient
            Get
                Return mobjTargetSystemServiceClient.Value
            End Get
        End Property

        <Import(GetType(IValidationObjectCreation))>
        Private mobjValidationObjectCreation As Lazy(Of IValidationObjectCreation) = Nothing

        Private ReadOnly Property ValidationObjectCreation As IValidationObjectCreation
            Get
                Return mobjValidationObjectCreation.Value
            End Get
        End Property

#End Region

        ''' <summary>
        ''' Executes the validation.
        ''' </summary>
        ''' <param name="pobjJob">The pobj job.</param>
        ''' <param name="pobjRule">The pobj rule.</param>
        ''' <param name="pobjRequestContextService">The pobj request context service.</param>
        ''' <returns></returns>
        Public Function ExecuteValidation(pobjJob As IVTLJob, pobjRule As IVTLValidationRule, pobjRequestContextService As ISPFRequestContextService) As IEnumerable(Of VTLValidationObject) Implements IVTLValidationRuleExecution.ExecuteValidation
            Dim lcolValidationResults As New List(Of VTLValidationObject)

            If Not pobjRule.VTLTurnValidationRuleOff Then

                Dim lobjTargetSystem As IVTLTargetSystem = pobjJob.GetValidationTargetSystem()
                '
                ' Get the class definition and property definitions that we want to validate
                '
                Dim lobjClassDefToValidate As IClassDef = pobjRule.GetValidationRuleClassDef()
                Dim lcolPropertyDefToValidate As IPropertyDef = CType(pobjRule.ToInterface("IVTLPropertyValidationRule"), IVTLPropertyValidationRule).GetValidationRulePropertyDef()
                '
                ' Get the default SI UoM from target system
                '
                Dim lstrSessionID As String = TargetSystemServiceClient.Login(lobjTargetSystem.VTLTargetSystemURL, pobjRequestContextService.SPFRequestContext.ClientMachineName, pobjRequestContextService.SPFRequestContext.LoginUser.SPFLoginName, String.Empty)

                Dim lstrUoMUID As String = pobjRule.Interfaces("IVTLDefaultSiUoMValidationRule").Properties("VTLUomUID").Value.ToString()

                Dim lobjSQLRequest As VTLGetSQLQueryResultsRequest = New VTLGetSQLQueryResultsRequest("SELECT end2obj.OBJUID, end2obj.OBJNAME FROM SCHEMAOBJ end2obj INNER JOIN SCHEMAREL rel ON end2obj.OBJUID = rel.UID2 WHERE rel.UID1 = '" & lstrUoMUID & "' AND rel.DEFUID = 'HasDefaultSI'", lstrSessionID)
                Dim lobjSQLResponse As VTLGetSQLQueryResultsResponse = TargetSystemServiceClient.GetSQLQueryResults(lobjTargetSystem.VTLTargetSystemURL, lobjSQLRequest)

				TargetSystemServiceClient.Logout(lobjTargetSystem.VTLTargetSystemURL, pobjRequestContextService.SPFRequestContext.ClientMachineName, lstrSessionID)
				
                If lobjSQLResponse.SQLResults.Count() = 1 AndAlso lobjSQLResponse.SQLResults(0).Count() = 2 Then
                    Dim lstrUoM = lobjSQLResponse.SQLResults(0)(1)
                    ValidateDataInJobTables(lstrUoM, pobjRequestContextService, pobjJob, lobjClassDefToValidate, lcolPropertyDefToValidate, lobjTargetSystem, pobjRule)
                Else
                    Throw New Exception("Target system query did not return a default SI value for the specified UoM")
                End If

            End If

            Return lcolValidationResults
        End Function
 
        Private Function ValidateDataInJobTables(pstrUoM As string, 
                                            pobjRequestContextService As ISPFRequestContextService, 
                                            pobjJob As IVTLJob, 
                                            lobjClassDefToValidate As IClassDef, 
                                            lcolPropertyDefToValidate As IPropertyDef, 
                                            lobjTargetSystem As IVTLTargetSystem, 
                                            pobjRule As IVTLValidationRule) As List(Of VTLValidationObject)
            Dim lcolValidationResults As New List(Of VTLValidationObject)

            '
            ' Create a SQL query to find any properties on our objects that do not have the exact string value
            '
            Dim lobjProvider = pobjRequestContextService.SPFRequestContext.IDBProvider
            Dim lobjSql As New SPFSQL()
            lobjSql.Append("SELECT o.OBJNAME, o.OBJDEFUID, o.OBJUID, p.OBJOBID, p.OBID, p.PROPERTYDEFUID, p.STRVALUE, p.ACTION, p.COLUMNINDEX, p.COLUMNNAME, p.LINENUMBER, p.FILENAME, p.FILEOBJOBID ")
            lobjSql.Append(String.Format(" FROM VTL{0}OBJ o INNER JOIN VTL{0}OBJPR p ON o.OBID = p.OBJOBID ", pobjJob.Name))
            lobjSql.Append(" WHERE p.STRVALUE NOT LIKE ")
            lobjProvider.AppendBindVariable(lobjSql, "STRVALUE", "%" & pstrUoM)
            lobjSql.Append(" AND o.OBJDEFUID = ")
            lobjProvider.AppendBindVariable(lobjSql, "OBJDEFUID", lobjClassDefToValidate.UID)
            lobjSql.Append(" AND p.PROPERTYDEFUID = ")
            lobjProvider.AppendBindVariable(lobjSql, "PROPERTYDEFUID", lcolPropertyDefToValidate.UID)
            '
            ' Open a connection to get all of the property values that do not match the supplied string
            '
            lobjProvider.OpenConnection(False)
            Dim lobjReaderOfInvalidPropertyValues As IDataReader = lobjProvider.ExecuteDataReader(lobjSql)
            '
            ' For each of the rows get all of the required values and create a property validation object
            '
            While (lobjReaderOfInvalidPropertyValues.Read)
                lcolValidationResults.Add(CreateValidationResult(lobjReaderOfInvalidPropertyValues, lobjTargetSystem, pobjRule, pobjRequestContextService, pobjJob))
            End While

            lobjProvider.CloseConnection(False)

            Return lcolValidationResults
        End Function

        Private Function CreateValidationResult(lobjReaderOfInvalidPropertyValues As IDataReader, lobjTargetSystem As IVTLTargetSystem, pobjRule As IVTLValidationRule, pobjRequestContextService As ISPFRequestContextService, pobjJob As IVTLJob) As VTLValidationObject
            Dim lstrOBJNAME As String = lobjReaderOfInvalidPropertyValues.GetString(0)
            Dim lstrOBJDEFUID As String = lobjReaderOfInvalidPropertyValues.GetString(1)
            Dim lstrOBJUID As String = lobjReaderOfInvalidPropertyValues.GetString(2)
            Dim lstrOBJOBID As String = lobjReaderOfInvalidPropertyValues.GetString(3)
            Dim lstrPROPERTYOBID As String = lobjReaderOfInvalidPropertyValues.GetString(4)
            Dim lstrPROPERTYDEFUID As String = lobjReaderOfInvalidPropertyValues.GetString(5)
            Dim lstrSTRVALUE As String = lobjReaderOfInvalidPropertyValues.GetString(6)
            Dim lstrACTION As String = lobjReaderOfInvalidPropertyValues.GetString(7)
            Dim lstrCOLUMNINDEX As Integer = Convert.ToInt32(lobjReaderOfInvalidPropertyValues(8))
            Dim lstrCOLUMNNAME As String = lobjReaderOfInvalidPropertyValues.GetString(9)
            Dim lstrLINENUMBER As Integer = Convert.ToInt32(lobjReaderOfInvalidPropertyValues(10))
            Dim lstrFILENAME As String = lobjReaderOfInvalidPropertyValues.GetString(11)
            Dim lstrFILEOBJOBID As String = lobjReaderOfInvalidPropertyValues.GetString(12)

            Return ValidationObjectCreation.CreatePropertyValidationObject(lobjTargetSystem, pobjRule, pobjRequestContextService,
                                                                                             lstrACTION, lstrCOLUMNINDEX, lstrFILENAME, lstrFILEOBJOBID, lstrLINENUMBER, lstrCOLUMNNAME, lstrSTRVALUE, lstrPROPERTYOBID, lstrPROPERTYDEFUID,
                                                                                             lstrOBJOBID, lstrOBJUID, lstrOBJDEFUID, lstrOBJNAME, pobjJob.UID, pobjJob)
        End Function

    End Class


End Namespace
