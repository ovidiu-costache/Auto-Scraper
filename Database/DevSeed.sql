-- Synthetic development data only. Select AutoScraperDb explicitly in SSMS.
-- Run with @Cleanup = 0 to insert missing rows without updating existing data.
-- Cleanup: change @Cleanup to 1 and run this entire script to remove only this seed.
-- All ScrapedAt values are fixed UTC observations; they are not listing/sale dates.
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_NAME() <> N'AutoScraperDb'
    THROW 50001, 'Select AutoScraperDb before running this development script.', 1;

IF OBJECT_ID(N'dbo.CarListings', N'U') IS NULL
    THROW 50002, 'The expected dbo.CarListings table does not exist.', 1;

IF @@TRANCOUNT <> 0
    THROW 50003, 'Run this script outside an existing transaction.', 1;

DECLARE @Cleanup BIT = 0;

DECLARE @Seed TABLE
(
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    Make NVARCHAR(50) NOT NULL,
    Model NVARCHAR(50) NOT NULL,
    Year INT NOT NULL,
    Mileage INT NULL,
    Price DECIMAL(18, 2) NOT NULL,
    Currency CHAR(3) NOT NULL,
    FuelType NVARCHAR(30) NULL,
    EngineCapacity INT NULL,
    Drivetrain NVARCHAR(30) NULL,
    Transmission NVARCHAR(30) NULL,
    ListingUrl NVARCHAR(500) NOT NULL UNIQUE,
    ScrapedAt DATETIME2(3) NOT NULL
);

-- BMW/320: 12 rows (8 EUR, 4 RON). The first two share a timestamp to test Id ordering.
INSERT INTO @Seed
    (Id, Make, Model, Year, Mileage, Price, Currency, FuelType, EngineCapacity,
     Drivetrain, Transmission, ListingUrl, ScrapedAt)
