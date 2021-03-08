# Examples

The [examples](../examples) directory contains source code to
demonstrate various features of the Functions Framework. This page
provides commentary and explanation of those examples.

The examples all use the NuGet packages by default, to make them
easier to deploy and more representative of the normal developer
experience. However, if you wish to experiment with changes to the
Functions Framework - whether that's developing new features or
fixing a bug - you can do so by setting the MSBuild property
`LocalFunctionsFramework` to any non-empty value. Typically it's
most convenient to do this by setting an environment variable of the
same name before running `dotnet` or launching Visual Studio.

> **Tip**  
> If you start Visual Studio from the command line, it uses the
> environment variables from that command line session. This is a
> simple way of developing against the local framework just for a
> single session.

Some examples include multiple classes in a single source file. This
is purely to make the examples easier to follow when browsing on
GitHub.

## Running and deploying

Each example can be run locally from the command line by changing to
the example directory and running

```sh
dotnet run
```

Alternatively, you can open the solution file in the `examples`
directory, set the "Startup Project" to the example you want to run,
and just press F5 to run.

To deploy to Google Cloud Functions or Cloud Run, follow the
[deployment guide](deployment.md). Every example uses a function
class called `Function` in a namespace that's the same as the
directory. For example, the entry point for the function in the
`Google.Cloud.Functions.Examples.SimpleHttpFuntion` directory is
`Google.Cloud.Functions.Examples.SimpleHttpFunction.Function`.

A sample command line is given for each example, but you may need to
adapt this to specify your own Google Cloud resources such as Pub/Sub
topics or Storage buckets.

> **Note for Windows users**  
> The sample command lines shown below are split over multiple lines
> for readability, using `\` as a line continuation character. That
> allows the whole command line to be copied and pasted in one go
> within Unix shells such as bash, but won't work in a single step on
> the Windows command line. Copy and paste each line, removing the
> trailing `\`, and execute as a single command. For example, the
> command to deploy SimpleHttpFunction would be:
>
> ```sh
> gcloud functions deploy simple-http --runtime dotnet3 --trigger-http --allow-unauthenticated --entry-point Google.Cloud.Functions.Examples.SimpleHttpFunction.Function
> ```

Where environment variables are used in the command line (e.g. `$GCS_BUCKET_NAME`)
you can substitute the bucket name directly into the command line, or set an environment
variable to make it easier to run multiple command lines that all use the same
value. The environment variables shown are not used by the `gcloud functions deploy`
command itself; they're only shown here to indicate a "placeholder" value.

## SimpleHttpFunction

The [SimpleHttpFunction](../examples/Google.Cloud.Functions.Examples.SimpleHttpFunction)
function is the result of creating a new HTTP function via the
template using the following command line:

```sh
dotnet new gcf-http
```

Sample deployment:

```sh
gcloud functions deploy simple-http \
  --runtime dotnet3 \
  --trigger-http \
  --allow-unauthenticated \
  --entry-point Google.Cloud.Functions.Examples.SimpleHttpFunction.Function
```

## SimpleEventFunction

The [SimpleEventFunction](../examples/Google.Cloud.Functions.Examples.SimpleEventFunction)
function is the result of creating a new Cloud Event function via the
template using the following command line:

```sh
dotnet new gcf-event
```

This function expects Storage events, and logs details of the event it receives.

Sample deployment:

```sh
gcloud functions deploy simple-event \
  --runtime dotnet3 \
  --trigger-event google.storage.object.finalize \
  --trigger-resource $GCS_BUCKET_NAME \
  --entry-point Google.Cloud.Functions.Examples.SimpleEventFunction.Function
