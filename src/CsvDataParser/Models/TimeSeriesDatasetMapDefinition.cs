using System.Diagnostics.CodeAnalysis;

namespace CsvDataParser.Models
{
    public class TimeSeriesDatasetMapDefinition
    {
        public TimeSeriesDatasetMapDefinition() { }

        [SetsRequiredMembers]
        public TimeSeriesDatasetMapDefinition(DataEndpoint timestampColumn, DataEndpoint keyColumn, List<DataEndpoint> endpoints, bool hasHeader = true)
        {
            TimestampColumn = timestampColumn;
            KeyColumn = keyColumn;
            Endpoints = endpoints;
            HasHeader = hasHeader;
        }

        public bool HasHeader { get; set; }

        public required DataEndpoint TimestampColumn { get; set; }
        public required DataEndpoint KeyColumn { get; set; }
        public required List<DataEndpoint> Endpoints { get; set; }

    }
}
