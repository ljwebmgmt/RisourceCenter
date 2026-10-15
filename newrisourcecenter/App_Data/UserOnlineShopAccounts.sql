-- Create table for user-defined online shop accounts
CREATE TABLE [dbo].[UserOnlineShopAccounts] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [UserId] INT NOT NULL,
    [AccountName] NVARCHAR(100) NOT NULL,
    [EmailAddress] NVARCHAR(255) NOT NULL,
    [Password] NVARCHAR(255) NOT NULL,
    [CreatedAt] DATETIME NOT NULL CONSTRAINT DF_UserOnlineShopAccounts_CreatedAt DEFAULT (GETDATE()),
    CONSTRAINT [PK_UserOnlineShopAccounts] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

-- Create index on UserId for faster lookups
CREATE NONCLUSTERED INDEX [IX_UserOnlineShopAccounts_UserId] 
ON [dbo].[UserOnlineShopAccounts] ([UserId]);
GO
