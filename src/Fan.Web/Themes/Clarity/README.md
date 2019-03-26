# Clarity 

The Clarity is the default Fanray theme.  All Fanray themes must provide the following 2 css files

- **style.css**: the theme styles for client site.
- **content.css**: the content styles for the editor, it includes **typography** and the **content width** from the theme styles. It's used in Compose.cshtml, this css file enables user to see the exact post styling while the user is still composing the post.

## Scss

Clarity theme uses scss, on Windows do the following

1. run [RubyInstaller](https://rubyinstaller.org/)
2. run `gem install sass`
3. cd into `Fan.Web\Themes\Clarity`
4. run sass

  ```bash
  sass --update scss:../../wwwroot/themes/Clarity/css --style compressed
  ```
5. optionally run sass with --watch

  ```bash
  sass --watch scss:../../wwwroot/themes/Clarity/css --style compressed
  ```

### Fonts

The fonts are stored in `Clarity/scss/fonts`, they are referenced in `_font.scss`.

1. go to https://icomoon.io/ and click on `IcoMoon App`
2. add any icon lib necessary, select exact icons
3. click on Generate Font to download zip
4. copy files inside `/fonts` folder to `Themes/Clarity/scss/fonts` and `wwwroot/themes/Clarity/css/fonts`
5. copy the content of `style.css` to _fonts.scss
6. recompile to produce sytle.css

### Syntax

Due to current editor does not support `pre` tag, I'm using shortcode to post code which is
based on [syntaxhighlighter](https://github.com/syntaxhighlighter/syntaxhighlighter).