VALUES
    ('de5eed00-0000-4000-8000-000000000001', N'BMW', N'320', 2020, NULL, 15900.00, 'EUR', NULL, NULL, NULL, NULL, N'https://example.invalid/autoscraper/dev-seed/v1/001', '2026-10-14T12:00:00.000'),
    ('de5eed00-0000-4000-8000-000000000002', N'BMW', N'320', 2022, 42000, 129500.00, 'RON', N'Diesel', 1995, N'AWD', N'Automatic', N'https://example.invalid/autoscraper/dev-seed/v1/002', '2026-10-14T12:00:00.000'),
    ('de5eed00-0000-4000-8000-000000000003', N'BMW', N'320', 2018, 134000, 14600.00, 'EUR', N'Petrol', 1998, N'RWD', N'Manual', N'https://example.invalid/autoscraper/dev-seed/v1/003', '2026-10-14T11:00:00.000'),
    ('de5eed00-0000-4000-8000-000000000004', N'BMW', N'320', 2016, 178000, 11800.00, 'EUR', N'Diesel', 1995, N'RWD', N'Automatic', N'https://example.invalid/autoscraper/dev-seed/v1/004', '2026-10-14T10:00:00.000'),
    ('de5eed00-0000-4000-8000-000000000005', N'BMW', N'320', 2014, 221000, 44500.00, 'RON', N'Diesel', 1995, N'RWD', N'Manual', N'https://example.invalid/autoscraper/dev-seed/v1/005', '2026-10-14T09:00:00.000'),
    ('de5eed00-0000-4000-8000-000000000006', N'BMW', N'320', 2021, 65000, 21900.00, 'EUR', N'Petrol', 1998, N'AWD', N'Automatic', N'https://example.invalid/autoscraper/dev-seed/v1/006', '2026-10-14T08:00:00.000'),
    ('de5eed00-0000-4000-8000-000000000007', N'BMW', N'320', 2019, 112000, 17200.00, 'EUR', N'Diesel', 1995, NULL, N'Automatic', N'https://example.invalid/autoscraper/dev-seed/v1/007', '2026-10-14T07:00:00.000'),
    ('de5eed00-0000-4000-8000-000000000008', N'BMW', N'320', 2015, 196000, 47500.00, 'RON', N'Petrol', 1997, N'RWD', NULL, N'https://example.invalid/autoscraper/dev-seed/v1/008', '2026-10-14T06:00:00.000'),
    ('de5eed00-0000-4000-8000-000000000009', N'BMW', N'320', 2017, 156000, 13200.00, 'EUR', N'Diesel', 1995, N'RWD', N'Manual', N'https://example.invalid/autoscraper/dev-seed/v1/009', '2026-10-14T05:00:00.000'),
    ('de5eed00-0000-4000-8000-000000000010', N'BMW', N'320', 2023, 28000, 28900.00, 'EUR', N'Petrol', 1998, N'AWD', N'Automatic', N'https://example.invalid/autoscraper/dev-seed/v1/010', '2026-10-14T04:00:00.000'),
    ('de5eed00-0000-4000-8000-000000000011', N'BMW', N'320', 2013, NULL, 36500.00, 'RON', N'Diesel', 1995, N'RWD', N'Manual', N'https://example.invalid/autoscraper/dev-seed/v1/011', '2026-10-14T03:00:00.000'),
    ('de5eed00-0000-4000-8000-000000000012', N'BMW', N'320', 2011, 265000, 5900.00, 'EUR', N'Diesel', 1995, N'RWD', N'Manual', N'https://example.invalid/autoscraper/dev-seed/v1/012', '2026-10-14T02:00:00.000'),

    ('de5eed00-0000-4000-8000-000000000013', N'BMW', N'X5', 2017, 161000, 24500.00, 'EUR', N'Diesel', 2993, N'AWD', N'Automatic', N'https://example.invalid/autoscraper/dev-seed/v1/013', '2026-10-13T12:00:00.000'),
    ('de5eed00-0000-4000-8000-000000000014', N'BMW', N'X5', 2021, 72000, 45900.00, 'EUR', N'Hybrid', 2998, N'AWD', N'Automatic', N'https://example.invalid/autoscraper/dev-seed/v1/014', '2026-10-13T11:00:00.000'),
    ('de5eed00-0000-4000-8000-000000000015', N'Audi', N'A4', 2015, 202000, 10900.00, 'EUR', N'Diesel', 1968, N'FWD', N'Manual', N'https://example.invalid/autoscraper/dev-seed/v1/015', '2026-10-13T10:00:00.000'),
    ('de5eed00-0000-4000-8000-000000000016', N'Audi', N'A4', 2020, 94000, 20900.00, 'EUR', N'Petrol', 1984, N'AWD', N'Automatic', N'https://example.invalid/autoscraper/dev-seed/v1/016', '2026-10-13T09:00:00.000'),
    ('de5eed00-0000-4000-8000-000000000017', N'Audi', N'A4', 2012, 248000, 33500.00, 'RON', NULL, NULL, N'FWD', N'Manual', N'https://example.invalid/autoscraper/dev-seed/v1/017', '2026-10-13T08:00:00.000'),
    ('de5eed00-0000-4000-8000-000000000018', N'Audi', N'A6', 2018, 149000, 119500.00, 'RON', N'Diesel', 2967, N'AWD', N'Automatic', N'https://example.invalid/autoscraper/dev-seed/v1/018', '2026-10-13T07:00:00.000'),
    ('de5eed00-0000-4000-8000-000000000019', N'Volkswagen', N'Golf', 2010, 285000, 3900.00, 'EUR', N'Diesel', 1598, N'FWD', N'Manual', N'https://example.invalid/autoscraper/dev-seed/v1/019', '2026-10-13T06:00:00.000'),
    ('de5eed00-0000-4000-8000-000000000020', N'Volkswagen', N'Golf', 2019, 107000, 13900.00, 'EUR', N'Petrol', 1498, N'FWD', N'Automatic', N'https://example.invalid/autoscraper/dev-seed/v1/020', '2026-10-13T05:00:00.000'),
    ('de5eed00-0000-4000-8000-000000000021', N'Volkswagen', N'Golf', 2016, 173000, 42500.00, 'RON', N'Diesel', 1968, N'FWD', NULL, N'https://example.invalid/autoscraper/dev-seed/v1/021', '2026-10-13T04:00:00.000'),
    ('de5eed00-0000-4000-8000-000000000022', N'Volkswagen', N'Passat', 2021, 81000, 104500.00, 'RON', N'Hybrid', 1395, N'FWD', N'Automatic', N'https://example.invalid/autoscraper/dev-seed/v1/022', '2026-10-13T03:00:00.000'),
    ('de5eed00-0000-4000-8000-000000000023', N'Skoda', N'Octavia', 2014, 216000, 6900.00, 'EUR', N'Diesel', 1598, N'FWD', N'Manual', N'https://example.invalid/autoscraper/dev-seed/v1/023', '2026-10-13T02:00:00.000'),
    ('de5eed00-0000-4000-8000-000000000024', N'Skoda', N'Octavia', 2022, 39000, 18900.00, 'EUR', N'Petrol', 1498, N'FWD', N'Automatic', N'https://example.invalid/autoscraper/dev-seed/v1/024', '2026-10-13T01:00:00.000'),
    ('de5eed00-0000-4000-8000-000000000025', N'Skoda', N'Octavia', 2017, NULL, 53500.00, 'RON', N'Diesel', 1968, NULL, N'Manual', N'https://example.invalid/autoscraper/dev-seed/v1/025', '2026-10-12T12:00:00.000'),
    ('de5eed00-0000-4000-8000-000000000026', N'Dacia', N'Logan', 2012, 198000, 14500.00, 'RON', N'Petrol', 1149, N'FWD', N'Manual', N'https://example.invalid/autoscraper/dev-seed/v1/026', '2026-10-12T11:00:00.000'),
    ('de5eed00-0000-4000-8000-000000000027', N'Dacia', N'Logan', 2024, 0, 67500.00, 'RON', N'LPG', 999, N'FWD', N'Manual', N'https://example.invalid/autoscraper/dev-seed/v1/027', '2026-10-12T10:00:00.000'),
    ('de5eed00-0000-4000-8000-000000000028', N'Dacia', N'Duster', 2020, 88000, 11900.00, 'EUR', N'Diesel', 1461, N'AWD', N'Manual', N'https://example.invalid/autoscraper/dev-seed/v1/028', '2026-10-12T09:00:00.000');

