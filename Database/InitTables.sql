IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'AutoScraperDb')
BEGIN
    CREATE DATABASE AutoScraperDb;
END
GO

USE AutoScraperDb;
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CarListings]') AND type in (N'U'))
BEGIN
    CREATE TABLE CarListings (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        Make NVARCHAR(50) NOT NULL,
        Model NVARCHAR(50) NOT NULL,
        Year INT NOT NULL,
        Mileage INT NOT NULL,
        Price DECIMAL(18, 2) NOT NULL,
        FuelType NVARCHAR(30) NOT NULL,
        EngineCapacity INT NOT NULL,
        Transmission NVARCHAR(30) NOT NULL,
        
        -- ListingUrl is strictly used as a unique identifier to prevent the scraper 
        -- from inserting the same car twice on consecutive runs
        ListingUrl NVARCHAR(500) UNIQUE NOT NULL, 
        
        -- Use VARBINARY(MAX) to store the actual byte array of the image file
        CoverImage VARBINARY(MAX) NULL, 
        
        Source NVARCHAR(50) DEFAULT 'Autovit',
        ScrapedAt DATETIME2 NOT NULL DEFAULT GETDATE()
    );
END
GO

IF NOT EXISTS (SELECT TOP 1 1 FROM CarListings)
BEGIN
    INSERT INTO CarListings (Make, Model, Year, Mileage, Price, FuelType, EngineCapacity, Transmission, ListingUrl, CoverImage, Source)
    VALUES 
    -- VW Golf VII 2.0 TDI
    ('Volkswagen', 'Golf', 2018, 150000, 12500.50, 'Diesel', 1968, 'FWD', 'https://autovit.ro/vw-golf-2-0-tdi', 0xFFD8FFE0, 'Autovit'),
    
    -- VW Golf VIII 1.5 TSI
    ('Volkswagen', 'Golf', 2020, 85000, 17200.00, 'Petrol', 1498, 'FWD', 'https://autovit.ro/vw-golf-1-5-tsi', 0xFFD8FFE0, 'Autovit'),
    
    -- Opel Astra K 1.6 CDTI
    ('Opel', 'Astra', 2019, 120000, 10500.00, 'Diesel', 1598, 'FWD', 'https://autovit.ro/opel-astra-1-6-cdti', 0xFFD8FFE0, 'Autovit'),
    
    -- BMW 3 Series F30 320d
    ('BMW', 'Seria 3', 2017, 195000, 15900.00, 'Diesel', 1995, 'RWD', 'https://autovit.ro/bmw-320d-f30', 0xFFD8FFE0, 'Autovit'),
    
    -- BMW 3 Series G20 330e (Plug-in Hybrid)
    ('BMW', 'Seria 3', 2021, 45000, 28500.00, 'Hybrid', 1998, 'AWD', 'https://autovit.ro/bmw-330e-g20', 0xFFD8FFE0, 'Autovit');
    
    -- The 0xFFD8FFE0 is a dummy HEX value representing the standard header of a JPEG file
    -- It acts as a mock for our byte array until the C# scraper provides real image bytes
END
GO