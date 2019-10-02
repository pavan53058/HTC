Imports System.ComponentModel.Composition
Imports Intergraph.SPF.Services.RequestContextService.Implementation
Imports Intergraph.VTL.Server.Services.Common.Classes.Classes.StagingQuery
Imports Intergraph.VTL.Server.Services.Common.Classes.Interfaces.StageToTargetMappings
Imports Intergraph.VTL.Server.Services.Validation.Classes.Classes
Imports Intergraph.VTL.Server.Services.Validation.Classes.Interfaces.ErrorTable
Imports Intergraph.VTL.Server.Services.Validation.Classes.Interfaces.Propagation
Imports Intergraph.VTL.Server.Services.Validation.Classes.Interfaces.RuleExecution
Imports SPF.Common.DataAccessLayer.DataRetrieval
Imports SPF.Server.Schema.Collections
Imports SPF.Server.Schema.[Interface].Generated

Namespace Intergraph.VTL.Server.CustomRule

    ''' <summary>
    ''' Exact string validation rule: This is a simple example of how to validate the imported data set
    ''' Modified Rhanley CR-AM-86275 27/11/2014 - Support CONFIGLEVEL and CURRENTCONFIG requests
    ''' </summary>
    <Export(GetType(IVTLValidationRuleExecution)), PartCreationPolicy(CreationPolicy.NonShared)>
    Public Class VTLHasExactStringValidationRule
        Implements IVTLValidationRuleExecution, IVTLValidationRuleExecution(Of IVTLHasExactStringValidationRule)

#Region " MEF Imports "

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
                ' Get the string that we want to validate for
                '
                Dim lstrExactString As String = pobjRule.Interfaces("IVTLHasExactStringValidationRule").Properties("VTLExactString").Value.ToString()
                '
                ' Create a SQL query to find any properties on our objects that do not have the exact string value
                '
                Dim lobjProvider = pobjRequestContextService.SPFRequestContext.IDBProvider
                Dim lobjSql As New SPFSQL()
                lobjSql.Append("SELECT o.OBJNAME, o.OBJDEFUID, o.OBJUID, p.OBJOBID, p.OBID, p.PROPERTYDEFUID, p.STRVALUE, p.ACTION, p.COLUMNINDEX, p.COLUMNNAME, p.LINENUMBER, p.FILENAME, p.FILEOBJOBID ")
                lobjSql.Append(String.Format(" FROM VTL{0}OBJ o INNER JOIN VTL{0}OBJPR p ON o.OBID = p.OBJOBID ", pobjJob.Name))
                lobjSql.Append(" WHERE p.STRVALUE <> ")
                lobjProvider.AppendBindVariable(lobjSql, "STRVALUE", lstrExactString)
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

            End If

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
