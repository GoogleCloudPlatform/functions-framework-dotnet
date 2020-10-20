# Customization through Functions Startup classes

For simple functions, all you need is the code for the function. In
many scenarios, additional customization is needed, including:

- Providing services for dependency injection
- Adding more application configuration sources
- Adding middleware to execute in the request pipeline before the
  function
- (More rarely) Reconfiguring logging

In a regular ASP.NET Core application, all of these would be likely
to be specified in a startup class. The Functions Framework reuses
the idea of a startup class, but in a more composition-friendly
manner that gets out of the way when no customization is required.

The startup classes in the Functions Framework are called *Functions
Startup classes*.

> **Note**
>
> The Functions Framework expects a function to always be "the final
> part of the request pipeline". The intention of Functions Startup
> classes is to add extra customization, rather than to take
> complete control over the application. If you need a greater
> degree of flexibility than Function Startup classes provide,
> you may want to create a host yourself instead of using the Hosting
> package.
>
> The approach taken by the Functions Framework is similar to the
> [Functions Startup classes in Azure
> Functions](https://docs.microsoft.com/en-us/azure/azure-functions/functions-dotnet-dependency-injection),
> but allowing more wide-ranging customization.

## Specifying a Functions Startup class

When the Functions Framework is running normally (either in
production or locally), Functions Startup classes are discovered via
the `FunctionsStartupAttribute` assembly or class attribute. This attribute
specifies a type to use as a Functions Startup class. The attribute
can be specified multiple times, so you can separate your
customization steps cleanly. In most cases the order in which
Functions Startup classes are used may be irrelevant, but the
attribute has an `Order` property to allow relative ordering where
necessary. (This is most likely to be important when adding
middleware to the request pipeline.)

The Functions Startup class itself must be a subclass of
`FunctionStartup`. This is an abstract class with virtual methods to
override depending on what aspect you wish to customize. Later
sections of this document demonstrate which methods are used to
customize which aspect.

Example using an attribute on the function type:

```csharp
using Google.Cloud.Functions.Hosting;

namespace Example
{
    public class Startup : FunctionsStartup
    {
        // Virtual methods in the base class are overridden
        // here to perform customization.
    }
    
    [FunctionsStartup(typeof(Startup))]
    public class Function : IHttpFunction
    {
        // Actual funtion implementation
    }
}
```

Example using an assembly attribute:

```csharp
using Google.Cloud.Functions.Hosting;

// Specify the Functions Startup class
[assembly: FunctionsStartup(typeof(Example.Startup))]

namespace Example
{
    public class Startup : FunctionsStartup
    {
        // Virtual methods in the base class are overridden
        // here to perform customization.
    }
}
```

The attribute has to be in the same assembly as the function
(because that's how it's discovered) but the Functions Startup class
could be in another assembly. It has to be public, with a public
parameterless constructor.

When writing integration tests, it's often important to be able to
swap out your "production" Functions Startup classes with "test"
versions, for example to use fakes or mocks for services configured
via dependency injection. The `FunctionTestServerBuilder` makes this
easy by allowing Functions Startup instances to be specified while
building the test server. See [the testing
documentation](testing.md) for more details.

Typically, specifying the attribute on the function class itself is
the simplest approach. However, if you have multiple functions in
the same project, you can specify startup classes for common
configuration as assembly attributes, and then add attributes for
function-specific startup classes to the individual function types.
Going even further, the attribute is inherited, so you can create a
function base class that specifies all the startup classes for
functions derived from that class.

## Customizing Dependency Injection using `ConfigureServices`

The Functions Framework Hosting package uses [ASP.NET Core dependency
injection](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection)
when constructing instances of the target function type to handle
calls.

See [SimpleDependencyInjection.Function](../examples/Google.Cloud.Functions.Examples.SimpleDependencyInjection/Function.cs)
for an example of this, where the function constructor has an
`ILogger<T>` parameter so that a logger can be injected.

In more complex scenarios, users may wish to provide additional
dependencies themselves through Functions Startup classes, by
overriding the `ConfigureServices` method:

```csharp
public virtual void ConfigureServices(WebHostBuilderContext context, IServiceCollection services)
```

The dependencies are then applied by ASP.NET Core dependency
injection in the normal way. See
[AdvancedDependencyInjection.Function](../examples/Google.Cloud.Functions.Examples.AdvancedDependencyInjection/Function.cs)
for an example of using scoped and singleton services within a single function. Each function invocation uses
a new instance of its scoped dependency, but uses the same instance of the singleton dependency.

## Customizing the request pipeline using `Configure`

The `Configure` method in the Functions Startup class is provided
with the web host builder context and the application builder, as it
would be if you called `IWebHostBuilder.Configure` with a
configuration delegate:

```csharp
public virtual void Configure(WebHostBuilderContext context, IApplicationBuilder app)
```

The code in the Functions Startup is run during application
configuration, before the Functions Framework configures its
own request handling. This allows you to add additional middleware.
Note that while nothing prevents you from installing arbitrary
middleware that terminates the pipeline early, the expectation is
that successful requests will always end up reaching the function.
(For example, installing middleware that terminates the request with
an error for certain conditions is fine; installing middleware to
respond with files from the file system is less conventional.)

See [Middleware.Function](../examples/Google.Cloud.Functions.Examples.Middleware/Function.cs)
for an example of a small piece of middleware providing extra per-request logging.

## Customizing application configuration sources using `ConfigureAppConfiguration`

As shown in the
[Configuration](../examples/Google.Cloud.Functions.Examples.Configuration)
example, standard ASP.NET Core configuration is available out of the
box. If you wish to add more configuration sources, this can be
performed by overriding the `ConfigureAppConfiguration` method in a Functions Startup class:

```csharp
public virtual void ConfigureAppConfiguration(WebHostBuilderContext context, IConfigurationBuilder configuration)
```

See [CustomConfiguration](../examples/Google.Cloud.Functions.Examplles.CustomConfiguration/Function.cs)
for an example that installs the [Steeltoe](https://steeltoe.io/)
random value provider to the available configuration sources.

Any additional configuration is applied from all Functions Startup
classes before any of the other methods are called. This means that
the configuration can be used for logging, or additional services,
or middleware.

## Customizing logging using `ConfigureLogging`

In most scenarios the console logging that comes out-of-the-box will
be all that's required, but if you need to add more, you can do so
by overriding the `ConfigureLogging` method in a Functions Startup
class:

```csharp
public virtual void ConfigureLogging(WebHostBuilderContext context, ILoggingBuilder logging)
```

Note that this will be called *after* Functions Framework logging
has been configured, so don't call `logging.ClearProviders()` unless
you genuinely intend to remove the Functions Framework console logging.

Logging for tests is provided automatically in the `FunctionTestServer`; see [the testing
documentation](testing.md) for more details.

## Startup class mismatches

If you haven't received an error referring you to this section of
the documentation, you *may* want to skip this section.

It is possible (but very rare) for startup classes to affect which
type is chosen as the target function type for the server. Given
that the target function type determines which startup classes are
used, that creates a chicken and egg situation. The startup class
associated with `FunctionA` could change the target function type to
`FunctionB` which needs other startup classes, but those startup
classes wouldn't have been used during server startup.

While it's entirely normal during testing for "unexpected" startup
classes to be used, the entry point for running the server in
production (`Google.Cloud.Functions.Hosting.EntryPoint`) validates
that the startup classes chosen right at the start are still the
ones that would be chosen for the finally-selected function target
type. If that validation fails, an exception is thrown which will
refer you to this documentation.

The solution to the problem involves working out how you got here in
the first place. First look at all the startup classes you're
specifying, and check whether any of them tries to reconfigure the
target function type. Should it do so? Work out what you *want* to
happen in terms of which target function type you expect to serve
requests, and what startup classes it really needs. Ideally, avoid
reconfiguring the target function type within startup classes
(because that can be confusing). If you really need to, consider
specifying all of your startup classes using an assembly-level
attribute, so that the same startup classes are used regardless of
which target function type ends up being selected.

If you run into this problem and can't work out why, please [file an
issue](https://github.com/GoogleCloudPlatform/functions-framework-dotnet/issues/new)
so we can look into it further.
