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
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Google.Cloud.Functions.Examples.MultipleFunctions
{
    public class Function3Startup : FunctionsStartup
    {
        private static readonly Dictionary<string, string> s_overridingConfig =
            new Dictionary<string, string>
            {
                { "FunctionsFramework:FunctionTarget", "Google.Cloud.Functions.Examples.MultipleFunctions.Function1" }
            };

        public override void ConfigureAppConfiguration(WebHostBuilderContext context, IConfigurationBuilder configuration) =>
            configuration.AddInMemoryCollection(s_overridingConfig);
    }

    /// <summary>
    /// This function provokes the failure detailed in
    /// https://github.com/GoogleCloudPlatform/functions-framework-dotnet/blob/master/docs/customization.md#specifying-a-functions-startup-class
    /// 
    /// It uses a startup which changes the target function type, to a function
    /// which doesn't have that same startup (and needs a different one).
    /// This causes a failure on server startup. If the startup changed the
    /// target function type to another one which used Function3Startup, that would
    /// be valid (but still not recommended, in terms of clarity).
    /// </summary>
    [FunctionsStartup(typeof(Function3Startup))]
    public class Function3 : IHttpFunction
    {
        public async Task HandleAsync(HttpContext context)
        {
            await context.Response.WriteAsync("This response should never be written.");
        }
    }
}
