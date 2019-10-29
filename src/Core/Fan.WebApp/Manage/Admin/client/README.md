# Fanray Admin Panel Client

```bash
cd src/Core/Fan.WebApp/Manage/Admin/client
npm install
```

## JavaScript

### Develop

Develop individual file with watch

```bash
babel js/admin.js --out-dir ../../../wwwroot/admin/js --source-maps --watch
babel js/plugins.js --out-dir ../../../wwwroot/admin/js --source-maps --watch
babel js/widgets.js --out-dir ../../../wwwroot/admin/js --source-maps --watch
babel js/blog-categories.js --out-dir ../../../wwwroot/admin/js --source-maps --watch
babel js/blog-media.js --out-dir ../../../wwwroot/admin/js --source-maps --watch
babel js/blog-settings.js --out-dir ../../../wwwroot/admin/js --source-maps --watch
babel js/blog-tags.js --out-dir ../../../wwwroot/admin/js --source-maps --watch
babel js/setup.js --out-dir ../../../wwwroot/admin/js --source-maps --watch
babel js/site-users.js --out-dir ../../../wwwroot/admin/js --source-maps --watch
babel js/compose-page.js --out-dir ../../../wwwroot/admin/js --source-maps --watch
babel js/compose-pagenav.js --out-dir ../../../wwwroot/admin/js --source-maps --watch
babel js/compose-post.js --out-dir ../../../wwwroot/admin/js --source-maps --watch
babel js/blog-posts.js --out-dir ../../../wwwroot/admin/js --source-maps --watch
babel js/blog-pages.js --out-dir ../../../wwwroot/admin/js --source-maps --watch
babel js/navigation.js --out-dir ../../../wwwroot/admin/js --source-maps --watch
```

Develop all files with watch

```bash
babel js --out-dir ../../../wwwroot/admin/js --source-maps --watch
```

### Release

Release individual file

```bash
babel js/admin.js --out-dir ../../../wwwroot/admin/js --source-maps --plugins transform-remove-console
babel js/plugins.js --out-dir ../../../wwwroot/admin/js --source-maps --plugins transform-remove-console
babel js/widgets.js --out-dir ../../../wwwroot/admin/js --source-maps --plugins transform-remove-console
babel js/blog-categories.js --out-dir ../../../wwwroot/admin/js --source-maps --plugins transform-remove-console
babel js/blog-media.js --out-dir ../../../wwwroot/admin/js --source-maps --plugins transform-remove-console
babel js/blog-settings.js --out-dir ../../../wwwroot/admin/js --source-maps --plugins transform-remove-console
babel js/blog-tags.js --out-dir ../../../wwwroot/admin/js --source-maps --plugins transform-remove-console
babel js/setup.js --out-dir ../../../wwwroot/admin/js --source-maps --plugins transform-remove-console
babel js/site-users.js --out-dir ../../../wwwroot/admin/js --source-maps --plugins transform-remove-console
babel js/compose-page.js --out-dir ../../../wwwroot/admin/js --source-maps --plugins transform-remove-console
babel js/compose-pagenav.js --out-dir ../../../wwwroot/admin/js --source-maps --plugins transform-remove-console
babel js/compose-post.js --out-dir ../../../wwwroot/admin/js --source-maps --plugins transform-remove-console
babel js/blog-posts.js --out-dir ../../../wwwroot/admin/js --source-maps --plugins transform-remove-console
babel js/blog-pages.js --out-dir ../../../wwwroot/admin/js --source-maps --plugins transform-remove-console
```

Release all files

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