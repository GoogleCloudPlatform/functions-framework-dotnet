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
using System.Collections.Generic;
using System.IO;
using System.Text;
using static System.FormattableString;

namespace Google.Cloud.Functions.Hosting.Logging
{
    internal class SimpleConsoleLogger : LoggerBase
    {
        private readonly TextWriter _console;

        internal SimpleConsoleLogger(string category, TextWriter console)
            : base(category) => _console = console;

        protected override void LogImpl<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, string formattedMessage)
        {
            // Note: these are deliberately the same values used by ASP.NET Core's console logger.
            string briefLevel = logLevel switch
            {
                LogLevel.Trace => "trce",
                LogLevel.Debug => "dbug",
                LogLevel.Information => "info",
                LogLevel.Warning => "warn",
                LogLevel.Error => "fail",
                LogLevel.Critical => "crit",
                _ => throw new ArgumentOutOfRangeException(nameof(logLevel))
            };

            StringBuilder scopeBuilder = new StringBuilder();
            ScopeProvider.ForEachScope(AppendScope, scopeBuilder);

            _console.WriteLine(Invariant($"{DateTime.UtcNow:yyyy-MM-dd'T'HH:mm:ss.fff'Z'} [{Category}] [{briefLevel}] {formattedMessage}"));
            // Note: it's not ideal to break out of the "one line per log entry" approach for either scopes or exceptions,
            // but there's no particularly nice way of getting all the relevant information otherwise.
            if (scopeBuilder.Length != 0)
            {
                _console.WriteLine($"Scopes: [{scopeBuilder}]");
            }

            if (exception is object)
            {
                _console.WriteLine(ToInvariantString(exception));
            }
        }

        private void AppendScope(object value, StringBuilder builder)
        {
            if (builder.Length != 0)
            {
                builder.Append(", ");
            }
            // If the scope is a dictionary, format it as key/value pairs
            if (value is IEnumerable<KeyValuePair<string, object>> kvps)
            {
                builder.Append("{ ");
                bool first = true;
                foreach (var pair in kvps)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        builder.Append(", ");
                    }
                    builder.Append(pair.Key);
                    builder.Append(": ");
                    builder.Append(ToInvariantString(pair.Value));
                }
                builder.Append(" }");
            }
            // Otherwise, format it invariently
            else
            {
                builder.Append(ToInvariantString(value));
            }
        }
    }
}
