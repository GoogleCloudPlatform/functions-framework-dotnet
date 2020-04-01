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
    /// <summary>
    /// Simple logger provider that just calls a factory method each time it's asked for logger.
    /// </summary>
    internal class FactoryLoggerProvider : ILoggerProvider
    {
        private readonly Func<string, ILogger> _factory;

        internal FactoryLoggerProvider(Func<string, ILogger> factory) =>
            _factory = factory;

        public ILogger CreateLogger(string categoryName) => _factory(categoryName);

        public void Dispose()
        {
            // No-op
        }
    }
}
