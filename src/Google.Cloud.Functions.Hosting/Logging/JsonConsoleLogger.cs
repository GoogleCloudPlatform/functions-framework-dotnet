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
using Newtonsoft.Json;
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
        internal JsonConsoleLogger(string category) : base(category)
        {
        }

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

            StringBuilder builder = new StringBuilder();
            JsonWriter writer = new JsonTextWriter(new StringWriter(builder));
            writer.WriteStartObject();
            writer.WritePropertyName("message");
            writer.WriteValue(formattedMessage);
            writer.WritePropertyName("category");
            writer.WriteValue(Category);
            if (exception != null)
            {
                writer.WritePropertyName("exception");
                writer.WriteValue(exception.ToString());
            }
            writer.WritePropertyName("severity");
            writer.WriteValue(severity);

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
                    writer.WriteValue(pair.Value?.ToString() ?? "");
                }
                writer.WriteEndObject();
            }
            writer.WriteEndObject();
            Console.WriteLine(builder);

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
    }
}
