using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace AutoScraper.Tests
{
    public class CarStatsEndpointTests
    {
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

            var validParameters = new Dictionary<string, string>
            {
                ["make"] = "BMW",
                ["model"] = "320",
                ["minYear"] = "2015",
                ["maxYear"] = "2022",
                ["currency"] = "EUR"
            };
            var invalidParameters = new (string Parameter, string? Value, string ErrorKey)[]
            {
                ("make", null, "query"),
                ("model", null, "query"),
                ("minYear", null, "query"),
                ("maxYear", null, "query"),
                ("currency", null, "query"),
                ("make", "", "make"),
                ("make", " \t ", "make"),
                ("make", new string('x', 51), "make"),
                ("model", "", "model"),
                ("model", " \t ", "model"),
                ("model", new string('x', 51), "model"),
                ("minYear", "", "query"),
                ("minYear", "abc", "query"),
                ("minYear", "2147483648", "query"),
                ("maxYear", "", "query"),
                ("maxYear", "abc", "query"),
                ("maxYear", "2147483648", "query"),
                ("minYear", "1885", "minYear"),
                ("minYear", "10000", "minYear"),
                ("maxYear", "1885", "maxYear"),
                ("maxYear", "10000", "maxYear"),
                ("minYear", "2023", "minYear"),
                ("currency", "", "currency"),
                ("currency", " \t ", "currency"),
                ("currency", "USD", "currency")
            };

            foreach (var (parameter, value, errorKey) in invalidParameters)
            {
                var parameters = new Dictionary<string, string>(validParameters);
                if (value is null)
                {
                    parameters.Remove(parameter);
                }
                else
                {
                    parameters[parameter] = value;
                }

                var query = string.Join("&", parameters.Select(pair =>
                    $"{pair.Key}={Uri.EscapeDataString(pair.Value)}"));
                using var response = await client.GetAsync("/api/car-stats?" + query);

                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
                using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                Assert.Equal(400, document.RootElement.GetProperty("status").GetInt32());
                Assert.True(document.RootElement.GetProperty("errors").TryGetProperty(errorKey, out _), query);
            }
        }

        [Fact]
        public async Task Get_OpenApiDescribesRequiredQueryAndResponseContracts()
        {
            using var app = new CarListingsApiFactory();
            using var client = app.CreateClient();
            using var response = await client.GetAsync("/openapi/v1.json");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var operation = document.RootElement.GetProperty("paths").GetProperty("/api/car-stats").GetProperty("get");
            Assert.Equal("GetCarStats", operation.GetProperty("operationId").GetString());
            var parameters = operation.GetProperty("parameters").EnumerateArray()
                .ToDictionary(parameter => parameter.GetProperty("name").GetString()!);
            Assert.Equal(5, parameters.Count);

            foreach (var name in new[] { "make", "model", "minYear", "maxYear", "currency" })
            {
                var parameter = parameters[name];
                Assert.Equal("query", parameter.GetProperty("in").GetString());
                Assert.True(parameter.GetProperty("required").GetBoolean());
                var schema = parameter.GetProperty("schema");
                var type = schema.GetProperty("type");
                if (name is "minYear" or "maxYear")
                {
                    Assert.Equal("integer", type.GetString());
                    Assert.Equal("int32", schema.GetProperty("format").GetString());
                }
                else
                {
                    Assert.Equal("string", type.GetString());
                }
            }

            var responses = operation.GetProperty("responses");
            var successSchema = responses.GetProperty("200").GetProperty("content")
                .GetProperty("application/json").GetProperty("schema");
            Assert.EndsWith("/CarStatsDto", successSchema.GetProperty("$ref").GetString());
            Assert.True(responses.GetProperty("400").GetProperty("content")
                .TryGetProperty("application/problem+json", out _));
        }
    }
}
