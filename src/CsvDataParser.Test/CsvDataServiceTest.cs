using AutoFixture.Xunit2;
using CsvParser.Lib.Models;
using CsvParser.Lib.Service;
using FluentAssertions;

namespace CsvDataParser.Test
{
    public class CsvDataServiceTest
    {
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

        [Theory(DisplayName = "Ensure Error When File Not Found"), AutoData]
        public void Ensure_Error_WhenFileNotFound(string fileLocation)
        {
            // arrange //
            var map = GetSeriesDatasetMapHeaderFile();
            var sut = new CsvParsingService();

            // act //
            var result = sut.FileExsits(fileLocation);
            
            // assert //
            result.IsFailed.Should().BeTrue();
            result.Errors[0].Message.Should().BeSameAs(CsvParsingService.ErrorMessages.FileNotFound);
        }

        [Fact(DisplayName = "Ensure Error When Invalid Data Type for Timestamp Column")]
        public void Ensure_Error_WhenInvalidDataTypeforTimestampColumn()
        {
            // arrange //
            var map = new TimeSeriesDatasetMapDefinition(
                timestampColumn: new DataEndpoint() { DataType = typeof(int), Name = "Date" },
                keyColumn: new DataEndpoint() { DataType = typeof(string), Name = "Email" },
                endpoints: new List<DataEndpoint>());
            var sut = new CsvParsingService();

            // act //
            var result = sut.ValidateFileMap(map);

            // assert //
            result.IsFailed.Should().BeTrue();
            result.Errors[0].Message.Should().BeEquivalentTo(CsvParsingService.ErrorMessages.InvalidColumnDataTypeColumn(typeof(int).ToString(), "TimestampColumn"));
        }

        [Fact(DisplayName = "Ensure ValidateFilemap can return more than one error")]
        public void EnsureValidateFilemapCanReturnMoreThanOneError()
        {
            // arrange //
            var map = new TimeSeriesDatasetMapDefinition( hasHeader: false,
                timestampColumn: new DataEndpoint() { DataType = typeof(int), Name = "Date" },
                keyColumn: new DataEndpoint() { DataType = typeof(string), Name = "Email" },
                endpoints: new List<DataEndpoint>());
            var sut = new CsvParsingService();

            // act //
            var result = sut.ValidateFileMap(map);

            // assert //
            result.IsFailed.Should().BeTrue();
            result.Errors.Count().Should().BeGreaterThan(1);
        }

        [Theory(DisplayName = "Ensure Error When Mismatch Key Header")]
        [InlineData("TestDataFiles\\InvalidKeyHeaderTest.csv")]
        public void Ensure_Error_WhenMismatchKeyHeader(string fileLocation)
        {
            // arrange //
            var map = GetSeriesDatasetMapHeaderFile();
            var sut = new CsvParsingService();

            // act //
            var data = sut.RetrieveDataFromFile(map, fileLocation);

            //assert //
            data.Where(x=>x.IsFailed).Count().Should().Be(1);
            data.Where(x=>x.IsFailed).First().Errors[0].Message.Should().BeSameAs(CsvParsingService.ErrorMessages.MissingKeyColumn);
        }

        [Theory(DisplayName = "Ensure Error When Mismatch Timestamp Header")]
        [InlineData("TestDataFiles\\InvalidTimestampHeaderTest.csv")]
        public void Ensure_Error_WhenMismatchTimestampHeader(string fileLocation)
        {
            // arrange //
            var map = GetSeriesDatasetMapHeaderFile();
            var sut = new CsvParsingService();

            // act //
            var data = sut.RetrieveDataFromFile(map, fileLocation);

            //assert //
            data.Where(x => x.IsFailed).Count().Should().Be(1);
            data.Where(x => x.IsFailed).First().Errors[0].Message.Should().BeSameAs(CsvParsingService.ErrorMessages.MissingTimestampColumn);
        }

        [Theory(DisplayName = "Ensure Success When Valid File")]
        [InlineData("TestDataFiles\\ValidHeaderTest.csv")]
        public void Ensure_Success_When_Valid_File(string fileLocation)
        {
            // arrange //
            var map = GetSeriesDatasetMapHeaderFile();
            var sut = new CsvParsingService();

            // act //
            var parseResult = sut.RetrieveDataFromFile(map, fileLocation);

            //assert //
            parseResult.Where(x=>x.IsFailed).Count().Should().Be(0);
            parseResult.Where(x=>x.IsSuccess).Should().NotBeNullOrEmpty();
        }

        [Theory(DisplayName = "Ensure Error When Invalid Decimal Value")]
        [InlineData("TestDataFiles\\InvalidDecimalTest.csv")]
        public void Ensure_Error_When_Invalid_Decimal_Value(string fileLocation)
        {
            // arrange //
            var map = GetSeriesDatasetMapHeaderFile();
            var sut = new CsvParsingService();

            // act //
            var parseResult = sut.RetrieveDataFromFile(map, fileLocation);

            //assert //
            parseResult.Where(x => x.IsFailed).Count().Should().Be(1);
            parseResult.Where(x => x.IsFailed).First().Errors[0].Message.Should().BeEquivalentTo(CsvParsingService.ErrorMessages.EndpointColumnParse("DecimalValue"));
        }

        [Theory(DisplayName = "Ensure Error When Invalid Decimal Header")]
        [InlineData("TestDataFiles\\InvalidDecimalHeader.csv")]
        public void Ensure_Error_When_Invalid_Decimal_Header(string fileLocation)
        {
            // arrange //
            var map = GetSeriesDatasetMapHeaderFile();
            var sut = new CsvParsingService();

            // act //
            var parseResult = sut.RetrieveDataFromFile(map, fileLocation);

            //assert //
            parseResult.Where(x => x.IsFailed).Count().Should().Be(1);
            parseResult.Where(x => x.IsFailed).First().Errors[0].Message.Should().BeEquivalentTo(CsvParsingService.ErrorMessages.EndpointColumnParse("DecimalValue"));
        }

        [Theory(DisplayName = "Ensure Error When No Header Present")]
        [InlineData("TestDataFiles\\NoHeaderFile.csv")]
        public void Ensure_Success_When_No_Header(string fileLocation)
        {
            // arrange //
            var map = GetSeriesDatasetMapNoHeaderFile();
            var sut = new CsvParsingService();

            // act //
            var parseResult = sut.RetrieveDataFromFile(map, fileLocation);

            //assert //
            parseResult.Where(x => x.IsFailed).Should().HaveCount(0);
            parseResult.Where(x => x.IsSuccess).Should().HaveCountGreaterThan(0);
        }
    }
}