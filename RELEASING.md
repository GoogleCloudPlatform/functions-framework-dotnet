# Releasing the .NET Functions Frameworks packages

This is a largely automated process. Steps:

- Decide on a new version number, following semantic versioning, but
  don't change any files.
- Document the new version in [docs/history.md]() (but don't commit)
- Run `./create-release-commit.sh {new-version}` from the root directory
- Push the commit to GitHub and create a pull request
- Add the "autorelease: pending" tag to the PR, and request a review
- Once the PR is merged:
  - A GitHub release and tags will be created automatically
  - A Kokoro job will be launched to build and publish the NuGet packages

The `create-release-commit.sh` script is responsible for:

- Updating the `src/CommonProperties.xml` file which contains the version number
- Updating all project references in templates and examples
- Updating `README.md` to give instructions for installing the templates
- Committing all changes with an appropriate message based on the
  version history

Note that the script expects the version history file to follow the
existing format. It assumes that the third line of the file is the
header for the new release, and uses everything from the fourth line
until the next "##" line, which is expected to be the previous
release.
