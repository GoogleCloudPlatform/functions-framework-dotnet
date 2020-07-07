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

namespace Google.Cloud.Functions.Invoker.Testing
{
    /// <summary>
    /// A very simplified representation of a log entry, aimed at testing.
    /// </summary>
    public sealed class TestLogEntry
    {
        /// <summary>
        /// The category name of the log entry.
        /// </summary>
        public string CategoryName { get; }

        /// <summary>
        /// The level of the log entry.
        /// </summary>
        public LogLevel Level { get; }

        /// <summary>
        /// The exception in the log entry, or null if there was no exception.
        /// </summary>
        public Exception? Exception { get; }

        /// <summary>
        /// The event ID of the log entry.
        /// </summary>
        public EventId EventId { get; }

        /// <summary>
        /// The message of the log entry.
        /// </summary>
        public string Message { get; }

        private TestLogEntry(string categoryName, LogLevel level, EventId eventId, string message, Exception? exception) =>
            (CategoryName, Level, EventId, Message, Exception) =
            (categoryName, level, eventId, message, exception);

        internal static TestLogEntry Create<TState>(string categoryName, LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var message = formatter(state, exception);
            return new TestLogEntry(categoryName, logLevel, eventId, message, exception);
        }
    }
}
