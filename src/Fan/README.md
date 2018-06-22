# Fan

This class library provides the infrastructure to all the other libraries, such caching, data access, media service etc.

## Migrations

The Migrations folder contains EF migrations.  

### Working with migrations

If you make changes to any model class that derives from Entity, for example say you added a property to Post class, do the following

1. Add migration

Open Package Manager Console, select "src\Fan" as the Default project, run

`Add-Migration UpdatePost`

This will create a migration named UdpatePost prefixed with timestamp.

2. Make sure code compiles

EF adds some uncessary using statements to your migration and the snapshot class, remove these statements.

3. Hit ctrl + f5 to run the application

The new migration will be applied automatically to your existing database. 
If you are starting fresh with a new database, all migrations will be applied as well.

### Tips on migrations

- `Remove-Migration` will remove the very last migration you created, however if you already applied migration to db, it won't work.

- If you applied a migration after which you made more changes to the data model, you can redo it by
  - delete the migration files
  - roll back the snapshot file with git
  - point to a new db and run the app

- `Script-Migration` will generate a SQL script from migrations.
e.g. `Script-Migration -Idempotent -From FanSchemaV1` will generate a script since the `FanSchemaV1` which covers only `FanV1_1`

- `Update-Database` should apply your migration without running the app, but it could error out with a message like this one,

"The index 'IX_Blog_Post_Slug' is dependent on column 'Slug'.
ALTER TABLE ALTER COLUMN Slug failed because one or more objects access this column."

- If you rename a property as well as add new property, make sure the generated migration gets them correctly as it does not always do.