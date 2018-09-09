# Admin Console

This folder contains client side `/js`, `/scss` and server side `/Pages` for the Admin Console.
For client development, I recommend open this folder in VS Code.
To build client side artifacts, open bash in this folder then perform the following.

## JavaScript

First to get ready by `npm install`

### Develop

Develop an inidividual file, replace the filename.

```bash
npx babel js/admin.js --out-dir ../../wwwroot/admin --source-maps --watch
npx babel js/blog-categories.js --out-dir ../../wwwroot/admin --source-maps --watch
npx babel js/blog-compose.js --out-dir ../../wwwroot/admin/js --source-maps --watch
npx babel js/blog-media.js --out-dir ../../wwwroot/admin/js --source-maps --watch
npx babel js/blog-tags.js --out-dir ../../wwwroot/admin --source-maps --watch
npx babel js/setup.js --out-dir ../../wwwroot/admin/js --source-maps --watch
```

Develop entire folder.

```bash
npx babel js --out-dir ../../wwwroot/admin/js --source-maps --watch
```

### Release

Build individual file, replace the filename.

```bash
npx babel js/admin.js --out-dir ../../wwwroot/admin --source-maps --plugins transform-remove-console
npx babel js/blog-categories.js --out-dir ../../wwwroot/admin --source-maps --plugins transform-remove-console
npx babel js/blog-compose.js --out-dir ../../wwwroot/admin/js --source-maps --plugins transform-remove-console
npx babel js/blog-media.js --out-dir ../../wwwroot/admin/js --source-maps --plugins transform-remove-console
npx babel js/blog-tags.js --out-dir ../../wwwroot/admin --source-maps --plugins transform-remove-console
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
sass --watch scss/admin.scss:../../wwwroot/admin/css/admin.css
sass --watch scss/compose.scss:../../wwwroot/admin/css/compose.css
```

To release

```bash
sass scss/admin.scss ../../wwwroot/admin/css/admin.css --style compressed
sass scss/compose.scss ../../wwwroot/admin/css/compose.css --style compressed
```