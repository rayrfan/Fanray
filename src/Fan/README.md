# Fan

This class library provides the infrastructure to all the other libraries, such caching, data access etc.

## Migrations

The Migrations folder contains EF migrations.  If you make changes to any model class that derives from Entity, say you added a property to Post class, then follow these steps

1. Open Package Manager Console, select "src\Fan" as the Default project, then run for example

`Add-Migration UpdatePost`

This will create folder "Fan/Migrations" if not already there with a migration named UdpatePost prefixed with timestamp.

2. Then you can just run the app ctrl + f5, the site is up running with db automatically created.

Run `Update-Database` should work too without running the app, but it gave me a error message 

"The index 'IX_Blog_Post_Slug' is dependent on column 'Slug'.
ALTER TABLE ALTER COLUMN Slug failed because one or more objects access this column."

## Azure Blob Storage

Nuget: WindowsAzure.Storage