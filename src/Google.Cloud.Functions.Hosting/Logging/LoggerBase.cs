﻿// Copyright 2020, Google LLC
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
using System.Globalization;

namespace Google.Cloud.Functions.Hosting.Logging
{
    /// <summary>
    /// Base class for loggers that don't do any of their own filtering, and don't support scopes.
    /// </summary>
    internal abstract class LoggerBase : ILogger
    {
        // Visible for testing.
        internal const string KestrelCategory = "Microsoft.AspNetCore.Server.Kestrel";

        // As defined in https://github.com/dotnet/aspnetcore/blob/main/src/Servers/Kestrel/Core/src/Internal/Infrastructure/KestrelTrace.cs
        // This has been stable since April 2017, so it seems reasonable to rely on it.
        // Visible for testing.
        internal const int HeartbeatSlowEventId = 22;

        private readonly bool _isKestrelCategory;
        protected string Category { get; }
        protected IExternalScopeProvider ScopeProvider { get; }

        protected LoggerBase(string category, IExternalScopeProvider? scopeProvider) =>
            (Category, _isKestrelCategory, ScopeProvider) =
            (category, category == KestrelCategory, scopeProvider ?? new LoggerExternalScopeProvider());

        public IDisposable BeginScope<TState>(TState state) where TState : notnull =>
            ScopeProvider.Push(state);

        // Note: log level filtering is handled by other logging infrastructure, so we don't do any of it here.
        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        /// <summary>
        /// Performs common filtering and formats the message, before delegating
        /// to <see cref="LogImpl{TState}"/>.
        /// </summary>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
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
        protected abstract void LogImpl<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, string formattedMessage);

        /// <summary>
        /// Convenience method to convert a value into a string using the invariant
        /// culture for formatting. Null input is converted into an empty string.
        /// </summary>
        protected static string ToInvariantString(object value) => Convert.ToString(value, CultureInfo.InvariantCulture) ?? "";
    }
}
