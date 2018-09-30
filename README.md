<p align="center">
  <a href="https://www.fanray.com/">
    <img src="https://user-images.githubusercontent.com/633119/45599313-0d112980-b99e-11e8-9997-d2fcff65347f.png" alt="" width=72 height=72>
  </a>
  <h3 align="center">Fanray</h3>
  <p align="center">
    A simple and elegant blog.
  </p>
  <p align="center">
	<a href="#features">Features</a> •
	<a href="#quick-start">Quick Start</a> •
	<a href="#contribute">Contribute</a> •
	<a href="#license">License</a>
  </p>
  <p align="center">
	<a href="https://ci.appveyor.com/project/FanrayMedia/fanray/branch/master">
		<img src="https://ci.appveyor.com/api/projects/status/25ifr0ahvcxn48f5/branch/master?svg=true&passingText=master%20-%20passing&failingText=master%20-%20failing&pendingText=master%20-%20pending" alt="master branch status">
	</a>
	<a href="https://ci.appveyor.com/project/FanrayMedia/fanray/branch/dev">
		<img src="https://ci.appveyor.com/api/projects/status/25ifr0ahvcxn48f5/branch/dev?svg=true&passingText=dev%20-%20passing&failingText=dev%20-%20failing&pendingText=dev%20-%20pending" alt="dev branch status">
	</a>
	<a href="https://ci.appveyor.com/project/FanrayMedia/fanray">
		<img src="https://ci.appveyor.com/api/projects/status/25ifr0ahvcxn48f5?svg=true&passingText=feature%20-%20passing&failingText=feature%20-%20failing&pendingText=feature%20-%20pending" alt="feature branch status">
	</a>
  </p>
</p>

## Features

Please check out the [Wiki](https://github.com/FanrayMedia/Fanray/wiki) for details.

| Blog | Infrastructure | Libs / Frameworks
| --- | --- |  --- | 
| Autosave draft    | Caching                                   | ASP.NET Core
| Categories, Tags  | Error Handling						    | AutoMapper
| Comments (Disqus) | Image Resizing                            | Bootstrap4
| Google Analytics  | Logging (File, Seq, ApplicationInsights)  | CKEditor5
| Media Gallery     | MetaWeblog API                            | Entity Framework Core
| Open Live Writer  | Settings                                  | FluentValidation
| Preferred Domain  | Storage (File System, Azure Blob Storage) | HtmlAgilityPack
| RSS               | Testing (Unit, Integration)               | Magick.NET
| SEO               | Validation								| Moq
| Shortcodes		|											| Serilog
| Site Setup        |                                           | Vue.js
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