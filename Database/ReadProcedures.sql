USE AutoScraperDb;
GO

IF OBJECT_ID('sp_GetCars', 'P') IS NOT NULL
    DROP PROCEDURE sp_GetCars;
GO

CREATE PROCEDURE sp_GetCars
    @Make NVARCHAR(50),
    @Model NVARCHAR(50)
AS
BEGIN
    SELECT 
        Id, Make, Model, Year, Mileage, Price, FuelType, 
        EngineCapacity, Transmission, ListingUrl, CoverImage, Source, ScrapedAt
    FROM CarListings
    WHERE Make = @Make AND Model = @Model;
END
GO

IF OBJECT_ID('sp_GetCarStats', 'P') IS NOT NULL
    DROP PROCEDURE sp_GetCarStats;
GO

-- Calculate the averages directly in the database because it's heavily optimized for math operations on large datasets
CREATE PROCEDURE sp_GetCarStats
    @Make NVARCHAR(50),
    @Model NVARCHAR(50),
    @MinYear INT,
    @MaxYear INT
AS
BEGIN
    SELECT 
        @Make AS Make,
        @Model AS Model,

        COUNT(Id) AS TotalListings,
        
        ISNULL(AVG(Price), 0) AS AveragePrice,
        ISNULL(AVG(Mileage), 0) AS AverageMileage,
        ISNULL(AVG(EngineCapacity), 0) AS AverageEngineCapacity
        
    FROM CarListings
    WHERE Make = @Make 
      AND Model = @Model 
      AND Year >= @MinYear 
      AND Year <= @MaxYear;
END
GO