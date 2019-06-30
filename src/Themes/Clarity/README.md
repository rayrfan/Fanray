# Clarity 

Default Fanray theme.

## client

This folder contains scss source which will be compiled to the `wwwroot` folder when you do `npm run build`. During development you can also do watch by `npm run scss:watch`.

## wwwroot

Public assets for Clarity.

### fonts

Fonts referenced by `_font.scss`. To get the fonts

1. go to https://icomoon.io/ and click on `IcoMoon App`
2. add any icon lib necessary, select exact icons
3. click on Generate Font to download zip
4. copy files inside `/fonts` folder to `wwwroot/themes/clarity/css/fonts`
5. copy the content of `style.css` to _fonts.scss
6. recompile to produce sytle.css

### style.css and content.css

All themes must provide these 2 css files

- **style.css**: theme styles for client site.
- **content.css**: content styles for the editor. It includes _typography_ and _content width_ styles from **style.css**, it allows user to see post styles while composing the post.

### theme.png

Theme's cover, a 1200px by 900px .png file, placed right under the `wwwroot/themes/clarity` folder.
