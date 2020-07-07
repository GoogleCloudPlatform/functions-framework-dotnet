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

using Google.Cloud.Functions.Invoker.Testing;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Xunit;

namespace Google.Cloud.Functions.Examples.IntegrationTests
{
    public class SimpleDependencyInjectionTest
    {
        [Fact]
        public async Task LogEntryIsRecorded()
        {
            using (var server = new FunctionTestServer<SimpleDependencyInjection.Function>())
            {
                // We shouldn't have any log entries (for the function's category) at the start of the test.
                Assert.Empty(server.GetLogEntries(typeof(SimpleDependencyInjection.Function)));

                var client = server.CreateClient();

                // Make a request to the function.
                var response = await client.GetAsync("sample-path");
                response.EnsureSuccessStatusCode();

                // Check that we got the expected log entry.

                var logs = server.GetLogEntries(typeof(SimpleDependencyInjection.Function));
                var entry = Assert.Single(logs);

                Assert.Equal(LogLevel.Information, entry.Level);
                Assert.Equal("Function called with path /sample-path", entry.Message);
            }
        }
    }
}
