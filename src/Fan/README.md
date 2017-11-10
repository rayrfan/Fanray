# Fan

This class library provides the infrastructure to all the other libraries, such caching, data access etc.

## Migrations

The Migrations folder contains EF migrations, I try to keep one migration for each release.  Follow these steps to get started.

1. Open Package Manager Console, select "src\Fan" as the Default project, then run

  `Add-Migration NameOfTheMigration`

This will add a new folder "Fan/Migrations" with a migration named NameOfTheMigration

2. Either run the app ctrl + f5, the site is up running with db automatically created, or run

`Update-Database`

3. If any of the Entity derived models are updated, for example say by adding a property to Post class, then repeat the process

`Add-Migration NewMigrationName`

This will add a new migration, ctrl + f5, site is up running with Post table having the new column added automatically in db.

## Azure Blob Storage

Nuget: WindowsAzure.Storage