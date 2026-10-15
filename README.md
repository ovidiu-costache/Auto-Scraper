# AutoScraper

AutoScraper analyzes Romanian second-hand vehicle listings. Prices are asking prices, not completed transaction prices. `ScrapedAt` records the latest successful observation in UTC; it is not a listing publication or sale date. A disappearing listing does not prove a sale.

## Stack and current functionality

- .NET 10 / ASP.NET Core Minimal API, EF Core 10 and SQL Server.
- `Domain`: entities; `Application`: queries and DTOs; `Infrastructure`: persistence and statistics; `Api`: HTTP endpoints.
- `ScraperWorker` is reserved for future collection. No scraping is implemented. Angular is the intended frontend and is not implemented yet.
- `GET /api/car-listings` supports optional exact `make`/`model` filters under the SQL Server collation, inclusive `minYear`/`maxYear`, and `currency` (`EUR` or `RON`). Text filters are trimmed; currency is normalized to uppercase.
- Years must be between 1886 and 9999. `page` starts at 1; `pageSize` defaults to 20 and accepts 1–100. Offsets exceeding `Int32.MaxValue` are rejected.
- Results contain `items`, `page`, `pageSize` and `hasMore`, ordered by `ScrapedAt` descending, then `Id` ascending. Images are excluded. Invalid queries return `400` Problem Details.
- Currency-specific statistics are available through `DbServices.GetStatsAsync` and `dbo.GetCarStats`; no statistics HTTP endpoint exists yet.

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

## Verification

```powershell
dotnet build AutoScraper.slnx -c Debug
dotnet build AutoScraper.slnx -c Release
dotnet test AutoScraper.slnx -c Release
dotnet list AutoScraper.slnx package --vulnerable --include-transitive
git diff --check
```

Stop running instances before rebuilding their output directories. Tests use xUnit, `WebApplicationFactory` and a separate EF Core InMemory store per test host. They exercise query validation and the real HTTP pipeline, filtering, pagination, ordering, nullable fields and image exclusion without accessing the local SQL database. They do not verify SQL Server translation, collation or constraints; verify those separately using read-only API requests against a prepared development database.
