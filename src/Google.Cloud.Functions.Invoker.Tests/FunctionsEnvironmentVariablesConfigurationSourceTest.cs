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
using System.Net;
using Xunit;

namespace Google.Cloud.Functions.Invoker.Tests
{
    public class FunctionsEnvironmentVariablesConfigurationSourceTest
    {
        [Fact]
        public void NoEnvironmentVariables()
        {
            var source = new FunctionsEnvironmentVariablesConfigurationSource(_ => null);
            var options = BindOptions(source);
            Assert.Null(options.FunctionTarget);
            Assert.Equal(IPAddress.Loopback, options.GetIPAddress());
            Assert.Equal(8080, options.Port);
            Assert.False(options.JsonLogging);
        }

        [Fact]
        public void Port_Invalid()
        {
            var source = CreateSource(EntryPoint.PortEnvironmentVariable, "not a number");
            Assert.Throws<InvalidOperationException>(() => BindOptions(source));
        }

        [Fact]
        public void Port_Valid()
        {
            var source = CreateSource(EntryPoint.PortEnvironmentVariable, "12345");
            var options = BindOptions(source);
            Assert.Equal(12345, options.Port);
        }

        [Fact]
        public void JsonLogging_InKnative()
        {
            var source = CreateSource("K_SERVICE", "some-service");
            var options = BindOptions(source);
            Assert.True(options.JsonLogging);
        }

        [Fact]
        public void TargetFunction()
        {
            var source = CreateSource(EntryPoint.FunctionTargetEnvironmentVariable, "MyFunction");
            var options = BindOptions(source);
            Assert.Equal("MyFunction", options.FunctionTarget);
        }

        [Fact]
        public void Address_InContainer()
        {
            var source = CreateSource("DOTNET_RUNNING_IN_CONTAINER", "true");
            var options = BindOptions(source);
            Assert.Equal(IPAddress.Any, options.GetIPAddress());
        }

        private static FunctionsFrameworkOptions BindOptions(IConfigurationSource source)
        {
            var configuration = new ConfigurationBuilder().Add(source).Build();
            return FunctionsFrameworkOptions.FromConfiguration(configuration);
        }

        /// <summary>
        /// Creates a source that acts as if a single environment variable is set.
        /// </summary>
        private static FunctionsEnvironmentVariablesConfigurationSource CreateSource(string variable, string value) =>
            new FunctionsEnvironmentVariablesConfigurationSource(key => variable == key ? value : null);
    }
}
