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

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Google.Cloud.Functions.Hosting
{
    /// <summary>
    /// Base class for startup classes providing additional configuration for functions
    /// in the Functions Framework, typically configuring services, but also with the ability
    /// to use additional application configuration sources or alternative logging providers.
    /// These types are discovered using <see cref="FunctionsStartupAttribute"/>,
    /// which also allows ordering to be specified.
    /// </summary>
    public abstract class FunctionsStartup
    {
        /// <summary>
        /// Configures additional services for the Functions Framework.
        /// The default implementation of this method does nothing.
        /// </summary>
        /// <param name="context">The context for the web host being built.</param>
        /// <param name="services">The service collection to configure.</param>
        public virtual void ConfigureServices(WebHostBuilderContext context, IServiceCollection services)
        {
        }

        /// <summary>
        /// Configures the application, typically to add middleware into the pipeline.
        /// This method is called at the start of pipeline configuration; after this has been called on
        /// all startup classes, the Functions Framework adds the mapping for prohibited paths (e.g. "favicon.ico")
        /// and the pipeline termination which calls the function target.
        /// The default implementation of this method does nothing.
        /// </summary>
        /// <param name="context">The context for the web host being built.</param>
        /// <param name="app">The application builder.</param>
        public virtual void Configure(WebHostBuilderContext context, IApplicationBuilder app)
        {
        }

        /// <summary>
        /// Configures the application configuration, typically by adding another configuration source.
        /// </summary>
        /// <param name="context">The context for the web host being built.</param>
        /// <param name="configuration">The configuration builder.</param>
        public virtual void ConfigureAppConfiguration(WebHostBuilderContext context, IConfigurationBuilder configuration)
        {
        }

        /// <summary>
        /// Configures the application logging, typically by adding another logging provider.
        /// </summary>
        /// <param name="context">The context for the web host being built.</param>
        /// <param name="logging">The configuration builder.</param>
        public virtual void ConfigureLogging(WebHostBuilderContext context, ILoggingBuilder logging)
        {
        }
    }
}
