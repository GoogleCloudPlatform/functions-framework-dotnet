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
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace Google.Cloud.Functions.Invoker
{
    public static class EntryPoint
    {
        public static async Task<int> StartAsync(Assembly functionAssembly, string[] args)
        {            
            RequestDelegate handler = BuildHandler(functionAssembly, args);
            int port = DeterminePort(args);
            var builder = Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => webBuilder
                    .ConfigureKestrel(serverOptions => serverOptions.Listen(IPAddress.Any, port))
                    .Configure(app => app.Run(handler))
                );
            await builder.Build().RunAsync();
            return 0;
        }

        private static RequestDelegate BuildHandler(Assembly functionAssembly, string[] args)
        {
            // TODO: Better command line parsing, e.g. --target=xyz --port=5000
            var target = args.FirstOrDefault() ?? Environment.GetEnvironmentVariable("FUNCTION_NAME");
            if (string.IsNullOrEmpty(target))
            {
                throw new Exception("No target provided");
            }
            var type = functionAssembly.GetType(target);
            if (type is null)
            {
                throw new Exception($"Can't load target type '{target}'");
            }
            var instance = Activator.CreateInstance(type);
            return instance switch
            {
                IHttpFunction function => context => function.HandleAsync(context),
                _ => throw new Exception("Function doesn't support known interfaces")
            };
        }

        private static int DeterminePort(string[] args)
        {
            var environmentVariable = Environment.GetEnvironmentVariable("PORT");
            if (!string.IsNullOrEmpty(environmentVariable))
            {
                if (!int.TryParse(environmentVariable, NumberStyles.None, CultureInfo.InvariantCulture, out var parsed))
                {
                    throw new Exception($"Can't part PORT environment variable '{environmentVariable}'");
                }
                return parsed;
            }
            // TODO: Parse args
            return 8080;
        }
    }
}
