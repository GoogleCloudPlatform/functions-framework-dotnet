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

using Google.Cloud.Functions.Framework;
using Google.Cloud.Functions.Invoker;
using Google.Cloud.Functions.Invoker.Testing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

[assembly: FunctionsStartup(typeof(Google.Cloud.Functions.Invoker.Tests.FunctionsStartupTest.TestStartup1), Order = 3)]
[assembly: FunctionsStartup(typeof(Google.Cloud.Functions.Invoker.Tests.FunctionsStartupTest.TestStartup2), Order = 2)]

namespace Google.Cloud.Functions.Hosting.Tests
{
    /// <summary>
    /// Tests for everything to do with FunctionsStartup.
    /// </summary>
    public partial class FunctionsStartupTest
    {
        // Startups used for assembly attribute detection.
        public class TestStartup1 : FunctionsStartup { }
        public class TestStartup2 : FunctionsStartup { }

        // This is the only test that uses the assembly attribute detection.
        // Everything else specifies the startups explicitly.
        [Fact]
        public void GetStartups()
        {
            var startups = HostingInternals.GetStartups(typeof(FunctionsStartupTest).Assembly);
            var actualTypes = startups.Select(startup => startup.GetType()).ToArray();
            // TestStartup2 comes before TestStartup1 due to the Order properties.
            var expectedTypes = new[] { typeof(TestStartup2), typeof(TestStartup1) };
            Assert.Equal(expectedTypes, actualTypes);
        }

        /// <summary>
        /// This is just an overall test of how things hang together.
        /// </summary>
        [Fact]
        public async Task DependencyScopingIntegration()
        {
            // We create a test server, but we bypass most of it - we're really just using it to create
            // services we can execute an HttpContext against. (This is simpler than interacting via
            // an actual HTTP request and response.)
            var host = Host.CreateDefaultBuilder()
                .ConfigureWebHost(webHostBuilder => webHostBuilder
                    .ConfigureServices((context, services) => services.AddFunctionTarget<DependencyTestFunction>())
                    .UseFunctionsStartup(new DependencyStartup())
                    .Configure((context, app) => app.UseFunctionsFramework(context))
                    .UseTestServer())
                .Build();
            var testServer = host.GetTestServer();

            // Execute two requests, and remember the dependencies we saw in each of them.
            var request1Deps = await MakeRequestAsync();
            var request2Deps = await MakeRequestAsync();

            // We should get dependencies on every call
            Assert.NotNull(request1Deps.dep1);
            Assert.NotNull(request1Deps.dep2);
            Assert.NotNull(request2Deps.dep1);
            Assert.NotNull(request2Deps.dep2);

            // Dependency1 is registered as a singleton, so we should have the same value in both requests.
            Assert.Same(request1Deps.dep1, request2Deps.dep1);
            // Dependency2 is registered as a scoped service, so we should have different values in each request.
            Assert.NotSame(request1Deps.dep2, request2Deps.dep2);

            async Task<(object dep1, object dep2)> MakeRequestAsync()
            {
                using (var scope = testServer.Services.CreateScope())
                {
                    var context = new DefaultHttpContext { RequestServices = scope.ServiceProvider };
                    await HostingInternals.Execute(context);
                    return (context.Items[DependencyTestFunction.Dependency1Key], context.Items[DependencyTestFunction.Dependency2Key]);
                }
            }
        }

        public class DependencyTestFunction : IHttpFunction
        {
            public const string Dependency1Key = "Dependency1";
            public const string Dependency2Key = "Dependency2";

            private readonly IDependency1 _dep1;
            private readonly IDependency2 _dep2;

            public DependencyTestFunction(IDependency1 dep1, IDependency2 dep2) =>
                (_dep1, _dep2) = (dep1, dep2);

            public Task HandleAsync(HttpContext context)
            {
                context.Items[Dependency1Key] = _dep1;
                context.Items[Dependency2Key] = _dep2;
                return Task.CompletedTask;
            }
        }

        public class DependencyStartup : FunctionsStartup
        {
            public override void ConfigureServices(WebHostBuilderContext context, IServiceCollection services) =>
                services
                    .AddSingleton<IDependency1, Dependency1>()
                    .AddScoped<IDependency2, Dependency2>();
        }


        public interface IDependency1 { }
        public class Dependency1 : IDependency1 { }
        public interface IDependency2 { }
        public class Dependency2 : IDependency2 { }

