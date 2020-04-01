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
using Xunit;

namespace Google.Cloud.Functions.Invoker.Tests
{
    public class ConfigurationVariableProviderTest
    {
        private static readonly IDictionary<string, string> CommandLineMapping = new Dictionary<string, string>
        {
            { "key1", "variable1" },
            { "key2", "variable2" },
            { "key3", "variable3" }
        };

        [Fact]
        public void FromDictionary()
        {
            var dictionary = new Dictionary<string, string>
            {
                { "NOT_EMPTY", "value" },
                { "EMPTY", "" },
                { "WHITESPACE", "   " }
            };
            var provider = ConfigurationVariableProvider.FromDictionary(dictionary);
            Assert.Equal("value", provider["NOT_EMPTY"]);
            Assert.Null(provider["EMPTY"]);
            Assert.Null(provider["WHITESPACE"]);
            Assert.Null(provider["NOT_PRESENT"]);
        }

        [Fact]
        public void FromCommandLine_Valid()
        {
            string[] commandLine = "--key3 value3 --key1 value1".Split(' ');
            var variables = ConfigurationVariableProvider.FromCommandLine(commandLine, CommandLineMapping);
            Assert.Equal("value1", variables["variable1"]);
            Assert.Null(variables["variable2"]);
            Assert.Equal("value3", variables["variable3"]);
        }

        [Theory]
        [InlineData("just-one-value")]
        [InlineData("--key1", "value1", "odd-number-of-values")]
        [InlineData("--key1 value1 value2 value3")]
        [InlineData("--unknown-key value")]
        [InlineData("--key1 value --key1 repeated-value")]
        public void FromCommandLine_Invalid(params string[] commandLine) =>
            Assert.Throws<ArgumentException>(() => ConfigurationVariableProvider.FromCommandLine(commandLine, CommandLineMapping));

        [Fact]
        public void Combine()
        {
            var primaryDictionary = new Dictionary<string, string>
            {
                {  "key1", "primary-value1" },
                {  "key2", "primary-value2" },
            };
            var secondaryDictionary = new Dictionary<string, string>
            {
                {  "key2", "secondary-value2" },
                {  "key3", "secondary-value3" },
            };
            var primaryProvider = ConfigurationVariableProvider.FromDictionary(primaryDictionary);
            var secondaryProvider = ConfigurationVariableProvider.FromDictionary(secondaryDictionary);

            var combined = ConfigurationVariableProvider.Combine(primaryProvider, secondaryProvider);
            Assert.Equal("primary-value1", combined["key1"]);
            Assert.Equal("primary-value2", combined["key2"]);
            Assert.Equal("secondary-value3", combined["key3"]);
            Assert.Null(combined["key4"]);
        }
    }
}
