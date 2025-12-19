
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 05/07/2019 12:41:07
-- Generated from EDMX file: D:\Maaz Projects\rittal_risourcecenter\newrisourcecenter\Models\RittalUSModel.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [DB_Inv_Sys];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------


-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[RittalUSModelStoreContainer].[tbl_MRK_Zipcode]', 'U') IS NOT NULL
    DROP TABLE [RittalUSModelStoreContainer].[tbl_MRK_Zipcode];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'tbl_MRK_Zipcode'
CREATE TABLE [dbo].[tbl_MRK_Zipcode] (
    [id] int IDENTITY(1,1) NOT NULL,
    [PostalCode] nvarchar(10)  NULL,
    [CGrp] nvarchar(5)  NULL,
    [V1] nvarchar(30)  NULL,
    [V1Name] nvarchar(60)  NULL,
    [V1Email] nvarchar(200)  NULL,
    [V1Phone] nvarchar(18)  NULL,
    [V6] nvarchar(30)  NULL,
    [V6Name] nvarchar(60)  NULL,
    [V6Phone] nvarchar(18)  NULL,
    [V6Email] nvarchar(200)  NULL,
    [DateImported] varchar(10)  NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [id] in table 'tbl_MRK_Zipcode'
ALTER TABLE [dbo].[tbl_MRK_Zipcode]
ADD CONSTRAINT [PK_tbl_MRK_Zipcode]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------