# Fanray

**A blog application built with [ASP.NET Core](https://github.com/aspnet/Home) and supports [Open Live Writer](http://openlivewriter.org/).**

## Get Started

Just clone and run, either from Visual Studio 2017 or command line.

- From VS2017, open Fanray.sln, ctrl + F5
- From command line, do the following, then go to http://localhost:5000
 ```
cd <sln folder>
dotnet restore
cd src/Fan.Web
dotnet run
```

Either way you will see the blog setup page on app initial launch.

## Database

Database is created for you on app initial launch. Out of box it uses SQLite, and SQL Server is also supported.
To switch to SQL Server, go to `appsettings.json` you will see the following.

```
  "AppSettings": {
    "Version": "1.0.0",
    "Database": "sqlite",
    "PreferredDomain": "auto",
    "UseHttps": false
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=Fanray;Trusted_Connection=True;MultipleActiveResultSets=true"
  },

```

Change `sqlite` to `sqlserver` and update your connection string accordingly.

## Open Live Writer

Right now the only way to post is through [Open Live Writer](http://openlivewriter.org/). To get started,

- Install OLW
- Launch the web app 
- Open OLW > Add blog account... > Other services > type in
  - Web address of your blog
  - User name
  - Password