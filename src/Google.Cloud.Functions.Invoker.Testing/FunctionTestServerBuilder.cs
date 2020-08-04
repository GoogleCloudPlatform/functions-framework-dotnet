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
using System.Collections;
using System.Collections.Generic;

namespace Google.Cloud.Functions.Invoker.Testing
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

        private IEnumerable<FunctionsServiceProvider>? _serviceProviders;
        private IEnumerable<FunctionsApplicationConfigurer>? _applicationConfigurers;
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
        /// Creates a builder for the specified function type. This is a convenience method for calling
        /// <see cref="UseServiceProviders(IEnumerable{FunctionsServiceProvider})"/> with a parameter array.
        /// </summary>
        /// <param name="targetFunction">The function type to be executed by the server.</param>
        /// <returns>A test server builder for the given function type.</returns>
        public static FunctionTestServerBuilder Create(Type targetFunction) => new FunctionTestServerBuilder(targetFunction);

        /// <summary>
        /// Specifies the service providers to use in the test server. By default, the regular Functions Framework
        /// behavior of checking the assembly containing function target for <see cref="FunctionsServiceProviderAttribute"/>
        /// is used. This method is designed to allow test-specific configuration instead.
        /// </summary>
        /// <param name="serviceProviders">The service providers to use, or
        /// <c>null</c> to use ones specified in the assembly containing the function target.</param>
        /// <returns>The same test server builder, for method chaining.</returns>
        public FunctionTestServerBuilder UseServiceProviders(IEnumerable<FunctionsServiceProvider> serviceProviders)
        {
            _serviceProviders = serviceProviders;
            return this;
        }

        /// <summary>
        /// Specifies the service providers to use in the test server. 
        /// </summary>
        /// <param name="serviceProviders">The service providers to use, or
        /// <c>null</c> to use ones specified in the assembly containing the function target.</param>
        /// <returns>The same test server builder, for method chaining.</returns>
        public FunctionTestServerBuilder UseServiceProviders(params FunctionsServiceProvider[] serviceProviders) =>
            UseServiceProviders((IEnumerable<FunctionsServiceProvider>) serviceProviders);

        /// <summary>
        /// Specifies the application configurers to use in the test server. By default, the regular Functions Framework
        /// behavior of checking the assembly containing function target for <see cref="FunctionsApplicationConfigurerAttribute"/>
        /// is used. This method is designed to allow test-specific configuration instead.
        /// </summary>
        /// <param name="applicationConfigurers">The application configurers to use, or
        /// <c>null</c> to use ones specified in the assembly containing the function target.</param>
        /// <returns>The same test server builder, for method chaining.</returns>
        public FunctionTestServerBuilder UseApplicationConfigurers(IEnumerable<FunctionsApplicationConfigurer> applicationConfigurers)
        {
            _applicationConfigurers = applicationConfigurers;
            return this;
        }

        /// <summary>
        /// Specifies the application configurers to use in the test server. This is a convenience method for calling
        /// <see cref="UseApplicationConfigurers(IEnumerable{FunctionsApplicationConfigurer})"/> with a parameter array.
        /// </summary>
        /// <param name="applicationConfigurers">The application configurers to use, or <c>null</c> to use ones specified in the function target assembly</param>
        /// <returns>The same test server builder, for method chaining.</returns>
        public FunctionTestServerBuilder UseApplicationConfigurers(params FunctionsApplicationConfigurer[] applicationConfigurers) =>
            UseApplicationConfigurers((IEnumerable<FunctionsApplicationConfigurer>) applicationConfigurers);

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
                        .ConfigureServices(services => services.AddSingleton(loggerProvider))
                        .ConfigureFunctionsFrameworkTarget(FunctionTarget);

                    // Configuration based on builder state.
                    if (_serviceProviders is null)
                    {
                        webHostBuilder.ConfigureFunctionsServiceProviders(FunctionTarget.Assembly);
                    }
                    else
                    {
                        webHostBuilder.ConfigureFunctionsServiceProviders(_serviceProviders);
                    }
                    if (_applicationConfigurers is null)
                    {
                        webHostBuilder.ConfigureFunctionsApplicationConfigurers(FunctionTarget.Assembly);
                    }
                    else
                    {
                        webHostBuilder.ConfigureFunctionsApplicationConfigurers(_applicationConfigurers);
                    }
                    if (_consoleLogging)
                    {
                        webHostBuilder.ConfigureLogging((context, logging) => logging.AddFunctionsFrameworkConsoleLogging(context));
                    }

                    // When everything else is done, always configure the application to use the Functions Framework.
                    webHostBuilder.ConfigureApplicationForFunctionsFramework();
                })
                .Build();
            host.Start();
            return host.GetTestServer();
        }
    }
}
