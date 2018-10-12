# Welcome to the contributing guide for Fanray

## Good first issue

Looking for issues to work on? Awesome! The ones that are [not assigned to anyone](https://github.com/FanrayMedia/Fanray/issues?q=is%3Aopen+is%3Aissue+no%3Aassignee) or issues [labeled with help wanted](https://github.com/FanrayMedia/Fanray/issues?q=is%3Aissue+is%3Aopen+label%3A%22help+wanted%22) are a good place to start.  If you are not sure about something always leave a comment.

## Bug Report & Feature Request

Please seach both [Open](https://github.com/FanrayMedia/Fanray/issues) and [Closed](https://github.com/FanrayMedia/Fanray/issues?q=is%3Aissue+is%3Aclosed) issues for duplicates before [file a new one](https://github.com/FanrayMedia/Fanray/issues/new/choose).  For new features please also check the [roadmap](https://github.com/FanrayMedia/Fanray/wiki/Roadmap) to see if your idea fits with the scope and aims of the project.

## Pull Request

- Don't hesitate to talk about the features/fixes you want to develop by creating/commenting an issue before start working on them. All commits must be associated with an issue.

- Development is done on the `dev` branch, please branch off from dev for your development.

- Commit message should start with a prefix and end with an issue number, e.g. `fix: My commit message #202` is a commit message for a bug fix and it will link this commit to issue #202.

```text
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

- Write code comments and when you use a snippet of code from the Internet always leave a link to reference.

- Provide Unit / Integration tests where applicable because testing is essential for a strong code base.

## License

By contributing your code, you agree to license your contribution under the [Apache 2.0 License](LICENSE).