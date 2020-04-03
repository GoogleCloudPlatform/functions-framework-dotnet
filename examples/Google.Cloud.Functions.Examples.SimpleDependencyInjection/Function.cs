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
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Google.Cloud.Functions.Examples.SimpleDependencyInjection
{
    /// <summary>
    /// Simple example of dependency injection, with no additional configuration.
    /// Loggers are provided out-of-the-box by ASP.NET Core, which means you can inject a logger
    /// into your function constructor.
    /// </summary>
    public class Function : IHttpFunction
    {
        private readonly ILogger<Function> _logger;

        public Function(ILogger<Function> logger) =>
            _logger = logger;

        public async Task HandleAsync(HttpContext context)
        {
            _logger.LogInformation("Function called with path {path}", context.Request.Path);
            await context.Response.WriteAsync("Written the request path to the logger provided by dependency injection.");
        }
    }
}
