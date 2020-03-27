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

using System;

namespace Google.Cloud.Functions.Invoker
{
    /// <summary>
    /// Provides environment variables, whether "real" or fake-for-test, with clearly
    /// defined semantics for missing and empty variables.
    /// </summary>
    internal abstract class EnvironmentVariableProvider
    {
        internal static EnvironmentVariableProvider System { get; } = new SystemEnvironmentVariableProvider();

        /// <summary>
        /// Returns the value of the specified environment variable, or null if the environment
        /// variable value consisted only of whitespace, or was empty.
        /// </summary>
        /// <param name="variable">The name of the variable to retrieve.</param>
        /// <returns></returns>
        internal string? this[string variable]
        {
            get
            {
                var value = GetVariable(Preconditions.CheckNotNull(variable, nameof(variable)))?.Trim();
                return value == "" ? null : value;
            }
        }

        /// <summary>
        /// Attempts to retrieve the given environment variable, returning it as-is (without any trimming etc).
        /// </summary>
        /// <param name="variable">The name of the variable to retrieve.</param>
        /// <returns>The value of the variable. If the variable was not present, this may return null or an empty string.</returns>
        protected abstract string? GetVariable(string variable);

        private sealed class SystemEnvironmentVariableProvider : EnvironmentVariableProvider
        {
            protected override string? GetVariable(string variable) => Environment.GetEnvironmentVariable(variable);
        }
    }
}
