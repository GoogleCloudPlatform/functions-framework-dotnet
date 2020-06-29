# Functions Framework for .NET documentation

This directory provides documentation for those who need more detail
than is given in the [top-level README](../README.md). This
documentation is not published separately; the Markdown files should
be readable in GitHub.

## Packages

This repository contains the source code for the following NuGet packages:

- [Google.Cloud.Functions.Framework](https://www.nuget.org/packages/Google.Cloud.Functions.Framework)
  is a very small package, primarily containing interfaces for Cloud
  Functions to implement, as well as adapters between function types.
- [Google.Cloud.Functions.Invoker](https://www.nuget.org/packages/Google.Cloud.Functions.Invoker)
  contains code to start up an ASP.NET Core webserver based on
  conventional environment variables etc.
- [Google.Cloud.Functions.Invoker.Testing](https://www.nuget.org/packages/Google.Cloud.Functions.Invoker.Testing)
  contains code to help simplify testing functions.
- [Google.Cloud.Functions.Templates](https://www.nuget.org/packages/Google.Cloud.Functions.Templates)
  contains templates for the `dotnet` command line to create a very
  simple getting-started experience.

See [the packages guide](packages.md) for more details about each
package.

## Repository layout

The repository is split into the following directories:

- [src](../src): Source code for the production packages and tests
- [examples](../examples): Source code for example functions
- [docs](.): Documentation

The example functions are split into their own directory (with their
own solution file) so that we can have many examples, each of which
is a complete, standlone project. The example functions refer to the
projects in `src` to allow for easy development. However, the expectation
is that unless they use unreleased features, each example could be
extracted from the repo, project references changed to package
references, additional MSBuild imports removed, and the example should still work.

## Additional documentation in this directory:

- [Version History](history.md)
- [Package Details](packages.md)
- [Deployment](deployment.md)
- [Testing Functions](testing.md)
- [Examples](examples.md)
- [Dependency Injection](dependency-injection.md)
- [Microsoft.NET.Sdk.Web and launchSettings.json](launch-settings.md)
