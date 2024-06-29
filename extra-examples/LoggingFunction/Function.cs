// Copyright 2021, Google LLC
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
using System;
using System.IO;
using System.Threading.Tasks;

namespace LoggingFunction
{
    public class Function : IHttpFunction
    {
        private readonly ILogger _logger;

        public Function(ILogger<Function> logger) => _logger = logger;

        public async Task HandleAsync(HttpContext context)
        {
            _logger.LogInformation("Start of request");
            var request = context.Request;
            _logger.LogInformation("HTTP method: {method}", request.Method);
            _logger.LogInformation("Headers:");
            foreach (var header in request.Headers)
            {
                _logger.LogInformation("{key}: {values}", header.Key, string.Join(",", header.Value));
            }            
            using var reader = new StreamReader(request.Body);
            string text = await reader.ReadToEndAsync();
            _logger.LogInformation("Body:");
            _logger.LogInformation("{body}", text);
            _logger.LogInformation("End of request");
        }
    }
}
