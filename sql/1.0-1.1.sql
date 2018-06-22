IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20180530163323_FanV1_1')
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Blog_Post]') AND [c].[name] = N'Slug');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Blog_Post] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [Blog_Post] ALTER COLUMN [Slug] nvarchar(256) NULL;
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20180530163323_FanV1_1')
BEGIN
    EXEC sp_rename N'[Core_Media].[AppId]', N'AppType', N'COLUMN';
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20180530163323_FanV1_1')
BEGIN
    ALTER TABLE [Core_Media] ADD [Caption] nvarchar(max) NULL;
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20180530163323_FanV1_1')
BEGIN
    ALTER TABLE [Core_Media] ADD [FileType] nvarchar(256) NOT NULL DEFAULT N'';
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20180530163323_FanV1_1')
BEGIN
    ALTER TABLE [Core_Media] ADD [Width] int NOT NULL DEFAULT 0;
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20180530163323_FanV1_1')
BEGIN
    ALTER TABLE [Core_Media] ADD [Height] int NOT NULL DEFAULT 0;
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20180530163323_FanV1_1')
BEGIN
    ALTER TABLE [Core_Media] ADD [Optimized] bit NOT NULL DEFAULT 0;
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20180530163323_FanV1_1')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20180530163323_FanV1_1', N'2.1.1-rtm-30846');
END;

GO