```

## SimpleUntypedEventFunction

The [SimpleUntypedEventFunction](../examples/Google.Cloud.Functions.Examples.SimpleUntypedEventFunction)
function is the result of creating a new untyped Cloud Event function via the
template using the following command line:

```sh
dotnet new gcf-untyped-event
```

This function can handle any event type, and logs some generic
details of the event it receives.

Sample deployment to listen to Pub/Sub events:

```sh
gcloud functions deploy simple-untyped-event \
  --runtime dotnet3 \
  --trigger-topic $PUBSUB_TOPIC_NAME \
  --entry-point Google.Cloud.Functions.Examples.SimpleUntypedEventFunction.Function
```

## VbHttpFunction

The [VbHttpFunction](../examples/Google.Cloud.Functions.Examples.VbHttpFunction)
function is the result of creating a new HTTP function via the
template using the following command line:

```sh
dotnet new gcf-http -lang vb
```

Sample deployment:

```sh
gcloud functions deploy vb-http \
  --runtime dotnet3 \
  --trigger-http \
  --allow-unauthenticated \
  --entry-point Google.Cloud.Functions.Examples.VbHttpFunction.CloudFunction
```

## VbEventFunction

The [VbEventFunction](../examples/Google.Cloud.Functions.Examples.VbEventFunction)
function is the result of creating a new Cloud Event function via the
template using the following command line:

```sh
dotnet new gcf-event -lang vb
```

This function expects Storage events, and logs details of the event it receives.

Sample deployment:

```sh
gcloud functions deploy vb-event \
  --runtime dotnet3 \
  --trigger-event google.storage.object.finalize \
  --trigger-resource $ \
  --entry-point Google.Cloud.Functions.Examples.VbEventFunction.CloudFunction
```

## VbUntypedEventFunction

The
[VbUntypedEventFunction](../examples/Google.Cloud.Functions.Examples.VbUntypedEventFunction)
function is the result of creating a new untyped Cloud Event function via the
template using the following command line:

```sh
dotnet new gcf-untyped-event -lang vb
```

This function can handle any event type, and logs some generic
details of the event it receives.

Sample deployment to listen to Pub/Sub events:

```sh
gcloud functions deploy vb-untyped-event \
  --runtime dotnet3 \
  --trigger-topic $PUBSUB_TOPIC_NAME \
  --entry-point Google.Cloud.Functions.Examples.VbUntypedEventFunction.CloudFunction
```

## FSharpHttpFunction

The [FSharpHttpFunction](../examples/Google.Cloud.Functions.Examples.FSharpHttpFunction)
function is the result of creating a new HTTP function via the
template using the following command line:

```sh
dotnet new gcf-http -lang f#
```

Sample deployment:

```sh
gcloud functions deploy fsharp-http \
  --runtime dotnet3 \
  --trigger-http \
  --allow-unauthenticated \
  --entry-point Google.Cloud.Functions.Examples.FSharpHttpFunction.Function
```

## FSharpEventFunction

The [FSharpEventFunction](../examples/Google.Cloud.Functions.Examples.FSharpEventFunction)
function is the result of creating a new Cloud Event function via the
template using the following command line:

```sh
dotnet new gcf-event -lang f#
```

This function expects Storage events, and logs details of the event it receives.

Sample deployment:

```sh
gcloud functions deploy fsharp-event \
  --runtime dotnet3 \
  --trigger-event google.storage.object.finalize \
  --trigger-resource $GCS_BUCKET_NAME \
  --entry-point Google.Cloud.Functions.Examples.FSharpEventFunction.Function
```

## FSharpUntypedEventFunction

The [FSharpUntypedEventFunction](../examples/Google.Cloud.Functions.Examples.FSharpUntypedEventFunction)
function is the result of creating a new untyped Cloud Event function via the
template using the following command line:

```sh
dotnet new gcf-untyped-event -lang f#
```

This function can handle any event type, and logs some generic
details of the event it receives.

Sample deployment to listen to Pub/Sub events:

```sh
gcloud functions deploy fsharp-untyped-event \
  --runtime dotnet3 \
  --trigger-topic $PUBSUB_TOPIC_NAME \
  --entry-point Google.Cloud.Functions.Examples.FSharpUntypedEventFunction.Function
