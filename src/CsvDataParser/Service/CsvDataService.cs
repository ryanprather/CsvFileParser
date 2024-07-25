using CsvDataParser.Models;
using CsvHelper;
using CsvHelper.Configuration;
using FluentResults;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("CsvDataParser.Test")]
namespace CsvDataParser.Service
{
    public class CsvDataService : ICsvDataService
    {
        private readonly TimeSeriesDatasetMapDefinition _jsonFileMap;
        private readonly string _dataFileLocation;

        public CsvDataService(TimeSeriesDatasetMapDefinition jsonFileMap, string fileLocation)
        {
            if (string.IsNullOrEmpty(fileLocation)) throw new ArgumentNullException(nameof(fileLocation));
            _jsonFileMap = jsonFileMap ?? throw new ArgumentNullException(nameof(jsonFileMap));
            _dataFileLocation = fileLocation;
        }

        public Result<IEnumerable<CsvTimeSeriesFlatData>> RetrieveFlatCsvData()
        {
            var data = GetTimeSeriesData();
            if (data.Any(x => x.Errors.Count() > 0))
            {
                var errors = data.Where(x => x.IsFailed).SelectMany(x => x.Errors);
                return Result.Fail(errors);
            }

            return Result.Ok(data.Select(x => x.Value));
        }

        internal IEnumerable<Result<CsvTimeSeriesFlatData>> GetTimeSeriesData()
        {
            bool hasBreakingError = false;
            var config = new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = _jsonFileMap.HasHeader };
            using (var reader = new StreamReader(_dataFileLocation))
            using (var csvReader = new CsvReader(reader, config))
            {
                while (csvReader.Read())
                {
                    var dataRow = csvReader.GetRecord<dynamic>();
                    var dictionaryRow = (IDictionary<string, object>)dataRow;
                    // Timestamp column // 
                    var timestampValue = GetValueFromDataRow(dictionaryRow, _jsonFileMap.TimestampColumn);
                    var dateTimeComlumnResult = GetDateTimeColumnValue(timestampValue, _jsonFileMap.TimestampColumn);
                    if (!dateTimeComlumnResult.IsSuccess)
                    {
                        hasBreakingError = true;
                        yield return Result.Fail(ErrorMessages.MissingTimestampColumn);
                    }

                    // Key column validation //
                    var keyColumnValue = GetValueFromDataRow(dictionaryRow, _jsonFileMap.KeyColumn);
                    if (keyColumnValue is null)
                    {
                        hasBreakingError = true;
                        yield return Result.Fail(ErrorMessages.MissingKeyColumn);
                    }

                    if (hasBreakingError)
                        yield break;

                    var tsd = new CsvTimeSeriesFlatData(dateTimeComlumnResult.Value, keyColumnValue);
                    foreach (var endpoint in _jsonFileMap.Endpoints)
                    {
                        if (endpoint.DataType == typeof(Int32))
                        {
                            var value = GetValueFromDataRow(dictionaryRow, endpoint);
                            var endpointResult = GetIntColumnValue(value, endpoint);
                            if (!endpointResult.IsSuccess)
                                yield return Result.Fail(ErrorMessages.EndpointColumnParse(endpoint.Name));
                            else
                                tsd.IntEndpoints.Add(endpoint.Name, endpointResult.Value);
                        }
                        else if (endpoint.DataType == typeof(double))
                        {
                            var value = GetValueFromDataRow(dictionaryRow, endpoint);
                            var endpointResult = GetDoubleColumnValue(value, endpoint);
                            if (!endpointResult.IsSuccess)
                                yield return Result.Fail(ErrorMessages.EndpointColumnParse(endpoint.Name));
                            else
                                tsd.DoubleEndpoints.Add(endpoint.Name, endpointResult.Value);
                        }
                        else if (endpoint.DataType == typeof(float))
                        {
                            var value = GetValueFromDataRow(dictionaryRow, endpoint);
                            var endpointResult = GetFloatColumnValue(value, endpoint);
                            if (!endpointResult.IsSuccess)
                                yield return Result.Fail(ErrorMessages.EndpointColumnParse(endpoint.Name));
                            else
                                tsd.FloatEndpoints.Add(endpoint.Name, endpointResult.Value);
                        }
                        else if (endpoint.DataType == typeof(bool))
                        {
                            var value = GetValueFromDataRow(dictionaryRow, endpoint);
                            var endpointResult = GetBoolColumnValue(value, endpoint);
                            if (!endpointResult.IsSuccess)
                                yield return Result.Fail(ErrorMessages.EndpointColumnParse(endpoint.Name));
                            else
                                tsd.BoolEndpoints.Add(endpoint.Name,endpointResult.Value);
                        }
                    }
                    yield return Result.Ok(tsd);
                }
                yield break;
            }
        }

        internal string GetValueFromDataRow(IDictionary<string, object> dictionaryRow, DataEndpoint endpoint) 
        {
            if (!_jsonFileMap.HasHeader)
                return dictionaryRow.ElementAt(endpoint.Index.GetValueOrDefault()).Value as string;
            else
                return dictionaryRow.FirstOrDefault(x => x.Key == endpoint.Name).Value as string;

        }

        internal Result<bool> GetBoolColumnValue(string value , DataEndpoint endpoint)
        {
            bool boolValue;    
            if (String.IsNullOrWhiteSpace(value)
                || !bool.TryParse(value, out boolValue))
                return Result.Fail(ErrorMessages.EndpointColumnParse(endpoint.Name));

            return Result.Ok(boolValue);
        }

        internal Result<float> GetFloatColumnValue(string value, DataEndpoint endpoint)
        {
            float floatValue;
            if (String.IsNullOrWhiteSpace(value)
                || !float.TryParse(value, out floatValue))
                return Result.Fail(ErrorMessages.EndpointColumnParse(endpoint.Name));

            return Result.Ok(floatValue);
        }

        internal Result<double> GetDoubleColumnValue(string value, DataEndpoint endpoint)
        {
            double doubleValue;    
            if (String.IsNullOrWhiteSpace(value)
                || !Double.TryParse(value, out doubleValue))
                return Result.Fail(ErrorMessages.EndpointColumnParse(endpoint.Name));

            return Result.Ok(doubleValue);
        }

        internal Result<Int32> GetIntColumnValue(string value, DataEndpoint endpoint)
        {
            Int32 intValue;
            if (String.IsNullOrWhiteSpace(value)
                || !Int32.TryParse(value, out intValue))
                    return Result.Fail(ErrorMessages.EndpointColumnParse(endpoint.Name));

            return Result.Ok(intValue);
        }

        internal Result<DateTime> GetDateTimeColumnValue(string value, DataEndpoint endpoint)
        {
            DateTime dateTimeValue;
            if (String.IsNullOrWhiteSpace(value)
                || !DateTime.TryParse(value, out dateTimeValue))
                return Result.Fail(ErrorMessages.EndpointColumnParse(endpoint.Name));

            return Result.Ok(dateTimeValue);
        }

        internal class ErrorMessages
        {
            public static readonly string NullMappingErrorMessage = "Mapping could not be loaded";
            public static readonly string MissingTimestampColumn = "Timestamp column could not be found or parsed";
            public static readonly string MissingKeyColumn = "Key column could not be found or parsed";
            public static string EndpointColumnParse(string endpointname) => $"Could not be found or the value could not be parsed {endpointname} into datatype";

        }
    }
}
