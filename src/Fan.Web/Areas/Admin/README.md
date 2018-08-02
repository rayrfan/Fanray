# Admin Console

This folder contains client side `/js`, `/scss` and server side `/Pages` for the Admin Console.
For client development, I recommend open this folder in VS Code.
To build client side artifacts, open bash in this folder then perform the following.

## JavaScript

### Prerequisite

`npm install`

### Operation

To develop (watch and won't remove logs)

```bash
npm run build:w
```

To release (logs are removed)

```bash
npm run build
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