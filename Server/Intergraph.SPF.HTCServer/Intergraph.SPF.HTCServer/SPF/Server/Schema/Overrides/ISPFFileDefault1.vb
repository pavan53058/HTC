Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.FileIO
Imports SPF.Server.Context
Imports SPF.Utilities
Imports SPF.Server.Schema.Interface.Generated
Imports SPF.Server.QueryClasses
Imports Intergraph.SPF.DAL.Common.Criteria.Object
Imports SPF.Server.Schema.Collections
Imports Intergraph.VTL.Common.Import.Classes.FileReaders
Imports SPF.Server.Schema.Model

Namespace SPF.Server.Schema.Interface.Default

    Public Class ISPFFileDefault1
        Inherits ISPFFileDefault

        Public Sub New(ByVal pblnInstantiateRequiredItems As Boolean)
            MyBase.New(pblnInstantiateRequiredItems)
        End Sub



        Public Overrides Sub MoveFileToVault(pobjVault As Generated.ISPFVault)
            '
            ' Begin the File Move Transaction
            '
            If Me.ClassDefinitionUID = "SPFDesignFile" AndAlso Me.GetEnd1Relationships.GetRel("SPFFileComposition") IsNot Nothing AndAlso Me.GetEnd1Relationships.GetRel("SPFFileComposition").GetEnd2 IsNot Nothing AndAlso
            Me.GetEnd1Relationships.GetRel("SPFFileComposition").GetEnd2.ClassDefinitionUID = "SDALoader" Then

                'To load Tags-this applies only for RDL Update
                If GetJobDef(Me.GetEnd1Relationships.GetRel("SPFFileComposition").GetEnd2) = "HTCTagUpdate(Inverted)" Then
                    Dim tempfilepath = DirectoryHelper.GetAppServerTempPath()
                    Dim sessionid = SPFRequestContext.Instance.SessionID

                    Dim decompressedFilePath As String = Path.GetTempPath + SPFLocalFileName
                    If File.Exists(decompressedFilePath) Then
                        File.Delete(decompressedFilePath)
                    End If
                    SPF.Utilities.FileUtilities.DeCompressFile(tempfilepath + "\SPFFileService\" + sessionid + "\" + Me.SPFLocalFileName, decompressedFilePath)

                    'Transpose input file to inverted tag mappings format
                    TransposeCSVFile(decompressedFilePath, True)

                    SPF.Utilities.FileUtilities.CompressFile(decompressedFilePath, Path.GetTempPath + "decompressed" + Me.SPFLocalFileName)

                    File.Copy(Path.GetTempPath + "decompressed" + Me.SPFLocalFileName, tempfilepath + "\SPFFileService\" + sessionid + "\" + Me.SPFLocalFileName, True)

                    If File.Exists(Path.GetTempPath + "decompressed" + Me.SPFLocalFileName) Then
                        File.Delete(Path.GetTempPath + "decompressed" + Me.SPFLocalFileName)
                    End If
                    If File.Exists(decompressedFilePath) Then
                        File.Delete(decompressedFilePath)
                    End If
                End If

                Dim lJobDefName As String = GetJobDef(Me.GetEnd1Relationships.GetRel("SPFFileComposition").GetEnd2)
                'To load Documents-This applies only for the Document and Vendor print import Jobs
                If lJobDefName.StartsWith("HTC_DocCreate") OrElse lJobDefName.Equals("HTC_DocUpdate") Then
                    Dim noOfFiles = GetNoOfFiles(Me.GetEnd1Relationships.GetRel("SPFFileComposition").GetEnd2)
                    Try
                        Dim tempfilepath = DirectoryHelper.GetAppServerTempPath()
                        Dim sessionid = SPFRequestContext.Instance.SessionID

                        Dim decompressedFilePath As String = Path.GetTempPath + SPFLocalFileName
                        If File.Exists(decompressedFilePath) Then
                            File.Delete(decompressedFilePath)
                        End If
                        SPF.Utilities.FileUtilities.DeCompressFile(tempfilepath + "\SPFFileService\" + sessionid + "\" + Me.SPFLocalFileName, decompressedFilePath)

                        If (lJobDefName).Contains("Extend") Then
                            SeperateFileColInCSVFile(decompressedFilePath, noOfFiles, True, True)
                        Else
                            SeperateFileColInCSVFile(decompressedFilePath, noOfFiles, True, False)
                        End If
                        'Split file names seperated by comma from input file and write o discrete columns

                        SPF.Utilities.FileUtilities.CompressFile(decompressedFilePath, Path.GetTempPath + "decompressed" + Me.SPFLocalFileName)

                        File.Copy(Path.GetTempPath + "decompressed" + Me.SPFLocalFileName, tempfilepath + "\SPFFileService\" + sessionid + "\" + Me.SPFLocalFileName, True)

                        If File.Exists(Path.GetTempPath + "decompressed" + Me.SPFLocalFileName) Then
                            File.Delete(Path.GetTempPath + "decompressed" + Me.SPFLocalFileName)
                        End If
                        If File.Exists(decompressedFilePath) Then
                            File.Delete(decompressedFilePath)
                        End If
                    Catch ex As Exception
                        Throw ex
                    End Try
                End If

            End If

            MyBase.MoveFileToVault(pobjVault)


        End Sub

        ''' <summary>
        ''' Gets the job def name to validate
        ''' </summary>
        ''' <param name="pobjLoader"></param>
        ''' <returns></returns>
        Public Function GetJobDef(pobjLoader As Generated.IObject) As String
            Dim lstrJobName As String = String.Empty

            Dim lobjPrimaryClassification = CType(pobjLoader.ToInterface("ISPFClassifiedItem"), ISPFClassifiedItem).GetPrimaryClassification
            Dim lobjJobDetails = lobjPrimaryClassification.GetEnd1Relationships.GetRel("SPFObjClassToSDVJobDetails").GetEnd2
            lstrJobName = lobjJobDetails.GetEnd1Relationships.GetRel("SDVJobDetailsJobDefinition").GetEnd2.Name
            Return lstrJobName
        End Function
        ''' <summary>
        ''' Gets the no.of allowed files from the import def instead of hard coding
        ''' </summary>
        ''' <param name="pobjLoader"></param>
        ''' <returns></returns>
        Public Function GetNoOfFiles(pobjLoader As Generated.IObject) As String
            Dim noOfFiles As UInteger = 0

            Dim lobjPrimaryClassification = CType(pobjLoader.ToInterface("ISPFClassifiedItem"), ISPFClassifiedItem).GetPrimaryClassification
            Dim lobjJobDetails = lobjPrimaryClassification.GetEnd1Relationships.GetRel("SPFObjClassToSDVJobDetails").GetEnd2
            Dim lobjJobName = lobjJobDetails.GetEnd1Relationships.GetRel("SDVJobDetailsJobDefinition").GetEnd2
            Dim lobjJobDef = lobjJobName.GetEnd1Relationships.GetRel("VTLJobDefinitionImportDefinition").GetEnd2
            Dim lobjFiles = lobjJobDef.GetRels("VTLImportDefHeader").GetEnd2s.Values
            For Each lobjMapHeader As IObject In lobjFiles
                If lobjMapHeader.Name.StartsWith("File") AndAlso Not (lobjMapHeader.Name.EndsWith("ame") Or lobjMapHeader.Name.EndsWith("cation")) Then
                    noOfFiles = noOfFiles + 1
                End If
            Next
            Return noOfFiles
        End Function
        ''' <summary>
        ''' This is for the Tag import
        '''Given Tag CSV will get transposed(invereted form) to support multiple classification types-otherwise,create specific classification imports need to be created
        ''' </summary>
        ''' <param name="pstrDecompressedFilePath"></param>
        ''' <param name="pblnHasHeaders"></param>

        Public Sub TransposeCSVFile(pstrDecompressedFilePath As String, pblnHasHeaders As String)
            Try
                Dim lcolResult As StringBuilder = New StringBuilder()
                Dim lcolCSVContents As New List(Of String)

                Using lobjCsvReader As VTLCSVFileReader = New VTLCSVFileReader(pstrDecompressedFilePath, pblnHasHeaders, False)

                    Dim inputFileName As String = Path.GetFileName(pstrDecompressedFilePath)

                    Dim lcolHeaders As New List(Of String)

                    If lobjCsvReader.HasHeaders Then
                        lcolHeaders = lobjCsvReader.GetHeaders().ToList()
                        lcolResult.Append("plantcode,Object_Name,Property_Name,Property_Value,Property_Value_UoM")
                        lcolResult.Append(Environment.NewLine)
                    End If

                    Dim namecolIndex As Integer = lcolHeaders.IndexOf("Name")

                    While lobjCsvReader.Read()
                        Dim lcolRowValues As List(Of String) = New List(Of String)()
                        Dim tagUPCcode As String = GetUPCCode(lobjCsvReader(namecolIndex))

                        For index As Integer = 0 To lobjCsvReader.FieldCount - 1

                            If index <> lcolHeaders.IndexOf("plantcode") AndAlso Not lcolHeaders(index).Contains("-UOM") AndAlso
                                        Not String.IsNullOrEmpty(lobjCsvReader(index)) AndAlso Not lobjCsvReader(index).Equals("") AndAlso Not lcolHeaders(index).Contains("cost center") Then

                                lcolResult.Append(tagUPCcode + ",")
                                lcolResult.Append(lobjCsvReader(lcolHeaders.IndexOf("Name")) + ",")
                                lcolResult.Append(lcolHeaders(index) + ",")
                                If lobjCsvReader(index).Contains(",") Then
                                    lcolResult.Append("""" + lobjCsvReader(index) + """" + ",")
                                Else
                                    lcolResult.Append(lobjCsvReader(index) + ",")
                                End If
                                If (index <> (lobjCsvReader.FieldCount - 1)) AndAlso lcolHeaders(index + 1) = lcolHeaders(index) + "-UOM" Then
                                    lcolResult.Append(lobjCsvReader(index + 1))
                                    lcolResult.Append(Environment.NewLine)
                                Else
                                    lcolResult.Append(Environment.NewLine)
                                End If
                            End If
                        Next
                        lcolResult.Remove(lcolResult.Length - 1, 1)
                    End While
                End Using

                lcolCSVContents.Add(lcolResult.ToString)
                File.Delete(pstrDecompressedFilePath)
                File.WriteAllLines(pstrDecompressedFilePath, lcolCSVContents, Encoding.UTF8)
            Catch ex As Exception
                Throw ex
            End Try

        End Sub
        ''' <summary>
        ''' Get UPC code from the tag-this will be part of inverted/transposed CSV for RDL update import def
        ''' </summary>
        ''' <param name="pTagName"></param>
        ''' <returns></returns>
        Public Function GetUPCCode(pTagName As String) As String
            Dim lUPCcode As String = String.Empty
            Dim lobjAdvDocSearchCriteria As DynamicQuery = New DynamicQuery()
            lobjAdvDocSearchCriteria.Query.Criteria = New ObjectCriteria(Function(x) x.HasInterface("IHTCTag") And x.Name = pTagName)
            Dim lIobjectList As IObjectDictionary = lobjAdvDocSearchCriteria.ExecuteToIObjectDictionary()
            If lIobjectList.Count > 0 Then
                Dim lobjTag As IObject = lIobjectList.Values(0)
                Dim lobjPlant As IObject = lobjTag.GetEnd2Relationships().GetRel("HTCPlantCodeToTag").GetEnd1()
                lUPCcode = lobjPlant.Interfaces("IHTCPlantCodeDetails").Properties("HTCUPCCode").ToDisplayValue
            End If

            Return lUPCcode
        End Function
        ''' <summary>
        ''' Splits the comma seperated file column into multiple columns in the CSV--This is to avoid many computed columns to hanlde in import def
        ''' </summary>
        ''' <param name="pstrDecompressedFilePath"></param>
        ''' <param name="noOfFiles"></param>
        ''' <param name="pblnHasHeaders"></param>
        Private Sub SeperateFileColInCSVFile(pstrDecompressedFilePath As String, noOfFiles As UInteger, pblnHasHeaders As Boolean, pblnIsExtendCreateDef As Boolean)
            Try
                Dim lcolResult As StringBuilder = New StringBuilder()
                Dim lcolCSVContents As New List(Of String)

                Using lobjCsvReader As VTLCSVFileReader = New VTLCSVFileReader(pstrDecompressedFilePath, pblnHasHeaders, False)

                    Dim inputFileName As String = Path.GetFileName(pstrDecompressedFilePath)
                    Dim filecolIndex As Integer = -1
                    '
                    ''Prepare headers based on the file count from the import def
                    '
                    If lobjCsvReader.HasHeaders Then
                        Dim lcolRowValues As List(Of String) = New List(Of String)()
                        Dim lcolHeaders As List(Of String) = lobjCsvReader.GetHeaders().ToList()

                        If Not pblnIsExtendCreateDef Then
                            lcolResult.Append("JobNo,")
                        End If

                        For index As Integer = 0 To lcolHeaders.Count() - 1
                            If lcolHeaders(index).Equals("파일명") Then
                                filecolIndex = index
                                For i As UInteger = 1 To noOfFiles
                                    lcolResult.Append("File_Name" + i.ToString() + ",")
                                Next i
                            Else
                                lcolResult.Append(lcolHeaders(index) + ",")
                            End If
                        Next
                        lcolResult.Remove(lcolResult.Length - 1, 1)
                        lcolResult.Append(Environment.NewLine)
                    End If
                    '
                    ''Prepare content,split the file names according to the headers added
                    '
                    While lobjCsvReader.Read()
                        Dim lcolRowValues As List(Of String) = New List(Of String)()
                        Dim lInputFileName As String = inputFileName.Split(".")(0)
                        ''These are the starting Names of EW/Job object.This is to validate whether the given name is EW object or not.If it matches then we add a new column to the input file
                        Dim lstrPrefixValidation = String.Empty
                        If SPFRequestContext.Instance.SPFOptions.Interfaces("ISPFConfigurationOptions").Properties("HTCEWPrefix") IsNot Nothing AndAlso
                            SPFRequestContext.Instance.SPFOptions.Interfaces("ISPFConfigurationOptions").Properties("HTCEWPrefix").Value IsNot Nothing Then
                            lstrPrefixValidation = SPFRequestContext.Instance.SPFOptions.Interfaces("ISPFConfigurationOptions").Properties("HTCEWPrefix").Value.ToString
                        End If

                        If String.IsNullOrWhiteSpace(lstrPrefixValidation) Then
                            Throw New ArgumentNullException("EW Prefix", "There is no value for 'HTCEWPrefix' in the SPF Options")
                        End If
                        Dim lJobAllowableValues As List(Of String) = lstrPrefixValidation.Split(",").ToList

                        If Not pblnIsExtendCreateDef Then
                            If lJobAllowableValues.Any(Function(x) lInputFileName.StartsWith(x.Trim())) = True Then
                                lcolResult.Append(lInputFileName + ",")
                            Else
                                lcolResult.Append(String.Empty + ",")
                            End If
                        End If

                        For index As Integer = 0 To lobjCsvReader.FieldCount - 1
                            If index = filecolIndex Then
                                Dim lcolFileValues As String() = lobjCsvReader(index).Split(",")
                                For Each lFileValue In lcolFileValues
                                    lcolResult.Append(lFileValue + ",")
                                Next
                                For i As Short = lcolFileValues.Count + 1 To noOfFiles
                                    lcolResult.Append(",")
                                Next i
                            Else
                                If index <> filecolIndex AndAlso lobjCsvReader(index).Contains(",") Then
                                    lcolResult.Append("""" + lobjCsvReader(index) + """" + ",")
                                Else
                                    lcolResult.Append(lobjCsvReader(index) + ",")
                                End If
                            End If
                        Next
                        lcolResult.Remove(lcolResult.Length - 1, 1)
                        lcolResult.Append(Environment.NewLine)
                    End While
                End Using

                lcolCSVContents.Add(lcolResult.ToString)
                File.Delete(pstrDecompressedFilePath)
                File.WriteAllLines(pstrDecompressedFilePath, lcolCSVContents, Encoding.UTF8)
            Catch ex As Exception
                Throw ex
            End Try
        End Sub


    End Class


End Namespace
