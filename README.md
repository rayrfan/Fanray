<p align="center">
  <a href="https://www.fanray.com/">
    <img src="https://user-images.githubusercontent.com/633119/45599313-0d112980-b99e-11e8-9997-d2fcff65347f.png" alt="" width=72 height=72>
  </a>
  <h3 align="center">Fanray</h3>
  <p align="center">
    A simple and elegant blog.
  </p>
  <p align="center">
	<a href="#screenshots">Screenshots</a> •
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
	<a href="https://fanray.visualstudio.com/Fanray/_build/latest?definitionId=2">
	  <img src="https://fanray.visualstudio.com/Fanray/_apis/build/status/Fanray-CI" alt="feature branch status">
	</a>
  </p>
  <p align="center">
    <a href="https://gitter.im/Fanray-project/Fanray?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge">
	  <img src="https://badges.gitter.im/Fanray-project/Fanray.svg" alt="feature branch status">
    </a>
    <a href="https://github.com/FanrayMedia/Fanray/contributors/">
      <img src="https://img.shields.io/github/contributors/FanrayMedia/Fanray.svg" alt="Contributors" />
    </a>
  </p>
</p>

## Screenshots

<p align="center">
  <img src="https://user-images.githubusercontent.com/633119/54874702-c87fa400-4dad-11e9-86e5-54de38b3319e.png" title="Composer" />
  <img src="https://user-images.githubusercontent.com/633119/54874701-c87fa400-4dad-11e9-8147-1f54ccd0dab4.png" title="Clarity theme" />
</p>

## Features

Please check out the [Wiki](https://github.com/FanrayMedia/Fanray/wiki) for details.

| Blog | Infrastructure | Libs / Frameworks
| --- | --- |  --- | 
| Autosave draft    | Caching                                   | ASP.NET Core
| Categories, Tags  | Error Handling						    | Bootstrap4
| Comments (Disqus) | Events									| CKEditor5
| Google Analytics  | Image Resizing                            | Entity Framework Core
| Media Gallery     | Logging (File, Seq, ApplicationInsights)  | HtmlAgilityPack
| Open Live Writer  | MetaWeblog API                            | Magick.NET
| Preferred Domain  | Settings                                  | Mediatr
| RSS               | Storage (File System, Azure Blob Storage) | Moq
| SEO-Friendly URLs | Testing (Unit, Integration)               | Serilog
| Shortcodes		| Validation								| Vue.js 
| Site Setup        |                                           | Vuetify.js
| Widgets           |											| xUnit
 
## Quick Start

Fanray v1.1 runs on [.NET Core 2.2](https://www.microsoft.com/net/download) and [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads). Any of the free SQL Server editions, LocalDB, Express, Developer will be sufficient.

Clone the repo then run from either [VS2017](https://www.visualstudio.com/vs/community/) or command line.

- VS2017: open `Fanray.sln`, make sure `Fan.Web` is the startup project, ctrl + F5
- Command line: do the following, then go to https://localhost:5001
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

## Support Me

<a href="https://www.buymeacoffee.com/Fanray" target="_blank"><img src="https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png" alt="Buy Me A Coffee" style="height: auto !important;width: auto !important;" ></a>

## License

[Apache 2.0](LICENSE)