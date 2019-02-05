IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20180530163323_FanV2_0')
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

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20180530163323_FanV2_0')
BEGIN
    EXEC sp_rename N'[Core_Media].[AppId]', N'AppType', N'COLUMN';
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20180530163323_FanV2_0')
BEGIN
    ALTER TABLE [Core_Media] ADD [Caption] nvarchar(max) NULL;
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20180530163323_FanV2_0')
BEGIN
    ALTER TABLE [Core_Media] ADD [ContentType] nvarchar(256) NOT NULL DEFAULT N'';
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20180530163323_FanV2_0')
BEGIN
    ALTER TABLE [Core_Media] ADD [Width] int NOT NULL DEFAULT 0;
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20180530163323_FanV2_0')
BEGIN
    ALTER TABLE [Core_Media] ADD [Height] int NOT NULL DEFAULT 0;
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20180530163323_FanV2_0')
BEGIN
    ALTER TABLE [Core_Media] ADD [Alt] nvarchar(max) NULL;
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20180530163323_FanV2_0')
BEGIN
    ALTER TABLE [Core_Media] ADD [ResizeCount] int NOT NULL DEFAULT 0;
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20180530163323_FanV2_0')
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Core_User]') AND [c].[name] = N'DisplayName');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Core_User] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [Core_User] ALTER COLUMN [DisplayName] nvarchar(256) NULL;
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20180530163323_FanV2_0')
BEGIN
    ALTER TABLE [Core_Meta] DROP CONSTRAINT [PK_Core_Meta];
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20180530163323_FanV2_0')
BEGIN
    DROP INDEX [IX_Core_Meta_Key] ON [Core_Meta];
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20180530163323_FanV2_0')
BEGIN
    ALTER TABLE [Core_Meta] ADD [Type] int NOT NULL DEFAULT 0;
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20180530163323_FanV2_0')
BEGIN
    ALTER TABLE [Core_Meta] ADD CONSTRAINT [PK_Core_Meta] PRIMARY KEY ([Id]);
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20180530163323_FanV2_0')
BEGIN
    CREATE INDEX [IX_Core_Meta_Type] ON [Core_Meta] ([Type]);
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20180530163323_FanV2_0')
BEGIN
    UPDATE [Core_Meta] SET [Key] = 'blogsettings.allowcomments' WHERE [Key] = 'blogsettings.allowcommentsonblogpost';
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20180530163323_FanV2_0')
BEGIN
    UPDATE [Core_Meta] SET [Key] = 'blogsettings.feedshowexcerpt' WHERE [Key] = 'blogsettings.rssshowexcerpt';
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20180530163323_FanV2_0')
BEGIN
    UPDATE [Core_Meta] SET [Key] = 'blogsettings.postperpage' WHERE [Key] = 'blogsettings.pagesize';
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20180530163323_FanV2_0')
BEGIN
    UPDATE [Core_Meta] SET [Value] = 1 WHERE [Key] = 'blogsettings.showexcerpt';
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20180530163323_FanV2_0')
BEGIN
    UPDATE [Core_Meta] SET [Key] = 'blogsettings.postlistdisplay' WHERE [Key] = 'blogsettings.showexcerpt';
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20180530163323_FanV2_0')
BEGIN
    DELETE FROM [Core_Meta] WHERE [Key] = 'blogsettings.excerptwordlimit';
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20180530163323_FanV2_0')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20180530163323_FanV2_0', N'2.2.1-servicing-10028');
END;

GO
