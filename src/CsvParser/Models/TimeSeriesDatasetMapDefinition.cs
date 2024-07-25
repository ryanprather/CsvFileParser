﻿using System.Collections.Generic;

namespace CsvParser.Models
{
    public class TimeSeriesDatasetMapDefinition
    {
        public TimeSeriesDatasetMapDefinition() { }

        public TimeSeriesDatasetMapDefinition(DataEndpoint timestampColumn, DataEndpoint keyColumn, List<DataEndpoint> endpoints, bool hasHeader = true)
        {
            TimestampColumn = timestampColumn;
            KeyColumn = keyColumn;
            Endpoints = endpoints;
            HasHeader = hasHeader;
        }

        public bool HasHeader { get; set; }

        public DataEndpoint TimestampColumn { get; set; }
        public DataEndpoint KeyColumn { get; set; }
        public List<DataEndpoint> Endpoints { get; set; }

    }
}
