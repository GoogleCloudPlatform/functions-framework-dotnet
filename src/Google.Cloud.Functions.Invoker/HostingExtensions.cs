using Google.Cloud.Functions.Invoker.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)
namespace Google.Cloud.Functions.Invoker
{
    // Note: this file is intended to be very simple; see HostingInternals for the implementation details.

    /// <summary>
    /// Extension methods for configuring hosting for the Functions Framework.
    /// </summary>
    public static class HostingExtensions
    {
        /// <summary>
        /// Adds a Functions Framework configuration provider based on environment variables (e.g. PORT and FUNCTION_TARGET).
        /// </summary>
        /// <param name="configBuilder">The configuration builder to add the provider to.</param>
        /// <returns>The original builder, for method chaining.</returns>
        public static IConfigurationBuilder AddFunctionsFrameworkEnvironment(this IConfigurationBuilder configBuilder) =>
            configBuilder.Add(new FunctionsEnvironmentVariablesConfigurationProvider());

        /// <summary>
        /// Adds Functions Framework configuration based on command line arguments.
        /// </summary>
        /// <param name="configBuilder">The configuration builder to add the provider to.</param>
        /// <param name="args">The command line arguments to use for configuration.</param>
        /// <returns>The original builder, for method chaining.</returns>
        public static IConfigurationBuilder AddFunctionsFrameworkCommandLine(this IConfigurationBuilder configBuilder, string[] args)
        {
            HostingInternals.AddCommandLineArguments(configBuilder, args);
            return configBuilder;
        }

        /// <summary>
        /// Configures Functions Framework logging for the given logging builder. This is a convenience method to
        /// clear all existing providers and then add add a Functions Framework console logger, as per
        /// <see cref="AddFunctionsFrameworkConsoleLogging(ILoggingBuilder, WebHostBuilderContext)"/>.
        /// </summary>
        /// <param name="builder">The logging builder to configure.</param>
        /// <returns>The original builder, for method chaining.</returns>
        public static ILoggingBuilder AddFunctionsFrameworkLogging(this ILoggingBuilder builder, WebHostBuilderContext context) =>
            builder.ClearProviders().AddFunctionsFrameworkConsoleLogging(context);

        /// <summary>
        /// Adds a Functions Framework console logger, either using a "single line per log entry" plain text format or a JSON format,
        /// depending on the execution environment.
        /// </summary>
        /// <param name="builder">The logging builder to add the logger to.</param>
        /// <returns>The original builder, for method chaining.</returns>
        public static ILoggingBuilder AddFunctionsFrameworkConsoleLogging(this ILoggingBuilder builder, WebHostBuilderContext context)
        {
            var options = FunctionsFrameworkOptions.FromContext(context);
            ILoggerProvider provider = options.JsonLogging
                ? new FactoryLoggerProvider(category => new JsonConsoleLogger(category))
                : new FactoryLoggerProvider(category => new SimpleConsoleLogger(category));
            builder.AddProvider(provider);
            return builder;
        }

        /// <summary>
        /// Configures Kestrel to listen to the port and address specified in the Functions Framework configuration.
        /// </summary>
        /// <param name="builder">The web host builder to configure.</param>
        /// <returns>The original builder, for method chaining.</returns>
        public static IWebHostBuilder ConfigureKestrelForFunctionsFramework(this IWebHostBuilder builder) =>
            builder.ConfigureKestrel((context, serverOptions) =>
            {
                var options = FunctionsFrameworkOptions.FromContext(context);
                serverOptions.Listen(options.GetIPAddress(), options.Port);
            });

        /// <summary>
        /// Adds the services for the Functions Framework to use
        /// a target function from the given assembly. If the Functions Framework configuration (typically provided by
        /// command line arguments or environment variables) does not specify a target function, the assembly is
        /// scanned for a single compatible function type.
        /// </summary>
        /// <param name="services">The service collection to add the service to.</param>
        /// <param name="assembly">The assembly expected to contain the Functions Framework target function.</param>
        /// <returns>The original service collection, for method chaining.</returns>
        public static IServiceCollection AddFunctionsFrameworkTarget(this IServiceCollection services, WebHostBuilderContext context, Assembly assembly)
        {
            HostingInternals.AddServicesForTarget(services, HostingInternals.GetFunctionTarget(context, assembly));
            return services;
        }

