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

using Google.Cloud.Functions.Hosting;
using Google.Cloud.Functions.Hosting.Logging;
using Microsoft.AspNetCore.Hosting;

// Namespace by convention
namespace Microsoft.Extensions.Logging
{
    /// <summary>
    /// Extensions for configuration logging for the Functions Framework.
    /// </summary>
    public static class FunctionsFrameworkLoggingExtensions
    {
        /// <summary>
        /// Adds a Functions Framework console logger, either using a "single line per log entry" plain text format or a JSON format,
        /// depending on the execution environment.
        /// </summary>
        /// <param name="builder">The logging builder to add the logger to.</param>
        /// <param name="context">The context of the web host builder being configured.</param>
        /// <returns>The original builder, for method chaining.</returns>
        public static ILoggingBuilder AddFunctionsConsoleLogging(this ILoggingBuilder builder, WebHostBuilderContext context)
        {
            var options = FunctionsFrameworkOptions.FromConfiguration(context.Configuration);
            ILoggerProvider provider = options.JsonLogging
                ? new FactoryLoggerProvider(category => new JsonConsoleLogger(category))
                : new FactoryLoggerProvider(category => new SimpleConsoleLogger(category));
            builder.AddProvider(provider);
            return builder;
        }
    }
}
