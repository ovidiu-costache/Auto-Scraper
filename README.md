# AutoScraper

AutoScraper analyzes Romanian second-hand vehicle listings. Prices are asking prices, not completed transaction prices. `ScrapedAt` records the latest successful observation in UTC; it is not a listing publication or sale date. A disappearing listing does not prove a sale.

## Stack and current functionality

- .NET 10 / ASP.NET Core Minimal API, EF Core 10 and SQL Server.
- `Domain`: entities; `Application`: queries and DTOs; `Infrastructure`: persistence and statistics; `Api`: HTTP endpoints.
- `ScraperWorker` is reserved for future collection. No scraping is implemented. Angular is the intended frontend and is not implemented yet.
- `GET /api/car-listings` supports optional exact `make`/`model` filters under the SQL Server collation, inclusive `minYear`/`maxYear`, and `currency` (`EUR` or `RON`). Text filters are trimmed; currency is normalized to uppercase.
- Years must be between 1886 and 9999. `page` starts at 1; `pageSize` defaults to 20 and accepts 1–100. Offsets exceeding `Int32.MaxValue` are rejected.
- Results contain `items`, `page`, `pageSize` and `hasMore`, ordered by `ScrapedAt` descending, then `Id` ascending. Images are excluded. Invalid queries return `400` Problem Details.
- `GET /api/car-stats` exposes currency-specific statistics through `DbServices.GetStatsAsync` and `dbo.GetCarStats`.

## Local setup

Install the .NET 10 SDK and a local SQL Server instance accessible to your account. Run the following steps from the repository root.

1. Create an empty application database in SQL Server, for example `AutoScraperDb`. Select it explicitly in SSMS or your SQL client.
2. Run `Database/InitTables.sql` once in that empty database to create the table, constraints and index. Do not rerun it against an existing schema.
3. Run `Database/ReadProcedures.sql` in the same database to create/update `dbo.GetCarStats`.
4. Optionally run `Database/DevSeed.sql` against `AutoScraperDb` to add synthetic development data. It inserts missing seed rows without updating existing rows. Its optional cleanup mode removes matching seed identities, URLs and source only. Never use this script for production data.
5. Configure the API connection string locally using its existing User Secrets ID:

   ```powershell
   dotnet user-secrets set --project Api "ConnectionStrings:DefaultConnection" "<local SQL Server connection string>"
   ```

   Replace the placeholder with your local connection string. Keep credentials out of repository files and command examples shared with others. Tests require neither SQL Server nor User Secrets.

Restore and run the API:

```powershell
dotnet restore AutoScraper.slnx
dotnet dev-certs https --trust
dotnet run --project Api --launch-profile https
```

- Swagger UI: https://localhost:7118/swagger/index.html
- OpenAPI: https://localhost:7118/openapi/v1.json
- Example: https://localhost:7118/api/car-listings?make=BMW&model=320&currency=EUR&page=1&pageSize=5

Swagger/OpenAPI are enabled only in Development. `Api/Api.http` contains additional request examples. Without a configured connection string the API fails at startup; an empty listings table returns an empty page.

## Car statistics endpoint

`GET /api/car-stats?make=BMW&model=320&minYear=2015&maxYear=2022&currency=EUR`

All five query parameters are required, with no defaults:

| Parameter | Validation |
| --- | --- |
| `make`, `model` | Trimmed; 1–50 characters. Exact matching under the database collation. |
| `minYear`, `maxYear` | Integers between 1886 and 9999, inclusive; `minYear <= maxYear`. |
| `currency` | Trimmed and normalized to uppercase; `EUR` or `RON`. No currency conversion. |

A successful request returns `200 application/json` with `make`, `model`, `currency`, `totalListings`, `mileageListings`, `engineCapacityListings`, `averagePrice`, `averageMileage` and `averageEngineCapacity`. Counts are 64-bit integers; averages are nullable decimals rounded to two decimal places. Mileage and engine capacity averages exclude missing values, and their counts identify the respective sample sizes. No matching listings returns `200` with zero counts and null averages.

Statistics describe stored listings and asking prices within the requested year range. They do not deduplicate vehicles or imply active listings, completed sales or transaction prices. Response make/model values echo the trimmed request, and the requested year bounds are not included in the response.

Missing parameters, invalid integers and validation failures return `400 application/problem+json`. The `errors` object uses parameter names for validation failures and `query` for binding failures.

To test manually, run the API with the `https` launch profile, open https://localhost:7118/swagger/index.html, expand `GET /api/car-stats` and select **Try it out**. Enter `BMW`, `320`, `2015`, `2022`, `EUR` and select **Execute**. Try `currency=USD` for a validation error or an unmatched make/model for zero counts and null averages. `Api/Api.http` also includes missing-parameter and malformed-integer requests that Swagger may prevent submitting.

Permanent statistics tests verify normalization, valid boundaries, invalid HTTP requests and the OpenAPI contract without SQL Server. EF Core InMemory cannot execute this stored procedure; verify successful statistics responses separately through read-only API requests against a prepared database, comparing them with `EXEC dbo.GetCarStats` using the same five filters. Do not run initialization or seed scripts as part of verification against an existing database.

## Verification

```powershell
dotnet build AutoScraper.slnx -c Debug
dotnet build AutoScraper.slnx -c Release
dotnet test AutoScraper.slnx -c Release
dotnet list AutoScraper.slnx package --vulnerable --include-transitive
git diff --check
```

Stop running instances before rebuilding their output directories. Tests use xUnit, `WebApplicationFactory` and a separate EF Core InMemory store per test host. They exercise query validation and the real HTTP pipeline, filtering, pagination, ordering, nullable fields and image exclusion without accessing the local SQL database. They do not verify SQL Server translation, collation or constraints; verify those separately using read-only API requests against a prepared development database.
