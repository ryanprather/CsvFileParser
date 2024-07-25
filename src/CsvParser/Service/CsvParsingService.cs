using CsvHelper;
using CsvHelper.Configuration;
using FluentResults;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using CsvParser.Lib.Models;

[assembly: InternalsVisibleTo("CsvDataParser.Test")]
namespace CsvParser.Lib.Service
{
    public class CsvParsingService : ICsvParsingService
    {
        private static List<Type> SupportedDataTypes = new List<Type>()
        {
            typeof(double),
            typeof(float),
            typeof(int),
            typeof(bool),
        };

        public CsvParsingService() { }

        public IEnumerable<Result<CsvTimeSeriesData>> RetrieveDataFromFile(TimeSeriesDatasetMapDefinition fileMap, string dataFileLocation)
        {
            var fileExistsResult = FileExsits(dataFileLocation);
            if (!fileExistsResult.IsSuccess)
                yield return fileExistsResult;
            var fileMapValidationResult = ValidateFileMap(fileMap);
            if (!fileMapValidationResult.IsSuccess)
                yield return fileMapValidationResult;

            var config = new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = fileMap.HasHeader };
            using (var reader = new StreamReader(dataFileLocation))
            using (var csvReader = new CsvReader(reader, config))
            {

                bool hasBreakingError = false;
                while (csvReader.Read())
                {
                    var dataRow = csvReader.GetRecord<dynamic>();
                    var dictionaryRow = (IDictionary<string, object>)dataRow;
                    // Timestamp column // 
                    var timestampValue = GetValueFromDataRow(dictionaryRow, fileMap.HasHeader, fileMap.TimestampColumn);
                    var dateTimeComlumnResult = GetDateTimeColumnValue(timestampValue, fileMap.TimestampColumn);
                    if (!dateTimeComlumnResult.IsSuccess)
                    {
                        hasBreakingError = true;
                        yield return Result.Fail(ErrorMessages.MissingTimestampColumn);
                    }

                    // Key column validation //
                    var keyColumnValue = GetValueFromDataRow(dictionaryRow, fileMap.HasHeader, fileMap.KeyColumn);
                    if (keyColumnValue is null)
                    {
                        hasBreakingError = true;
                        yield return Result.Fail(ErrorMessages.MissingKeyColumn);
                    }

                    if (hasBreakingError)
                        yield break;

                    var tsd = new CsvTimeSeriesData(dateTimeComlumnResult.Value, keyColumnValue);
                    foreach (var endpoint in fileMap.Endpoints)
                    {
                        if (endpoint.DataType == typeof(int))
                        {
                            var value = GetValueFromDataRow(dictionaryRow, fileMap.HasHeader, endpoint);
                            var endpointResult = GetIntColumnValue(value, endpoint);
                            if (!endpointResult.IsSuccess)
                                yield return Result.Fail(ErrorMessages.EndpointColumnParse(endpoint.Name));
                            else
                                tsd.IntEndpoints.Add(endpoint.Name, endpointResult.Value);
                        }
                        else if (endpoint.DataType == typeof(double))
                        {
                            var value = GetValueFromDataRow(dictionaryRow, fileMap.HasHeader, endpoint);
                            var endpointResult = GetDoubleColumnValue(value, endpoint);
                            if (!endpointResult.IsSuccess)
                                yield return Result.Fail(ErrorMessages.EndpointColumnParse(endpoint.Name));
                            else
                                tsd.DoubleEndpoints.Add(endpoint.Name, endpointResult.Value);
                        }
                        else if (endpoint.DataType == typeof(float))
                        {
                            var value = GetValueFromDataRow(dictionaryRow, fileMap.HasHeader, endpoint);
                            var endpointResult = GetFloatColumnValue(value, endpoint);
                            if (!endpointResult.IsSuccess)
                                yield return Result.Fail(ErrorMessages.EndpointColumnParse(endpoint.Name));
                            else
                                tsd.FloatEndpoints.Add(endpoint.Name, endpointResult.Value);
                        }
                        else if (endpoint.DataType == typeof(bool))
                        {
                            var value = GetValueFromDataRow(dictionaryRow, fileMap.HasHeader, endpoint);
                            var endpointResult = GetBoolColumnValue(value, endpoint);
                            if (!endpointResult.IsSuccess)
                                yield return Result.Fail(ErrorMessages.EndpointColumnParse(endpoint.Name));
                            else
                                tsd.BoolEndpoints.Add(endpoint.Name, endpointResult.Value);
                        }
                    }
                    yield return Result.Ok(tsd);
                }
                yield break;
            }
        }

        #region data parsers
        internal string GetValueFromDataRow(IDictionary<string, object> dictionaryRow, bool hasHeaderRow, DataEndpoint endpoint)
        {
            if (!hasHeaderRow)
                return dictionaryRow.ElementAt(endpoint.Index.GetValueOrDefault()).Value as string;
            else
                return dictionaryRow.FirstOrDefault(x => x.Key == endpoint.Name).Value as string;

        }

