# Functions Framework for .NET

An open source FaaS (Function as a service) framework for writing portable
Java functions -- brought to you by the Google Cloud Functions team.

The Functions Framework lets you write lightweight functions that run in many
different environments, including:

*   [Google Cloud Functions](https://cloud.google.com/functions/)
*   Your local development machine
*   [Cloud Run and Cloud Run on GKE](https://cloud.google.com/run/)
*   [Knative](https://github.com/knative/)-based environments

## Installation

The Functions Framework for .NET requires the [.NET Core SDK 3.1](https://dotnet.microsoft.com/download).

## Quickstart: Hello, World on your local machine

First, install the template package into the .NET tooling:

```sh
$ dotnet new -i Google.Cloud.Functions.Templates::1.0.0-alpha01
```

Next, create a directory for your project, and use `dotnet new` to
create a new HTTP function:

```sh
$ mkdir HelloFunctions
$ cd HelloFunctions
$ dotnet new gcf-http
```

That will create `HelloFunctions.csproj` and `Function.cs` in
the current directory. Edit `Function.cs` to have a look at what's
required, and provide a custom message if you want.

## Run the function

The Functions Framework needs to know the target function to run.
This is the name of the class implementing `IHttpFunction` (and other interfaces later).
There are two ways of specifying this:

- Using the `FUNCTION_NAME` environment variable
- Passing it on the command line as the first argument to the program

```sh
$ dotnet run HelloFunctions.Function
```

The default port is 8080; this can be changed with the `PORT`
environment variable.

Once the server is running, browse to http://localhost:8080 to
invoke the function. Press Ctrl-C in the console to stop the server.
