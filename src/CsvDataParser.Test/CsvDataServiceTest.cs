using AutoFixture.Xunit2;
using CsvDataParser.Models;
using CsvDataParser.Service;
using CsvHelper;
using FluentAssertions;
using Moq;
using System.ComponentModel.DataAnnotations;

namespace CsvDataParser.Test
{
    public class CsvDataServiceTest
    {

        public CsvDataServiceTest()
        {
        }

        private TimeSeriesDatasetMapDefinition GetSeriesDatasetMapHeaderFile()
        {
            var endpoints = new List<DataEndpoint>();
            endpoints.Add(new DataEndpoint { DataType = typeof(Int32), Name = "IntValue" });
            endpoints.Add(new DataEndpoint { DataType = typeof(double), Name = "DecimalValue" });
            endpoints.Add(new DataEndpoint { DataType = typeof(float), Name = "FloatValue" });
            endpoints.Add(new DataEndpoint { DataType = typeof(bool), Name = "BoolValue" });
            return new TimeSeriesDatasetMapDefinition(
                timestampColumn: new DataEndpoint() { DataType = typeof(DateTime), Name = "Date" },
                keyColumn: new DataEndpoint() { DataType = typeof(string), Name = "Email" },
                endpoints: endpoints);
        }

        private TimeSeriesDatasetMapDefinition GetSeriesDatasetMapNoHeaderFile()
        {
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
        }


        [Fact(DisplayName = "Ensure Constructor Exception When Null JsonFileMap")]
        public void Ensure_ConstructorException_WhenNullJsonFileMap()
        {
            Action action = () => { new CsvDataService(null, "test"); };
            action.Should().Throw<ArgumentNullException>();
        }

        [Theory(DisplayName = "Ensure Constructor Exception When Empty FileLocation"), AutoData]
        public void Ensure_ConstructorException_WhenEmptyFileLocation(TimeSeriesDatasetMapDefinition jsonFileMap)
        {
            Action action = () => { new CsvDataService(jsonFileMap, ""); };
            action.Should().Throw<ArgumentNullException>();
        }

        [Theory(DisplayName = "Ensure Error When Mismatch Key Header")]
        [InlineData("TestDataFiles\\InvalidKeyHeaderTest.csv")]
        public void Ensure_Error_WhenMismatchKeyHeader(string fileLocation)
        {
            // arrange //
            var map = GetSeriesDatasetMapHeaderFile();
            var sut = new CsvDataService(map, fileLocation);

            // act //
            var parseResult = sut.RetrieveFlatCsvData();

            //assert //
            parseResult.Errors.Should().HaveCount(1);
            parseResult.Errors[0].Message.Should().BeSameAs(CsvDataService.ErrorMessages.MissingKeyColumn);
        }

        [Theory(DisplayName = "Ensure Error When Mismatch Timestamp Header")]
        [InlineData("TestDataFiles\\InvalidTimestampHeaderTest.csv")]
        public void Ensure_Error_WhenMismatchTimestampHeader(string fileLocation)
        {
            // arrange //
            var map = GetSeriesDatasetMapHeaderFile();
            var sut = new CsvDataService(map, fileLocation);

            // act //
            var parseResult = sut.RetrieveFlatCsvData();

            //assert //
            parseResult.Errors.Should().HaveCount(1);
            parseResult.Errors[0].Message.Should().BeSameAs(CsvDataService.ErrorMessages.MissingTimestampColumn);
        }

        [Theory(DisplayName = "Ensure Success When Valid File")]
        [InlineData("TestDataFiles\\ValidHeaderTest.csv")]
        public void Ensure_Success_When_Valid_File(string fileLocation)
        {
            // arrange //
            var map = GetSeriesDatasetMapHeaderFile();
            var sut = new CsvDataService(map, fileLocation);

            // act //
            var parseResult = sut.RetrieveFlatCsvData();

            //assert //
            parseResult.Errors.Should().HaveCount(0);
            parseResult.Value.Should().NotBeNullOrEmpty();
        }

        [Theory(DisplayName = "Ensure Error When Invalid Decimal Value")]
        [InlineData("TestDataFiles\\InvalidDecimalTest.csv")]
        public void Ensure_Error_When_Invalid_Decimal_Value(string fileLocation)
        {
            // arrange //
            var map = GetSeriesDatasetMapHeaderFile();
            var sut = new CsvDataService(map, fileLocation);

            // act //
            var parseResult = sut.RetrieveFlatCsvData();

            //assert //
            parseResult.Errors.Should().HaveCount(1);
            parseResult.Errors[0].Message.Should().BeEquivalentTo(CsvDataService.ErrorMessages.EndpointColumnParse("DecimalValue"));
        }

        [Theory(DisplayName = "Ensure Error When Invalid Decimal Header")]
        [InlineData("TestDataFiles\\InvalidDecimalHeader.csv")]
        public void Ensure_Error_When_Invalid_Decimal_Header(string fileLocation)
        {
            // arrange //
            var map = GetSeriesDatasetMapHeaderFile();
            var sut = new CsvDataService(map, fileLocation);

            // act //
            var parseResult = sut.RetrieveFlatCsvData();

            //assert //
            parseResult.Errors.Should().HaveCount(1);
            parseResult.Errors[0].Message.Should().BeEquivalentTo(CsvDataService.ErrorMessages.EndpointColumnParse("DecimalValue"));
        }

        [Theory(DisplayName = "Ensure Error When No Header Present")]
        [InlineData("TestDataFiles\\NoHeaderFile.csv")]
        public void Ensure_Success_When_No_Header(string fileLocation)
        {
            // arrange //
            var map = GetSeriesDatasetMapNoHeaderFile();
            var sut = new CsvDataService(map, fileLocation);

            // act //
            var parseResult = sut.RetrieveFlatCsvData();

            //assert //
            parseResult.Errors.Should().HaveCount(0);
            parseResult.Value.Should().HaveCountGreaterThan(0);
        }
    }
}