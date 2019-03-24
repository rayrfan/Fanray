# Working with migrations

Migrations can be done inside VS by opening up Package Manager Console.

## Add-Migration

If you make changes to any Entity derived model classes or ModelBuilders, say you added a property to Post class.

`Add-Migration UpdatePost` will create a migration named UdpatePost prefixed with today's timestamp.

## Update-Database

Make sure `appsettings.json` is pointing to the right database.

`Update-Database` will create database and apply all migrations.
`Update-Database -Migration FanV1` will create db and apply only up to migration FanV1.

## Remove-Migration

`Remove-Migration` will remove the very last migration you created, however if you already applied migration to db, it won't work.

## Script-Migration

`Script-Migration` will generate a SQL script from migrations.
`Script-Migration -Idempotent -From FanSchemaV1` will generate a script since the `FanSchemaV1` which covers only `FanV2_0`


## Note

When migrations are applied to a db by running the app or doing `Update-Database`, records will be inserted to __EFMigrationsHistory table, the MigrationId column will get the migration designer's migration value and the ProductVersion will get the installed .NET SDK version.