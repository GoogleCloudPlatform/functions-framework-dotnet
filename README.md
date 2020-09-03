# Functions Framework for .NET

[![Build Status](https://img.shields.io/endpoint.svg?url=https%3A%2F%2Factions-badge.atrox.dev%2FGoogleCloudPlatform%2Ffunctions-framework-dotnet%2Fbadge&style=flat)](https://actions-badge.atrox.dev/GoogleCloudPlatform/functions-framework-dotnet/goto)

An open source FaaS (Function as a service) framework for writing portable
.NET functions -- brought to you by the Google Cloud Functions team.

The Functions Framework lets you write lightweight functions that run in many
different environments, including:

* [Google Cloud Functions](https://cloud.google.com/functions/)
* Your local development machine
* [Cloud Run and Cloud Run on GKE](https://cloud.google.com/run/)
* [Knative](https://github.com/knative/)-based environments

## Prerequisites

The Functions Framework for .NET requires the [.NET Core SDK 3.1](https://dotnet.microsoft.com/download).

## Quickstart: Hello, World on your local machine

First, install the template package into the .NET tooling:

```sh
dotnet new -i Google.Cloud.Functions.Templates::1.0.0-alpha11
```

Next, create a directory for your project, and use `dotnet new` to
create a new HTTP function:

```sh
mkdir HelloFunctions
cd HelloFunctions
dotnet new gcf-http
```

That will create `HelloFunctions.csproj` and `Function.cs` in
the current directory. Edit `Function.cs` to have a look at what's
required, and provide a custom message if you want.

Run the function:

```sh
dotnet run
```

Once the server is running, browse to http://localhost:8080 to
invoke the function. Press Ctrl-C in the console to stop the server.

## Cloud Event Functions

After installing the same template package described above, use the
`gcf-event` template:

```sh
mkdir HelloEvents
cd HelloEvents
dotnet new gcf-event
```

That will create the same set of files as before, but the `Function`
class now implements `ICloudEventFunction<StorageObjectData>`. This is a
function that responds to [CNCF Cloud
Events](https://cloudevents.io/), expecting data corresponding to a
Google Cloud Storage object. If you deploy the function with a
trigger of `google.storage.object.finalize` and then upload a new
object to Google Cloud Storage, the sample event will log the
details of the new event, including some properties of the storage
object.

The procedure for running a Cloud Event Function is exactly the same
as for an HTTP Function.

The type argument to the generic `ICloudEventFunction<TData>` interface
expresses the type of data your function expects within the
Cloud Event. The data type should be annotated with
[CloudEventDataConverterAttribute](https://github.com/googleapis/google-cloudevents-dotnet/blob/master/src/Google.Events/CloudEventDataConverterAttribute.cs)
to indicate how to convert from a CloudEvent to the data type.
Typically this is a type from the
[Google.Events.Protobuf](https://www.nuget.org/packages/Google.Events.Protobuf) or
[Google.Events.SystemTextJson](https://www.nuget.org/packages/Google.Events.SystemTextJson)
package. See the [google-cloudevents-dotnet
README](https://github.com/googleapis/google-cloudevents-dotnet/blob/master/README.md)
for more information about these packages.

> **Note:**  
> Google Cloud Functions support for events predates the CNCF Cloud
> Events initiative. The types in the `Google.Cloud.Functions.Framework.GcfEvents`
> namespace provide payloads for these events. The Functions Framework
> converts the Google Cloud Functions representation into a Cloud Event
> representation transparently, so as a developer you only need to
> handle Cloud Events.

### Untyped Cloud Event Functions

If you are experimenting with Cloud Events and don't yet have a
payload data model you wish to commit to, or you want your function
to be able to handle *any* Cloud Event, you can implement the
non-generic `ICloudEventFunction` interface. Your function's method
will then just be passed a `CloudEvent`, with no separate data object.

After installing the template package described earlier, use the
`gcf-untyped-event` template:

```sh
mkdir HelloUntypedEvents
cd HelloUntypedEvents
dotnet new gcf-untyped-event
```

This will create a function that simply logs the information about
any Cloud Event it receives.

## VB and F# support

The templates package also supports VB and F# projects. Just use `-lang vb`
or `-lang f#` in the `dotnet new` command. For example, the HTTP function example
above can be used with VB like this:

```sh
mkdir HelloFunctions
cd HelloFunctions
dotnet new gcf-http -lang vb
```

The examples and documentation are primarily written in C# for the
moment, but the same concepts and features apply equally to VB.

F# support is currently not "idiomatic F#", but regular F# functions
should be easy to wrap using the code in the templates. Feedback on
how we can provide a more familiar F# experience is welcome.

## Run your function on serverless platforms

### Google Cloud Functions

Google Cloud Function does not currently support .NET functions.

### Cloud Run/Cloud Run on GKE

Once you've written your function and added the Functions Framework, all that's
left is to create a container image. [Check out the Cloud Run
quickstart](https://cloud.google.com/run/docs/quickstarts/build-and-deploy) for
C# to create a container image and deploy it to Cloud Run. You'll write a
`Dockerfile` when you build your container. This `Dockerfile` allows you to specify
exactly what goes into your container (including custom binaries, a specific
operating system, and more).

If you want even more control over the environment, you can [deploy your
container image to Cloud Run on
GKE](https://cloud.google.com/run/docs/quickstarts/prebuilt-deploy-gke). With
Cloud Run on GKE, you can run your function on a GKE cluster, which gives you
additional control over the environment (including use of GPU-based instances,
longer timeouts and more).

### Container environments based on Knative

Cloud Run and Cloud Run on GKE both implement the [Knative Serving
API](https://www.knative.dev/docs/). The Functions Framework is designed to be
compatible with Knative environments. Just build and deploy your container to a
Knative environment.

## Configure the Functions Framework

You can configure the Functions Framework using command-line flags or
environment variables. If you specify both, the environment variable will be
ignored. For convenience, if you specify just a single command line
argument, that is assumed to be the target.

Command-line flag             | Environment variable      | Description
----------------------------- | ------------------------- | -----------
`--port`                      | `PORT`                    | The port on which the Functions Framework listens for requests. Default: `8080`
`--target` (or only argument) | `FUNCTION_TARGET`         | The name of the target function (implementing `IHttpFunction`, `ICloudEventFunction` or `ICloudEventFunction<TData>`) to be invoked in response to requests.

If the function isn't specified at all, the assembly is scanned for
compatible types. If a single suitable type is found, that is used
as the function. If multiple types are found, the target type must
be specified.

Examples:

- `dotnet run`
- `dotnet run HelloFunctions.Function`
- `dotnet run --target HelloFunctions.Function`
- `dotnet run --target HelloFunctions.Function --port 8000`

## Further documentation

For more information, see the files in the [docs](docs) directory.
