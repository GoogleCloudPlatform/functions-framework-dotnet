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

To deploy an example using the local version of the Functions
Framework, you can use `--set-build-env-vars` to set the
`LOCALFUNCTIONSFRAMEWORK` environment variable (which must be in
all-caps for the Buildpack to work) to true, along with
`GOOGLE_BUILDABLE` to select the project. This has to be run from
the root directory, in order to pick up `src` as well as `examples`.
So for example, to run the `SimpleHttpFunction` example using a
modified Functions Framework, you could run:

```sh
gcloud functions deploy ff-test \
  --runtime=dotnet3 \
  --trigger-http \
  --set-build-env-vars=GOOGLE_BUILDABLE=examples/Google.Cloud.Functions.Examples.SimpleHttpFunction,LOCALFUNCTIONSFRAMEWORK=true \
  --entry-point=Google.Cloud.Functions.Examples.SimpleHttpFunction.Function
```