        /// <summary>
        /// Adds the services for the Functions Framework to use
        /// the target function type specified by the <typeparamref name="TFunction"/> type parameter.
        /// </summary>
        /// <typeparam name="TFunction">The target function type.</typeparam>
        /// <param name="services">The service collection to add the service to.</param>
        /// <returns>The original service collection, for method chaining.</returns>
        public static IServiceCollection AddFunctionsFrameworkTarget<TFunction>(this IServiceCollection services) =>
            services.AddFunctionsFrameworkTarget(typeof(TFunction));

        /// <summary>
        /// Adds the services for the Functions Framework to use
        /// the specified target function type.
        /// </summary>
        /// <param name="services">The service collection to add the service to.</param>
        /// <param name="type">The target function type.</param>
        /// <returns>The original service collection, for method chaining.</returns>
        public static IServiceCollection AddFunctionsFrameworkTarget(this IServiceCollection services, Type type)
        {
            HostingInternals.AddServicesForTarget(services, type);
            return services;
        }

        /// <summary>
        /// Adds the services specified by
        /// assembly attributes.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <param name="assembly">The assembly to query for attributes specifying service providers.</param>
        /// <returns>The original service collection, for method chaining.</returns>
        public static IServiceCollection AddFunctionsUserServices(this IServiceCollection services, WebHostBuilderContext context, Assembly assembly) =>
            services.AddFunctionsUserServices(context, HostingInternals.GetServiceProviders(assembly));

        /// <summary>
        /// Adds the specified services.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <param name="providers">The services to add.</param>
        /// <returns>The original service collection, for method chaining.</returns>
        public static IServiceCollection AddFunctionsUserServices(this IServiceCollection services, WebHostBuilderContext context, IEnumerable<FunctionsServiceProvider> providers)
        {
            HostingInternals.ConfigureServiceProviders(context, services, providers);
            return services;
        }

        /// <summary>
        /// Adds application configurers services specified by
        /// assembly attributes. The application configurers are then executed by
        /// <see cref="UseFunctionsFramework(IApplicationBuilder, WebHostBuilderContext)"/>.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <param name="assembly">The assembly to query for attributes specifying application configurers.</param>
        /// <returns>The original service collection, for method chaining.</returns>
        public static IServiceCollection AddFunctionsApplicationConfigurers(this IServiceCollection services, WebHostBuilderContext context, Assembly assembly) =>
            services.AddFunctionsApplicationConfigurers(context, HostingInternals.GetApplicationConfigurers(assembly));

        /// <summary>
        /// Adds the specified application configurers services.
        /// The application configurers are then executed by <see cref="UseFunctionsFramework(IApplicationBuilder, WebHostBuilderContext)"/>.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <param name="configurers">The application configurers.</param>
        /// <returns>The original service collection, for method chaining.</returns>
        public static IServiceCollection AddFunctionsApplicationConfigurers(this IServiceCollection services, WebHostBuilderContext context, IEnumerable<FunctionsApplicationConfigurer> configurers)
        {
            HostingInternals.ConfigureApplicationConfigurers(context, services, configurers);
            return services;
        }

        /// <summary>
        /// Configures the given app builder to use the Functions Framework.
        /// This method executes any application configurers before adding handlers for fixed paths to
        /// return "not found" responses for fixed paths (e.g. "favicon.ico") and setting the terminal
        /// handler to execute the target function.
        /// </summary>
        /// <param name="app">The application builder to configure.</param>
        /// <returns>The original builder, for method chaining.</returns>
        public static IApplicationBuilder UseFunctionsFramework(this IApplicationBuilder app, WebHostBuilderContext context)
        {
            HostingInternals.ConfigureApplication(context, app);
            return app;
        }

        /// <summary>
        /// Helper to configure Functions Framework with default settings.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="functionAssembly"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IWebHostBuilder UseFunctionsFramework(this IWebHostBuilder builder, Assembly functionAssembly, string[] args)
        {
            builder.ConfigureAppConfiguration(configBuilder =>
            {
                configBuilder.AddFunctionsFrameworkEnvironment();
                configBuilder.AddFunctionsFrameworkCommandLine(args);
            });
            builder.ConfigureKestrelForFunctionsFramework();
            builder.ConfigureLogging((context, loggingBuilder) => loggingBuilder.AddFunctionsFrameworkLogging(context));
            builder.ConfigureServices((context, services) =>
            {
                services.AddFunctionsFrameworkTarget(context, functionAssembly);
                services.AddFunctionsUserServices(context, functionAssembly);
                services.AddFunctionsApplicationConfigurers(context, functionAssembly);
            });
            builder.Configure((context, app) => app.UseFunctionsFramework(context));

            return builder;
        }
    }
}
