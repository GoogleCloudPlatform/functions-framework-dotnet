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

using Google.Cloud.Functions.Hosting;
using Google.Cloud.Functions.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Xunit;

namespace Google.Cloud.Functions.Examples.IntegrationTests
{
    /// <summary>
    /// By default, the test server will use the same Functions Startup classes
    /// as normal. Applying the FunctionsStartup attribute to the test class (or the assembly) signals to FunctionTestBase
    /// which startups should be used to inject test dependencies instead of the production ones.
    /// An alternative is to declare a parameterless constructor that creates a FunctionTestServer
    /// to pass into the base class constructor.
    /// </summary>
    [FunctionsStartup(typeof(TestStartup))]
    public class TestableDependenciesTest : FunctionTestBase<TestableDependencies.Function>
    {
        [Fact]
        public async Task FunctionOutputDoesNotReferToProduction()
        {
            string text = await ExecuteHttpGetRequestAsync();
            Assert.DoesNotContain("Production dependency", text);
            Assert.Contains("Test dependency", text);
        }

        // Functions Startup class specified in the FunctionTestStartup attribute on the test class,
        // so that test-only dependencies can be used.
        private class TestStartup : FunctionsStartup
        {
            public override void ConfigureServices(WebHostBuilderContext context, IServiceCollection services) =>
                services.AddSingleton<TestableDependencies.IDependency, TestDependency>();
        }

        public class TestDependency : TestableDependencies.IDependency
        {
            public string Name => "Test dependency";
        }
    }
}
