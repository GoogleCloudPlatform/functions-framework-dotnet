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
using Google.Cloud.Functions.Hosting;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Reflection;

// Namespace by convention
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extensions for adding services to the Functions Framework.
    /// </summary>
    public static class FunctionsFrameworkServiceCollectionExtensions
    {
        /// <summary>
        /// Adds required services to the service collection within the given web host builder for the Functions Framework
        /// to use a target function from the given assembly. If the Functions Framework configuration within the web host
        /// builder (typically provided by command line arguments or environment variables)
        /// does not specify a target function, the assembly is scanned for a single compatible function type.
        /// </summary>
        /// <remarks>
        /// If this method completes successfully, a binding for <see cref="IHttpFunction"/> will definitely
        /// have been added to the service collection. Other bindings may also be present, in order to adapt
        /// the function to <see cref="IHttpFunction"/>.
        /// </remarks>
        /// <param name="services">The service collection to configure.</param>
        /// <param name="context">The context of the web host builder being configured.</param>
        /// <param name="assembly">The assembly expected to contain the Functions Framework target function.</param>
        /// <returns>The original builder, for method chaining.</returns>
        public static IServiceCollection AddFunctionTarget(this IServiceCollection services, WebHostBuilderContext context, Assembly assembly) =>
            HostingInternals.AddServicesForFunctionTarget(services, HostingInternals.GetFunctionTarget(context, assembly));

        /// <summary>
        /// Adds services required for the Functions Framework to use the function target type specified by the
        /// <typeparamref name="TFunction"/> type parameter.
        /// </summary>
        /// <remarks>
        /// If this method completes successfully, a binding for <see cref="IHttpFunction"/> will definitely
        /// have been added to the service collection. Other bindings may also be present, in order to adapt
        /// the function to <see cref="IHttpFunction"/>.
        /// </remarks>
        /// <typeparam name="TFunction">The function target type.</typeparam>
        /// <param name="services">The service collection to configure.</param>
        /// <returns>The original service collection, for method chaining.</returns>
        public static IServiceCollection AddFunctionTarget<TFunction>(this IServiceCollection services) =>
            services.AddFunctionTarget(typeof(TFunction));

        /// <summary>
        /// Adds services required for the Functions Framework to use the specified function target type, which must
        /// implement one of the Functions Framework function interfaces. 
        /// </summary>
        /// <remarks>
        /// If this method completes successfully, a binding for <see cref="IHttpFunction"/> will definitely
        /// have been added to the service collection. Other bindings may also be present, in order to adapt
        /// the function to <see cref="IHttpFunction"/>.
        /// </remarks>
        /// <param name="services">The service collection to configure.</param>
        /// <param name="type">The target function type.</param>
        /// <returns>The original service collection, for method chaining.</returns>
        public static IServiceCollection AddFunctionTarget(this IServiceCollection services, Type type) =>
            HostingInternals.AddServicesForFunctionTarget(services, type);
    }
}
