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

using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;

namespace Google.Cloud.Functions.Invoker.Testing
{
    /// <summary>
    /// A test server to simplify in-memory testing.
    /// </summary>
    public class FunctionTestServer : IDisposable
    {
        /// <summary>
        /// The underlying <see cref="TestServer"/> hosting the function.
        /// </summary>
        public TestServer Server { get; }

        private readonly MemoryLoggerProvider _testLoggerProvider;

        private FunctionTestServer(IHostBuilder builder)
        {
            _testLoggerProvider = new MemoryLoggerProvider();
            var host = builder
                .ConfigureWebHost(webBuilder => webBuilder.UseTestServer())
                .ConfigureLogging(logging => logging.AddProvider(_testLoggerProvider))
                .Build();
            host.Start();
            Server = host.GetTestServer();
        }

        /// <summary>
        /// Creates a FunctionTestServer for the specified environment and function assembly.
        /// </summary>
        /// <param name="environment">The fake environment variables to use when constructing the server.</param>
        /// <param name="functionAssembly">The assembly containing the target function.</param>
        public FunctionTestServer(IReadOnlyDictionary<string, string> environment, Assembly functionAssembly)
            : this(EntryPoint.CreateHostBuilder(environment, Preconditions.CheckNotNull(functionAssembly, nameof(functionAssembly))))
        {
        }

        /// <summary>
        /// Creates a FunctionTestServer for the specified function type.
        /// </summary>
        /// <param name="functionType">The function type to host in the server.</param>
        public FunctionTestServer(Type functionType)
            : this(EntryPoint.CreateHostBuilder(functionType))
        {
        }

        /// <summary>
        /// Creates an <see cref="HttpClient"/> to make requests to the test server.
        /// </summary>
        /// <returns>An <see cref="HttpClient"/> that will connect to <see cref="Server"/> by default.</returns>
        public HttpClient CreateClient() => Server.CreateClient();

        /// <summary>
        /// Clears all logs. This does not affect any loggers that have already been created, but causes
        /// new loggers to be created for future requests.
        /// </summary>
        public void ClearLogs() => _testLoggerProvider.Clear();

        /// <summary>
        /// Returns a list of log entries for the category with the given type's full name. If no logs have been
        /// written for the given category, an empty list is returned.
        /// </summary>
        /// <param name="type">The type whose name is used as a logger category name.</param>
        /// <returns>A list of log entries for the given category name.</returns>
        public List<TestLogEntry> GetLogEntries(Type type) =>
            GetLogEntries(Preconditions.CheckNotNull(type, nameof(type)).FullName ?? "");

        /// <summary>
        /// Returns a list of log entries for the given category name. If no logs have been
        /// written for the given category, an empty list is returned.
        /// </summary>
        /// <param name="categoryName">The category name for which to get log entries.</param>
        /// <returns>A list of log entries for the given category name.</returns>
        public List<TestLogEntry> GetLogEntries(string categoryName) =>
            _testLoggerProvider.GetLogEntries(Preconditions.CheckNotNull(categoryName, nameof(categoryName)));
        
        /// <summary>
        /// Disposes of the test server.
        /// </summary>
        public void Dispose() => Server.Dispose();
    }

    /// <summary>
    /// Generic version of <see cref="FunctionTestServer"/>, making it particularly simple to specify as
    /// a test fixture.
    /// </summary>
    /// <typeparam name="TFunction">The function type to host in the server</typeparam>
    public class FunctionTestServer<TFunction> : FunctionTestServer
    {
        /// <summary>
        /// Constructs a new instance serving <typeparamref name="TFunction"/>.
        /// </summary>
        public FunctionTestServer() : base(typeof(TFunction))
        {
        }

        /// <summary>
        /// Returns a list of the log entries for the category associated with the 
        /// </summary>
        /// <returns></returns>
        public List<TestLogEntry> GetFunctionLogEntries() => GetLogEntries(typeof(TFunction));
    }
}
