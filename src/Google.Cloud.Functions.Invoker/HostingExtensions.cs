using Google.Cloud.Functions.Invoker.Logging;
using Microsoft.AspNetCore.Hosting;
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
        /// Adds Functions Framework configuration based on environment variables (e.g. PORT and FUNCTION_TARGET).
        /// </summary>
        /// <param name="builder">The web host builder to configure.</param>
        /// <returns>The original builder, for method chaining.</returns>
        public static IWebHostBuilder AddFunctionsFrameworkEnvironmentConfiguration(this IWebHostBuilder builder) =>
            builder.ConfigureAppConfiguration(configBuilder => configBuilder.Add(new FunctionsEnvironmentVariablesConfigurationSource()));

        /// <summary>
        /// Adds Functions Framework configuration based on command line arguments.
        /// </summary>
        /// <param name="builder">The web host builder to configure.</param>
        /// <param name="args">The command line arguments to use for configuration.</param>
        /// <returns>The original builder, for method chaining.</returns>
        public static IWebHostBuilder AddFunctionsFrameworkCommandLineConfiguration(this IWebHostBuilder builder, string[] args) =>
            builder.ConfigureAppConfiguration(configBuilder => HostingInternals.AddCommandLineArguments(configBuilder, args));

        /// <summary>
        /// Configures Functions Framework logging for the given web host builder. This is a convenience method
        /// for calling <see cref="ConfigureFunctionsFrameworkLogging(ILoggingBuilder, WebHostBuilderContext)"/>.
        /// </summary>
        /// <param name="builder">The web host builder to configure.</param>
        /// <returns>The original builder, for method chaining.</returns>
        public static IWebHostBuilder ConfigureFunctionsFrameworkLogging(this IWebHostBuilder builder) =>
            builder.ConfigureLogging((context, logging) => logging.ConfigureFunctionsFrameworkLogging(context));

        /// <summary>
        /// Configures Functions Framework logging for the given logging builder. This is a convenience method to
        /// clear all existing providers and then add add a Functions Framework console logger, as per
        /// <see cref="AddFunctionsFrameworkConsoleLogging(ILoggingBuilder, WebHostBuilderContext)"/>.
        /// </summary>
        /// <param name="builder">The logging builder to configure.</param>
        /// <returns>The original builder, for method chaining.</returns>
        public static ILoggingBuilder ConfigureFunctionsFrameworkLogging(this ILoggingBuilder builder, WebHostBuilderContext context) =>
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
        /// Configures the services within the given web host builder for the Functions Framework to use
        /// a target function from the given assembly. If the Functions Framework configuration (typically provided by
        /// command line arguments or environment variables) does not specify a target function, the assembly is
        /// scanned for a single compatible function type.
        /// </summary>
        /// <param name="builder">The web host builder to configure.</param>
        /// <param name="assembly">The assembly expected to contain the Functions Framework target function.</param>
        /// <returns>The original builder, for method chaining.</returns>
        public static IWebHostBuilder ConfigureFunctionsFrameworkTarget(this IWebHostBuilder builder, Assembly assembly) =>
            builder.ConfigureServices((context, serviceCollection) =>
                HostingInternals.AddServicesForTarget(serviceCollection, HostingInternals.GetFunctionTarget(context, assembly)));

        /// <summary>
        /// Configures the services within the given web host builder for the Functions Framework to use
        /// the target function type specified by the <typeparamref name="TFunction"/> type parameter.
        /// </summary>
        /// <typeparam name="TFunction">The target function type.</typeparam>
        /// <param name="builder">The web host builder to configure.</param>
        /// <returns>The original builder, for method chaining.</returns>
        public static IWebHostBuilder ConfigureFunctionsFrameworkTarget<TFunction>(this IWebHostBuilder builder) =>
            builder.ConfigureFunctionsFrameworkTarget(typeof(TFunction));

        /// <summary>
        /// Configures the services within the given web host builder for the Functions Framework to use
        /// the specified target function type.
        /// </summary>
        /// <param name="builder">The web host builder to configure.</param>
        /// <param name="type">The target function type.</param>
        /// <returns>The original builder, for method chaining.</returns>
        public static IWebHostBuilder ConfigureFunctionsFrameworkTarget(this IWebHostBuilder builder, Type type) =>
            builder.ConfigureServices(services => HostingInternals.AddServicesForTarget(services, type));

        /// <summary>
        /// Configures the services within the given web host builder using service providers specified by
        /// assembly attributes.
        /// </summary>
        /// <param name="builder">The web host builder to configure.</param>
        /// <param name="assembly">The assembly to query for attributes specifying service providers.</param>
        /// <returns>The original builder, for method chaining.</returns>
        public static IWebHostBuilder ConfigureFunctionsServiceProviders(this IWebHostBuilder builder, Assembly assembly) =>
            builder.ConfigureFunctionsServiceProviders(HostingInternals.GetServiceProviders(assembly));

        /// <summary>
        /// Configures the services within the given web host builder using the specified service providers.
        /// </summary>
        /// <param name="builder">The web host builder to configure.</param>
        /// <param name="providers">The providers to configure.</param>
        /// <returns>The original builder, for method chaining.</returns>
        public static IWebHostBuilder ConfigureFunctionsServiceProviders(this IWebHostBuilder builder, IEnumerable<FunctionsServiceProvider> providers) =>
            builder.ConfigureServices((context, services) => HostingInternals.ConfigureServiceProviders(context, services, providers));

        /// <summary>
        /// Configures the given web host builder to use application configurers specified by
        /// assembly attributes. The application configurers are then executed by
        /// <see cref="ConfigureApplicationForFunctionsFramework(IWebHostBuilder)"/>.
        /// </summary>
        /// <param name="builder">The web host builder to configure.</param>
        /// <param name="assembly">The assembly to query for attributes specifying application configurers.</param>
        /// <returns>The original builder, for method chaining.</returns>
        public static IWebHostBuilder ConfigureFunctionsApplicationConfigurers(this IWebHostBuilder builder, Assembly assembly) =>
            builder.ConfigureFunctionsApplicationConfigurers(HostingInternals.GetApplicationConfigurers(assembly));

        /// <summary>
        /// Configures the given web host builder to use the specified application configurers.
        /// The application configurers are then executed by <see cref="ConfigureApplicationForFunctionsFramework(IWebHostBuilder)"/>.
        /// </summary>
        /// <param name="builder">The web host builder to configure.</param>
        /// <param name="configurers">The application configurers.</param>
        /// <returns>The original builder, for method chaining.</returns>
        public static IWebHostBuilder ConfigureFunctionsApplicationConfigurers(this IWebHostBuilder builder, IEnumerable<FunctionsApplicationConfigurer> configurers) =>
            builder.ConfigureServices((context, services) => HostingInternals.ConfigureApplicationConfigurers(context, services, configurers));

        /// <summary>
        /// Configures the given web host builder to use the Functions Framework.
        /// This method executes any application configurers before adding handlers for fixed paths to
        /// return "not found" responses for fixed paths (e.g. "favicon.ico") and setting the terminal
        /// handler to execute the target function.
        /// </summary>
        /// <param name="builder">The web host builder to configure.</param>
        /// <returns>The original builder, for method chaining.</returns>
        public static IWebHostBuilder ConfigureApplicationForFunctionsFramework(this IWebHostBuilder builder) =>
            builder.Configure(HostingInternals.ConfigureApplication);
    }
}
