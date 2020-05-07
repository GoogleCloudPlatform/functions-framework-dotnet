// Copyright 2020, Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using CloudNative.CloudEvents;
using Google.Cloud.Functions.Framework;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Google.Cloud.Functions.Invoker.Tests
{
    // Note: the tests here have hard-coded environment variable names and default values, for readability.
    // If the environment variables or default values are ever changed, these tests will need changing too.
    public class FunctionEnvironmentTest
    {
        private static readonly string DefaultFunctionName = typeof(DefaultFunction).FullName;
        private static readonly string[] DefaultFunctionCommandLine = new[] { DefaultFunctionName };
        // An empty string array, used for both an empty set of command line arguments and an empty set of environment variables.
        private static readonly string[] Empty = new string[0];

        [Fact]
        public void LaunchSettingsGiveInstructions()
        {
            // See https://github.com/GoogleCloudPlatform/functions-framework-dotnet/issues/41 for the background
            var exception = AssertBadEnvironment(new[] { "%LAUNCHER_ARGS%" }, Empty);
            // The exception should contain a message with a link to documentation.
            Assert.Contains("https://github.com", exception.Message);
        }

        [Fact]
        public void Port_DefaultsTo8080()
        {
            var environment = CreateEnvironment(DefaultFunctionCommandLine, Empty);
            Assert.Equal(8080, environment.Port);
        }

        [Fact]
        public void Port_SetViaEnvironmentVariable()
        {
            var environment = CreateEnvironment(DefaultFunctionCommandLine, new[] { "PORT=12345" });
            Assert.Equal(12345, environment.Port);
        }

        [Fact]
        public async Task Port_SetViaCommandLine()
        {
            var environment = CreateEnvironment(new[] { "--port", "12345", "--target", DefaultFunctionName }, Empty);
            Assert.Equal(12345, environment.Port);
            await AssertHttpFunctionType<DefaultFunction>(environment);
        }

        [Fact]
        public async Task Port_CommandLineOverridesEnvironment()
        {
            var environment = CreateEnvironment(new[] { "--port", "12345", "--target", DefaultFunctionName }, new[] { "PORT=98765" });
            Assert.Equal(12345, environment.Port);
            await AssertHttpFunctionType<DefaultFunction>(environment);
        }

        [Fact]
        public void Port_InvalidEnvironmentVariable() =>
            AssertBadEnvironment(DefaultFunctionCommandLine, new[] { "PORT=INVALID" });

        [Fact]
        public async Task TargetFunction_CommandLine_SingleArgument()
        {
            var environment = CreateEnvironment(DefaultFunctionCommandLine, Empty);
            await AssertHttpFunctionType<DefaultFunction>(environment);
        }

        [Fact]
        public async Task TargetFunction_CommandLine_ArgumentWithKey()
        {
            var environment = CreateEnvironment(new[] { "--target", DefaultFunctionName }, Empty);
            await AssertHttpFunctionType<DefaultFunction>(environment);
        }

        [Fact]
        public async Task TargetFunction_EnvironmentVariable()
        {
            var environment = CreateEnvironment(Empty, new[] { $"FUNCTION_TARGET={DefaultFunctionName}" });
            await AssertHttpFunctionType<DefaultFunction>(environment);
        }

        [Fact]
        public async Task TargetFunction_CommandLineOverridesEnvironmentVariable()
        {
            var environment = CreateEnvironment(DefaultFunctionCommandLine, new[] { $"FUNCTION_TARGET=ThisTargetDoesNotExist" });
            await AssertHttpFunctionType<DefaultFunction>(environment);
        }

        // The test will pass because CreateEnvironment throws: the environment is "bad"
        // because there are multiple function types in this assembly.
        [Fact]
        public void TargetFunction_Unspecified() =>
            AssertBadEnvironment(Empty, Empty);

        [Fact]
        public void TargetFunction_NonFunctionType() =>
            AssertBadEnvironment(new[] { typeof(NonFunction).FullName }, Empty);

        [Fact]
        public async Task TargetFunction_NonInstantiableFunctionType()
        {
            var environment = CreateEnvironment(new[] { typeof(NonInstantiableFunction).FullName }, Empty);
            // We only find out when we try to execute a request that we can't actually instantiate the function.
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await ExecuteRequest(environment, new DefaultHttpContext()));
        }

        [Fact]
        public void TargetFunction_MultipleCloudEventTypes() =>
            AssertBadEnvironment(new[] { typeof(MultipleCloudEventTypes).FullName }, Empty);

        [Fact]
        public async Task TargetFunction_EventFunction_NonGeneric()
        {
            var environment = CreateEnvironment(new[] { typeof(EventIdRememberingFunction).FullName }, Empty);
            Assert.Equal(typeof(EventIdRememberingFunction), environment.FunctionType);
            string eventId = Guid.NewGuid().ToString();
            var context = new DefaultHttpContext
            {
                Request =
                {
                    // No actual content, but that's okay.
                    ContentType = "application/json",
                    Headers =
                    {
                        { "ce-specversion", "1.0" },
                        { "ce-source", "test" },
                        { "ce-type", "test" },
                        { "ce-id", eventId }
                    }
                }
            };

            await ExecuteRequest(environment, context);
            Assert.Equal(eventId, EventIdRememberingFunction.LastEventId);
        }

        [Fact]
        public async Task TargetFunction_EventFunction_Generic()
        {
            var environment = CreateEnvironment(new[] { typeof(StringCloudEventFunction).FullName }, Empty);
            Assert.Equal(typeof(StringCloudEventFunction), environment.FunctionType);
            var eventId = Guid.NewGuid().ToString();
            var httpContext = new DefaultHttpContext
            {
                Request =
                {
                    ContentType = "application/json",
                    Body = new MemoryStream(Encoding.UTF8.GetBytes("\"testdata\"")),
                    Headers =
                    {
                        { "ce-specversion", "1.0" },
                        { "ce-source", "test" },
                        { "ce-type", "test" },
                        { "ce-id", eventId }
                    }
                },
            };
            await ExecuteRequest(environment, httpContext);
            Assert.Equal(eventId, StringCloudEventFunction.LastEventId);
            Assert.Equal("testdata", StringCloudEventFunction.LastData);
        }

        [Fact]
        public void FindDefaultFunctionType_NoFunctionTypes() =>
            Assert.Throws<ArgumentException>(() => FunctionEnvironment.FindDefaultFunctionType(
                typeof(FunctionEnvironmentTest),
                typeof(ConfigurationVariableProviderTest),
                typeof(TestHttpFunctionBase), // Abstract, doesn't count
                typeof(INamedHttpFunction)    // Interface, doesn't count
                ));

        [Fact]
        public void FindDefaultFunctionType_MultipleFunctionTypes() =>
            Assert.Throws<ArgumentException>(() => FunctionEnvironment.FindDefaultFunctionType(
                typeof(DefaultFunction),
                typeof(EventIdRememberingFunction)));

        [Fact]
        public void FindDefaultFunctionType_SingleFunctionType()
        {
            var expected = typeof(DefaultFunction);
            var actual = FunctionEnvironment.FindDefaultFunctionType(
                typeof(FunctionEnvironmentTest),
                typeof(ConfigurationVariableProviderTest),
                typeof(TestHttpFunctionBase), // Abstract, doesn't count
                typeof(INamedHttpFunction),    // Interface, doesn't count
                typeof(DefaultFunction));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FindDefaultFunctionType_TypedCloudEventFunction()
        {
            var expected = typeof(StringCloudEventFunction);
            var actual = FunctionEnvironment.FindDefaultFunctionType(typeof(StringCloudEventFunction));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FindDefaultFunctionType_MultipleCloudEventTypesFunction() =>
            Assert.Throws<ArgumentException>(() => FunctionEnvironment.FindDefaultFunctionType(
                typeof(MultipleCloudEventTypes)));

        [Fact]
        public void Address_NoEnvironmentVariable()
        {
            var environment = CreateEnvironment(DefaultFunctionCommandLine, Empty);
            Assert.Equal(IPAddress.Loopback, environment.Address);
        }

        [Theory]
        [InlineData("true")]
        [InlineData("TRUE")]
        [InlineData("True")]
        public void Address_DotnetRunningInContainer_True(string value)
        {
            var environment = CreateEnvironment(DefaultFunctionCommandLine, new[] { $"DOTNET_RUNNING_IN_CONTAINER={value}" });
            Assert.Equal(IPAddress.Any, environment.Address);
        }

        [Fact]
        public void Address_DotnetRunningInContainer_NotTrue()
        {
            var environment = CreateEnvironment(DefaultFunctionCommandLine, new[] { "DOTNET_RUNNING_IN_CONTAINER=not-true" });
            Assert.Equal(IPAddress.Loopback, environment.Address);
        }

        private static Exception AssertBadEnvironment(string[] commandLine, string[] variables) =>
            Assert.ThrowsAny<Exception>(() => CreateEnvironment(commandLine, variables));

        private async Task AssertHttpFunctionType<T>(FunctionEnvironment environment) where T : IHttpFunction
        {
            Assert.Equal(typeof(T), environment.FunctionType);
            var context = new DefaultHttpContext();
            await ExecuteRequest(environment, context);
            Assert.Equal(typeof(T), context.Items[TestHttpFunctionBase.TypeKey]);
        }

        /// <summary>
        /// Creates a FunctionEnvironment using the given command line and environment variables, and this
        /// (test) assembly.
        /// </summary>
        /// <param name="variables">The environment variables to emulator, in "Name=Value" format</param>
        /// <returns>The function environment</returns>
        private static FunctionEnvironment CreateEnvironment(string[] commandLine, string[] variables)
        {
            var variableDictionary = variables.ToDictionary(v => v.Split('=')[0], v => v.Split('=')[1]);
            var provider = ConfigurationVariableProvider.FromDictionary(variableDictionary);
            return FunctionEnvironment.Create(typeof(FunctionEnvironmentTest).Assembly, commandLine, provider);
        }

        public class DefaultFunction : TestHttpFunctionBase
        {
        }

        public class EventIdRememberingFunction : ICloudEventFunction
        {
            /// <summary>
            /// Horrible way of testing which function was actually used: remember the last event ID.
            /// Currently we only have a single test using this; if we have more, we'll need to disable
            /// parallelism.
            /// </summary>
            public static string LastEventId { get; private set; }

            public Task HandleAsync(CloudEvent cloudEvent, CancellationToken cancellationToken)
            {
                LastEventId = cloudEvent.Id;
                return Task.CompletedTask;
            }
        }

        public class StringCloudEventFunction : ICloudEventFunction<string>
        {
            public static string LastEventId { get; set; }
            public static string LastData { get; set; }

            public Task HandleAsync(CloudEvent cloudEvent, string data, CancellationToken cancellationToken)
            {
                LastEventId = cloudEvent.Id;
                LastData = data;
                return Task.CompletedTask;
            }
        }

        public class NonFunction
        {
        }

        public class NonInstantiableFunction : TestHttpFunctionBase
        {
            private NonInstantiableFunction()
            {
            }
        }

        // Just a sample interface extending a function interface.
        public interface INamedHttpFunction : IHttpFunction
        {
            public string Name { get; }
        }

        public class MultipleCloudEventTypes : ICloudEventFunction<string>, ICloudEventFunction<object>
        {
            public Task HandleAsync(CloudEvent cloudEvent, string data, CancellationToken cancellationToken)
                => Task.CompletedTask;
            public Task HandleAsync(CloudEvent cloudEvent, object data, CancellationToken cancellationToken)
                => Task.CompletedTask;
        }

        /// <summary>
        /// Executes the given request in a function environment. The context will be populated
        /// with a default service provider.
        /// </summary>
        private async Task ExecuteRequest(FunctionEnvironment environment, HttpContext context)
        {
            var services = new ServiceCollection();
            // Normally ASP.NET Core provides logging for everything, so it's natural to depend on it.
            // Our adapters may need logging, so let's make sure it's available.
            services.AddSingleton<ILoggerFactory>(new LoggerFactory());
            services.AddLogging();
            environment.ConfigureServices(services);
            var providerFactory = new DefaultServiceProviderFactory();

            var provider = providerFactory.CreateServiceProvider(services);
            using (var scope = provider.CreateScope())
            {
                context.RequestServices = scope.ServiceProvider;
                await environment.Execute(context);
            }
        }
    }
}
