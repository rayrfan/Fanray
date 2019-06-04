# Clarity 

Clarity is the default Fanray theme, use it as an example to create your theme.

## dist

The **dist** folder contains client side artifacts such as minified css and js, theme.png etc. The entire content of this folder will be copied into the wwwroot/themes folder for this theme during theme installation.

## theme.png

Theme's cover, a 1200px by 900px .png file, placed right under the **dist** folder.

## style.css and content.css

All themes must provide these 2 css files

- **style.css**: theme styles for client site.
- **content.css**: content styles for the editor. It includes _typography_ and _content width_ styles from **style.css**, it allows user to see post styles while composing the post.

## Views

The **Views** folder contains razor view files.

## theme.json

A **theme.json** file descibes your theme.

## Clarity specific notes

### scss

Clarity theme uses scss.

- To install sass on Windows download [RubyInstaller](https://rubyinstaller.org/), then run `gem install sass`.
- To build `npm run build`
- To watch `npm run scss:watch`

### fonts

The `dist/css/fonts` folder contains fonts referenced by `_font.scss`. To get the fonts

1. go to https://icomoon.io/ and click on `IcoMoon App`
2. add any icon lib necessary, select exact icons
3. click on Generate Font to download zip
4. copy files inside `/fonts` folder to `Themes/Clarity/dist/css/fonts`
5. copy the content of `style.css` to _fonts.scss
6. recompile to produce sytle.css
