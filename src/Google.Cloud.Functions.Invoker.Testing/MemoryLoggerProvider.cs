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

using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Google.Cloud.Functions.Invoker.Testing
{
    /// <summary>
    /// A logger provider that creates instances of <see cref="MemoryLogger"/>.
    /// </summary>
    internal sealed class MemoryLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, MemoryLogger> _loggersByCategory =
            new ConcurrentDictionary<string, MemoryLogger>();

        public ILogger CreateLogger(string categoryName) =>
            _loggersByCategory.GetOrAdd(categoryName, name => new MemoryLogger(name));

        internal void Clear() => _loggersByCategory.Clear();

        /// <summary>
        /// Returns a list of log entries for the given category name. If no logs have been
        /// written for the given category, an empty list is returned.
        /// </summary>
        /// <param name="categoryName">The category name for which to get log entries.</param>
        /// <returns>A list of log entries for the given category name.</returns>
        internal List<TestLogEntry> GetLogEntries(string categoryName) =>
            _loggersByCategory.TryGetValue(categoryName, out var logger)
            ? logger.ListLogEntries() : new List<TestLogEntry>();

        public void Dispose()
        {
            // No-op
        }
    }
}
