# Functions Framework for .NET

[![Build Status](https://travis-ci.com/GoogleCloudPlatform/functions-framework-dotnet.svg?branch=master)](https://travis-ci.com/GoogleCloudPlatform/functions-framework-dotnet)

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
dotnet new -i Google.Cloud.Functions.Templates::1.0.0-alpha01
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
dotnet run HelloFunctions.Function
```

Once the server is running, browse to http://localhost:8080 to
invoke the function. Press Ctrl-C in the console to stop the server.

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
operating system, and more). In the `Dockerfile`, you need to set an environment
variable `FUNCTION_TARGET` to point to the target function to be invoked.

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
`--target` (or only argument) | `FUNCTION_TARGET`         | The name of the target function implementing `IHttpFunction` to be invoked in response to requests.

Examples:

- `dotnet run HelloFunctions.Function`
- `dotnet run --target HelloFunctions.Function`
- `dotnet run --target HelloFunctions.Function --port 8000`
