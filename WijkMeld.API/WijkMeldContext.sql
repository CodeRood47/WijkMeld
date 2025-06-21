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
    WHERE [MigrationId] = N'20250518124933_InitialCreate'
)
BEGIN
    CREATE TABLE [Locations] (
        [Id] uniqueidentifier NOT NULL,
        [Lat] float NOT NULL,
        [Long] float NOT NULL,
        [Address] nvarchar(max) NULL,
        CONSTRAINT [PK_Locations] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250518124933_InitialCreate'
)
BEGIN
    CREATE TABLE [Users] (
        [Id] uniqueidentifier NOT NULL,
        [UserName] nvarchar(max) NOT NULL,
        [PasswordHash] nvarchar(max) NOT NULL,
        [Email] nvarchar(max) NOT NULL,
        [Role] int NOT NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250518124933_InitialCreate'
)
BEGIN
    CREATE TABLE [Incidents] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [UserId] uniqueidentifier NULL,
        [LocationId] uniqueidentifier NOT NULL,
        [Priority] int NOT NULL,
        [Status] int NOT NULL,
        [Created] datetime2 NOT NULL,
        CONSTRAINT [PK_Incidents] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Incidents_Locations_LocationId] FOREIGN KEY ([LocationId]) REFERENCES [Locations] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Incidents_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250518124933_InitialCreate'
)
BEGIN
    CREATE TABLE [Notifications] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [IncidentId] uniqueidentifier NOT NULL,
        [Message] nvarchar(max) NOT NULL,
        [SendDate] datetime2 NOT NULL,
        [Read] bit NOT NULL,
        CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Notifications_Incidents_IncidentId] FOREIGN KEY ([IncidentId]) REFERENCES [Incidents] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Notifications_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250518124933_InitialCreate'
)
BEGIN
    CREATE TABLE [StatusUpdates] (
        [Id] uniqueidentifier NOT NULL,
        [IncidentId] uniqueidentifier NOT NULL,
        [NewStatus] int NOT NULL,
        [ChangedById] uniqueidentifier NOT NULL,
        [Date] datetime2 NOT NULL,
        [Note] nvarchar(max) NOT NULL,
        [NewPrio] int NULL,
        CONSTRAINT [PK_StatusUpdates] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_StatusUpdates_Incidents_IncidentId] FOREIGN KEY ([IncidentId]) REFERENCES [Incidents] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_StatusUpdates_Users_ChangedById] FOREIGN KEY ([ChangedById]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250518124933_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Incidents_LocationId] ON [Incidents] ([LocationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250518124933_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Incidents_UserId] ON [Incidents] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250518124933_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Notifications_IncidentId] ON [Notifications] ([IncidentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250518124933_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Notifications_UserId] ON [Notifications] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250518124933_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_StatusUpdates_ChangedById] ON [StatusUpdates] ([ChangedById]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250518124933_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_StatusUpdates_IncidentId] ON [StatusUpdates] ([IncidentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250518124933_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250518124933_InitialCreate', N'9.0.5');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250522093329_MakeLocationOwned'
)
BEGIN
    ALTER TABLE [Incidents] DROP CONSTRAINT [FK_Incidents_Locations_LocationId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250522093329_MakeLocationOwned'
)
BEGIN
    DROP TABLE [Locations];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250522093329_MakeLocationOwned'
)
BEGIN
    DROP INDEX [IX_Incidents_LocationId] ON [Incidents];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250522093329_MakeLocationOwned'
)
BEGIN
    DECLARE @var sysname;
    SELECT @var = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Incidents]') AND [c].[name] = N'LocationId');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [Incidents] DROP CONSTRAINT [' + @var + '];');
    ALTER TABLE [Incidents] DROP COLUMN [LocationId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250522093329_MakeLocationOwned'
)
BEGIN
    ALTER TABLE [Incidents] ADD [Location_Address] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250522093329_MakeLocationOwned'
)
BEGIN
    ALTER TABLE [Incidents] ADD [Location_Lat] float NOT NULL DEFAULT 0.0E0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250522093329_MakeLocationOwned'
)
BEGIN
    ALTER TABLE [Incidents] ADD [Location_Long] float NOT NULL DEFAULT 0.0E0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250522093329_MakeLocationOwned'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250522093329_MakeLocationOwned', N'9.0.5');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250522114853_Changes'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StatusUpdates]') AND [c].[name] = N'Note');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [StatusUpdates] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [StatusUpdates] ALTER COLUMN [Note] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250522114853_Changes'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[StatusUpdates]') AND [c].[name] = N'NewStatus');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [StatusUpdates] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [StatusUpdates] ALTER COLUMN [NewStatus] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250522114853_Changes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250522114853_Changes', N'9.0.5');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250522143454_kleinAanpassingIncidents'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250522143454_kleinAanpassingIncidents', N'9.0.5');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250522180030_IncidentsZouNuAanUserVastMoetenZitten'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250522180030_IncidentsZouNuAanUserVastMoetenZitten', N'9.0.5');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250526184514_ToevoegenPhotos'
)
BEGIN
    CREATE TABLE [IncidentPhotos] (
        [Id] uniqueidentifier NOT NULL,
        [FilePath] nvarchar(max) NOT NULL,
        [UploadedAt] datetime2 NOT NULL,
        [IncidentId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_IncidentPhotos] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_IncidentPhotos_Incidents_IncidentId] FOREIGN KEY ([IncidentId]) REFERENCES [Incidents] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250526184514_ToevoegenPhotos'
)
BEGIN
    CREATE INDEX [IX_IncidentPhotos_IncidentId] ON [IncidentPhotos] ([IncidentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250526184514_ToevoegenPhotos'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250526184514_ToevoegenPhotos', N'9.0.5');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250526185648_aanpassingphoto'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250526185648_aanpassingphoto', N'9.0.5');
END;

COMMIT;
GO