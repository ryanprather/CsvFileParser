using CsvParser.Lib.Models;
using FluentResults;
using System.Collections.Generic;

namespace CsvParser.Lib.Service
{
    public interface ICsvParsingService
    {
        Result ValidateFileMap(TimeSeriesDatasetMapDefinition fileMap);
        IEnumerable<Result<CsvTimeSeriesData>> RetrieveDataFromFile(TimeSeriesDatasetMapDefinition fileMap, string dataFileLocation);
    }
}
