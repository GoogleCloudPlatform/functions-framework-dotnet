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
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Xunit;

namespace Google.Cloud.Functions.Examples.IntegrationTests
{
    /// <summary>
    /// Example of a unit test using a MemoryLogger. Often integration tests are
    /// the simplest form of testing for functions, but MemoryLogger allows log entries
    /// to be tested in unit tests by implementing ILogger.
    /// </summary>
    public class SimpleDependencyInjectionUnitTest
    {
        [Fact]
        public async Task LogEntryIsRecorded()
        {
            var logger = new MemoryLogger<SimpleDependencyInjection.Function>();
            var function = new SimpleDependencyInjection.Function(logger);

            // Constructing the function does not create any log entries.
            Assert.Empty(logger.ListLogEntries());

            // Make a request to the function.
            var context = new DefaultHttpContext
            {
                Request = { Path = "/sample-path" }
            };
            await function.HandleAsync(context);

            var logs = logger.ListLogEntries();
            var entry = Assert.Single(logs);

            Assert.Equal(LogLevel.Information, entry.Level);
            Assert.Equal("Function called with path /sample-path", entry.Message);
        }
    }
}
