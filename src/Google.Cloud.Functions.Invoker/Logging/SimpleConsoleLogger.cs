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

namespace Google.Cloud.Functions.Invoker.Logging
{
    internal class SimpleConsoleLogger : LoggerBase
    {
        internal SimpleConsoleLogger(string category) : base(category)
        {
        }

        public override void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            string message = formatter(state, exception);
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

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

            Console.WriteLine($"{DateTime.UtcNow:yyyy-MM-dd'T'HH:mm:ss.fff'Z'} [{Category}] [{briefLevel}] {message}");
            // Note: it's not ideal to break out of the "one line per log entry" approach here, but there's no particularly
            // nice way of getting all the relevant information otherwise.
            if (exception is object)
            {
                Console.WriteLine(exception);
            }
        }
    }
}
