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
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Steeltoe.Extensions.Configuration.RandomValue;
using System.Threading.Tasks;

// FunctionsServiceProviderAttribute is applied to the assembly to tell the Functions Framework which startup class
// to load. If you have multiple startup classes, you can apply the attribute multiple times, optionally
// using the Order property to specify ordering.
// The attribute must be applied to the assembly containing the function, but it can potentially refer
// to a startup class in a different assembly.
[assembly: FunctionsStartup(typeof(Google.Cloud.Functions.Examples.CustomConfiguration.Startup))]

namespace Google.Cloud.Functions.Examples.CustomConfiguration
{

    /// <summary>
    /// The startup class can be used to perform additional configuration, including
    /// adding application configuration sources, reconfiguring logging, providing services
    /// for dependency injection, and adding middleware to the eventual application pipeline.
    /// In this case, we add the "random value" provider from Steeltoe.
    /// See https://steeltoe.io/docs/2/configuration/random-value-provider for more details.
    /// </summary>
    public class Startup : FunctionsStartup
    {
        public override void ConfigureAppConfiguration(WebHostBuilderContext context, IConfigurationBuilder configuration) =>
            configuration.AddRandomValueSource();
    }

    public class Function : IHttpFunction
    {
        private readonly IConfiguration _configuration;

        public Function(IConfiguration configuration) =>
            _configuration = configuration;

        public async Task HandleAsync(HttpContext context)
        {
            int randomValue = _configuration.GetValue<int>("random:int");
            await context.Response.WriteAsync($"Here's a random integer: {randomValue}");
        }
    }
}
