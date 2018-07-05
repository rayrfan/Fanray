# Clarity

Clarity theme uses scss, on Windows do the following

1. run [RubyInstaller](https://rubyinstaller.org/)
2. run `gem install sass`
3. cd into `Fan.Web\Themes\Clarity`
4. `sass --watch scss/style.scss:../../wwwroot/css/style.min.css --style compressed`

Syntax

Based on default style of [hightlightjs](https://github.com/isagalaev/highlight.js)

Fonts

1. go to https://icomoon.io/
2. add any icon lib necessary, select exact icons
3. click on Generate Font to download zip
4. copy `/fonts` folder to theme and content of `style.css` to _fonts.scss