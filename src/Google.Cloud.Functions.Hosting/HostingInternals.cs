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
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace Google.Cloud.Functions.Hosting
{
    internal static class HostingInternals
    {
        private static readonly IDictionary<string, string> CommandLineMappings = new Dictionary<string, string>
        {
            { "--target", $"{FunctionsFrameworkOptions.ConfigSection}:{nameof(FunctionsFrameworkOptions.FunctionTarget)}" },
            { "--port", $"{FunctionsFrameworkOptions.ConfigSection}:{nameof(FunctionsFrameworkOptions.Port)}" },
        };

        internal static IConfigurationBuilder AddCommandLineArguments(IConfigurationBuilder configBuilder, string[] args)
        {
            if (args is null || args.Length == 0)
            {
                return configBuilder;
            }
            if (args.Length == 1)
            {
                if (args[0] == "%LAUNCHER_ARGS%")
                {
                    throw new InvalidOperationException("Unable to launch Web SDK project with launch settings. Please see https://github.com/GoogleCloudPlatform/functions-framework-dotnet/blob/master/docs/launch-settings.md");
                }
                args = new[] { "--target", args[0] };
            }
            configBuilder.AddCommandLine(args, CommandLineMappings);
            return configBuilder;
        }

        internal static IApplicationBuilder ConfigureApplication(WebHostBuilderContext context, IApplicationBuilder app)
        {
            var configurers = app.ApplicationServices.GetServices<FunctionsStartup>();
            foreach (var configurer in configurers)
            {
                configurer.Configure(context, app);
            }

            app.Map("/robots.txt", ReturnNotFound);
            app.Map("/favicon.ico", ReturnNotFound);
            app.Run(Execute);

            // Slight hack using dependency injection to propagate the original function from ConfigureFunctionsFrameworkTarget
            // to here.
            var functionType = app.ApplicationServices.GetRequiredService<FunctionTypeProvider>().FunctionType;
            // Note: we can't use ILogger<EntryPoint> as EntryPoint is static. This is an equivalent.
            app.ApplicationServices
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger(typeof(EntryPoint).FullName)
                .LogInformation($"Serving function {functionType.FullName}");
            return app;

            static void ReturnNotFound(IApplicationBuilder app) =>
                app.Run(context =>
                {
                    context.Response.StatusCode = (int) HttpStatusCode.NotFound;
                    return Task.CompletedTask;
                });
        }

        internal static IWebHostBuilder AddStartup(IWebHostBuilder builder, FunctionsStartup startup)
        {
            builder.ConfigureAppConfiguration(startup.ConfigureAppConfiguration);
            builder.ConfigureLogging(startup.ConfigureLogging);
            builder.ConfigureServices(startup.ConfigureServices);

            // Remember the startup for application configuration time as well.
            builder.ConfigureServices(services => services.AddSingleton(startup));
            return builder;
        }

        /// <summary>
        /// Executes a request by asking the dependency injection context for an IHttpFunction, then
        /// executing it.
        /// </summary>
        internal static async Task Execute(HttpContext context)
        {
            var function = context.RequestServices.GetRequiredService<IHttpFunction>();
            await function.HandleAsync(context);
        }

        internal static Type GetFunctionTarget(IConfiguration configuration, Assembly assembly)
        {
            var options = FunctionsFrameworkOptions.FromConfiguration(configuration);
            string? target = options.FunctionTarget;
            return target is null
                ? FindDefaultFunctionType(assembly.GetTypes())
                : assembly.GetType(target) ?? throw new InvalidOperationException($"Can't load specified function type '{target}'");
        }

        internal static Type? TryGetFunctionTarget(IConfiguration configuration, Assembly assembly)
        {
            var options = FunctionsFrameworkOptions.FromConfiguration(configuration);
            string? target = options.FunctionTarget;
            return target is null
                ? TryFindDefaultFunctionType(assembly.GetTypes())
                : assembly.GetType(target) ?? throw new InvalidOperationException($"Can't load specified function type '{target}'");
        }

        internal static IServiceCollection AddServicesForFunctionTarget(IServiceCollection services, Type functionType)
        {
            // Make sure we can determine the actual function type later on.
            services.AddSingleton(new FunctionTypeProvider(functionType));

            if (typeof(IHttpFunction).IsAssignableFrom(functionType))
            {
                services.AddScoped(typeof(IHttpFunction), functionType);
            }
            else if (typeof(ICloudEventFunction).IsAssignableFrom(functionType))
            {
                services
                    .AddScoped<IHttpFunction, CloudEventAdapter>()
                    .AddScoped(typeof(ICloudEventFunction), functionType);
            }
            else if (GetGenericInterfaceImplementationTypeArgument(functionType, typeof(ICloudEventFunction<>)) is Type payloadType)
            {
                services
                    .AddScoped(typeof(IHttpFunction), typeof(CloudEventAdapter<>).MakeGenericType(payloadType))
                    .AddScoped(typeof(ICloudEventFunction<>).MakeGenericType(payloadType), functionType);
            }
            else
            {
                throw new InvalidOperationException($"Function type '{functionType.FullName}' doesn't support any known function interfaces.");
            }
            return services;
        }

        /// <summary>
        /// Attempts to find a single valid non-abstract function class within the given set of types.
        /// (Note that "non-abstract class" is a pretty low bar; we could enforce non-generic etc, but we'll
        /// discover problems there easily enough anyway.)
        /// </summary>
        /// <remarks>
        /// This method is internal rather private for the sake of testability.
        /// </remarks>
        /// <param name="types">The types to search through.</param>
        /// <returns>The function type to use by default</returns>
        /// <exception cref="ArgumentException">There isn't a single valid function type.</exception>
        internal static Type FindDefaultFunctionType(params Type[] types)
        {
            var validTypes = FindValidFunctionTypes(types);
            return validTypes.Count switch
            {
                0 => throw new ArgumentException("No valid Cloud Function types found."),
                1 => validTypes[0],
                _ => throw new ArgumentException(
                    $"Multiple Cloud Function types found. Please specify the function to run via the command line or the {EntryPoint.FunctionTargetEnvironmentVariable} environment variable."),
            };

        }

        /// <summary>
        /// Attempts to find a single valid non-abstract function class within the given set of types, but returns
        /// null (instead of failing) if there isn't exactly one such class.
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        internal static Type? TryFindDefaultFunctionType(params Type[] types)
        {
            var validTypes = FindValidFunctionTypes(types);
            return validTypes.Count switch
            {
                0 => null,
                1 => validTypes[0],
                _ => null
            };
        }

        /// <summary>
        /// Returns a list of valid function types. Designed to be used by <see cref="FindDefaultFunctionType(Type[])"/>
        /// and <see cref="TryFindDefaultFunctionType(Type[])"/>; think carefully before using in a different context.
        /// </summary>
        private static List<Type> FindValidFunctionTypes(params Type[] types)
        {
            return types.Where(IsFunctionClass).ToList();
            static bool IsFunctionClass(Type t) =>
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

        internal sealed class FunctionTypeProvider
        {
            internal Type FunctionType { get; }

            internal FunctionTypeProvider(Type functionType) =>
                FunctionType = functionType;
        }
    }
}
