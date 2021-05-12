# Detailed package descriptions

This repository contains source code for the following NuGet packages:

- [Google.Cloud.Functions.Framework](https://www.nuget.org/packages/Google.Cloud.Functions.Framework)
  is a very small package, primarily containing interfaces for Cloud
  Functions to implement, as well as adapters between function types.
- [Google.Cloud.Functions.Hosting](https://www.nuget.org/packages/Google.Cloud.Functions.Hosting)
  contains code to start up an ASP.NET Core webserver based on
  conventional environment variables etc.
- [Google.Cloud.Functions.Testing](https://www.nuget.org/packages/Google.Cloud.Functions.Testing)
  contains code to help simplify testing functions.
- [Google.Cloud.Functions.Templates](https://www.nuget.org/packages/Google.Cloud.Functions.Templates)
  contains templates for the `dotnet` command line to create a very
  simple getting-started experience.

This page describes when you might wish to use which package - and
when you might want to opt for no package at all.

## Google.Cloud.Functions.Templates

If you want to use the Google.Cloud.Functions.Hosting package
described later, the templates package is the simplest way to get
started. Install the templates, and just run `dotnet new gcf-http`
(or specify any of the other templates provided) and you're away.

The templates are very simple; each is just a project file and a
single source file. The project is just a console application with a
dependency on Google.Cloud.Functions.Hosting. If you're happy
creating the project yourself, you definitely don't *need* to use
the templates. You can create a regular console application, replace
the `Program` file with a function, add the dependency, and
you're away.

## Google.Cloud.Functions.Testing

The testing package makes it easier to write tests which start an
in-memory ASP.NET Core server to respond to requests. It's not
generally needed to write unit tests for functions, and it's not
*required* even to write integration tests - it's just convenient.

See the [testing documentation](testing.md) for more details and
links to sample code.

## Google.Cloud.Functions.Framework

This package is the core of the Functions Framework for .NET. It
contains:

- The interfaces that functions are expected to implement
  (`IHttpFunction`, `ICloudEventFunction` etc)
- Adapter classes to make it easy to wrap any function other than
  `IHttpFunction` *as* an `IHttpFunction`.
- Event payload types (e.g. `StorageObject`, `PubSubMessage` and
  `FirestoreEvent`). Note that in a future version of this package,
  these types may be moved to a different package.

The framework depends on ASP.NET Core, and also the
[CloudNative.CloudEvents
package](https://github.com/cloudevents/sdk-csharp) for
[CNCF CloudEvents](https://cloudevents.io/).

Cloud Functions can depend on just the types within this package in
order to be portable however they're hosted.

## Google.Cloud.Functions.Hosting

The hosting package is expected to be used as a dependency from a
*project*, but not usually from *source code* itself other than for
Functions Startup classes, used for [customization](customization.md).

The purpose of the package is to allow you to get up and running
with absolutely minimal code. When a function package depends on the
hosting package, an entry point (a class with a `Main` method) is
generated automatically via MSBuild magic when the function package
is built. That entry point calls into `EntryPoint.StartAsync` method
within the hosting package to start the web server.

(Note: the entry point generation can be disabled by setting the
`AutoGenerateEntryPoint` MSBuild property to any value other than
`true`, which is the default.)

The main roles of the hosting package are:

- Handle the environment variables and any command line arguments to
  work out which function to invoke, what port to listen on etc.
- Set up ASP.NET Core dependency injection and configuration,
  including any adapters required to handle your function, and any
  Functions Startup classes you have registered.
- Set up logging in a manner appropriate for your environment.
- Run the ASP.NET Core server, with a pipeline to call the function.

## Creating a function server without the hosting package

Nothing ties Cloud Run (or Cloud Functions) to the hosting
package. It's entirely possible to deploy a regular ASP.NET Core
application to Google Cloud Functions, with some changes. This
approach is better suited to Cloud Run than Cloud Functions, but
will work on both platforms.

At a minimum, you would need to configure the server to listen on
the port specified by the `PORT` environment variable. Additionally,
you would need to be careful about request routing: while a regular
ASP.NET Core application expects to serve different requests using
different methods, usually based on the path in the request, Google
Cloud Functions currently makes no guarantees about the request path
received by your server. The expectation is that each function
really is exactly one function.

Rather than deploying a regular MVC application as a function, a
more likely scenario is that you wish to replace the hosting package
with your own code to start the ASP.NET Core server - but still
ultimately using a single kind of `RequestDelegate` to server your
function. This would provide more control over the request pipeline,
at the cost of convenience.

If you take this approach, it's entirely up to you whether you
depend on the Google.Cloud.Functions.Framework package. If you
include the dependency and implement `IHttpFunction`, you can switch
freely between your custom code and the hosting package. Likewise
you may wish to use the adapters to perform the boilerplate request
parsing for CloudEvents. But no server-side code will treat your
function differently based on whether or not it happens to depend on
Google.Cloud.Functions.Framework.
