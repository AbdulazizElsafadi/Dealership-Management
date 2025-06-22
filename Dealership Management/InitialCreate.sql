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
CREATE TABLE [Users] (
    [Id] int NOT NULL IDENTITY,
    [FullName] nvarchar(100) NOT NULL,
    [Email] nvarchar(100) NOT NULL,
    [PasswordHash] nvarchar(max) NOT NULL,
    [Role] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);

CREATE TABLE [Vehicles] (
    [Id] int NOT NULL IDENTITY,
    [Make] nvarchar(50) NOT NULL,
    [Model] nvarchar(50) NOT NULL,
    [Year] int NOT NULL,
    [Price] decimal(18,2) NOT NULL,
    [Color] nvarchar(20) NULL,
    [Mileage] int NULL,
    [Description] nvarchar(500) NULL,
    [IsAvailable] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Vehicles] PRIMARY KEY ([Id])
);

CREATE TABLE [OtpCodes] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [Code] nvarchar(10) NOT NULL,
    [Purpose] nvarchar(50) NOT NULL,
    [ExpiresAt] datetime2 NOT NULL,
    [IsUsed] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_OtpCodes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_OtpCodes_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Purchases] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [VehicleId] int NOT NULL,
    [PurchaseDate] datetime2 NOT NULL,
    [PriceAtPurchase] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_Purchases] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Purchases_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Purchases_Vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicles] ([Id]) ON DELETE NO ACTION
);

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'Email', N'FullName', N'PasswordHash', N'Role') AND [object_id] = OBJECT_ID(N'[Users]'))
    SET IDENTITY_INSERT [Users] ON;
INSERT INTO [Users] ([Id], [CreatedAt], [Email], [FullName], [PasswordHash], [Role])
VALUES (1, '2025-06-22T11:38:48.2640030Z', N'admin@dealership.com', N'Admin User', N'hashed_password', 1),
(2, '2025-06-22T11:38:48.2640380Z', N'customer@dealership.com', N'Customer User', N'hashed_password', 0);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'Email', N'FullName', N'PasswordHash', N'Role') AND [object_id] = OBJECT_ID(N'[Users]'))
    SET IDENTITY_INSERT [Users] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Color', N'CreatedAt', N'Description', N'IsAvailable', N'Make', N'Mileage', N'Model', N'Price', N'Year') AND [object_id] = OBJECT_ID(N'[Vehicles]'))
    SET IDENTITY_INSERT [Vehicles] ON;
INSERT INTO [Vehicles] ([Id], [Color], [CreatedAt], [Description], [IsAvailable], [Make], [Mileage], [Model], [Price], [Year])
VALUES (1, N'Silver', '2025-06-22T11:38:48.2651040Z', N'Well-maintained sedan with low mileage', CAST(1 AS bit), N'Toyota', 15000, N'Camry', 25000.0, 2022),
(2, N'Blue', '2025-06-22T11:38:48.2651370Z', N'Reliable SUV with great fuel economy', CAST(1 AS bit), N'Honda', 22000, N'CR-V', 28000.0, 2021);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Color', N'CreatedAt', N'Description', N'IsAvailable', N'Make', N'Mileage', N'Model', N'Price', N'Year') AND [object_id] = OBJECT_ID(N'[Vehicles]'))
    SET IDENTITY_INSERT [Vehicles] OFF;

CREATE INDEX [IX_OtpCodes_UserId] ON [OtpCodes] ([UserId]);

CREATE INDEX [IX_Purchases_UserId] ON [Purchases] ([UserId]);

CREATE INDEX [IX_Purchases_VehicleId] ON [Purchases] ([VehicleId]);

CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250622113849_InitialCreate', N'9.0.2');

COMMIT;
GO

