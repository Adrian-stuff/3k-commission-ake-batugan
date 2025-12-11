USE SimpleAuthDB;
GO

-- Add FullName to Users if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Users') AND name = 'FullName')
BEGIN
    ALTER TABLE Users ADD FullName NVARCHAR(100) NULL;
END
GO

-- Create Posts table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'Posts') AND type in (N'U'))
BEGIN
    CREATE TABLE Posts (
        Id INT PRIMARY KEY IDENTITY(1,1),
        UserId INT NOT NULL,
        Content NVARCHAR(MAX) NOT NULL,
        CreatedAt DATETIME DEFAULT GETDATE(),
        FOREIGN KEY (UserId) REFERENCES Users(Id)
    );
END
GO
