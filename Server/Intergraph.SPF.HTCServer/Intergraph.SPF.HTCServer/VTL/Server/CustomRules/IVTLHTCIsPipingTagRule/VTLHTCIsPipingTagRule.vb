Imports System.ComponentModel.Composition
Imports Intergraph.SPF.Services.RequestContextService.Implementation
Imports Intergraph.VTL.Server.Services.Common.Classes.Classes.StagingQuery
Imports Intergraph.VTL.Server.Services.Common.Classes.Interfaces.StageToTargetMappings
Imports Intergraph.VTL.Server.Services.Validation.Classes.Classes
Imports Intergraph.VTL.Server.Services.Validation.Classes.Interfaces.ErrorTable
Imports Intergraph.VTL.Server.Services.Validation.Classes.Interfaces.Propagation
Imports Intergraph.VTL.Server.Services.Validation.Classes.Interfaces.RuleExecution
Imports SPF.Common.DataAccessLayer.DataRetrieval
Imports SPF.Common.DataAccessLayer.ProviderClasses
Imports SPF.Server.Schema.Collections
Imports SPF.Server.Schema.[Interface].Generated

Namespace Intergraph.VTL.Server.CustomRule

    ''' <summary>
    ''' Validation rule to check the name of non piping tags
    ''' Non-piping tags should have unique name across the system
    ''' </summary>
    <Export(GetType(IVTLValidationRuleExecution)), PartCreationPolicy(CreationPolicy.NonShared)>
    Public Class VTLHTCIsPipingTagRule
        Implements IVTLValidationRuleExecution, IVTLValidationRuleExecution(Of IVTLHTCIsPipingTagRule)

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
            ''HTCTagUpdate(Common)

            If Not pobjRule.VTLTurnValidationRuleOff AndAlso pobjRule.Interfaces.Contains("IVTLHTCIsPipingTagRule") Then

                Dim lobjTargetSystem As IVTLTargetSystem = pobjJob.GetValidationTargetSystem()
                '
                ' Get the class definition and property definitions that we want to validate
                '
                Dim lobjClassDefToValidate As IClassDef = pobjRule.GetValidationRuleClassDef()
                ''Dim lcolPropertyDefToValidate As IPropertyDef = CType(pobjRule.ToInterface("IVTLPropertyValidationRule"), IVTLPropertyValidationRule).GetValidationRulePropertyDef()
                '
                ' Get the string that we want to validate for
                '
                '
                ' Create a SQL query to find any properties on our objects that do not have the exact string value
                '
                Dim lstrExactString As String = String.Empty
                If pobjRule.Interfaces("IVTLHTCIsPipingTagRule").Properties("VTLIsPipingTag") IsNot Nothing Then
                    lstrExactString = pobjRule.Interfaces("IVTLHTCIsPipingTagRule").Properties("VTLIsPipingTag").Value.ToString()

                    If pobjJob.GetJobDefinition.Name = "HTCTagUpdate(Common)" Then
                        Dim lobjProvider As IDBProvider = pobjRequestContextService.SPFRequestContext.IDBProvider

                        lobjProvider.OpenConnection(False)

                        ''This SQL gets the non-piping tags that are existing in the existing CSV
                        ''For Update job,check only the duplicate tags in the Existing CSV as we dont know what tags are getting updated and created--Unless we have a specific column to differentiate as U and C
                        Dim lobjReaderOfInvalidPropertyValues As IDataReader = GetDuplicatesInExistingCSV(lobjProvider, pobjJob, lstrExactString)
                        '
                        ' For each of the rows get all of the required values and create a property validation object
                        '
                        While (lobjReaderOfInvalidPropertyValues.Read)
                            lcolValidationResults.Add(CreateValidationResult(lobjReaderOfInvalidPropertyValues, lobjTargetSystem, pobjRule, pobjRequestContextService, pobjJob))
                        End While
                        lobjProvider.CloseConnection(False)

                    ElseIf pobjJob.GetJobDefinition.Name = "HTCTagCreate(Common)" Then
                        Dim lobjProvider As IDBProvider = pobjRequestContextService.SPFRequestContext.IDBProvider

                        lobjProvider.OpenConnection(False)
                        ''This is for Create job
                        ''For Create job,the system should check for both the existing Tags in the target system and also the duplicate tags in the existing CSV

                        ''Check the duplicate Tags in the CSV
                        Dim lobjReaderOfInvalidPropertyValues As IDataReader = GetDuplicatesInExistingCSV(lobjProvider, pobjJob, lstrExactString)
                        '
                        ' For each of the rows get all of the required values and create a property validation object
                        '
                        While (lobjReaderOfInvalidPropertyValues.Read)
                            lcolValidationResults.Add(CreateValidationResult(lobjReaderOfInvalidPropertyValues, lobjTargetSystem, pobjRule, pobjRequestContextService, pobjJob))
                        End While

                        ' Check Tags in the target system
                        '

                        Dim lobjDuplicateTagNamesInTarget As IDataReader = GetDuplicatesInTargetSystem(lobjProvider, pobjJob, lstrExactString)

                        '
                        ' For each of the rows get all of the required values and create a property validation object
                        '
                        While (lobjDuplicateTagNamesInTarget.Read)
                            lcolValidationResults.Add(CreateValidationResult(lobjDuplicateTagNamesInTarget, lobjTargetSystem, pobjRule, pobjRequestContextService, pobjJob))
                        End While
                        lobjProvider.CloseConnection(False)
                    End If
                End If
            End If

            Return lcolValidationResults
        End Function
        ''' <summary>
        ''' Prepare the reponse validation with all the error details
        ''' </summary>
        ''' <param name="lobjReaderOfInvalidPropertyValues"></param>
        ''' <param name="lobjTargetSystem"></param>
        ''' <param name="pobjRule"></param>
        ''' <param name="pobjRequestContextService"></param>
        ''' <param name="pobjJob"></param>
        ''' <returns></returns>
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
        ''' <summary>
        ''' Gets the duplicate tags from the existing CSV
        ''' </summary>
        ''' <param name="pobjProvider">Provider</param>
        ''' <param name="pobjJob">job</param>
        ''' <param name="pstrExactString">constant string to differentiate piping classification</param>
        ''' <returns>duplicate tags thats in the current CSV</returns>
        Private Function GetDuplicatesInExistingCSV(pobjProvider As IDBProvider, pobjJob As IVTLJob, pstrExactString As String) As IDataReader

            Dim lobjSqlForDuplicateInExistingFile As New SPFSQL()
            Try
                lobjSqlForDuplicateInExistingFile.Append("select o.OBJNAME, o.OBJDEFUID, o.OBJUID, p.OBJOBID, p.OBID, p.PROPERTYDEFUID, p.STRVALUE, p.ACTION, p.COLUMNINDEX, p.COLUMNNAME, p.LINENUMBER, p.FILENAME,p.FILEOBJOBID ")
                lobjSqlForDuplicateInExistingFile.Append(String.Format(" from VTL{0}OBJ o INNER JOIN VTL{0}OBJPR p ON o.OBID = p.OBJOBID ", pobjJob.VTLJobNumber))
                lobjSqlForDuplicateInExistingFile.Append(" where o.objname in  ")
                lobjSqlForDuplicateInExistingFile.Append(String.Format("( SELECT objname FROM VTL{0}OBJ vtlobj join VTL{0}REL vtlrel on vtlrel .uid2=vtlobj.OBJUID ", pobjJob.VTLJobNumber))
                lobjSqlForDuplicateInExistingFile.Append(String.Format("where vtlrel.defuid like '%primaryclassification' and vtlrel. uid1 not like '%{0}%' GROUP BY objname ", pstrExactString))
                lobjSqlForDuplicateInExistingFile.Append("HAVING COUNT(*) > 1 ) and p.propertydefuid='Name'")
            Catch ex As Exception
                Throw ex
            End Try

            Return pobjProvider.ExecuteDataReader(lobjSqlForDuplicateInExistingFile)
        End Function
        ''' <summary>
        ''' Gets the duplicate tags from the target system
        ''' </summary>
        ''' <param name="pobjProvider">Provider</param>
        ''' <param name="pobjJob">job</param>
        ''' <param name="pstrExactString">constant string to differentiate piping classification</param>
        ''' <returns>Duplicate tags in the target systems</returns>
        Private Function GetDuplicatesInTargetSystem(pobjProvider As IDBProvider, pobjJob As IVTLJob, pstrExactString As String) As IDataReader

            Dim lobjSqlForDuplicateInTargetSystem As New SPFSQL()
            Try
                ''This SQL gets the non-piping 
                lobjSqlForDuplicateInTargetSystem.Append("SELECT o.OBJNAME, o.OBJDEFUID, o.OBJUID, p.OBJOBID, p.OBID, p.PROPERTYDEFUID, p.STRVALUE, p.ACTION, p.COLUMNINDEX, p.COLUMNNAME, p.LINENUMBER, p.FILENAME, p.FILEOBJOBID ")
                lobjSqlForDuplicateInTargetSystem.Append(String.Format(" FROM VTL{0}OBJ o INNER JOIN VTL{0}OBJPR p ON o.OBID = p.OBJOBID ", pobjJob.VTLJobNumber))
                lobjSqlForDuplicateInTargetSystem.Append(" WHERE o.OBJName in  ")
                lobjSqlForDuplicateInTargetSystem.Append(String.Format("(select objname from dataobj where objname in(select vtlobj.OBJNAME  from VTL{0}OBJ vtlobj ", pobjJob.VTLJobNumber))
                lobjSqlForDuplicateInTargetSystem.Append(String.Format("join VTL{0}REL vtlrel on vtlrel .uid2=vtlobj.OBJUID ", pobjJob.VTLJobNumber))
                lobjSqlForDuplicateInTargetSystem.Append(String.Format("where vtlrel.defuid like '%primaryclassification' and vtlrel. uid1 NOT like '%{0}%') and OBJDEFUID ='HTCTag') and propertydefuid='Name'", pstrExactString))
                '
            Catch ex As Exception
                Throw ex
            End Try


            Return pobjProvider.ExecuteDataReader(lobjSqlForDuplicateInTargetSystem)

        End Function

    End Class


End Namespace
