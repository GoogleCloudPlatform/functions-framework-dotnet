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

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace Google.Cloud.Functions.Invoker
{
    /// <summary>
    /// Convenience type to allow options to be bound within <see cref="HostingInternals"/>.
    /// </summary>
    internal sealed class FunctionsFrameworkOptions
    {
        internal const string ConfigSection = "FunctionsFramework";

        public int Port { get; set; } = 8080;
        public string? FunctionTarget { get; set; }
        public string? Address { get; set; }
        public bool JsonLogging { get; set; }

        internal IPAddress GetIPAddress() =>
            Address switch
            {
                "Any" => IPAddress.Any,
                "Loopback" => IPAddress.Loopback,
                null => IPAddress.Loopback,
                string addr => IPAddress.Parse(addr)
            };

        internal static FunctionsFrameworkOptions FromConfiguration(IConfiguration configuration) =>
            configuration.GetSection(ConfigSection).Get<FunctionsFrameworkOptions>() ?? new FunctionsFrameworkOptions();
    }
}