BEGIN TRY
    BEGIN TRANSACTION;

    IF @Cleanup = 1
    BEGIN
        -- Match the exact seed identities, URLs and source; never delete by source alone.
        DELETE listing
        FROM dbo.CarListings AS listing
        INNER JOIN @Seed AS seed
            ON listing.Id = seed.Id
            AND listing.ListingUrl = seed.ListingUrl
        WHERE listing.Source = N'Test';

        SELECT @@ROWCOUNT AS DeletedSeedRows;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.CarListings
            (Id, Make, Model, Year, Mileage, Price, Currency, FuelType, EngineCapacity,
             Drivetrain, Transmission, ListingUrl, CoverImage, Source, ScrapedAt)
        SELECT
            seed.Id, seed.Make, seed.Model, seed.Year, seed.Mileage, seed.Price,
            seed.Currency, seed.FuelType, seed.EngineCapacity, seed.Drivetrain,
            seed.Transmission, seed.ListingUrl, NULL, N'Test', seed.ScrapedAt
        FROM @Seed AS seed
        WHERE NOT EXISTS
        (
            SELECT 1
            FROM dbo.CarListings AS listing
            WHERE listing.Id = seed.Id OR listing.ListingUrl = seed.ListingUrl
        );

        SELECT @@ROWCOUNT AS InsertedSeedRows;
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
