-- Create database for login system
DROP DATABASE AuthDb

CREATE DATABASE NiceDentistAuthDb;
GO
USE NiceDentistAuthDb;
GO

-- User Table for Login
CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(256) NOT NULL,
    Email NVARCHAR(256) NOT NULL UNIQUE,
    Role NVARCHAR(50) NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    IsActive BIT NOT NULL DEFAULT 1
);