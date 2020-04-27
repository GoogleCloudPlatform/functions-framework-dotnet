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

## SimpleHttpFunction

The [SimpleHttpFunction](../examples/Google.Cloud.Functions.Examples.SimpleHttpFunction)
function is the result of creating a new HTTP function via the
template using the following command line:

```sh
dotnet new gcf-http
```

## SimpleEventFunction

The [SimpleEventFunction](../examples/Google.Cloud.Functions.Examples.SimpleEventFunction)
function is the result of creating a new Cloud Event function via the
template using the following command line:

```sh
dotnet new gcf-event
```

## SimpleLegacyEventFunction

The [SimpleLegacyEventFunction](../examples/Google.Cloud.Functions.Examples.SimpleLegacyEventFunction)
function is the result of creating a new Legacy Event function via the
template using the following command line:

```sh
dotnet new gcf-legacy-event
```

## VbHttpFunction

The [VbHttpFunction](../examples/Google.Cloud.Functions.Examples.VbHttpFunction)
function is the result of creating a new HTTP function via the
template using the following command line:

```sh
dotnet new gcf-http -lang vb
```

## VbEventFunction

The [VbEventFunction](../examples/Google.Cloud.Functions.Examples.VbEventFunction)
function is the result of creating a new Cloud Event function via the
template using the following command line:

```sh
dotnet new gcf-event -lang vb
```

## FSharpHttpFunction

The [FSharpHttpFunction](../examples/Google.Cloud.Functions.Examples.FSharpHttpFunction)
function is the result of creating a new HTTP function via the
template using the following command line:

```sh
dotnet new gcf-http -lang f#
```

## FSharpEventFunction

The [FSharpEventFunction](../examples/Google.Cloud.Functions.Examples.FSharpEventFunction)
function is the result of creating a new Cloud Event function via the
template using the following command line:

```sh
dotnet new gcf-event -lang f#
```

## SimpleDependencyInjection

The [SimpleDependencyInjection](../examples/Google.Cloud.Functions.Examples.SimpleDependencyInjection)
example demonstrates out-of-the-box dependency injection without
any additional configuration.

See the [dependency injection documentation](dependency-injection.md) for more details.

## AdvancedDependencyInjection

The [AdvancedDependencyInjection](../examples/Google.Cloud.Functions.Examples.AdvancedDependencyInjection)
example demonstrates dependency injection with extra configuration
provided via a startup class, including scoped and singleton
dependencies.

See the [dependency injection documentation](dependency-injection.md) for more details.

## TimeZoneConverter

The [TimeZoneConverter](../examples/Google.Cloud.Functions.Examples.TimeZoneConverter)
example provides a function with more business logic than most simple examples. It uses
the [Noda Time](https://nodatime.org) library to perform time zone conversions based
on the [IANA time zone database](https://www.iana.org/time-zones).

See the [README.md file](../examples/Google.Cloud.Functions.Examples.TimeZoneConverter/README.md)
for more details.

## StorageImageAnnotator

The [StorageImageAnnotator](../examples/Google.Cloud.Functions.Examples.StorageImageAnnotator)
example is an event function that's triggered when a file is
uploaded into a Google Cloud Storage bucket. It uses the Google
Cloud Vision API to perform various aspects of image recognition,
then writes the results as a new Storage object.

See the [README.md file](../examples/Google.Cloud.Functions.Examples.StorageImageAnnotator/README.md)
for more details.

## Integration Tests

The [IntegrationTests](../examples/Google.Cloud.Functions.Examples.IntegrationTests)
directory contains integration tests for the example functions.
These can also be used as examples of how functions can be tested.
See the [testing documentation](testing.md) for more details.
