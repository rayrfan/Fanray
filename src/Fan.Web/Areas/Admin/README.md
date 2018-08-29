# Admin Console

This folder contains client side `/js`, `/scss` and server side `/Pages` for the Admin Console.
For client development, I recommend open this folder in VS Code.
To build client side artifacts, open bash in this folder then perform the following.

## JavaScript

First to get ready by `npm install`

### Develop

Develop an inidividual file, replace the filename.

```bash
babel js/setup.js --out-dir ../../wwwroot/admin --source-maps --watch
```

Develop entire folder.

```bash
babel js --out-dir ../../wwwroot/admin --source-maps --watch
```

### Release

Build individual file, replace the filename.

```bash
babel js/setup.js --out-dir ../../wwwroot/admin --source-maps --plugins transform-remove-console
```

Build entire folder.

```bash
babel js --out-dir ../../wwwroot/admin --source-maps --plugins transform-remove-console
```

## Scss

### Prerequisite

1. download [RubyInstaller](https://rubyinstaller.org/)
2. run `gem install sass`

### Operation

To develop

```bash
sass --watch scss:../../wwwroot/admin/css
```

To release

```bash
sass --update scss:../../wwwroot/admin/css --style compressed
```