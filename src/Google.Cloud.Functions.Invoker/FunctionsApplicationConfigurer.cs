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

namespace Google.Cloud.Functions.Invoker
{
    /// <summary>
    /// Configures the final Functions Framework application, allowing for the configuration of
    /// middleware to be executed before function invocation in the request pipeline.
    /// These types are discovered using <see cref="FunctionsApplicationConfigurerAttribute"/>,
    /// which also allows ordering to be specified.
    /// </summary>
    public abstract class FunctionsApplicationConfigurer
    {
        /// <summary>
        /// Configures the application.
        /// </summary>
        /// <param name="context">The context for the web host being built.</param>
        /// <param name="app">The application builder.</param>
        public virtual void Configure(WebHostBuilderContext context, IApplicationBuilder app)
        {
        }
    }
}
