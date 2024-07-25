using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvDataParser.Models
{
    public class DataEndpoint
    {
        public string Name { get; set; }
        public Type DataType { get; set; }
        public int? Index {  get; set; } 
    }
}
