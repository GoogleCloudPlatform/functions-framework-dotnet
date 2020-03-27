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

using Google.Cloud.Functions.Framework;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Google.Cloud.Functions.Invoker.Tests
{
    // Note: the tests here have hard-coded environment variable names and default values, for readability.
    // If the environment variables or default values are ever changed, these tests will need changing too.
    public class FunctionEnvironmentTest
    {
        private static readonly string DefaultFunctionName = typeof(DefaultFunction).FullName;
        private static readonly string[] DefaultFunctionCommandLine = new[] { DefaultFunctionName };
        // An empty string array, used for both an empty set of command line arguments and an empty set of environment variables.
        private static readonly string[] Empty = new string[0];

        [Fact]
        public void Port_DefaultsTo8080()
        {
            var environment = CreateEnvironment(DefaultFunctionCommandLine, Empty);
            Assert.Equal(8080, environment.Port);
        }

        [Fact]
        public void Port_SetViaEnvironmentVariable()
        {
            var environment = CreateEnvironment(DefaultFunctionCommandLine, new[] { "PORT=12345" });
            Assert.Equal(12345, environment.Port);
        }

        [Fact]
        public void Port_InvalidEnvironmentVariable() =>
            AssertBadEnvironment(DefaultFunctionCommandLine, new[] { "PORT=INVALID" });

        [Fact]
        public async Task TargetFunction_CommandLine()
        {
            var environment = CreateEnvironment(DefaultFunctionCommandLine, Empty);
            await AssertRequestHandlerType<DefaultFunction>(environment);
        }

        [Fact]
        public async Task TargetFunction_EnvironmentVariable()
        {
            var environment = CreateEnvironment(Empty, new[] { $"FUNCTION_TARGET={DefaultFunctionName}" });
            await AssertRequestHandlerType<DefaultFunction>(environment);
        }

        [Fact]
        public void TargetFunction_Unspecified() =>
            AssertBadEnvironment(Empty, Empty);

        [Fact]
        public void TargetFunction_NonFunctionType() =>
            AssertBadEnvironment(new[] { typeof(NonFunction).FullName }, Empty);

        [Fact]
        public void TargetFunction_NonInstantiableFunctionType() =>
            AssertBadEnvironment(new[] { typeof(NonInstantiableFunction).FullName }, Empty);

        private static void AssertBadEnvironment(string[] commandLine, string[] variables) =>
            Assert.ThrowsAny<Exception>(() => CreateEnvironment(commandLine, variables));

        private static async Task AssertRequestHandlerType<T>(FunctionEnvironment environment) where T : IHttpFunction
        {
            var context = new DefaultHttpContext();
            await environment.RequestHandler(context);
            Assert.Equal(typeof(T), context.Items[TestFunctionBase.TypeKey]);
        }

        /// <summary>
        /// Creates a FunctionEnvironment using the given command line and environment variables, and this
        /// (test) assembly.
        /// </summary>
        /// <param name="variables">The environment variables to emulator, in "Name=Value" format</param>
        /// <returns>The function environment</returns>
        private static FunctionEnvironment CreateEnvironment(string[] commandLine, string[] variables)
        {
            var variableDictionary = variables.ToDictionary(v => v.Split('=')[0], v => v.Split('=')[1]);
            var provider = new DictionaryEnvironmentVariableProvider(variableDictionary);
            return FunctionEnvironment.Create(typeof(FunctionEnvironmentTest).Assembly, commandLine, provider);
        }

        public class DefaultFunction : TestFunctionBase
        {
        }

        public class NonFunction
        {
        }

        public class NonInstantiableFunction : TestFunctionBase
        {
            private NonInstantiableFunction()
            {
            }
        }
    }
}
