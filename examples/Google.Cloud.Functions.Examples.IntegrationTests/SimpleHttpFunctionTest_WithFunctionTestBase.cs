﻿// Copyright 2020, Google LLC
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

using Google.Cloud.Functions.Invoker.Testing;
using System.Threading.Tasks;
using Xunit;

namespace Google.Cloud.Functions.Examples.IntegrationTests
{
    public class SimpleHttpFunctionTest_WithFunctionTestBase : FunctionTestBase<SimpleHttpFunction.Function>
    {
        [Fact]
        public async Task FunctionWritesHelloFunctionsFramework()
        {
            string content = await ExecuteHttpGetRequestAsync();
            Assert.Equal("Hello, Functions Framework.", content);
        }
    }
}
