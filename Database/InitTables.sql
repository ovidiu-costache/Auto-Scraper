-- Run in an explicitly selected, empty application database.
CREATE TABLE dbo.CarListings
(
    Id UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT DF_CarListings_Id DEFAULT NEWID(),

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
    ListingUrl NVARCHAR(500) NOT NULL,
    CoverImage VARBINARY(MAX) NULL,
    Source NVARCHAR(50) NOT NULL,

    -- UTC time of the latest successful observation; refresh explicitly on update.
    ScrapedAt DATETIME2(3) NOT NULL
        CONSTRAINT DF_CarListings_ScrapedAt DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_CarListings
        PRIMARY KEY CLUSTERED (Id),

    CONSTRAINT UQ_CarListings_ListingUrl
        UNIQUE NONCLUSTERED (ListingUrl),

    CONSTRAINT CK_CarListings_Make
        CHECK (LEN(LTRIM(RTRIM(Make))) > 0),

    CONSTRAINT CK_CarListings_Model
        CHECK (LEN(LTRIM(RTRIM(Model))) > 0),

    CONSTRAINT CK_CarListings_Year
        CHECK (Year BETWEEN 1886 AND 9999),

    CONSTRAINT CK_CarListings_Mileage
        CHECK (Mileage IS NULL OR Mileage >= 0),

    CONSTRAINT CK_CarListings_Price
        CHECK (Price > 0),

    CONSTRAINT CK_CarListings_Currency
        CHECK (Currency IN ('EUR', 'RON')),

    CONSTRAINT CK_CarListings_EngineCapacity
        CHECK (EngineCapacity IS NULL OR EngineCapacity > 0),

    CONSTRAINT CK_CarListings_ListingUrl
        CHECK (LEN(LTRIM(RTRIM(ListingUrl))) > 0),

    CONSTRAINT CK_CarListings_Source
        CHECK (LEN(LTRIM(RTRIM(Source))) > 0)
);
GO

CREATE INDEX IX_CarListings_Make_Model_Currency_Year
    ON dbo.CarListings (Make, Model, Currency, Year)
    INCLUDE (Price, Mileage, EngineCapacity);
GO
