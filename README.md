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
	<a href="https://ci.appveyor.com/project/FanrayMedia/fanray">
	  <img src="https://ci.appveyor.com/api/projects/status/github/fanraymedia/fanray?svg=true" alt="AppVeyor">
	</a>
	<a href="https://fanray.visualstudio.com/Fanray/_build/latest?definitionId=2">
	  <img src="https://fanray.visualstudio.com/Fanray/_apis/build/status/Fanray-CI" alt="Azure Pipelines">
	</a>
	<a href="https://github.com/996icu/996.ICU/blob/master/LICENSE">
	  <img src="https://img.shields.io/badge/license-Anti%20996-blue.svg" alt="Anti 996">
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

| Blog | | Infrastructure |
| --- | --- |  --- | 
| Autosave Draft    | Shortcodes		| Caching                                   
| Categories		| Site Installation	| Error Handling						    
| Comments (Disqus) | Tags				| Events									
| Google Analytics  | Users				| Image Resizing                            
| Media Gallery     | Widgets			| Logging (File, Seq, ApplicationInsights)  
| Open Live Writer  |					| MetaWeblog API                            
| Preferred Domain  |					| Settings                                  
| Responsive Images |					| Storage (File System, Azure Blob Storage) 
| RSS				|					| Testing (Unit, Integration)               
| SEO-Friendly URLs |					| Validation								

 
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