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

using Google.Cloud.Functions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;

// Namespace by convention
namespace Microsoft.AspNetCore.Hosting
{
    /// <summary>
    /// Extensions for configuring a WebHostBuilder for the Functions Framework.
    /// </summary>
    public static class FunctionsFrameworkWebHostBuilderExtensions
    {
        /// <summary>
        /// Performs all the necessary configuration on an <see cref="IWebHostBuilder"/> so that it can uses the Functions
        /// Framework. Most users will not need to call this method directly, but it allows for additional customization
        /// of the host builder.
        /// </summary>
        /// <param name="builder">The web host builder to configure.</param>
        /// <param name="functionAssembly">The assembly containing the function.</param>
        /// <param name="args">The command line arguments.</param>
        /// <returns>The original builder, for method chaining.</returns>
        public static IWebHostBuilder ConfigureFunctionsFramework(this IWebHostBuilder builder, Assembly functionAssembly, string[] args)
        {
            // Guess the function type by creating a configuration with just the environment variables and command line
            // arguments in it. We do this so we can work out the function startup classes to use - and then validate that
            // when we've used those functions startups and worked out the actual function target, the set of
            // function startups is still valid. Note that it's possible for this to return null, if an assembly-specified
            // function startup determines the actual function target. That's valid, so long as the function target doesn't
            // later require any specific startups. (It would be very, very rare for a startup to affect which function is
            // used, but I can imagine some scenarios where it's useful.)
            var expectedFunctionTarget = GuessFunctionTarget();

            var ret = builder.ConfigureAppConfiguration(builder => builder.AddFunctionsEnvironment().AddFunctionsCommandLine(args))
                .ConfigureLogging((context, logging) => logging.ClearProviders().AddFunctionsConsoleLogging(context))
                .ConfigureKestrelForFunctionsFramework()
                .ConfigureServices((context, services) => services.AddFunctionTarget(context, functionAssembly))
                .UseFunctionsStartups(functionAssembly, expectedFunctionTarget);
            ret = ret.Configure((context, app) => app.UseFunctionsFramework(context, validateStartups: true));
            return ret;

            Type? GuessFunctionTarget()
            {
                var configuration = new ConfigurationBuilder()
                    .AddFunctionsEnvironment()
                    .AddFunctionsCommandLine(args)
                    .Build();
                return HostingInternals.TryGetFunctionTarget(configuration, functionAssembly);
            }
        }

        /// <summary>
        /// Configures Kestrel to listen to the port and address specified in the Functions Framework configuration.
        /// </summary>
        /// <param name="builder">The web host builder to configure.</param>
        /// <returns>The original builder, for method chaining.</returns>
        public static IWebHostBuilder ConfigureKestrelForFunctionsFramework(this IWebHostBuilder builder) =>
            builder.ConfigureKestrel((context, serverOptions) =>
            {
                var options = FunctionsFrameworkOptions.FromConfiguration(context.Configuration);
                serverOptions.Listen(options.GetIPAddress(), options.Port);
            });

        /// <summary>
        /// Uses the functions startup classes specified by <see cref="FunctionsStartupAttribute"/> when configuring the host.
        /// Startup classes can contribute to logging, configuration sources, services, and application configuration.
        /// </summary>
        /// <param name="webHostBuilder">The web host builder to configure.</param>
        /// <param name="assembly">The assembly to query for attributes specifying startup classes.</param>
        /// <param name="functionType">The function type to query for attributes specifying startup classes, or null to skip this query.</param>
        /// <returns>The original builder, for method chaining.</returns>
        public static IWebHostBuilder UseFunctionsStartups(this IWebHostBuilder webHostBuilder, Assembly assembly, Type? functionType)
        {
            foreach (var startupClass in FunctionsStartupAttribute.GetStartupTypes(assembly, functionType))
            {
                var startup = (FunctionsStartup) Activator.CreateInstance(startupClass)!;
                webHostBuilder.UseFunctionsStartup(startup);
            }
            return webHostBuilder;
        }

        /// <summary>
        /// Adds the given startup class into the service collection. This method can be called multiple times, and all startup
        /// classes will be used.
        /// Startup classes can contribute to logging, configuration sources, services, and application configuration.
        /// </summary>
        /// <param name="webHostBuilder">The web host builder to configure.</param>
        /// <param name="startup">The startup class to use.</param>
        /// <returns>The original builder, for method chaining.</returns>
        public static IWebHostBuilder UseFunctionsStartup(
            this IWebHostBuilder webHostBuilder, FunctionsStartup startup) =>
            HostingInternals.AddStartup(webHostBuilder, startup);
    }
}
