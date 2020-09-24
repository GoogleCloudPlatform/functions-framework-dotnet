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
using System.Reflection;

namespace Google.Cloud.Functions.Invoker.Testing
{
    /// <summary>
    /// Helper method to determine the category for an ILogger{T}, given the type T.
    /// This isn't just the full name of the type, because generics are made prettier - but
    /// the method used isn't easily accessible.
    /// </summary>
    internal static class LoggerTypeNameHelper
    {
        internal static string GetCategoryNameForType<T>()
        {
            var wrapper = new Logger<T>(NoOpLoggerFactory.Instance);
            return ((NoOpLogger) wrapper.BeginScope(null)).CategoryName;
        }

        internal static string GetCategoryNameForType(Type type)
        {
            // Annoyingly, we have to use reflection here...
            var method = typeof(LoggerTypeNameHelper).GetMethod(
                nameof(GetCategoryNameForType), 1, BindingFlags.NonPublic | BindingFlags.Static,
                binder: null, Type.EmptyTypes, modifiers: null);
            method = method!.MakeGenericMethod(type);
            return (string?) method!.Invoke(null, null) ?? throw new InvalidOperationException("Method returned null");
        }

        private class NoOpLoggerFactory : ILoggerFactory
        {
            internal static ILoggerFactory Instance { get; } = new NoOpLoggerFactory();

            public void AddProvider(ILoggerProvider provider) =>
                throw new NotImplementedException();

            public ILogger CreateLogger(string categoryName) =>
                new NoOpLogger(categoryName);

            public void Dispose() =>
                throw new NotImplementedException();
        }

        /// <summary>
        /// Logger which returns itself when BeginScope is called, allowing us
        /// to get at the category. Yes, this is very weird.
        /// </summary>
        private class NoOpLogger : ILogger, IDisposable
        {
            internal string CategoryName { get; }

            internal NoOpLogger(string categoryName) =>
                CategoryName = categoryName;

            public IDisposable BeginScope<TState>(TState state) => this;

            public void Dispose() =>
                throw new NotImplementedException();

            public bool IsEnabled(LogLevel logLevel) =>
                throw new NotImplementedException();

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) =>
                throw new NotImplementedException();
        }
    }
}
