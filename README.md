<p align="center">
  <a href="https://www.fanray.com/">
    <img src="https://user-images.githubusercontent.com/633119/45599313-0d112980-b99e-11e8-9997-d2fcff65347f.png" alt="" width=72 height=72>
  </a>
  <h3 align="center">Fanray</h3>
  <p align="center">
    A simple and elegant blog
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
	<a href="https://fanray.visualstudio.com/Fanray/_build?definitionId=2">
	  <img src="https://fanray.visualstudio.com/Fanray/_apis/build/status/Fanray-CI?branchName=dev" alt="Azure Pipelines">
	</a>
	<a href="https://travis-ci.org/FanrayMedia/Fanray">
	  <img src="https://travis-ci.org/FanrayMedia/Fanray.svg?branch=master" alt="Travis CI">
	</a>
  </p>
</p>

## Screenshots

<p align="center">
  <img src="https://raw.githubusercontent.com/wiki/FanrayMedia/fanraymedia.github.io/img/readme/post-composer2.png" title="Post Composer" />
  <img src="https://raw.githubusercontent.com/wiki/FanrayMedia/fanraymedia.github.io/img/readme/clarity-theme2.png" title="Clarity Theme" />
</p>

## Features

Please see [**Docs**](https://fanray.com/docs) for more details.

| Blog | | Infrastructure |
| --- | --- |  --- | 
| Autosave Draft    | Preview           	| Caching
| Categories		| Rich Text / Markdown  | Error Handling
| Comments (Disqus) | RSS				    | Events
| Google Analytics  | SEO	                | Extensibility (Plugin, Widget, Theme)	 
| Media Gallery     | Shortcodes		    | Image Resizing                            
| Navigation		| Site Installation	    | Logging (File, Seq, Application Insights) 
| Open Live Writer  | Tags				    | Responsive Images
| Pages				| Theme 			    | Settings                                  
| Plugins			| Users				    | Storage (File System, Azure Blob Storage) 
| Posts				| Widgets			    | Testing (Unit, Integration)
| Preferred Domain  | 
 
## Quick Start

Fanray v1.1 runs on [.NET Core 3.1](https://www.microsoft.com/net/download) and [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads). Any of the free SQL Server editions, LocalDB, Express, Developer will be sufficient.

Clone the repo then run from either [VS2019](https://www.visualstudio.com/vs/community/) or command line.

- VS2019: open `Fanray.sln`, make sure `Fan.WebApp` is the startup project, Ctrl + F5
- Command line: do the following, then go to https://localhost:5001

```bash
cd <sln folder>
dotnet restore
cd src/Core/Fan.WebApp
dotnet run
```

Database is created for you on app initial launch. Below is the default connection string, to adjust it go to `appsettings.json`

```javascript
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=Fanray;Trusted_Connection=True;MultipleActiveResultSets=true"
},
```

Note the 404 page is only displayed in Production when an invalid URL is accessed, in Development the developer exception page is shown.

## Contribute

Please refer to [Contributing Guide](CONTRIBUTING.md).

## Support Me

If you find this project useful please consider support it, your contribution will help a lot! Thank you!

<a href="https://www.buymeacoffee.com/Fanray" target="_blank"><img src="https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png" alt="Buy Me A Coffee"></a>

<a href="https://paypal.me/FanrayMedia" target="_blank"><img src="https://user-images.githubusercontent.com/633119/67154590-d1891300-f2b3-11e9-83d2-c7e6232a09df.jpg" alt="PayPal Me" width="135" height="35"></a>

## License

[Apache 2.0](LICENSE)
