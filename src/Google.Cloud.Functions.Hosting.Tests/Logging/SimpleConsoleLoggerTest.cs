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
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Xunit;

namespace Google.Cloud.Functions.Hosting.Logging.Tests
{
    public class SimpleConsoleLoggerTest
    {
        [Fact]
        public void SimpleLogEntry()
        {
            var lines = GetLogLines("test",
                logger => logger.LogInformation("Message {param}", "value"));
            string line = Assert.Single(lines);
            var postTimestamp = SkipTimestamp(line);
            Assert.Equal("[test] [info] Message value", postTimestamp);
        }

        [Fact]
        public void Scopes()
        {
            var lines = GetLogLines("with-scopes", logger =>
            {
                // Note: using SortedDictionary to guarantee output order.
                using (logger.BeginScope(new SortedDictionary<string, object> { ["key"] = "1234567890123", ["jobId"] = 54 }))
                {
                    using (logger.BeginScope("simplescope"))
                    {
                        logger.LogWarning("Message {param}", "value");
                    }
                }
            });
            Assert.Equal(2, lines.Count);
            var postTimestamp = SkipTimestamp(lines[0]);
            Assert.Equal("[with-scopes] [warn] Message value", postTimestamp);
            Assert.Equal("Scopes: [{ jobId: 54, key: 1234567890123 }, simplescope]", lines[1]);
        }

        [Fact]
        public void Exception()
        {
            var lines = GetLogLines("ex", logger =>
            {
                try
                {
                    ((object) null).ToString();
                }
                catch (NullReferenceException e)
                {
                    logger.LogError(e, "Bang!");
                }
            });
            var postTimestamp = SkipTimestamp(lines[0]);
            Assert.Equal("[ex] [fail] Bang!", postTimestamp);
            // Don't check the message, as that can vary with culture (and is set before we get to format it invariantly)
            Assert.StartsWith("System.NullReferenceException: ", lines[1]);
            // The stack trace can vary a bit, but the first line should at least have something from this class...
            Assert.StartsWith($"   at {typeof(SimpleConsoleLoggerTest).FullName}", lines[2]);
        }

        [Fact]
        public void ExceptionAndScope()
        {
            var lines = GetLogLines("full", logger =>
            {
                // Note: using SortedDictionary to guarantee output order.
                using (logger.BeginScope("scopevalue"))
                {
                    logger.LogError(new Exception("Bang!"), "Failure");
                }
            });
            var postTimestamp = SkipTimestamp(lines[0]);
            Assert.Equal("[full] [fail] Failure", postTimestamp);
            Assert.Equal("Scopes: [scopevalue]", lines[1]);
            Assert.Equal("System.Exception: Bang!", lines[2]);
            // Don't bother checking the stack trace - the order of scope+exception is clear enough.
        }

        [Fact]
        public void KestrelHeartbeatLogSkipped()
        {
            var lines = GetLogLines(LoggerBase.KestrelCategory,
                logger => logger.Log(LogLevel.Information, new EventId(LoggerBase.HeartbeatSlowEventId), "Slow heartbeat"));
            Assert.Empty(lines);
        }

        /// <summary>
        /// Validate that the given line starts with a valid timestamp. It's not worth having an extra field
        /// per logger instance just for a clock abstraction in this case.
        /// </summary>
        private static string SkipTimestamp(string line)
        {
            string timestamp = line.Substring(0, "2021-01-27T07:43:00.123Z ".Length);
            DateTime.ParseExact(timestamp, "yyyy-MM-dd'T'HH:mm:ss.fff'Z '", CultureInfo.InvariantCulture);
            return line.Substring(timestamp.Length);
        }

        private static List<string> GetLogLines(string category, Action<ILogger> action)
        {
            var builder = new StringBuilder();
            ILogger logger = new SimpleConsoleLogger(category, new StringWriter(builder));
            action(logger);
            var reader = new StringReader(builder.ToString());
            List<string> ret = new List<string>();
            while (reader.ReadLine() is string line)
            {
                ret.Add(line);
            }
            return ret;
        }
    }
}
