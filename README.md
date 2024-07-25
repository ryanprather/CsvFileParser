# Csv File Parser

## Inputs
 - When constructing the object a ```TimeSeriesDatasetMapDefinition``` and  a ```fileLocation``` is required
    - The TimeSeriesDatasetMapDefinition is a definition for the csv file, a timestamp column, a key column and a list of endpoints are required for parsing. 
 ```
TimeSeriesDatasetMapDefinition(DataEndpoint timestampColumn, DataEndpoint keyColumn, List<DataEndpoint> endpoints, bool hasHeader = true)
 ```
 - DataEndpoints objects are what is used to define the columns in your csv. 
    - **Name** refers to the text value for the name of header of column. *(if your csv has headers)*
    - **DataType** .net datatype you wish to have the value parsed into. 
        - accepted datatypes (System.Boolean, System.Single, System.Double, System.Int32)
    - **Index** is a zero based index for when the csv doesn't have a header 
 ```
 public class DataEndpoint
{
    public string Name { get; set; }
    public Type DataType { get; set; }
    public int? Index {  get; set; } 
}
 ```
### Example Inputs
- csv file with a header
```
var endpoints = new List<DataEndpoint>();
endpoints.Add(new DataEndpoint { DataType = typeof(Int32), Name = "IntValue" });
endpoints.Add(new DataEndpoint { DataType = typeof(double), Name = "DecimalValue" });
endpoints.Add(new DataEndpoint { DataType = typeof(float), Name = "FloatValue" });
endpoints.Add(new DataEndpoint { DataType = typeof(bool), Name = "BoolValue" });
var test = new TimeSeriesDatasetMapDefinition(
    timestampColumn: new DataEndpoint() { DataType = typeof(DateTime), Name = "Date" },
    keyColumn: new DataEndpoint() { DataType = typeof(string), Name = "Email" },
    endpoints: endpoints);
```
- csv file with out a header
```
var endpoints = new List<DataEndpoint>();
endpoints.Add(new DataEndpoint { DataType = typeof(Int32), Index = 2, Name = "IntValue" });
endpoints.Add(new DataEndpoint { DataType = typeof(double), Index = 3, Name = "DecimalValue" });
endpoints.Add(new DataEndpoint { DataType = typeof(float), Index = 4, Name = "FloatValue" });
endpoints.Add(new DataEndpoint { DataType = typeof(bool), Index = 5, Name = "BoolValue" });
return new TimeSeriesDatasetMapDefinition(
    timestampColumn: new DataEndpoint() { DataType = typeof(DateTime), Index = 0, Name = "Date" },
    keyColumn: new DataEndpoint() { DataType = typeof(string), Index = 1, Name = "Email" },
    hasHeader: false,
    endpoints: endpoints);
```
- The TimeSeriesDatasetMapDefinition can also be parsed from json like the following
```
{
    "TimestampColumn":{
        "Name":"Date",
        "DataType":"System.DateTime"
        },
    "KeyColumn":{
        "Name":"Email",
        "DataType":"System.String"
        },   
    "Endpoints":[
        {
        "Name":"IntValue",
        "DataType":"System.Int32"
        },{
            "Name":"DoubleValue",
            "DataType":"System.Double"
        },{
            "Name":"FloatValue",
            "DataType":"System.Single"
        },{
            "Name":"Boolvalue",
            "DataType":"System.Boolean"
        }
    ]    
}
```
- then parsed using a newtonsoft like the following:
```
using (StreamReader r = new StreamReader(path to where ever))
{
    string json = r.ReadToEnd();
    return JsonConvert.DeserializeObject<TimeSeriesDatasetMapDefinition>(json);
}
```
## Execution
- In order to process the file can be done by contructing the object
```var service = new CsvDataService(timeSeriesDatasetMapDefinition, csvFilePath);```
- Then executing the ```var dataResult = service.RetrieveFlatCsvData();```

## Outputs
- TODO
