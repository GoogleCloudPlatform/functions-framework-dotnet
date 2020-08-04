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
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

// Namespace by convention
namespace Microsoft.AspNetCore.Hosting
{
    /// <summary>
    /// Extensions for configuring an ApplicationBuilder for the Functions Framework.
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Configures the given application builder to use the Functions Framework.
        /// This method executes the <see cref="FunctionsStartup.Configure(WebHostBuilderContext, AspNetCore.Builder.IApplicationBuilder)"/>
        /// method on any registered functions startup classes before adding handlers that
        /// return "not found" responses for fixed paths (e.g. "favicon.ico") and setting the terminal
        /// handler to execute the target function.
        /// </summary>
        /// <remarks>
        /// This method requires (at a minimum) that a function target has been registered,
        /// as it uses the <see cref="IHttpFunction"/> interface to handle requests.
        /// The target is typically registered using the
        /// <see cref="FunctionsFrameworkServiceCollectionExtensions.AddFunctionTarget(IServiceCollection, WebHostBuilderContext, System.Reflection.Assembly)"/>
        /// method or another <c>AddFunctionTarget</c> overload.
        /// </remarks>
        /// <param name="app">The application builder to configure.</param>
        /// <param name="context">The context of the web host builder being configured.</param>
        /// <returns>The original builder, for method chaining.</returns>
        public static IApplicationBuilder UseFunctionsFramework(this IApplicationBuilder app, WebHostBuilderContext context) =>
            HostingInternals.ConfigureApplication(context, app);
    }
}
