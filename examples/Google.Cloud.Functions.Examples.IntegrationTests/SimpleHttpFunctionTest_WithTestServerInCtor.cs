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

using Google.Cloud.Functions.Testing;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Google.Cloud.Functions.Examples.IntegrationTests
{
    /// <summary>
    /// Simple example of an integration test creating a <see cref="FunctionTestServer{TFunction}"/>
    /// in the constructor. This uses a different test server for each test, but makes the setup common
    /// across all tests.
    /// </summary>
    public class SimpleHttpFunctionTest_WithTestServerInCtor : IDisposable
    {
        // The function test server created in the constructor.
        private readonly FunctionTestServer<SimpleHttpFunction.Function> _server;

        public SimpleHttpFunctionTest_WithTestServerInCtor() =>
            _server = new FunctionTestServer<SimpleHttpFunction.Function>();

        // Dispose of the function test server, which in turn disposes of the underlying test server.
        // xUnit calls this automatically when a test is complete.
        public void Dispose() => _server.Dispose();

        [Fact]
        public async Task FunctionWritesHelloFunctionsFramework()
        {
            var client = _server.CreateClient();

            // Make a request to the function, and test that the response looks how we expect it to.
            var response = await client.GetAsync("request-uri");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal("Hello, Functions Framework.", content);
        }
    }
}
