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
using System.Collections.Generic;

namespace Google.Cloud.Functions.Invoker
{
    /// <summary>
    /// Provides configuration variables, typically either from the environment or from the command line,
    /// but also allowing for testing via a dictionary.
    /// </summary>
    internal abstract class ConfigurationVariableProvider
    {
        internal static ConfigurationVariableProvider System { get; } = new SystemEnvironmentVariableProvider();

        internal static ConfigurationVariableProvider Combine(ConfigurationVariableProvider primary, ConfigurationVariableProvider secondary) =>
            new CombinedVariableProvider(primary, secondary);

        internal static ConfigurationVariableProvider FromDictionary(IReadOnlyDictionary<string, string> environment) =>
            new DictionaryVariableProvider(environment);

        /// <summary>
        /// Converts command line parameters into a <see cref="ConfigurationVariableProvider"/>, allowing a transformation
        /// from command line argument name (e.g. "target") to a configuration variable name (e.g. "TARGET_FUNCTION").
        /// </summary>
        /// <param name="commandLine">The command line to parse.</param>
        /// <param name="argumentToVariableMapping">The mapping from command line argument name to configuration variable name.</param>
        /// <returns>A variable provider for the command line.</returns>
        internal static ConfigurationVariableProvider FromCommandLine(
            string[] commandLine, IDictionary<string, string> argumentToVariableMapping)
        {
            if ((commandLine.Length & 1) != 0)
            {
                throw new ArgumentException("Expected an even number of command line arguments");
            }
            Dictionary<string, string> variables = new Dictionary<string, string>();
            for (int i = 0; i < commandLine.Length; i += 2)
            {
                string key = commandLine[i];
                string value = commandLine[i + 1];
                if (!key.StartsWith("--"))
                {
                    throw new ArgumentException($"Invalid command line argument: {key}. Command line arguments must come in pairs, and the first argument in each pair must start with '--'.");
                }
                if (!argumentToVariableMapping.TryGetValue(key.Substring(2), out var variable))
                {
                    throw new ArgumentException($"Unknown command line argument: {key}.");
                }
                if (variables.ContainsKey("variable"))
                {
                    throw new ArgumentException($"Duplicate command line argument: {key}.");
                }
                variables[variable] = value;
            }
            return FromDictionary(variables);
        }

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

        /// <summary>
        /// Variable provider that reads from environment variables.
        /// </summary>
        private sealed class SystemEnvironmentVariableProvider : ConfigurationVariableProvider
        {
            protected override string? GetVariable(string variable) => Environment.GetEnvironmentVariable(variable);
        }

        /// <summary>
        /// Variable provider that uses an underlying dictionary for its variables.
        /// </summary>
        private sealed class DictionaryVariableProvider : ConfigurationVariableProvider
        {
            private readonly IReadOnlyDictionary<string, string> _environment;

            internal DictionaryVariableProvider(IReadOnlyDictionary<string, string> environment) =>
                _environment = Preconditions.CheckNotNull(environment, nameof(environment));

            protected override string? GetVariable(string variable) =>
                _environment.TryGetValue(variable, out var value) ? value : null;
        }

        /// <summary>
        /// Variable provider that consults two existing providers for any variable: a primary, then a secondary.
        /// </summary>
        private sealed class CombinedVariableProvider : ConfigurationVariableProvider
        {
            private readonly ConfigurationVariableProvider _primary;
            private readonly ConfigurationVariableProvider _secondary;

            internal CombinedVariableProvider(ConfigurationVariableProvider primary, ConfigurationVariableProvider secondary) =>
                (_primary, _secondary) = (primary, secondary);

            protected override string? GetVariable(string variable) => _primary[variable] ?? _secondary[variable];
        }
    }
}
