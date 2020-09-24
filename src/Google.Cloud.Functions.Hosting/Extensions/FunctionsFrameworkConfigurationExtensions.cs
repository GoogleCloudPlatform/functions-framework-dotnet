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
using Google.Cloud.Functions.Invoker;

// Namespace by convention
namespace Microsoft.Extensions.Configuration
{
    /// <summary>
    /// Extensions for adding configuration for the Functions Framework.
    /// </summary>
    public static class FunctionsFrameworkConfigurationExtensions
    {
        /// <summary>
        /// Adds a configuration source for the Functions Framework based on environment variables (e.g. PORT and FUNCTION_TARGET).
        /// </summary>
        /// <param name="builder">The configuration builder to add the source to.</param>
        /// <returns>The original builder, for method chaining.</returns>
        public static IConfigurationBuilder AddFunctionsEnvironment(this IConfigurationBuilder builder) =>
            builder.Add(new FunctionsEnvironmentVariablesConfigurationSource());

        /// <summary>
        /// Adds a configuration source for the Functions Framework based on command line arguments.
        /// </summary>
        /// <param name="builder">The configuration builder to add the source to.</param>
        /// <param name="args">The command line arguments to use for configuration.</param>
        /// <returns>The original builder, for method chaining.</returns>
        public static IConfigurationBuilder AddFunctionsCommandLine(this IConfigurationBuilder builder, string[] args) =>
            HostingInternals.AddCommandLineArguments(builder, args);
    }
}
