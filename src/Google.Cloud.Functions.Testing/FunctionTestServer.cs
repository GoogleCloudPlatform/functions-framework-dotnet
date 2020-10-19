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

using Google.Cloud.Functions.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Google.Cloud.Functions.Testing
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

        /// <summary>
        /// The function type executed by the server.
        /// </summary>
        public Type FunctionTarget { get; }

        private readonly MemoryLoggerProvider _testLoggerProvider;

        /// <summary>
        /// Creates a FunctionTestServer for the specified function type.
        /// </summary>
        /// <param name="functionTarget">The function type to host in the server.</param>
        public FunctionTestServer(Type functionTarget) : this(functionTarget, null)
        {
        }

        /// <summary>
        /// Creates a FunctionTestServer for the specified function type, with an optional
        /// type to inspect for <see cref="FunctionsStartupAttribute"/> attributes.
        /// </summary>
        /// <param name="functionTarget">The function type to host in the server.</param>
        /// <param name="attributedStartupType">A type to examine for <see cref="FunctionsStartupAttribute"/> attributes to provide further customization.
        /// If this is null, or no attributes are specified in the type (or its base class hierarchy) or the assembly it's declared in,
        /// the startup classes in the function assembly are used.</param>
        public FunctionTestServer(Type functionTarget, Type? attributedStartupType)
            : this(FunctionTestServerBuilder.Create(functionTarget).MaybeUseFunctionsStartupsFromAttributes(attributedStartupType).BuildTestServer(), functionTarget)
        {
        }

        internal FunctionTestServer(TestServer server, Type functionTarget)
        {
            Server = server;
            FunctionTarget = functionTarget;
            _testLoggerProvider = server.Services.GetRequiredService<MemoryLoggerProvider>();
        }

        /// <summary>
        /// Returns a list of the log entries for the category associated with the function.
        /// </summary>
        /// <returns>A list of log entries. The returned value may refer to an empty list,
        /// but is never null.</returns>
        public List<TestLogEntry> GetFunctionLogEntries() => GetLogEntries(FunctionTarget);

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
            GetLogEntries(LoggerTypeNameHelper.GetCategoryNameForType(Preconditions.CheckNotNull(type, nameof(type))));

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
        public FunctionTestServer() : this(null)
        {
        }

        // Note: it would be nice to make the constructor below public, but that means it can't be used in an
        // xUnit test fixture via constructor dependency injection.

        /// <summary>
        /// Constructs a new instance serving <typeparamref name="TFunction"/>, with an optional
        /// type to inspect for <see cref="FunctionsStartupAttribute"/> attributes.
        /// </summary>
        /// <param name="attributedStartupType">A type to examine for <see cref="FunctionsStartupAttribute"/> attributes to provide further customization.
        /// If this is null, or no attributes are specified in the type (or its base class hierarchy) or the assembly it's declared in,
        /// the startup classes in the function assembly are used.</param>
        internal FunctionTestServer(Type? attributedStartupType) : base(typeof(TFunction), attributedStartupType)
        {
        }
    }
}