        internal Result<bool> GetBoolColumnValue(string value, DataEndpoint endpoint)
        {
            bool boolValue;
            if (string.IsNullOrWhiteSpace(value)
                || !bool.TryParse(value, out boolValue))
                return Result.Fail(ErrorMessages.EndpointColumnParse(endpoint.Name));

            return Result.Ok(boolValue);
        }

        internal Result<float> GetFloatColumnValue(string value, DataEndpoint endpoint)
        {
            float floatValue;
            if (string.IsNullOrWhiteSpace(value)
                || !float.TryParse(value, out floatValue))
                return Result.Fail(ErrorMessages.EndpointColumnParse(endpoint.Name));

            return Result.Ok(floatValue);
        }

        internal Result<double> GetDoubleColumnValue(string value, DataEndpoint endpoint)
        {
            double doubleValue;
            if (string.IsNullOrWhiteSpace(value)
                || !double.TryParse(value, out doubleValue))
                return Result.Fail(ErrorMessages.EndpointColumnParse(endpoint.Name));

            return Result.Ok(doubleValue);
        }

        internal Result<int> GetIntColumnValue(string value, DataEndpoint endpoint)
        {
            int intValue;
            if (string.IsNullOrWhiteSpace(value)
                || !int.TryParse(value, out intValue))
                return Result.Fail(ErrorMessages.EndpointColumnParse(endpoint.Name));

            return Result.Ok(intValue);
        }

        internal Result<DateTime> GetDateTimeColumnValue(string value, DataEndpoint endpoint)
        {
            DateTime dateTimeValue;
            if (string.IsNullOrWhiteSpace(value)
                || !DateTime.TryParse(value, out dateTimeValue))
                return Result.Fail(ErrorMessages.EndpointColumnParse(endpoint.Name));

            return Result.Ok(dateTimeValue);
        }

        #endregion

        public Result ValidateFileMap(TimeSeriesDatasetMapDefinition fileMap)
        {
            var result = new Result();
            // check timestamp column //
            if (fileMap.TimestampColumn.DataType != typeof(DateTime))
                result.WithError(ErrorMessages.InvalidColumnDataTypeColumn(fileMap.TimestampColumn.DataType.ToString(), nameof(fileMap.TimestampColumn)));
            if (string.IsNullOrEmpty(fileMap.TimestampColumn.Name) || string.IsNullOrWhiteSpace(fileMap.TimestampColumn.Name))
                result.WithError(ErrorMessages.InvalidColumnName(nameof(fileMap.TimestampColumn)));
            if (fileMap.HasHeader == false && fileMap.TimestampColumn.Index is null)
                result.WithError(ErrorMessages.MissingIndexProperty(nameof(fileMap.TimestampColumn)));


            // check key column //
            if (fileMap.KeyColumn.DataType != typeof(string))
                result.WithError(ErrorMessages.InvalidColumnDataTypeColumn(fileMap.KeyColumn.DataType.ToString(), nameof(fileMap.KeyColumn)));
            if (string.IsNullOrEmpty(fileMap.KeyColumn.Name) || string.IsNullOrWhiteSpace(fileMap.KeyColumn.Name))
                result.WithError(ErrorMessages.InvalidColumnName(nameof(fileMap.KeyColumn)));
            if (fileMap.HasHeader == false && fileMap.KeyColumn.Index is null)
                result.WithError(ErrorMessages.MissingIndexProperty(nameof(fileMap.KeyColumn)));

            // loop and check all endpoints // 
            int i = 0;
            foreach (var endpoint in fileMap.Endpoints)
            {
                if (!SupportedDataTypes.Contains(endpoint.DataType))
                    result.WithError(ErrorMessages.InvalidColumnDataTypeColumn(endpoint.DataType.ToString(), nameof(endpoint.Name)));
                if (string.IsNullOrEmpty(endpoint.Name) || string.IsNullOrWhiteSpace(endpoint.Name))
                    result.WithError(ErrorMessages.InvalidColumnNameIndex(i.ToString()));
                if (fileMap.HasHeader == false && endpoint.Index is null)
                    result.WithError(ErrorMessages.MissingIndexProperty($"Endpoint name {endpoint.Name} at index {i}"));
                i++;
            }

            return result;
        }

        internal Result FileExsits(string fileLocation)
        {
            if (File.Exists(fileLocation))
                return Result.Ok();

            return Result.Fail(ErrorMessages.FileNotFound);

        }

        internal class ErrorMessages
        {
            public static readonly string FileNotFound = "File Not Found";
            public static readonly string InvalidTimestampColumn = "Invalid Data Type for Timestamp Column";
            public static readonly string MissingTimestampColumn = "Timestamp column could not be found or parsed";
            public static readonly string MissingKeyColumn = "Key column could not be found or parsed";

            public static string MissingIndexProperty(string columnName) => $"Column {columnName} must have an index set when HasHeader row is false";
            public static string EndpointColumnParse(string endpointname) => $"Could not be found or the value could not be parsed {endpointname} into datatype";
            public static string InvalidColumnName(string columnName) => $"Invalid Name for Column {columnName}";
            public static string InvalidColumnNameIndex(string index) => $"Endpoint at index {index} must have a name set";
            public static string InvalidColumnDataTypeColumn(string type, string columnName) => $"Invalid Data Type {type} for Column {columnName}";
        }
    }
}
