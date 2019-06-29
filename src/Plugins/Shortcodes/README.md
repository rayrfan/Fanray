# Shortcodes Plugin client

The source code shortcode requires [syntaxhighlighter](https://github.com/syntaxhighlighter/syntaxhighlighter).

### Develop

```bash
sass --watch client/scss/syntaxhighlighter.scss:wwwroot/plugins/shortcodes/css/syntaxhighlighter.css
```

### Release

```bash
sass client/scss/syntaxhighlighter.scss wwwroot/plugins/shortcodes/css/syntaxhighlighter.css --style compressed
```