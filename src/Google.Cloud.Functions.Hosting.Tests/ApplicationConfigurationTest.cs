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
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Google.Cloud.Functions.Hosting.Tests
{
    /// <summary>
    /// Tests for the final part of configuring the "application".
    /// </summary>
    public partial class ApplicationConfigurationTest
    {
        [Theory]
        [InlineData("robots.txt", HttpStatusCode.NotFound, "")]
        [InlineData("favicon.ico", HttpStatusCode.NotFound, "")]
        [InlineData("foo.txt", HttpStatusCode.OK, "Test message")]
        public async Task PathHandling(string path, HttpStatusCode expectedStatus, string expectedText)
        {
            var builder = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webHostBuilder => webHostBuilder
                    .ConfigureServices(services => services.AddFunctionTarget<SimpleFunction>())
                    .Configure((context, app) => app.UseFunctionsFramework(context))
                    .UseTestServer());
            using var server = await builder.StartAsync();
            using var client = server.GetTestServer().CreateClient();
            using var response = await client.GetAsync(path);
            Assert.Equal(expectedStatus, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal(expectedText, content);
        }

        public class SimpleFunction : IHttpFunction
        {
            public async Task HandleAsync(HttpContext context)
            {
                await context.Response.WriteAsync("Test message");
            }
        }
    }
}
