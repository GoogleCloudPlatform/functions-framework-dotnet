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
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Google.Cloud.Functions.Invoker.Testing
{
    /// <summary>
    /// A test logger that accumulates logs in memory.
    /// </summary>
    internal sealed class TestLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, ConcurrentQueue<TestLogEntry>> _logsByCategory =
            new ConcurrentDictionary<string, ConcurrentQueue<TestLogEntry>>();

        public ILogger CreateLogger(string categoryName) => new TestLogger(
            categoryName,
            _logsByCategory.GetOrAdd(categoryName, _ => new ConcurrentQueue<TestLogEntry>()));

        internal void Clear() => _logsByCategory.Clear();

        /// <summary>
        /// Returns a list of log entries for the given category name. If no logs have been
        /// written for the given category, an empty list is returned.
        /// </summary>
        /// <param name="categoryName">The category name for which to get log entries.</param>
        /// <returns>A list of log entries for the given category name.</returns>
        internal List<TestLogEntry> GetLogEntries(string categoryName) =>
            _logsByCategory.TryGetValue(categoryName, out var entries)
            ? entries.ToList() : new List<TestLogEntry>();

        public void Dispose()
        {
            // No-op
        }

        private class TestLogger : ILogger
        {
            private readonly string _categoryName;
            private readonly ConcurrentQueue<TestLogEntry> _logEntries;

            internal TestLogger(string categoryName, ConcurrentQueue<TestLogEntry> logEntries) =>
                (_categoryName, _logEntries) = (categoryName, logEntries);

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) =>
                _logEntries.Enqueue(TestLogEntry.Create(_categoryName, logLevel, eventId, state, exception, formatter));

            // We don't really support scopes
            public IDisposable BeginScope<TState>(TState state) => SingletonDisposable.Instance;

            // Note: log level filtering is handled by other logging infrastructure, so we don't do any of it here.
            public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

            // Used for scope handling.
            private class SingletonDisposable : IDisposable
            {
                internal static readonly SingletonDisposable Instance = new SingletonDisposable();
                private SingletonDisposable() { }
                public void Dispose() { }
            }
        }
    }
}
