// Copyright 2024, Google LLC
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
using Google.Cloud.Functions.Testing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Xunit;

namespace Google.Cloud.Functions.Hosting.Tests.Logging;

public class LoggingFunction : IHttpFunction
{
    private readonly ILogger _logger;

    public LoggingFunction(ILogger<LoggingFunction> logger) =>
        _logger = logger;

    public Task HandleAsync(HttpContext context)
    {
        _logger.LogInformation("Test log entry");
        return Task.CompletedTask;
    }
}

public class LoggingIntegrationTest : FunctionTestBase<LoggingFunction>
{
    // Note: this test doesn't test JsonConsoleLogger and SimpleConsoleLogger directly;
    // it's testing MemoryLoggerProvider's handling of scope providers - but that's basically
    // the same code as in FactoryLoggerProvider, so it helps to build confidence.
    // (The others have been tested manually.)
    [Fact]
    public async Task LogEntryHasScopes()
    {
        await ExecuteHttpGetRequestAsync();
        var logEntries = GetFunctionLogEntries();
        var entry = Assert.Single(logEntries);
        Assert.Equal("Test log entry", entry.Message);
        // Kestrel adds two scopes: one of SpanId/TraceId/ParentId, one of RequestPath/RequestId.
        Assert.Equal(2, entry.Scopes.Count);
    }
}
