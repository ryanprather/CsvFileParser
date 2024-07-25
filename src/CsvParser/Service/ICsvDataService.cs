using CsvParser.Models;
using FluentResults;
using System.Collections.Generic;

namespace CsvParser.Service
{
    public interface ICsvDataService
    {
        Result ValidateFileMap(TimeSeriesDatasetMapDefinition fileMap);
        IEnumerable<Result<CsvTimeSeriesData>> RetrieveDataFromFile(TimeSeriesDatasetMapDefinition fileMap, string dataFileLocation);
    }
}
