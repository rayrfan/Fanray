# Admin client

This folder contains javascript and scss for the Admin Console, open this folder in VS Code is recommended.

## JavaScript

### Prerequisite

`npm install`

### Operation

To do one-time compile of js files

```bash
npm run build
```

To develop with watch

```bash
npm run build:w
```

## Scss

### Prerequisite

1. download [RubyInstaller](https://rubyinstaller.org/)
2. run `gem install sass`

### Operation

To do one-time compile of all scss files

```bash
sass --update scss:../wwwroot/admin/css --style compressed
```

To develop with watch

```bash
sass --watch scss:../wwwroot/admin/css --style compressed
```