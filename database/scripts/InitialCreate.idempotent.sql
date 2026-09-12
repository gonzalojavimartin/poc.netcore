IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260912213217_InitialCreate'
)
BEGIN
    CREATE TABLE [Patients] (
        [Id] uniqueidentifier NOT NULL,
        [FirstName] nvarchar(100) NOT NULL,
        [LastName] nvarchar(100) NOT NULL,
        [DocumentType] nvarchar(30) NULL,
        [DocumentNumber] nvarchar(30) NULL,
        [BirthDate] date NOT NULL,
        [Email] nvarchar(150) NULL,
        [Phone] nvarchar(30) NULL,
        [Status] nvarchar(8) NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [UpdatedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_Patients] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Patients_DocumentPair] CHECK (([DocumentType] IS NULL AND [DocumentNumber] IS NULL) OR ([DocumentType] IS NOT NULL AND [DocumentNumber] IS NOT NULL AND LEN(LTRIM(RTRIM([DocumentType]))) > 0 AND LEN(LTRIM(RTRIM([DocumentNumber]))) > 0)),
        CONSTRAINT [CK_Patients_FirstName] CHECK (LEN(LTRIM(RTRIM([FirstName]))) > 0),
        CONSTRAINT [CK_Patients_LastName] CHECK (LEN(LTRIM(RTRIM([LastName]))) > 0),
        CONSTRAINT [CK_Patients_Status] CHECK ([Status] IN (N'Active', N'Inactive'))
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260912213217_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_Patients_Document] ON [Patients] ([DocumentType], [DocumentNumber]) WHERE [DocumentType] IS NOT NULL AND [DocumentNumber] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260912213217_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260912213217_InitialCreate', N'10.0.12');
END;

COMMIT;
GO

