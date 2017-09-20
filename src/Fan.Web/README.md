# Fan.Web

/client resources are managed by package.json only, these resources are either copied or transpiled into /wwwroot.

## Node Modules

### DevDependencies

- [cpx](https://github.com/mysticatea/cpx) (copies resources from client to wwwroot)
- [minifier](https://github.com/fizker/minifier) (minimize site.css after scss transpiles)
- [node-sass](https://github.com/sass/node-sass) (scss transpiler)
- [npm-run-all](https://github.com/mysticatea/npm-run-all) (runs all tasks like copy)

### Scripts
 - Copy (copy resources from client to wwwroot)
 - Scss (transpile and min site.css to wwwroot)

 You can consider to bind Copy and Scss to After Build binding, so they will execute after you build.  Or you can just manually execute them.

## Visual Studio Extensions

- [NPM Task Runner](https://marketplace.visualstudio.com/items?itemName=MadsKristensen.NPMTaskRunner) (helps with executing package.json scripts in VS)
- [Web Essentials 2017](https://marketplace.visualstudio.com/items?itemName=MadsKristensen.WebExtensionPack2017) (gives emmet, better file icon, markdown preview, css tools etc)