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

namespace Google.Cloud.Functions.Hosting.Logging
{
    /// <summary>
    /// Base class for loggers that don't do any of their own filtering, and don't support scopes.
    /// </summary>
    internal abstract class LoggerBase : ILogger
    {
        private const string KestrelCategory = "Microsoft.AspNetCore.Server.Kestrel";

        // As defined in https://github.com/dotnet/aspnetcore/blob/master/src/Servers/Kestrel/Core/src/Internal/Infrastructure/KestrelTrace.cs
        // This has been stable since April 2017, so it seems reasonable to rely on it.
        private const int HeartbeatSlowEventId = 22;

        private readonly bool _isKestrelCategory;
        protected string Category { get; }

        protected LoggerBase(string category) =>
            (Category, _isKestrelCategory) = (category, category == KestrelCategory);

        // We don't really support scopes
        public IDisposable BeginScope<TState>(TState state) => SingletonDisposable.Instance;

        // Note: log level filtering is handled by other logging infrastructure, so we don't do any of it here.
        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        /// <summary>
        /// Performs common filtering and formats the message, before delegating
        /// to <see cref="LogImpl"/>.
        /// </summary>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            // Functions expect to go for a long time without any CPU. It's reasonable to suppress this warning.
            if (_isKestrelCategory && eventId.Id == HeartbeatSlowEventId)
            {
                return;
            }

            string message = formatter(state, exception);
            if (string.IsNullOrEmpty(message))
            {
                return;
            }
            LogImpl(logLevel, eventId, state, exception, message);
        }

        /// <summary>
        /// Delegated "we've definitely got something to log" handling.
        /// </summary>
        protected abstract void LogImpl<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, string formattedMessage);

        // Used for scope handling.
        private class SingletonDisposable : IDisposable
        {
            internal static readonly SingletonDisposable Instance = new SingletonDisposable();
            private SingletonDisposable() { }
            public void Dispose() { }
        }
    }
}
