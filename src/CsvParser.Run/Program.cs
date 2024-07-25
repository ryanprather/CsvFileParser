using CsvParser.Models;
using CsvParser.Service;
using Newtonsoft.Json;

namespace CsvParser.Run
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var csvService = new CsvDataService();
            var jsonFilePath = @"C:\Users\ryanp\source\repos\CsvParserTest.Run\CsvParserTest.Run\Maps\TestTSDMap.json";
            var csvFilePath = @"C:\Users\ryanp\Downloads\CsvTestFile1.csv";
            var map = LoadJsonMapping(jsonFilePath);

            var mapValidation = csvService.ValidateFileMap(map);
            Console.WriteLine("Hello, World!");
        }

        public static TimeSeriesDatasetMapDefinition LoadJsonMapping(string filePath)
        {
            using (StreamReader r = new StreamReader(filePath))
            {
                string json = r.ReadToEnd();
                return JsonConvert.DeserializeObject<TimeSeriesDatasetMapDefinition>(json);
            }
        }
    }
}