        [Theory]
        [InlineData("/pass", HttpStatusCode.OK)]
        [InlineData("/fail", HttpStatusCode.BadRequest)]
        public async Task MiddlewareIntegration(string path, HttpStatusCode expectedStatusCode)
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureWebHost(webHostBuilder => webHostBuilder
                    .ConfigureServices((context, services) => services.AddFunctionTarget<MiddlewareTestFunction>())
                    .UseFunctionsStartup(new MiddlewareStartup())
                    .Configure((context, app) => app.UseFunctionsFramework(context))
                    .UseTestServer())
                .Build();
            await host.StartAsync();

            using var testServer = host.GetTestServer();
            using var client = testServer.CreateClient();
            using var response = await client.GetAsync(path);
            Assert.Equal(expectedStatusCode, response.StatusCode);
        }

        public class MiddlewareTestFunction : IHttpFunction
        {
            public async Task HandleAsync(HttpContext context) =>
                await context.Response.WriteAsync("Pass");
        }

        public class MiddlewareStartup : FunctionsStartup
        {
            public override void Configure(WebHostBuilderContext context, IApplicationBuilder app) =>
                app.UseMiddleware<FailPathMiddleware>();
        }

        public class FailPathMiddleware
        {
            private readonly RequestDelegate _next;

            public FailPathMiddleware(RequestDelegate next)
            {
                _next = next;
            }

            public async Task InvokeAsync(HttpContext context)
            {
                if (context.Request.Path == "/fail")
                {
                    context.Response.StatusCode = (int) HttpStatusCode.BadRequest;
                    return;
                }

                // Call the next delegate/middleware in the pipeline
                await _next(context);
            }
        }

        [Fact]
        public async Task StartupAppConfiguration()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureWebHost(webHostBuilder => webHostBuilder
                    .ConfigureServices((context, services) => services.AddFunctionTarget<ConfigTestFunction>())
                    .UseFunctionsStartup(new ConfigTestStartup())
                    .Configure((context, app) => app.UseFunctionsFramework(context))
                    .UseTestServer())
                .Build();
            await host.StartAsync();

            using var testServer = host.GetTestServer();
            using var client = testServer.CreateClient();
            using var response = await client.GetAsync("path");
            var text = await response.Content.ReadAsStringAsync();
            Assert.Equal("TestValue", text);
        }

        public class ConfigTestStartup : FunctionsStartup
        {
            public override void ConfigureAppConfiguration(WebHostBuilderContext context, IConfigurationBuilder configuration) =>
                configuration.AddInMemoryCollection(new[] { KeyValuePair.Create("TestKey", "TestValue") });
        }

        public class ConfigTestFunction : IHttpFunction
        {
            private readonly IConfiguration _config;

            public ConfigTestFunction(IConfiguration config) =>
                _config = config;

            public async Task HandleAsync(HttpContext context) =>
                await context.Response.WriteAsync(_config.GetValue<string>("TestKey"));
        }

        [Fact]
        public async Task StartupLoggingConfiguration()
        {
            var loggerProvider = new MemoryLoggerProvider();
            var host = Host.CreateDefaultBuilder()
                .ConfigureWebHost(webHostBuilder => webHostBuilder
                    .ConfigureServices((context, services) => services.AddFunctionTarget<LoggingTestFunction>())
                    .UseFunctionsStartup(new LoggingTestStartup(loggerProvider))
                    .Configure((context, app) => app.UseFunctionsFramework(context))
                    .UseTestServer())
                .Build();
            await host.StartAsync();

            using var testServer = host.GetTestServer();
            using var client = testServer.CreateClient();
            using var response = await client.GetAsync("path");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var categoryName = LoggerTypeNameHelper.GetCategoryNameForType<LoggingTestFunction>();
            var logEntry = Assert.Single(loggerProvider.GetLogEntries(categoryName));
            Assert.Equal("Test", logEntry.Message);
        }

        public class LoggingTestStartup : FunctionsStartup
        {
            private ILoggerProvider _provider;

            public LoggingTestStartup(ILoggerProvider provider) =>
                _provider = provider;

            public override void ConfigureLogging(WebHostBuilderContext context, ILoggingBuilder logging) =>
                logging.AddProvider(_provider);
        }

        public class LoggingTestFunction : IHttpFunction
        {
            private readonly ILogger _logger;

            public LoggingTestFunction(ILogger<LoggingTestFunction> logger) =>
                _logger = logger;

            public Task HandleAsync(HttpContext context)
            {
                _logger.LogInformation("Test");
                return Task.CompletedTask;
            }
        }
    }
}
