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
using Google.Cloud.Functions.Invoker.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

[assembly: FunctionsStartup(typeof(Google.Cloud.Functions.Invoker.Tests.DependencyInjectionTest.TestStartup1))]
[assembly: FunctionsStartup(typeof(Google.Cloud.Functions.Invoker.Tests.DependencyInjectionTest.TestStartup2))]

namespace Google.Cloud.Functions.Invoker.Tests
{
    /// <summary>
    /// Separate test from FunctionEnvironmentTest as it's a little more complex - and it's testing more than just
    /// the FunctionEnvironment code; it's making sure we understand how dependency injection works.
    /// </summary>
    public class DependencyInjectionTest
    {
        public const string Dependency1Key = "Dependency1";
        public const string Dependency2Key = "Dependency2";

        /// <summary>
        /// This is just an overall test of how things hang together.
        /// </summary>
        [Fact]
        public async Task Integration()
        {
            var environment = FunctionEnvironment.Create(
                typeof(DependencyInjectionTest).Assembly,
                new[] { typeof(DependencyTestFunction).FullName },
                ConfigurationVariableProvider.FromDictionary(new Dictionary<string, string>()));

            var services = new ServiceCollection();
            environment.ConfigureServices(services);
            var providerFactory = new DefaultServiceProviderFactory();

            var provider = providerFactory.CreateServiceProvider(services);
            // Execute two requests, and remember the dependencies we saw in each of them.
            var request1Deps = await MakeRequestAsync();
            var request2Deps = await MakeRequestAsync();

            // We should get dependencies on every call
            Assert.NotNull(request1Deps.dep1);
            Assert.NotNull(request1Deps.dep2);
            Assert.NotNull(request2Deps.dep1);
            Assert.NotNull(request2Deps.dep2);

            // Dependency1 is registered as a singleton, so we should have the same value in both requests.
            Assert.Same(request1Deps.dep1, request2Deps.dep1);
            // Dependency2 is registered as a scoped service, so we should have different values in each request.
            Assert.NotSame(request1Deps.dep2, request2Deps.dep2);

            async Task<(object dep1, object dep2)> MakeRequestAsync()
            {
                using (var scope = provider.CreateScope())
                {
                    var context = new DefaultHttpContext { RequestServices = scope.ServiceProvider };
                    await environment.Execute(context);
                    return (context.Items[Dependency1Key], context.Items[Dependency2Key]);
                }
            }
        }

        public class DependencyTestFunction : IHttpFunction
        {
            private readonly IDependency1 _dep1;
            private readonly IDependency2 _dep2;

            public DependencyTestFunction(IDependency1 dep1, IDependency2 dep2) =>
                (_dep1, _dep2) = (dep1, dep2);

            public Task HandleAsync(HttpContext context)
            {
                context.Items[Dependency1Key] = _dep1;
                context.Items[Dependency2Key] = _dep2;
                return Task.CompletedTask;
            }
        }

        public class TestStartup1 : FunctionsStartup
        {
            public override void Configure(IFunctionsHostBuilder builder) =>
                builder.Services.AddSingleton<IDependency1, Dependency1>();
        }

        public class TestStartup2 : FunctionsStartup
        {
            public override void Configure(IFunctionsHostBuilder builder) =>
                builder.Services.AddScoped<IDependency2, Dependency2>();
        }

        public interface IDependency1 { }
        public class Dependency1 : IDependency1 { }
        public interface IDependency2 { }
        public class Dependency2 : IDependency2 { }
    }
}
