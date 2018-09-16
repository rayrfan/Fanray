<p align="center">
  <a href="https://www.fanray.com/">
    <img src="https://user-images.githubusercontent.com/633119/33040809-0fec23d8-cdf1-11e7-8543-5b666e78f5b4.png" alt="" width=72 height=72>
  </a>
  <h3 align="center">Fanray</h3>
  <p align="center">
    A simple and elegant blog.
  </p>
  <p align="center">
	<a href="#build-status">Build Status</a> •
	<a href="#features">Features</a> •
	<a href="#quick-start">Quick Start</a> •
	<a href="#contribute">Contribute</a> •
	<a href="#license">License</a>
  </p>
</p>

## Build Status

| Branch | Status |
| ------ | ------ |
| Stable (master) | [![Build status](https://ci.appveyor.com/api/projects/status/25ifr0ahvcxn48f5/branch/master?svg=true)](https://ci.appveyor.com/project/FanrayMedia/fanray/branch/master) |
| Weekly (dev) | [![Build status](https://ci.appveyor.com/api/projects/status/25ifr0ahvcxn48f5/branch/dev?svg=true)](https://ci.appveyor.com/project/FanrayMedia/fanray/branch/dev) |
| Nightly (feature) | [![Build status](https://ci.appveyor.com/api/projects/status/25ifr0ahvcxn48f5?svg=true)](https://ci.appveyor.com/project/FanrayMedia/fanray) |

## Features

Please check out the [Wiki](https://github.com/FanrayMedia/Fanray/wiki) for details.

| Blog | Infrastructure | Libs / Frameworks
| --- | --- |  --- | 
| Autosave draft    | Caching                                   | ASP.NET Core
| Categories        | Error Handling						    | AutoMapper
| Comments (Disqus) | Image Resizing                            | Bootstrap4
| Google Analytics  | Logging (File, Seq, ApplicationInsights)  | CKEditor5
| Media Gallery     | MetaWeblog API                            | Entity Framework Core
| Open Live Writer  | Preferred Domain                          | FluentValidation
| Site Setup        | Settings                                  | HtmlAgilityPack
| RSS               | Storage (File System, Azure Blob Storage) | Magick.NET
| SEO               | Testing (Unit, Integration)               | Moq
| Shortcodes		| Validation								| Serilog
| Tags              |                                           | Vue.js
| Users             |											| Vuetify.js


## Quick Start

Fanray v1.1 runs on [.NET Core 2.1](https://www.microsoft.com/net/download) and [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads). Any of the free SQL Server editions, LocalDB, Express, Developer will be sufficient.

Clone the repo then run from either [VS2017](https://www.visualstudio.com/vs/community/) or command line.

- VS2017: open Fanray.sln, make sure Fan.Web is the startup project, ctrl + F5
- Command line: do the following, then go to http://localhost:5001
 ```
cd <sln folder>
dotnet restore
cd src/Fan.Web
dotnet run
```

Database is created for you on app initial launch. Below is the default connection string, to adjust it go to `appsettings.json`

```
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=Fanray;Trusted_Connection=True;MultipleActiveResultSets=true"
},
```

The blog setup page will show up on initial launch, simply fill the form out and create your blog.

## Contribute

Please refer to [Contributing Guide](CONTRIBUTING.md).

## License

[Apache 2.0](LICENSE)