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
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Google.Cloud.Functions.Hosting.Logging
{
    /// <summary>
    /// Logger that writes a single line of JSON to the console per event, in a format that Google Cloud Logging can consume and display nicely.
    /// </summary>
    internal class JsonConsoleLogger : LoggerBase
    {
        /// <summary>
        /// We don't get the current state of a Utf8JsonWriter, so when writing out scopes, we need to use current the depth of the writer to
        /// determine whether or not we've already started writing out the scopes.
        /// </summary>
        private const int NoScopesDepth = 1;

        private readonly TextWriter _console;

        internal JsonConsoleLogger(string category, TextWriter console)
            : base(category) => _console = console;

        protected override void LogImpl<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, string formattedMessage)
        {
            string severity = logLevel switch
            {
                LogLevel.Trace => "DEBUG",
                LogLevel.Debug => "DEBUG",
                LogLevel.Information => "INFO",
                LogLevel.Warning => "WARNING",
                LogLevel.Error => "ERROR",
                LogLevel.Critical => "CRITICAL",
                _ => throw new ArgumentOutOfRangeException(nameof(logLevel))
            };

            var outputStream = new MemoryStream();
            using (var writer = new Utf8JsonWriter(outputStream))
            {
                writer.WriteStartObject();
                writer.WritePropertyName("message");
                writer.WriteStringValue(formattedMessage);
                writer.WritePropertyName("category");
                writer.WriteStringValue(Category);
                if (exception != null)
                {
                    writer.WritePropertyName("exception");
                    writer.WriteStringValue(ToInvariantString(exception));
                }
                writer.WritePropertyName("severity");
                writer.WriteStringValue(severity);

                // If we have format params and its more than just the original message add them.
                if (state is IEnumerable<KeyValuePair<string, object>> formatParams &&
                    ContainsFormatParameters(formatParams))
                {
                    writer.WritePropertyName("format_parameters");
                    writer.WriteStartObject();
                    foreach (var pair in formatParams)
                    {
                        string key = pair.Key;
                        if (string.IsNullOrEmpty(key))
                        {
                            continue;
                        }
                        if (char.IsDigit(key[0]))
                        {
                            key = "_" + key;
                        }
                        writer.WritePropertyName(key);
                        writer.WriteStringValue(ToInvariantString(pair.Value));
                    }
                    writer.WriteEndObject();
                }

                // Write the scopes as an array property, but only if there are any.            
                ScopeProvider.ForEachScope(WriteScope, writer);
                // If there are no scopes, the write state will still be "object". If
                // we've written at least one scope, the write state will be "array".
                if (writer.CurrentDepth != NoScopesDepth)
                {
                    writer.WriteEndArray();
                }
                writer.WriteEndObject();
            }

            // It's unfortunate that we need to write to a stream, then convert the result
            // into a string. We can avoid creating an extra copy of the binary data though,
            // as we should always be able to get the underlying buffer.
            var buffer = outputStream.GetBuffer();
            var text = Encoding.UTF8.GetString(buffer, 0, (int) outputStream.Length);

            _console.WriteLine(text);

            // Checks that fields is:
            // - Non-empty
            // - Not just a single entry with a key of "{OriginalFormat}"
            // so we can decide whether or not to populate a struct with it.
            bool ContainsFormatParameters(IEnumerable<KeyValuePair<string, object>> fields)
            {
                using (var iterator = fields.GetEnumerator())
                {
                    // No fields? Nothing to format.
                    if (!iterator.MoveNext())
                    {
                        return false;
                    }
                    // If the first entry isn't the original format, we definitely want to create a struct
                    if (iterator.Current.Key != "{OriginalFormat}")
                    {
                        return true;
                    }
                    // If the first entry *is* the original format, we want to create a struct
                    // if and only if there's at least one more entry.
                    return iterator.MoveNext();
                }
            }
        }

        private static void WriteScope(object value, Utf8JsonWriter writer)
        {
            // Detect "first scope" and start the scopes array property.
            if (writer.CurrentDepth == NoScopesDepth)
            {
                writer.WritePropertyName("scopes");
                writer.WriteStartArray();
            }

            if (value is IEnumerable<KeyValuePair<string, object>> kvps)
            {
                writer.WriteStartObject();
                foreach (var pair in kvps)
                {
                    string key = pair.Key;
                    if (string.IsNullOrEmpty(key))
                    {
                        continue;
                    }
                    writer.WritePropertyName(key);
                    writer.WriteStringValue(ToInvariantString(pair.Value));
                }
                writer.WriteEndObject();
            }
            else
            {
                // TODO: Consider special casing integers etc.
                writer.WriteStringValue(ToInvariantString(value));
            }
        }
    }
}
