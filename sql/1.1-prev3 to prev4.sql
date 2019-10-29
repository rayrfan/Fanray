/* Upgrade from v1.1-preview3 to v1.1-preview4 */

USE [Fanray]
GO

-----------------
-- Schema Update
-----------------

-- add two columns to Blog_Post table
ALTER TABLE [Blog_Post] ADD [PageLayout] tinyint NULL
GO
ALTER TABLE [Blog_Post] ADD [Nav] nvarchar(max) NULL;
GO

---------------
-- Data Update
---------------

-- update migration history version
UPDATE [__EFMigrationsHistory]
   SET [ProductVersion] = '2.2.4-servicing-10062'
   WHERE MigrationId = '20190318202954_FanV1_1'
GO

-- fix Core_Meta Type value
UPDATE [Core_Meta] SET [Type] = 6 WHERE [Type] = 5 -- Plugin from 5 to 6
GO
UPDATE [Core_Meta] SET [Type] = 5 WHERE [Type] = 4 -- Widget from 4 to 5
GO

-- make sure Editor.md plugin exists
IF NOT EXISTS(select * from [Core_Meta] where [Key] = 'editor.md' and [Type] = 6)
BEGIN
    INSERT INTO [Core_Meta] VALUES(N'editor.md', N'{"language":"en","darkTheme":false,"codeMirrorTheme":"default","active":true,"folder":"Editor.md"}', 6);
END
GO

-- make sure Shortcodes plugin is active
UPDATE [Core_Meta] 
   SET [Value] = N'{"active":true,"folder":"Shortcodes"}'
   WHERE [Type] = 6 and [Key] = 'shortcodes'
GO

-- add page widget areas
INSERT [Core_Meta] ([Key], [Value], [Type]) VALUES (N'page-sidebar1', N'{"id":"page-sidebar1","widgetIds":[]}', 2)
GO
INSERT [Core_Meta] ([Key], [Value], [Type]) VALUES (N'page-sidebar2', N'{"id":"page-sidebar2","widgetIds":[]}', 2)
GO
INSERT [Core_Meta] ([Key], [Value], [Type]) VALUES (N'page-before-content', N'{"id":"page-before-content","widgetIds":[]}', 2)
GO
INSERT [Core_Meta] ([Key], [Value], [Type]) VALUES (N'page-after-content', N'{"id":"page-after-content","widgetIds":[]}', 2)
GO