```

## SimpleDependencyInjection

The [SimpleDependencyInjection](../examples/Google.Cloud.Functions.Examples.SimpleDependencyInjection)
example demonstrates out-of-the-box dependency injection without
any additional configuration.

See the [customization documentation](customization.md) for more details.

Sample deployment:

```sh
gcloud functions deploy simple-dependency-injection \
  --runtime dotnet3 \
  --trigger-http \
  --allow-unauthenticated \
  --entry-point Google.Cloud.Functions.Examples.SimpleDependencyInjection.Function
```

## AdvancedDependencyInjection

The [AdvancedDependencyInjection](../examples/Google.Cloud.Functions.Examples.AdvancedDependencyInjection)
example demonstrates dependency injection with services provided via a Functions Startup class,
including scoped and singleton dependencies.

See the [customization documentation](customization.md) for more details.

Sample deployment:

```sh
gcloud functions deploy advanced-dependency-injection \
  --runtime dotnet3 \
  --trigger-http \
  --allow-unauthenticated \
  --entry-point Google.Cloud.Functions.Examples.AdvancedDependencyInjection.Function
```

## TestableDependencies

The [TestableDependencies](../examples/Google.Cloud.Functions.Examples.TestableDependencies)
example demonstrates how a function with dependency injection configured by a Functions Startup class
can be [tested](../examples/Google.Cloud.Functions.Examples.IntegrationTests.TestableDependenciesTest.cs)
using test-only dependencies.

See the [testing documentation](testing.md) for more details.

Sample deployment:

```sh
gcloud functions deploy testable-dependencies \
  --runtime dotnet3 \
  --trigger-http \
  --allow-unauthenticated \
  --entry-point Google.Cloud.Functions.Examples.TestableDependencies.Function
```

## Middleware

The [Middleware](../examples/Google.Cloud.Functions.Examples.Middleware)
example demonstrates adding middleware to the request pipeline using a Functions Startup class.

See the [customization documentation](customization.md) for more details.

Sample deployment:

```sh
gcloud functions deploy middleware \
  --runtime dotnet3 \
  --trigger-http \
  --allow-unauthenticated \
  --entry-point Google.Cloud.Functions.Examples.Middleware.Function
```

## Standard configuration

The [Configuration](../examples/Google.Cloud.Functions.Examples.Configuration)
example demonstrates using `IConfiguration` to obtain information from
settings files to configure services.

This example uses three settings files:
[appsettings.json](../examples/Google.Cloud.Functions.Examples.Configuration/appsettings.json),
[appsettings.Development.json](../examples/Google.Cloud.Functions.Examples.Configuration/appsettings.Development.json)
and [appsettings.Production.json](../examples/Google.Cloud.Functions.Examples.Configuration/appsettings.Production.json).
The configuration is loaded in the normal way for .NET Core, based on the environment which is determined
by the `ASPNETCORE_ENVIRONMENT` and `DOTNET_ENVIRONMENT` environment variables.

> **Note on environment choice**
>
> When using configuration settings like this, please be aware that the default environment in
> ASP.NET Core is Production. You may wish to set the `ASPNETCORE_ENVIRONMENT` or `DOTNET_ENVIRONMENT`
> environment variables on a user level, to avoid accidentally attempting to run against the production
> configuration.

> **Note on AppSettings files**
>
> Currently, `appsettings.json` files are not copied by default when the project is published using `dotnet publish`.
> This means that although the function meay run as expected locally, when it is deployed the settings files would be
> absent. This can be fixed by explicitly including them in the project file, [as shown in this
> example](../examples/Google.Cloud.Functions.Examples.Configuration/Google.Cloud.Functions.Examples.Configuration.csproj).
> We hope to do this implicitly in a future release. See [issue
> 201](https://github.com/GoogleCloudPlatform/functions-framework-dotnet/issues/201) for more information.

Sample deployment:

```sh
gcloud functions deploy configuration-example \
  --runtime dotnet3 \
  --trigger-http \
  --allow-unauthenticated \
  --set-env-vars ASPNETCORE_ENVIRONMENT=Development \
  --entry-point Google.Cloud.Functions.Examples.Configuration.Function
