using Application.Queries;
using Xunit;

namespace AutoScraper.Tests
{
    public class CarListingsQueryTests
    {
        public static TheoryData<CarListingsQuery, string> InvalidQueries => new()
        {
            { new(Make: " "), "make" },
            { new(Make: new string('x', 51)), "make" },
            { new(Model: ""), "model" },
            { new(Model: new string('x', 51)), "model" },
            { new(MinYear: 1885), "minYear" },
            { new(MinYear: 10000), "minYear" },
            { new(MaxYear: 1885), "maxYear" },
            { new(MaxYear: 10000), "maxYear" },
            { new(MinYear: 2022, MaxYear: 2020), "minYear" },
            { new(Currency: "USD"), "currency" },
            { new(Currency: " "), "currency" },
            { new(Page: 0), "page" },
            { new(Page: -1), "page" },
            { new(Page: int.MaxValue, PageSize: 100), "page" },
            { new(PageSize: 0), "pageSize" },
            { new(PageSize: -1), "pageSize" },
            { new(PageSize: 101), "pageSize" }
        };

        [Theory]
        [MemberData(nameof(InvalidQueries))]
        public void GetValidationErrors_RejectsInvalidQuery(CarListingsQuery query, string parameter)
        {
            Assert.Contains(parameter, query.GetValidationErrors().Keys);
        }

        [Fact]
        public void Constructor_NormalizesTextFilters()
        {
            var query = new CarListingsQuery(" BMW ", " 320 ", Currency: " eur ");

            Assert.Equal("BMW", query.Make);
            Assert.Equal("320", query.Model);
            Assert.Equal("EUR", query.Currency);
            Assert.Empty(query.GetValidationErrors());
        }

        [Fact]
        public void GetValidationErrors_AcceptsDefaultsAndValidBoundaries()
        {
            var defaults = new CarListingsQuery();

            Assert.Equal(1, defaults.Page);
            Assert.Equal(20, defaults.PageSize);
            Assert.Empty(defaults.GetValidationErrors());
            Assert.Empty(new CarListingsQuery(
                Make: new string('x', 50), Model: new string('x', 50),
                MinYear: 1886, MaxYear: 9999, Currency: "RON", PageSize: 100).GetValidationErrors());
            Assert.Empty(new CarListingsQuery(MinYear: 2020, MaxYear: 2020).GetValidationErrors());
            Assert.Empty(new CarListingsQuery(Page: int.MaxValue, PageSize: 1).GetValidationErrors());
        }
    }
}
