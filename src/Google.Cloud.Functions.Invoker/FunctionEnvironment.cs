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
using Google.Cloud.Functions.Invoker.DependencyInjection;
using Google.Cloud.Functions.Invoker.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace Google.Cloud.Functions.Invoker
{
    /// <summary>
    /// Everything we need to know about a function we're trying to serve - but without
    /// serving it yet. The intention of the class is to make the "start a server" code trivial
    /// when it's presented with an instance of FunctionEnvironment, but for a FunctionEnvironment
    /// to be testable without starting a server.
    /// </summary>
    internal sealed class FunctionEnvironment
    {
        /// <summary>
        /// The service configuration to apply to the IServiceCollection during
        /// configuration. This is expected to provide an implementation for IHttpFunction.
        /// </summary>
        private readonly Action<IServiceCollection> _serviceConfiguration;

        /// <summary>
        /// The target function type.
        /// </summary>
        public Type FunctionType { get; }

        /// <summary>
        /// The IP Address to listen on.
        /// </summary>
        public IPAddress Address { get; }

        /// <summary>
        /// The port to listen on.
        /// </summary>
        public int Port { get; }

        /// <summary>
        /// The logger provider to use in the web server.
        /// </summary>
        public ILoggerProvider LoggerProvider { get; }

        /// <summary>
        /// The FunctionsStartup instances to use for configuring services.
        /// This is currently private because they're just used in <see cref="ConfigureServices(IServiceCollection)"/>;
        /// it could be made internal if we wanted.
        /// </summary>
        private IReadOnlyList<FunctionsStartup> Startups { get; }

        private FunctionEnvironment(Type functionType, Action<IServiceCollection> serviceConfiguration, IPAddress address, int port, ILoggerProvider loggerProvider, IReadOnlyList<FunctionsStartup> startups) =>
            (FunctionType, _serviceConfiguration, Address, Port, LoggerProvider, Startups) =
            (functionType, serviceConfiguration, address, port, loggerProvider, startups);

        internal static FunctionEnvironment Create(Assembly functionAssembly, string[] commandLine, ConfigurationVariableProvider variableProvider) =>
            new Builder(functionAssembly, commandLine, variableProvider).Build();

        /// <summary>
        /// Configures the services for the application by asking each <see cref="FunctionsStartup"/>
        /// that's been detected to contribute services, along with any we need in order to run the function.
        /// </summary>
        internal void ConfigureServices(IServiceCollection services)
        {
            // Perform any user-specified configuration.
            var builder = new FunctionsHostBuilder(services);
            foreach (var startup in Startups)
            {
                startup.Configure(builder);
            }
            // Then perform the configuration to make sure that IHttpFunction is provided.
            _serviceConfiguration(services);
        }

        /// <summary>
        /// Creates a host builder, including configuring kestrel in terms of address and port.
        /// If the builder is actually going to be used for testing instead, the kestrel configuration will
        /// be irrelevant by the time the host starts.
        /// </summary>
        /// <returns>The configured host builder.</returns>
        internal IHostBuilder CreateHostBuilder() =>
            Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder => webBuilder
                    .ConfigureLogging(logging =>
                    {
                        logging.ClearProviders();
                        logging.AddProvider(LoggerProvider);
                    })
                    .ConfigureKestrel(serverOptions => serverOptions.Listen(Address, Port))
                    .ConfigureServices(ConfigureServices)
                    .Configure(app =>
                    {
                        app.Map("/robots.txt", ReturnNotFound);
                        app.Map("/favicon.ico", ReturnNotFound);
                        app.Run(Execute);
                        // Note: we can't use ILogger<EntryPoint> as EntryPoint is static. This is an equivalent.
                        app.ApplicationServices
                            .GetRequiredService<ILoggerFactory>()
                            .CreateLogger(typeof(EntryPoint).FullName)
                            .LogInformation($"Serving function {FunctionType.FullName}");
                    }));

        /// <summary>
        /// Executes a request by asking the dependency injection context for an IHttpFunction, then
        /// executing it. Any additional "root" handling (e.g. extra logging, or exception handling).
        /// </summary>
        internal async Task Execute(HttpContext context)
        {
            var function = context.RequestServices.GetRequiredService<IHttpFunction>();
            await function.HandleAsync(context);
        }

        private static void ReturnNotFound(IApplicationBuilder app) =>
            app.Run(context =>
            {
                context.Response.StatusCode = (int) HttpStatusCode.NotFound;
                return Task.CompletedTask;
            });

        /// <summary>
        /// Attempts to find a single valid non-abstract function class within the given set of types.
        /// (Note that "non-abstract class" is a pretty low bar; we could enforce non-generic etc, but we'll
        /// discover problems there easily enough anyway.)
        /// </summary>
        /// <remarks>
        /// This method is internal and in this class rather than being private and in Builder for the sake of testability.
        /// </remarks>
        /// <param name="types">The types to search through.</param>
        /// <returns>The function type to use by default</returns>
        /// <exception cref="ArgumentException">There isn't a single valid function type.</exception>
        internal static Type FindDefaultFunctionType(params Type[] types)
        {
            var validTypes = types.Where(IsFunctionClass).ToList();
            return validTypes.Count switch
            {
                0 => throw new ArgumentException("No valid Cloud Function types found."),
                1 => validTypes[0],
                _ => throw new ArgumentException(
                    $"Multiple Cloud Function types found. Please specify the function to run via the command line or the {EntryPoint.FunctionTargetEnvironmentVariable} environment variable."),
            };

            bool IsFunctionClass(Type t) =>
                t.IsClass && !t.IsAbstract && !t.IsGenericType &&
                (typeof(IHttpFunction).IsAssignableFrom(t) ||
                typeof(ICloudEventFunction).IsAssignableFrom(t) ||
                GetGenericInterfaceImplementationTypeArgument(t, typeof(ICloudEventFunction<>)) is object);
        }

        /// <summary>
        /// Checks whether a given type implements a generic interface (assumed to have a single type parameter),
        /// and returns the type argument if so. For example, if we have "class MyFunction : ICloudEventFunction{PubSubMessage}"
        /// then a call to GetGenericInterfaceImplementationTypeArgument(typeof(MyFunction), typeof(ICloudEventFunction&lt;&gt;))
        /// will return typeof(PubSubMessage).
        /// </summary>
        /// <param name="target">The target type to check.</param>
        /// <param name="genericInterface">The generic interface</param>
        /// <returns></returns>
        private static Type? GetGenericInterfaceImplementationTypeArgument(Type target, Type genericInterface)
        {
            var matches = target.GetInterfaces()
                .Where(iface => iface.IsGenericType && iface.GetGenericTypeDefinition() == genericInterface);
            // We only want to return an unambiguous match. Note that this will evaluate the list twice in the "got one" case,
            // but that's okay.
            return matches.Count() == 1 ? matches.Single().GetGenericArguments()[0] : null;
        }

        private class Builder
        {
            private static readonly IDictionary<string, string> CommandLineArgumentToVariableMapping = new Dictionary<string, string>
            {
                { "target", EntryPoint.FunctionTargetEnvironmentVariable },
                { "port", EntryPoint.PortEnvironmentVariable }
            };

            private readonly Assembly _functionAssembly;
            private readonly ConfigurationVariableProvider _variables;

            internal Builder(Assembly functionAssembly, string[] commandLine, ConfigurationVariableProvider environmentVariables)
            {
                _functionAssembly = functionAssembly;
                // For the convenience of running from the command line, treat a single command line variable as if it's a "--target" value.
                if (commandLine.Length == 1)
                {
                    if (commandLine[0] == "%LAUNCHER_ARGS%")
                    {
                        throw new Exception("Unable to launch Web SDK project with launch settings. Please see https://github.com/GoogleCloudPlatform/functions-framework-dotnet/blob/master/docs/launch-settings.md");
                    }
                    commandLine = new[] { "--target", commandLine[0] };
                }
                var commandLineVariables = ConfigurationVariableProvider.FromCommandLine(commandLine, CommandLineArgumentToVariableMapping);
                _variables = ConfigurationVariableProvider.Combine(commandLineVariables, environmentVariables);
            }

            internal FunctionEnvironment Build()
            {
                Type functionType = DetermineFunctionType();
                var serviceConfiguration = BuildServiceConfiguration(functionType);
                int port = DeterminePort();
                IPAddress address = DetermineAddress();
                ILoggerProvider loggerProvider = DetermineLoggerProvider();
                IReadOnlyList<FunctionsStartup> startups = CreateStartups();
                return new FunctionEnvironment(functionType, serviceConfiguration, address, port, loggerProvider, startups);
            }

            private Type DetermineFunctionType()
            {
                var target = _variables[EntryPoint.FunctionTargetEnvironmentVariable];
                return target is null
                    ? FindDefaultFunctionType(_functionAssembly.GetTypes())
                    : _functionAssembly.GetType(target) ?? throw new Exception($"Can't load specified function type '{target}'");
            }

            private Action<IServiceCollection> BuildServiceConfiguration(Type type)
            {
                if (typeof(IHttpFunction).IsAssignableFrom(type))
                {
                    return services => services.AddScoped(typeof(IHttpFunction), type);
                }
                else if (typeof(ICloudEventFunction).IsAssignableFrom(type))
                {
                    return services => services
                        .AddScoped<IHttpFunction, CloudEventAdapter>()
                        .AddScoped(typeof(ICloudEventFunction), type);
                }
                else if (GetGenericInterfaceImplementationTypeArgument(type, typeof(ICloudEventFunction<>)) is Type payloadType)
                {
                    return services => services
                        .AddScoped(typeof(IHttpFunction), typeof(CloudEventAdapter<>).MakeGenericType(payloadType))
                        .AddScoped(typeof(ICloudEventFunction<>).MakeGenericType(payloadType), type);
                }
                throw new Exception("Function doesn't support known interfaces");
            }

            private int DeterminePort()
            {
                var portVariableOrDefault = _variables[EntryPoint.PortEnvironmentVariable] ?? "8080";
                return int.TryParse(portVariableOrDefault, NumberStyles.None, CultureInfo.InvariantCulture, out var parsed)
                    ? parsed
                    : throw new Exception($"Can't parse port value '{portVariableOrDefault}'");
            }

            // Using just the loopback environment variable avoids Windows Defender asking for permission...
            // but isn't useful in container environments.
            // At some point we might also want to allow this to be configured directly, e.g. via a FUNCTION_ADDRESS environment
            // variable or similar - but we'll wait until the requirement emerges.
            private IPAddress DetermineAddress() =>
                string.Equals(_variables["DOTNET_RUNNING_IN_CONTAINER"], "true", StringComparison.OrdinalIgnoreCase) ? IPAddress.Any : IPAddress.Loopback;

            private ILoggerProvider DetermineLoggerProvider()
            {
                bool useJsonLogging = _variables["K_SERVICE"] is object;
                return useJsonLogging
                    ? new FactoryLoggerProvider(category => new JsonConsoleLogger(category))
                    : new FactoryLoggerProvider(category => new SimpleConsoleLogger(category));
            }

            // TODO: Nicer error handling, e.g. if the type isn't a startup?
            private IReadOnlyList<FunctionsStartup> CreateStartups() =>
                _functionAssembly
                    .GetCustomAttributes<FunctionsStartupAttribute>()
                    .Select(attr => Activator.CreateInstance(attr.StartupType))
                    .Cast<FunctionsStartup>()
                    .ToList()
                    .AsReadOnly();
        }
    }
}
