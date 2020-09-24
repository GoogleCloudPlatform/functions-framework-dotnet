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

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Google.Cloud.Functions.Testing
{
    /// <summary>
    /// Builder type for <see cref="FunctionTestServer"/>, allowing for fine-tuning of the test server.
    /// </summary>
    public class FunctionTestServerBuilder
    {
        /// <summary>
        /// The function type to be executed by the server.
        /// </summary>
        public Type FunctionTarget { get; }

        private IEnumerable<FunctionsStartup>? _startups;
        private bool _consoleLogging;

        private FunctionTestServerBuilder(Type functionTarget) =>
            FunctionTarget = functionTarget;

        /// <summary>
        /// Creates a builder for the function type specified by <typeparamref name="TFunction"/>.
        /// </summary>
        /// <typeparam name="TFunction">The function type to be executed by the server.</typeparam>
        /// <returns>A test server builder for the given function type.</returns>
        public static FunctionTestServerBuilder Create<TFunction>() => new FunctionTestServerBuilder(typeof(TFunction));

        /// <summary>
        /// Creates a builder for the specified function type.
        /// </summary>
        /// <param name="targetFunction">The function type to be executed by the server.</param>
        /// <returns>A test server builder for the given function type.</returns>
        public static FunctionTestServerBuilder Create(Type targetFunction) => new FunctionTestServerBuilder(targetFunction);

        /// <summary>
        /// Specifies the startup classes to use in the test server. By default, the regular Functions Framework
        /// behavior of checking the assembly containing function target for <see cref="FunctionsStartupAttribute"/>
        /// is used. This method is designed to allow test-specific configuration instead. If you call this method
        /// multiple times, only the last-provided value is used.
        /// </summary>
        /// <param name="startups">The startup classes to use, or <c>null</c> to use ones specified in the
        /// assembly containing the function target.</param>
        /// <returns>The same test server builder, for method chaining.</returns>
        public FunctionTestServerBuilder UseFunctionsStartups(IEnumerable<FunctionsStartup>? startups)
        {
            _startups = startups;
            return this;
        }

        /// <summary>
        /// Specifies the startup classes to use in the test server.  This is a convenience method for calling
        /// <see cref="UseFunctionsStartups(IEnumerable{FunctionsStartup})"/> with a parameter array.
        /// </summary>
        /// <param name="startups">The startup classes to use, or <c>null</c> to use ones specified in the
        /// assembly containing the function target.</param>
        /// <returns>The same test server builder, for method chaining.</returns>
        public FunctionTestServerBuilder UseFunctionsStartups(params FunctionsStartup[] startups) =>
            UseFunctionsStartups((IEnumerable<FunctionsStartup>) startups);

        /// <summary>
        /// Specifies whether or not to use Functions Framework console logging in addition to the
        /// in-memory logging for test purposes. By default, this is disabled (so only the in-memory logging is present).
        /// </summary>
        /// <param name="enabled">True to enable Functions Framework console logging; False to disable it.</param>
        /// <returns>The same test server builder, for method chaining.</returns>
        public FunctionTestServerBuilder UseFunctionsFrameworkConsoleLogging(bool enabled)
        {
            _consoleLogging = enabled;
            return this;
        }

        /// <summary>
        /// Builds a <see cref="FunctionTestServer"/> representing the configuration of this builder.
        /// </summary>
        /// <returns>A <see cref="FunctionTestServer"/> using the configuration of this builder.</returns>
        public FunctionTestServer Build() => new FunctionTestServer(BuildTestServer(), FunctionTarget);

        internal TestServer BuildTestServer()
        {
            var loggerProvider = new MemoryLoggerProvider();
            var host = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webHostBuilder =>
                {
                    // Unconditional configuration
                    webHostBuilder
                        .UseTestServer()
                        .ConfigureLogging(logging => logging.ClearProviders().AddProvider(loggerProvider))
                        .ConfigureServices(services => services.AddSingleton(loggerProvider).AddFunctionTarget(FunctionTarget));

                    // Configuration based on builder state.
                    if (_startups is null)
                    {
                        webHostBuilder.UseFunctionsStartups(FunctionTarget.Assembly);
                    }
                    else
                    {
                        foreach (var startup in _startups)
                        {
                            webHostBuilder.UseFunctionsStartup(startup);
                        }
                    }

                    if (_consoleLogging)
                    {
                        webHostBuilder.ConfigureLogging((context, logging) => logging.AddFunctionsConsoleLogging(context));
                    }

                    // When everything else is done, always configure the application to use the Functions Framework.
                    webHostBuilder.Configure((context, app) => app.UseFunctionsFramework(context));
                })
                .Build();
            host.Start();
            return host.GetTestServer();
        }
    }
}
