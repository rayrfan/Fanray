/* Upgrade from v1.1-preview3 to v1.1-preview4 */

USE [Fanray]
GO

-- Update schema: Blog_Post table
ALTER TABLE [Blog_Post] ADD [PageLayout] tinyint NULL
GO
ALTER TABLE [Blog_Post] ADD [Nav] nvarchar(max) NULL;
GO

-- Fix data: mig history
UPDATE [dbo].[__EFMigrationsHistory]
   SET [ProductVersion] = '2.2.4-servicing-10062'
   WHERE MigrationId = '20190318202954_FanV1_1'
GO

-- Fix data: Core_Meta Type
UPDATE [dbo].[Core_Meta] SET [Type] = 6 WHERE [Type] = 5 -- Plugin from 5 to 6
GO
UPDATE [dbo].[Core_Meta] SET [Type] = 5 WHERE [Type] = 4 -- Widget from 4 to 5
GO

-- Add data: page widget areas
INSERT [dbo].[Core_Meta] ([Key], [Value], [Type]) VALUES (N'page-sidebar1', N'{"id":"page-sidebar1","widgetIds":[]}', 2)
GO
INSERT [dbo].[Core_Meta] ([Key], [Value], [Type]) VALUES (N'page-sidebar2', N'{"id":"page-sidebar2","widgetIds":[]}', 2)
GO
INSERT [dbo].[Core_Meta] ([Key], [Value], [Type]) VALUES (N'page-before-content', N'{"id":"page-before-content","widgetIds":[]}', 2)
GO
INSERT [dbo].[Core_Meta] ([Key], [Value], [Type]) VALUES (N'page-after-content', N'{"id":"page-after-content","widgetIds":[]}', 2)
GO
