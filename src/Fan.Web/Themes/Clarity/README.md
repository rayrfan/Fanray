# Clarity

Clarity theme uses scss, on Windows do the following

1. run [RubyInstaller](https://rubyinstaller.org/)
2. run `gem install sass`
3. cd into `Fan.Web\Themes\Clarity`
4. run sass, optionally with --watch when developing
  - style: `sass --watch scss/style.scss:../../wwwroot/themes/Clarity/css/style.min.css --style compressed`
  - editor: `sass --watch scss/editor.scss:../../wwwroot/themes/Clarity/css/editor.min.css --style compressed`

Syntax

Based on default style of [hightlightjs](https://github.com/isagalaev/highlight.js)

Fonts

1. go to https://icomoon.io/ and click on `IcoMoon App`
2. add any icon lib necessary, select exact icons
3. click on Generate Font to download zip
4. copy `/fonts` folder to `Themes/Clarity/scss` and content of `style.css` to _fonts.scss