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

using Google.Cloud.Functions.Invoker.Testing;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Google.Cloud.Functions.Examples.IntegrationTests
{
    public class AdvancedDependencyInjectionTest : FunctionTestBase<AdvancedDependencyInjection.Function>
    {
        [Fact]
        public async Task SingletonOperationIdRemainsStable()
        {
            // Make two requests to the function. They should both return the same SingletonOperationId value.
            var response1 = await CallFunctionAsync();
            var response2 = await CallFunctionAsync();

            Assert.Equal(response1.SingletonOperationId, response2.SingletonOperationId);
        }

        [Fact]
        public async Task ScopedOperationIdChangesPerRequest()
        {
            // Make two requests to the function. They should provide different ScopedOperationId values.
            var response1 = await CallFunctionAsync();
            var response2 = await CallFunctionAsync();

            Assert.NotEqual(response1.ScopedOperationId, response2.ScopedOperationId);
        }

        private async Task<ResponseModel> CallFunctionAsync()
        {
            var content = await ExecuteHttpGetRequestAsync();
            return JsonSerializer.Deserialize<ResponseModel>(content);
        }

        private class ResponseModel
        {
            public Guid SingletonOperationId { get; set; }
            public Guid ScopedOperationId { get; set; }
        }
    }
}
