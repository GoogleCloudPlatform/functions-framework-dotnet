// Copyright 2021, Google LLC
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
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace Google.Cloud.Functions.Hosting.Logging.Tests
{
    public class JsonConsoleLoggerTest
    {
        [Fact]
        public void SimpleEntry()
        {
            var entry = GetLogEntry("test", logger => logger.LogInformation("simple message"));
            Assert.Equal("simple message", (string) entry["message"]);
            Assert.Equal("INFO", (string) entry["severity"]);
            Assert.Equal("test", (string) entry["category"]);
            Assert.False(entry.ContainsKey("scopes"));
        }

        [Fact]
        public void FormattedMessage()
        {
            var entry = GetLogEntry("test", logger => logger.LogInformation("Key: {key}", "keyValue"));
            Assert.Equal("Key: keyValue", (string) entry["message"]);
            var parameters = (JObject) entry["format_parameters"];
            Assert.Equal(2, parameters.Count);
            Assert.Equal("Key: {key}", (string) parameters["{OriginalFormat}"]);
            Assert.Equal("keyValue", (string) parameters["key"]);
        }

        [Fact]
        public void Exception()
        {
            var entry = GetLogEntry("test", logger =>
            {
                // Make sure the stack trace is filled in.
                try
                {
                    throw new Exception("Bang!");
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Failure");
                }
            });
            var exception = (string) entry["exception"];
            // Check the exception property contains the type, message, and stack trace
            Assert.Contains("System.Exception", exception);
            Assert.Contains("Bang!", exception);
            Assert.Contains($" at {typeof(JsonConsoleLoggerTest).FullName}", exception);
        }

        [Fact]
        public void Scopes()
        {
            var entry = GetLogEntry("test", logger =>
            {
                using (logger.BeginScope(new Dictionary<string, object> { ["key"] = "1234567890123", ["jobId"] = 54 }))
                {
                    using (logger.BeginScope("simplescope"))
                    {
                        logger.LogInformation("Message");
                    }
                }
            });
            var scopes = (JArray) entry["scopes"];
            Assert.Equal(2, scopes.Count);

            var obj = (JObject) scopes[0];
            Assert.Equal(2, obj.Count);
            Assert.Equal("1234567890123", (string) obj["key"]);
            Assert.Equal("54", (string) obj["jobId"]);

            Assert.Equal("simplescope", (string) scopes[1]);
        }

        private static JObject GetLogEntry(string category, Action<ILogger> action)
        {
            var builder = new StringBuilder();
            ILogger logger = new JsonConsoleLogger(category, new StringWriter(builder));
            action(logger);
            return JObject.Parse(builder.ToString());
        }
    }
}
