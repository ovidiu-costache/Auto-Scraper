-- Run in the same database as InitTables.sql.
CREATE OR ALTER PROCEDURE dbo.GetCarStats
    @Make NVARCHAR(50),
    @Model NVARCHAR(50),
    @MinYear INT,
    @MaxYear INT,
    @Currency CHAR(3)
AS
BEGIN
    SET NOCOUNT ON;

    IF @Make IS NULL OR LEN(LTRIM(RTRIM(@Make))) = 0
        THROW 50001, 'Make is required.', 1;

    IF @Model IS NULL OR LEN(LTRIM(RTRIM(@Model))) = 0
        THROW 50002, 'Model is required.', 1;

    IF @MinYear IS NULL
       OR @MaxYear IS NULL
       OR @MinYear < 1886
       OR @MaxYear > 9999
       OR @MinYear > @MaxYear
        THROW 50003, 'Invalid year range.', 1;

    IF @Currency IS NULL OR @Currency NOT IN ('EUR', 'RON')
        THROW 50004, 'Unsupported currency.', 1;

    SELECT
        @Make AS Make,
        @Model AS Model,
        @Currency AS Currency,
        COUNT_BIG(*) AS TotalListings,
        COUNT_BIG(Mileage) AS MileageListings,
        COUNT_BIG(EngineCapacity) AS EngineCapacityListings,
        CAST(AVG(Price) AS DECIMAL(18, 2)) AS AveragePrice,
        CAST(
            AVG(CAST(Mileage AS DECIMAL(18, 2)))
            AS DECIMAL(18, 2)
        ) AS AverageMileage,
        CAST(
            AVG(CAST(EngineCapacity AS DECIMAL(18, 2)))
            AS DECIMAL(18, 2)
        ) AS AverageEngineCapacity
    FROM dbo.CarListings
    WHERE Make = @Make
      AND Model = @Model
      AND Currency = @Currency
      AND Year BETWEEN @MinYear AND @MaxYear;
END;
GO
