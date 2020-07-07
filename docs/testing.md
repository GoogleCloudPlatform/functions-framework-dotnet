# Testing Functions

In many cases, the unit tests for a function won't need to use the
Functions Framework at all. They can simply create the function
instance directly, specifying appropriate forms of any dependencies, and
invoke the function.

Integration testing is relatively straightforward, based on the
regular ASP.NET Core pattern of creating a test server, then
invoking it with an appropriately-configured `HttpClient`.

## Creating an IHostBuilder with EntryPoint.CreateHostBuilder

The invoker [EntryPoint](../src/Google.Cloud.Functions.Invoker/EntryPoint.cs) class
not only contains the `Main` method used automatically to start the
server, but an overloaded `CreateHostBuilder` method. The generic,
parameterless overload expects the type argument to be a function
type, and creates a suitable `IHostBuilder` with no additional
environment.

A non-generic overload accepting a `Type` parameter is equivalent to
the generic overload, but without needing to know the function type
at compile-time.

Finally, there's a non-generic overload accepting a string-to-string
dictionary of fake environment variables and a function assembly.
This provides more control for scenarios where you may want to
simulate running in a container, or running in a Knative environment.

The `IHostBuilder` can be used in conjunction with the
[Microsoft.AspNetCore.TestHost](https://www.nuget.org/packages/Microsoft.AspNetCore.TestHost)
NuGet package to create a test server and execute requests against it.

See [SimpleHttpFunctionTest](../examples/Google.Cloud.Functions.Examples.IntegrationTests/SimpleHttpFunctionTest.cs)
for an example of this.

## Creating a FunctionTestServer

Using `TestHost` directly can be slightly verbose - it's not too bad
for an occasional test, but not something you'd want to use in a
large number of tests. The
[Google.Cloud.Functions.Invoker.Testing](../src/Google.Cloud.Functions.Invoker.Testing)
package provides a [FunctionTestServer](../src/Google.Cloud.Functions.Invoker.Testing/FunctionTestServer.cs)
class to simplify this. The generic `FunctionTestServer<TFunction>`
class is a derived class allowing you to use the function type as a
type argument, avoiding the need for any other configuration.

`FunctionTestServer` is typically used in one of three ways. The
examples given below are for xUnit, but other test frameworks provide
similar functionality.

- It can be constructed directly within the test. This should be in the context
  of a `using` statement to dispose of the server afterwards. See
  [SimpleHttpFunctionTest_WithTestServerInTest](../examples/Google.Cloud.Functions.Examples.IntegrationTests/SimpleHttpFunctionTest_WithTestServerInTest.cs)
  for an example of this.
- It can be constructed directly in the test class constructor,
  and then disposed in an `IDisposable.Dispose()` implementation, which is
  automatically called by xUnit. This means a new server is constructed
  for each test. See
  [SimpleHttpFunctionTest_WithTestServerInCtor](../examples/Google.Cloud.Functions.Examples.IntegrationTests/SimpleHttpFunctionTest_WithTestServerInCtor.cs)
  for an example of this.
- It can be automatically constructed by xUnit as part of a fixture, then disposed
  of automatically when the fixture is disposed. This means a single server
  is used for all tests in the fixture. See
  [SimpleHttpFunctionTest_WithTestServerFixture](../examples/Google.Cloud.Functions.Examples.IntegrationTests/SimpleHttpFunctionTest_WithTestServerFixture.cs)
  for an example of this.
  
## Testing logs with FunctionTestServer

`FunctionTestServer` augments the default logging with an in-memory
logger provider, allowing you to retrieve logs by category using the
`GetLogEntries` method. See
[SimpleDependencyInjectionTest.cs](../examples/Google.Cloud.Functions.Examples.IntegrationTests/SimpleDependencyInjectionTest.cs)
for an example of how this can be used.

Currently the functionality is somewhat primitive, but should meet
the demands of most users. Aspects that may change later include:

- The logger provider is configured after any other services. If you
  manually configure services during startup, eagerly loading
  singletons, the in-memory logger will not be configured.
- If the test server is constructed as part of a fixture, the logs
  persist between tests. (A `ClearLogs` method allows you to
  explicitly clear the logs if you need to.)
- The logger provider is unconditionally installed, and has an
  unlimited buffer size. This makes the test server inappropriate
  for long-running soak tests, for example. In the future we may
  provide options to disable the in-memory logger, or limit its
  log entry buffer.
- The logger only provides the log message rather than separate
  parts of the log entry that may be available, such as placeholder
  replacement values.
- The logger does not support log scopes; all log entries are stored
  in a flat structure.
