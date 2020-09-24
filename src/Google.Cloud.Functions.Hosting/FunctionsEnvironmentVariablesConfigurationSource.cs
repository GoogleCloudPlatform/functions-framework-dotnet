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

using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace Google.Cloud.Functions.Hosting
{
    /// <summary>
    /// An IConfigurationSource that deals with *specific* Functions Framework environment variables
    /// (unlike the more general EnvironmentVariablesConfigurationSource).
    /// </summary>
    internal sealed class FunctionsEnvironmentVariablesConfigurationSource : IConfigurationSource
    {
        private readonly Func<string, string?> _variableProvider;

        internal FunctionsEnvironmentVariablesConfigurationSource()
            : this(Environment.GetEnvironmentVariable)
        {
        }

        // Testable 
        internal FunctionsEnvironmentVariablesConfigurationSource(Func<string, string?> variableProvider) =>
            _variableProvider = variableProvider;

        public IConfigurationProvider Build(IConfigurationBuilder builder) => new Provider(_variableProvider);

        private class Provider : ConfigurationProvider
        {
            private const string KeyPrefix = FunctionsFrameworkOptions.ConfigSection + ":";
            private readonly Func<string, string?> _variableProvider;

            internal Provider(Func<string, string?> variableProvider) =>
                _variableProvider = variableProvider;

            public override void Load()
            {
                var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                MaybeAdd(EntryPoint.FunctionTargetEnvironmentVariable, nameof(FunctionsFrameworkOptions.FunctionTarget), target => target);
                MaybeAdd(EntryPoint.PortEnvironmentVariable, "Port", port => port);
                MaybeAdd("DOTNET_RUNNING_IN_CONTAINER", "Address",
                    value => string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) ? "Any" : "Loopback");
                MaybeAdd("K_SERVICE", "JsonLogging", service => string.IsNullOrWhiteSpace(service) ? "False" : "True");

                Data = dict;

                void MaybeAdd(string variableName, string configurationName, Func<string?, string?> mapping)
                {
                    var variable = _variableProvider(variableName);
                    var mappedValue = mapping(variable);
                    if (!string.IsNullOrWhiteSpace(mappedValue))
                    {
                        dict[KeyPrefix + configurationName] = mappedValue;
                    }
                }
            }
        }
    }
}
