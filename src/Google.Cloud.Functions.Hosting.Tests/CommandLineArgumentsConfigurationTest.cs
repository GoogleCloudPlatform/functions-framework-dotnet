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
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Xunit;

namespace Google.Cloud.Functions.Hosting.Tests
{
    public class CommandLineArgumentsConfigurationTest
    {
        [Theory]
        [InlineData(null)]
        // Effectively just new string[0], but covariance becomes awkward.
        [InlineData(new object[] { new string[0] })]
        public static void NoCommandLineArguments(string[] args)
        {
            var options = BindOptions(args);
            Assert.Null(options.FunctionTarget);
            Assert.False(options.JsonLogging);
            Assert.Equal(8080, options.Port);
            Assert.Equal(IPAddress.Loopback, options.GetIPAddress());
        }

        [Fact]
        public static void SingleCommandLineArgument_FunctionTarget()
        {
            var options = BindOptions("MyFunction");
            Assert.Equal("MyFunction", options.FunctionTarget);
            Assert.Equal(8080, options.Port);
        }

        [Fact]
        public static void SingleCommandLineArgument_LauncherArgs()
        {
            var exception = Assert.Throws<InvalidOperationException>(() => BindOptions("%LAUNCHER_ARGS%"));
            // Check we have a helpful message for the failure.
            Assert.Contains("github", exception.Message);
        }

        [Theory]
        [InlineData("--port 12345", null, 12345)]
        [InlineData("--target MyFunction", "MyFunction", 8080)]
        [InlineData("--target MyFunction --port 80", "MyFunction", 80)]
        [InlineData("--port 10000 --target MyFunction", "MyFunction", 10000)]
        public static void MultipleArguments_Valid(string argsToSplit, string expectedFunctionTarget, int expectedPort)
        {
            var options = BindOptions(argsToSplit.Split(' '));
            Assert.Equal(expectedFunctionTarget, options.FunctionTarget);
            Assert.Equal(expectedPort, options.Port);
        }

        // The ASP.NET Core parser doesn't complain about these cases. That's probably okay.
        [Theory]
        [InlineData("--port 8080 MyFunction SomethingElse YetAnotherArgument")]
        [InlineData("--answer 42")]
        public static void ParserIsLenient(string argsToSplit)
        {
            var options = BindOptions(argsToSplit.Split(' '));
            Assert.Null(options.FunctionTarget);
        }

        private static FunctionsFrameworkOptions BindOptions(params string[] args)
        {
            var configuration = new ConfigurationBuilder().AddFunctionsCommandLine(args).Build();
            return FunctionsFrameworkOptions.FromConfiguration(configuration);
        }
    }
}
