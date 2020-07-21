# Example source code

## Documentation

For information about running these samples, along with a brief
description of what each example demonstrates, see the
[examples documentation page](../docs/examples.md).

## Generating

The following projects are the result of creating new projects from
the templates:

- Google.Cloud.Functions.Examples.SimpleHttpFunction
- Google.Cloud.Functions.Examples.SimpleEventFunction
- Google.Cloud.Functions.Examples.SimpleUntypedEventFunction
- Google.Cloud.Functions.Examples.FSharpHttpFunction
- Google.Cloud.Functions.Examples.FSharpEventFunction
- Google.Cloud.Functions.Examples.FSharpUntypedEventFunction
- Google.Cloud.Functions.Examples.VbHttpFunction
- Google.Cloud.Functions.Examples.VbEventFunction
- Google.Cloud.Functions.Examples.VbUntypedEventFunction

In each case, after creating the project, a copyright notice is
added to the code.

When built as they are, the example projects refer to packages on
nuget.org, which makes deployment to Google Cloud Functions simpler,
and is closer to the regular developer experience.

To build against the local version of the Functions Framework (in
the `src` directory), set an MSBuild property of
`LocalFunctionsFramework` to any non-empty value. This is often most
simply done from the command line. You can then either run the
example with `dotnet run`, or start Visual Studio with the
environment variable set.
