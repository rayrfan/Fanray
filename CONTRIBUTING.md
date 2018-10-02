# Contributing

:warning: Before you [file a new](https://github.com/FanrayMedia/Fanray/issues/new/choose) Bug Report or Feature Request, please first seach both **[Open](https://github.com/FanrayMedia/Fanray/issues)** and **[Closed](https://github.com/FanrayMedia/Fanray/issues?q=is%3Aissue+is%3Aclosed)** issues and PRs to ensure there hasn't been anything similar already filed.

## Bug Report

**Detail is the key!** It's really important for you to provide as much detail as you can, attach screenshots, provide links to online references etc. for people to easily reproduce the bug and examine possible fixes.

## Feature Request

**Please review the [Roadmap](https://github.com/FanrayMedia/Fanray/wiki/Roadmap)!** Take a moment to find out whether your idea fits with the scope and aims of the project. It's up to you to make a strong case to convince the project's maintainer of the merits of this feature.

## Pull Request

**Please communicate first!** After forking the repository please create a pull request before embarking on any significant fix. This way we can talk about how the fix will be implemeted. 

**Please following these tips** This will greatly increase your chance of your patch getting merged into the code base.

1. **Branches:** **master** is the latest release, **dev** is the active development branch all other feature branches merge back to, always create a **feature** branch off of **dev** first to work on your particular fix or enhancement
2. **Commit message** should always reference the issue number, e.g. `This is my commit message #202` this will link your commit to issue 202
3. **Commit message** should start with prefix, e.g. `fix: Consolidate email valid feature into existing Util class #234`
```
feat: A new feature
fix: A bug fix
docs: Documentation only changes
style: Changes that do not affect the meaning of the code (white-space, formatting, missing semi-colons, etc)
refactor: A code change that neither fixes a bug nor adds a feature
perf: A code change that improves performance
test: Adding missing tests or correcting existing tests
build: Changes that affect the build system, CI configuration or external dependencies (example scopes: gulp, broccoli, npm)
chore: Other changes that don't modify src or test files
``` 
4. **Write code comments** and when you use a snippet of code from the Internet always leave a link to reference
5. **Provide Unit / Integration tests** where applicable because testing is essential for a strong code base

## License

By contributing your code, you agree to license your contribution under the [Apache 2.0 License](LICENSE).