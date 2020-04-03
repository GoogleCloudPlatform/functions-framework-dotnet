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

using Google.Cloud.Functions.Invoker;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using Xunit;

namespace Google.Cloud.Functions.Examples.IntegrationTests
{
    /// <summary>
    /// Simple example of an integration test against a Cloud Function, without using
    /// the Google.Cloud.Functions.Invoker.Testing package.
    /// </summary>
    public class SimpleHttpFunctionTest
    {
        [Fact]
        public async Task FunctionWritesHelloFunctionsFramework()
        {
            var builder = EntryPoint
                .CreateHostBuilder<SimpleHttpFunction.Function>()
                .ConfigureWebHost(builder => builder.UseTestServer());
            using (var server = await builder.StartAsync())
            {
                var client = server.GetTestServer().CreateClient();
                
                // Make a request to the function, and test that the response looks how we expect it to.
                var response = await client.GetAsync("request-uri");
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();

                Assert.Equal("Hello, Functions Framework.", content);
            }
        }
    }
}