```

## Additional configuration sources

The [CustomConfiguration](../examples/Google.Cloud.Functions.Examples.CustomConfiguration)
example demonstrates adding a custom configuration source using a Functions Startup class.

See the [customization documentation](customization.md) for more details.

```sh
gcloud functions deploy additional-configuration-example \
  --runtime dotnet3 \
  --trigger-http \
  --allow-unauthenticated \
  --entry-point Google.Cloud.Functions.Examples.CustomConfiguration.Function
```

## TimeZoneConverter

The [TimeZoneConverter](../examples/Google.Cloud.Functions.Examples.TimeZoneConverter)
example provides a function with more business logic than most simple examples. It uses
the [Noda Time](https://nodatime.org) library to perform time zone conversions based
on the [IANA time zone database](https://www.iana.org/time-zones).

See the [README.md file](../examples/Google.Cloud.Functions.Examples.TimeZoneConverter/README.md)
for more details.

Sample deployment:

```sh
gcloud functions deploy time-zone-converter \
  --runtime dotnet3 \
  --trigger-http \
  --allow-unauthenticated \
  --entry-point Google.Cloud.Functions.Examples.TimeZoneConverter.Function
```

## StorageImageAnnotator

The [StorageImageAnnotator](../examples/Google.Cloud.Functions.Examples.StorageImageAnnotator)
example is an event function that's triggered when a file is
uploaded into a Google Cloud Storage bucket. It uses the Google
Cloud Vision API to perform various aspects of image recognition,
then writes the results as a new Storage object.

See the [README.md file](../examples/Google.Cloud.Functions.Examples.StorageImageAnnotator/README.md)
for more details.

Sample deployment:

```sh
gcloud functions deploy image-annotator \
  --runtime dotnet3 \
  --trigger-event google.storage.object.finalize \
  --trigger-resource $GCS_BUCKET_NAME \
  --entry-point=Google.Cloud.Functions.Examples.StorageImageAnnotator.Function
```

## MultiProjectFunction and MultiProjectDependency

All the other examples provided are standalone, but in real world
projects, often the project containing your function will depend on
another local project. The [MultiProjectFunction](../examples/Google.Cloud.Functions.Examples.MultiProjectFunction)
and [MultiProjectDependency](../examples/Google.Cloud.Functions.Examples.MultiProjectDependency)
directories provide an example of this: the MultiProjectFunction project depends on the
MultiProjectDependency project using a `<ProjectReference>` MSBuild element.

When deploying a function that depends on another local project, you
need to ensure that all the relevant source code is uploaded, and
that you indicate which project contains the function. See the
[deployment documentation](deployment.md#deploying-a-function-with-a-local-project-dependency)
for more details.

Sample deployment, from the `examples` directory:

```sh
gcloud beta functions deploy multi-project \
  --runtime dotnet3 \
  --trigger-http \
  --entry-point=Google.Cloud.Functions.Examples.MultiProjectFunction.Function
  --set-build-env-vars=GOOGLE_BUILDABLE=Google.Cloud.Functions.Examples.MultiProjectFunction
```

This uses the [.gcloudignore
file in the examples directory](../examples/.gcloudignore) to avoid uploading already-built
binaries.

Note that the ability to set build environment variables is currently in beta.

## Integration Tests

The [IntegrationTests](../examples/Google.Cloud.Functions.Examples.IntegrationTests)
directory contains integration tests for the example functions.
These can also be used as examples of how functions can be tested.
See the [testing documentation](testing.md) for more details.
