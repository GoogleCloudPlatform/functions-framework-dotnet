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

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Google.Cloud.Functions.Invoker
{
    /// <summary>
    /// Configures services within the Functions Framework. These types are discovered
    /// using <see cref="FunctionsServiceProviderAttribute"/>, which also allows ordering to
    /// be specified.
    /// </summary>
    public abstract class FunctionsServiceProvider
    {
        // Note: this is a virtual method rather than abstract so that we could potentially
        // provide alternatives, e.g. just ConfigureServices(IServiceCollection), and
        // let users override whichever one they want.

        /// <summary>
        /// Configures additional services for the Functions Framework.
        /// </summary>
        /// <param name="context">The context for the web host being built.</param>
        /// <param name="services">The service collection to configure.</param>
        public virtual void ConfigureServices(WebHostBuilderContext context, IServiceCollection services)
        {
        }
    }
}
