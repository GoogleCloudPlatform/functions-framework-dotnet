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

namespace Google.Cloud.Functions.Invoker.DependencyInjection
{
    // Note: this follows the exact pattern that Azure Functions uses. If instead we created
    // an interface, then a function application could use a single startup class targeting
    // both platforms. They can't do that with this approach, because no single class can derive
    // from both the Azure abstract class ant this abstract class. It's easy enough to adapt
    // one to another, admittedly.

    /// <summary>
    /// Abstract class used for additional configuration. Users should derive from
    /// this within their function assembly (potentially multiple times) and apply
    /// <see cref="FunctionsStartupAttribute"/> to the assembly to specify which types
    /// to use for configuration.
    /// </summary>
    public abstract class FunctionsStartup
    {
        /// <summary>
        /// Performs configuration for the given <see cref="IFunctionsHostBuilder"/>.
        /// This method will be called during server startup.
        /// </summary>
        /// <param name="builder">The builder to configure.</param>
        public abstract void Configure(IFunctionsHostBuilder builder);
    }
}
