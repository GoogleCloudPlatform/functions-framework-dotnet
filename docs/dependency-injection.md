# Dependency Injection

The Functions Framework Invoker uses [ASP.NET Core dependency
injection](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection)
when constructing instances of the target function type to handle
calls.

See [SimpleDependencyInjection.Function](../examples/Google.Cloud.Functions.Examples.SimpleDependencyInjection/Function.cs)
for an example of this, where the function constructor has an
`ILogger<T>` parameter so that a logger can be injected.

In more complex scenarios, users may wish to provide additional dependencies themselves.

For simplicity, consistency and familiarity, the Functions Framework has adopted a pattern which is
essentially identical to
[dependency injection in Azure Functions](https://docs.microsoft.com/en-us/azure/azure-functions/functions-dotnet-dependency-injection):

- Create a startup class deriving from `Google.Cloud.Functions.Invoker.DependencyInjection.FunctionsStartup`
- Override the `Configure(IFunctionsHostBuilder)` method, using `IFunctionsHostBuilder.Services` to access
  an `IServiceCollection` to register services with. You can access the configuration in this method using
  `IFunctionsHostBuilder.Configuration`.
- Apply the `Google.Cloud.Functions.Invoker.DependencyInjection.FunctionsStartupAttribute` to your assembly,
  specifying the startup class.
  - The attribute can be applied multiple times within the same assembly, so that multiple startup classes
    can provide services. The order in which the startup classes are used is not defined.
  - The attribute must be applied to the assembly that contains the function itself, but the startup classes
    can be part of another assembly.

The dependencies are then applied by ASP.NET Core dependency injection in the normal way.

See [AdvancedDependencyInjection.Function](../examples/Google.Cloud.Functions.Examples.AdvancedDependencyInjection/Function.cs)
for an example of using scoped and singleton services within a single function. Each function invocation uses
a new instance of its scoped dependency, but uses the same instance of the singleton dependency.

> **Note**
>
> While the pattern is the same as the one used in Azure Functions, these are separate types, to avoid
> any dependency from the Functions Framework onto types that are designed for Azure.
> Currently it is not possible to use a single startup class from both Azure Functions and the Functions Framework,
> as the class can't derive from both `FunctionsStartup` classes. In the future, it is possible that
> a provider-agnostic solution will be made available.

See [Configuration.Function](../examples/Google.Cloud.Functions.Examples.Configuration/Function.cs)
for an example of using the ASP.NET Core `IConfiguration` interface during service configuration.
This example uses three settings files:
[appsettings.json](../examples/Google.Cloud.Functions.Examples.Configuration/appsettings.json),
[appsettings.Development.json](../examples/Google.Cloud.Functions.Examples.Configuration/appsettings.Development.json)
and [appsettings.Production.json](../examples/Google.Cloud.Functions.Examples.Configuration/appsettings.Production.json).
The configuration is loaded in the normal way for .NET Core, based on the environment which is determined
by the `ASPNETCORE_ENVIRONMENT` and `DOTNET_ENVIRONMENT` environment variables.

> **Note**
>
> When using configuration settings like this, please be aware that the default environment in
> ASP.NET Core is Production. You may wish to set the `ASPNETCORE_ENVIRONMENT` or `DOTNET_ENVIRONMENT`
> environment variables on a user level, to avoid accidentally attempting to run against the production
> configuration.
