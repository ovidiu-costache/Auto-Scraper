using Application.DTOs;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace AutoScraper.Tests
{
    public class CarListingEndpointTests(CarListingsApiFactory factory)
        : IClassFixture<CarListingsApiFactory>, IDisposable
    {
        private readonly HttpClient _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        public void Dispose() => _client.Dispose();

        [Theory]
        [InlineData(1, 2, true, new[] { 1, 2 })]
        [InlineData(2, 2, false, new[] { 3, 4 })]
        [InlineData(3, 2, false, new int[] { })]
        [InlineData(1, 4, false, new[] { 1, 2, 3, 4 })]
        [InlineData(1, 3, true, new[] { 1, 2, 3 })]
        [InlineData(2, 3, false, new[] { 4 })]
        [InlineData(4, 1, false, new[] { 4 })]
        public async Task Get_ReturnsExpectedPage(int page, int pageSize, bool hasMore, int[] expectedIds)
        {
            var result = await _client.GetFromJsonAsync<CarListingsPageDto>(
                $"/api/car-listings?page={page}&pageSize={pageSize}");

            Assert.NotNull(result);
            Assert.Equal(page, result.Page);
            Assert.Equal(pageSize, result.PageSize);
            Assert.Equal(hasMore, result.HasMore);
            Assert.Equal(expectedIds.Select(CarListingsApiFactory.ListingId), result.Items.Select(item => item.Id));
        }

        [Fact]
        public async Task Get_RepeatedPagesKeepEqualTimestampsInIdOrder()
        {
            var first = await _client.GetStringAsync("/api/car-listings?pageSize=2");
            var second = await _client.GetStringAsync("/api/car-listings?pageSize=2&page=2");

            Assert.Equal(first, await _client.GetStringAsync("/api/car-listings?pageSize=2"));
            Assert.Equal(second, await _client.GetStringAsync("/api/car-listings?pageSize=2&page=2"));
            var result = await _client.GetFromJsonAsync<CarListingsPageDto>("/api/car-listings");
            Assert.NotNull(result);
            Assert.Equal(Enumerable.Range(1, 4).Select(CarListingsApiFactory.ListingId),
                result.Items.Select(item => item.Id));
            Assert.Equal(result.Items[1].ScrapedAt, result.Items[2].ScrapedAt);
        }

        [Fact]
        public async Task Get_AppliesCombinedFiltersAndInclusiveYears()
        {
            var result = await _client.GetFromJsonAsync<CarListingsPageDto>(
                "/api/car-listings?make=%20BMW%20&model=%20320%20&currency=%20eur%20&minYear=2020&maxYear=2022");

            Assert.NotNull(result);
            Assert.Equal(new[] { CarListingsApiFactory.ListingId(2), CarListingsApiFactory.ListingId(3) },
                result.Items.Select(item => item.Id));
            Assert.False(result.HasMore);
        }

        [Fact]
        public async Task Get_UnmatchedFilterReturnsEmptyPage()
        {
            var result = await _client.GetFromJsonAsync<CarListingsPageDto>("/api/car-listings?make=NoSuchMake");

            Assert.NotNull(result);
            Assert.Empty(result.Items);
            Assert.False(result.HasMore);
            Assert.Equal(1, result.Page);
            Assert.Equal(20, result.PageSize);
        }

        [Fact]
        public async Task Get_PreservesNullFieldsAndUtcWithoutExposingCoverImage()
        {
            var json = await _client.GetStringAsync("/api/car-listings");
            using var document = JsonDocument.Parse(json);
            var items = document.RootElement.GetProperty("items").EnumerateArray();

            Assert.Equal(4, document.RootElement.GetProperty("items").GetArrayLength());
            foreach (var item in items)
            {
                foreach (var field in new[] { "mileage", "fuelType", "engineCapacity", "drivetrain", "transmission" })
                {
                    Assert.Equal(JsonValueKind.Null, item.GetProperty(field).ValueKind);
                }

                Assert.EndsWith("Z", item.GetProperty("scrapedAt").GetString());
                Assert.DoesNotContain(item.EnumerateObject(),
                    property => property.Name.Equals("CoverImage", StringComparison.OrdinalIgnoreCase));
            }
        }

        [Theory]
        [InlineData("Development")]
        [InlineData("Production")]
        public async Task Get_InvalidQueryReturnsJsonValidationProblem(string environment)
        {
            using var app = new CarListingsApiFactory(environment);
            using var client = app.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });
            client.DefaultRequestHeaders.Accept.ParseAdd("text/html");

            var queries = new (string Query, string ErrorKey)[]
            {
                ("?minYear=abc", "query"),
                ("?page=2147483648", "query"),
                ("?page=", "query"),
                ("?pageSize=abc", "query"),
                ("?page=0", "page"),
                ("?page=2147483647&pageSize=100", "page"),
                ("?pageSize=101", "pageSize"),
                ("?currency=USD", "currency"),
                ("?minYear=2022&maxYear=2020", "minYear"),
                ("?make=%20", "make")
            };

            foreach (var (query, errorKey) in queries)
            {
                using var response = await client.GetAsync("/api/car-listings" + query);
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
                using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                Assert.Equal(400, document.RootElement.GetProperty("status").GetInt32());
                Assert.True(document.RootElement.GetProperty("errors").TryGetProperty(errorKey, out _), query);
            }
        }

        [Theory]
        [InlineData("Development", HttpStatusCode.OK)]
        [InlineData("Production", HttpStatusCode.NotFound)]
        public async Task Get_DocumentationIsDevelopmentOnlyAndWeatherForecastIsRemoved(
            string environment, HttpStatusCode expectedStatus)
        {
            using var app = new CarListingsApiFactory(environment);
            using var client = app.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });

            using var swagger = await client.GetAsync("/swagger/index.html");
            using var openApi = await client.GetAsync("/openapi/v1.json");
            using var weather = await client.GetAsync("/weatherforecast");
            Assert.Equal(expectedStatus, swagger.StatusCode);
            Assert.Equal(expectedStatus, openApi.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, weather.StatusCode);

            if (expectedStatus == HttpStatusCode.OK)
            {
                using var document = JsonDocument.Parse(await openApi.Content.ReadAsStringAsync());
                var paths = document.RootElement.GetProperty("paths");
                Assert.True(paths.TryGetProperty("/api/car-listings", out _));
                Assert.False(paths.TryGetProperty("/weatherforecast", out _));
            }
        }
    }
}
