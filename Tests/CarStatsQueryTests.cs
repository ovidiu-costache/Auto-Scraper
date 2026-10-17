using Application.Queries;
using Xunit;

namespace AutoScraper.Tests
{
    public class CarStatsQueryTests
    {
        [Theory]
        [InlineData(" eur ", "EUR")]
        [InlineData(" ron ", "RON")]
        public void Constructor_NormalizesTextFilters(string currency, string expectedCurrency)
        {
            var query = new CarStatsQuery(" BMW ", " 320 ", 2015, 2022, currency);

            Assert.Equal("BMW", query.Make);
            Assert.Equal("320", query.Model);
            Assert.Equal(expectedCurrency, query.Currency);
            Assert.Empty(query.GetValidationErrors());
        }

        [Theory]
        [InlineData(1886, 9999)]
        [InlineData(1886, 1886)]
        [InlineData(9999, 9999)]
        [InlineData(2020, 2020)]
        public void GetValidationErrors_AcceptsInclusiveBoundaries(int minYear, int maxYear)
        {
            var query = new CarStatsQuery(
                new string('x', 50), new string('x', 50), minYear, maxYear, "RON");

            Assert.Empty(query.GetValidationErrors());
        }
    }
}
