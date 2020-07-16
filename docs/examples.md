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
  --entry-point Google.Cloud.Functions.Examples.VbHttpFunction.Function
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
  --entry-point Google.Cloud.Functions.Examples.VbEventFunction.Function
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
  --entry-point Google.Cloud.Functions.Examples.VbUntypedEventFunction.Function
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

See the [dependency injection documentation](dependency-injection.md) for more details.

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
example demonstrates dependency injection with services provided via a startup class,
including scoped and singleton dependencies.

See the [dependency injection documentation](dependency-injection.md) for more details.

## Configuration

The [Configuration](../examples/Google.Cloud.Functions.Examples.Configuration)
example demonstrates using `IConfiguration` to obtain information from
settings files to configure services.

See the [dependency injection documentation](dependency-injection.md) for more details.

Sample deployment:

```sh
gcloud functions deploy advanced-dependency-injection \
  --runtime dotnet3 \
  --trigger-http \
  --allow-unauthenticated \
  --entry-point Google.Cloud.Functions.Examples.AdvancedDependencyInjection.Function
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

## Integration Tests

The [IntegrationTests](../examples/Google.Cloud.Functions.Examples.IntegrationTests)
directory contains integration tests for the example functions.
These can also be used as examples of how functions can be tested.
See the [testing documentation](testing.md) for more details.
