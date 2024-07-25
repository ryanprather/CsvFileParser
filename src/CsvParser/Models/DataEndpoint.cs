using System;

namespace CsvDataParser.Models
{
    public class DataEndpoint
    {
        public string Name { get; set; }
        public Type DataType { get; set; }
        public int? Index {  get; set; } 
    }
}
