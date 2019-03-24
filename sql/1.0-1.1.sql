IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20190318202954_FanV1_1')
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

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20190318202954_FanV1_1')
BEGIN
    EXEC sp_rename N'[Core_Media].[AppId]', N'AppType', N'COLUMN';
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20190318202954_FanV1_1')
BEGIN
    ALTER TABLE [Core_Media] ADD [Caption] nvarchar(max) NULL;
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20190318202954_FanV1_1')
BEGIN
    ALTER TABLE [Core_Media] ADD [ContentType] nvarchar(256) NOT NULL DEFAULT N'';
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20190318202954_FanV1_1')
BEGIN
    ALTER TABLE [Core_Media] ADD [Width] int NOT NULL DEFAULT 0;
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20190318202954_FanV1_1')
BEGIN
    ALTER TABLE [Core_Media] ADD [Height] int NOT NULL DEFAULT 0;
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20190318202954_FanV1_1')
BEGIN
    ALTER TABLE [Core_Media] ADD [Alt] nvarchar(max) NULL;
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20190318202954_FanV1_1')
BEGIN
    ALTER TABLE [Core_Media] ADD [ResizeCount] int NOT NULL DEFAULT 0;
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20190318202954_FanV1_1')
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

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20190318202954_FanV1_1')
BEGIN
    DROP INDEX [IX_Core_Meta_Key] ON [Core_Meta];
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20190318202954_FanV1_1')
BEGIN
    ALTER TABLE [Core_Meta] ADD [Type] int NOT NULL DEFAULT 0;
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20190318202954_FanV1_1')
BEGIN
    CREATE UNIQUE CLUSTERED INDEX [IX_Core_Meta_Type_Key] ON [Core_Meta] ([Type], [Key]);
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20190318202954_FanV1_1')
BEGIN
    UPDATE [Core_Meta] SET [Key] = 'blogsettings.allowcomments' WHERE [Key] = 'blogsettings.allowcommentsonblogpost';
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20190318202954_FanV1_1')
BEGIN
    UPDATE [Core_Meta] SET [Key] = 'blogsettings.feedshowexcerpt' WHERE [Key] = 'blogsettings.rssshowexcerpt';
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20190318202954_FanV1_1')
BEGIN
    UPDATE [Core_Meta] SET [Key] = 'blogsettings.postperpage' WHERE [Key] = 'blogsettings.pagesize';
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20190318202954_FanV1_1')
BEGIN
    UPDATE [Core_Meta] SET [Value] = 1 WHERE [Key] = 'blogsettings.showexcerpt';
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20190318202954_FanV1_1')
BEGIN
    UPDATE [Core_Meta] SET [Key] = 'blogsettings.postlistdisplay' WHERE [Key] = 'blogsettings.showexcerpt';
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20190318202954_FanV1_1')
BEGIN
    DELETE FROM [Core_Meta] WHERE [Key] = 'blogsettings.excerptwordlimit';
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20190318202954_FanV1_1')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20190318202954_FanV1_1', N'2.2.3-servicing-35854');
END;

GO
