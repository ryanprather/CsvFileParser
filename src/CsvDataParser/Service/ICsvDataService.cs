using CsvDataParser.Models;
using FluentResults;

namespace CsvDataParser.Service
{
    public interface ICsvDataService
    {
        Result<IEnumerable<CsvTimeSeriesFlatData>> RetrieveFlatCsvData();
    }
}
