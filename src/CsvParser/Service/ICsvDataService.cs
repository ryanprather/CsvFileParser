using CsvDataParser.Models;
using FluentResults;
using System.Collections.Generic;

namespace CsvDataParser.Service
{
    public interface ICsvDataService
    {
        Result ValidateFileMap(TimeSeriesDatasetMapDefinition fileMap);
        IEnumerable<Result<CsvTimeSeriesFlatData>> RetrieveDataFromFile(TimeSeriesDatasetMapDefinition fileMap, string dataFileLocation);
    }
}
