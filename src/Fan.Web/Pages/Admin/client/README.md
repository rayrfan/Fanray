# Admin Console

This folder contains the client for Fanray Admin Console. All npx commands below assume you open a bash in this folder `Fan.Web/Pages/Admin/client`.

## JavaScript

First to get ready by `npm install`

### Develop

Individual file
```bash
npx babel js/admin.js --out-dir ../../../wwwroot/admin/js --source-maps --watch
npx babel js/blog-categories.js --out-dir ../../../wwwroot/admin/js --source-maps --watch
npx babel js/blog-compose.js --out-dir ../../../wwwroot/admin/js --source-maps --watch
npx babel js/blog-media.js --out-dir ../../../wwwroot/admin/js --source-maps --watch
npx babel js/blog-settings.js --out-dir ../../../wwwroot/admin/js --source-maps --watch
npx babel js/blog-tags.js --out-dir ../../../wwwroot/admin/js --source-maps --watch
npx babel js/setup.js --out-dir ../../../wwwroot/admin/js --source-maps --watch
npx babel js/site-users.js --out-dir ../../../wwwroot/admin/js --source-maps --watch
```

Entire folder
```bash
npx babel js --out-dir ../../../wwwroot/admin/js --source-maps --watch
```

### Release

Individual file
```bash
npx babel js/admin.js --out-dir ../../../wwwroot/admin/js --source-maps --plugins transform-remove-console
npx babel js/blog-categories.js --out-dir ../../../wwwroot/admin/js --source-maps --plugins transform-remove-console
npx babel js/blog-compose.js --out-dir ../../../wwwroot/admin/js --source-maps --plugins transform-remove-console
npx babel js/blog-media.js --out-dir ../../../wwwroot/admin/js --source-maps --plugins transform-remove-console
npx babel js/blog-settings.js --out-dir ../../../wwwroot/admin/js --source-maps --plugins transform-remove-console
npx babel js/blog-tags.js --out-dir ../../../wwwroot/admin/js --source-maps --plugins transform-remove-console
npx babel js/setup.js --out-dir ../../../wwwroot/admin/js --source-maps --plugins transform-remove-console
npx babel js/site-users.js --out-dir ../../../wwwroot/admin/js --source-maps --plugins transform-remove-console
```

Entire folder
```bash
babel js --out-dir ../../../wwwroot/admin/js --source-maps --plugins transform-remove-console
```

## Scss

### Prerequisite

1. download [RubyInstaller](https://rubyinstaller.org/)
2. run `gem install sass`

### Develop

```bash
sass --watch scss/admin.scss:../../../wwwroot/admin/css/admin.css
sass --watch scss/compose.scss:../../../wwwroot/admin/css/compose.css
```

### Release

```bash
sass scss/admin.scss ../../../wwwroot/admin/css/admin.css --style compressed
sass scss/compose.scss ../../../wwwroot/admin/css/compose.css --style compressed
```