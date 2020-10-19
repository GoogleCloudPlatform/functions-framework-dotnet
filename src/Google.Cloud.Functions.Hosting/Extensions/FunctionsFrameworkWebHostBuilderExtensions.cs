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
