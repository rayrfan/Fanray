# Shortcodes Plugin client

The source code shortcode requires [syntaxhighlighter](https://github.com/syntaxhighlighter/syntaxhighlighter).

### Develop

```bash
sass --watch scss/syntaxhighlighter.scss:dist/css/syntaxhighlighter.css
```

### Release

```bash
sass scss/syntaxhighlighter.scss dist/css/syntaxhighlighter.css --style compressed
```