# Welcome to the contributing guide for Fanray

## Good first issue

Issues [not assigned to anyone](https://github.com/FanrayMedia/Fanray/issues?q=is%3Aopen+is%3Aissue+no%3Aassignee) or [labeled with help wanted](https://github.com/FanrayMedia/Fanray/issues?q=is%3Aissue+is%3Aopen+label%3A%22help+wanted%22) are a good place to start.  If you are not sure about something always leave a comment.

## Bug Report / Feature Request

Seach both [Open](https://github.com/FanrayMedia/Fanray/issues) and [Closed](https://github.com/FanrayMedia/Fanray/issues?q=is%3Aissue+is%3Aclosed) issues for duplicates before [file a new one](https://github.com/FanrayMedia/Fanray/issues/new/choose). 

## Pull Request

- **A PR must have a successful build** (Azure Pipeline, Appveyor and Travis CI) to be considered.

- Don't hesitate to talk about the features/fixes you want to develop by creating/commenting an issue before start working on them. **All commits must be associated with an issue**.

- Development is done on the `dev` branch, **please branch off from "dev" for your development**.

- **Commit message should start with a prefix and end with an issue number**, e.g. `fix: My commit message #202`.

  - **build**: Changes that affect the build system, CI or external dependencies (e.g. npm, nuget)
  - **docs**: Documentation only changes
  - **feat**: A new feature
  - **fix**: A bug fix
  - **refactor**: A code change that neither fixes a bug nor adds a feature such as code clean up etc.
  - **test**: Add missing tests or correcting existing tests

- **Write code comments** and when you use a snippet of code from the Internet always leave a link to reference.

- **Provide unit / integration tests** where applicable because testing is essential for a strong code base.

## License

By contributing your code, you agree to license your contribution under the [Apache 2.0 License](LICENSE).