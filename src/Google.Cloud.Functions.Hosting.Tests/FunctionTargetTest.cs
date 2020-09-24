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

using CloudNative.CloudEvents;
using Google.Cloud.Functions.Framework;
using Google.Events.Protobuf.Cloud.Storage.V1;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Google.Cloud.Functions.Hosting.Tests
{
    public class FunctionTargetTest
    {
        [Fact]
        public void NonFunctionType()
        {
            var services = new ServiceCollection();
            Assert.Throws<InvalidOperationException>(() => HostingInternals.AddServicesForFunctionTarget(services, typeof(NonFunction)));
        }

        [Fact]
        public void NonInstantiableFunctionType()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            HostingInternals.AddServicesForFunctionTarget(services, typeof(NonInstantiableFunction));
            // We only find out when we try to get an IHttpFunction that we can't actually instantiate the function.
            var provider = services.BuildServiceProvider();
            Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<IHttpFunction>());
        }

        [Fact]
        public void FunctionTypeImplementsMultipleInterfaces()
        {
            var services = new ServiceCollection();
            Assert.Throws<InvalidOperationException>(() => HostingInternals.AddServicesForFunctionTarget(services, typeof(MultipleCloudEventTypes)));
        }

        [Fact]
        public async Task NonGenericCloudEventFunction()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            HostingInternals.AddServicesForFunctionTarget(services, typeof(EventIdRememberingFunction));
            var provider = services.BuildServiceProvider();

            var function = provider.GetRequiredService<IHttpFunction>();
            Assert.Equal(typeof(EventIdRememberingFunction), provider.GetRequiredService<HostingInternals.FunctionTypeProvider>().FunctionType);
            string eventId = Guid.NewGuid().ToString();
            var context = new DefaultHttpContext
            {
                Request =
                {
                    // No actual content, but that's okay.
                    ContentType = "application/json",
                    Headers =
                    {
                        { "ce-specversion", "1.0" },
                        { "ce-source", "test" },
                        { "ce-type", "test" },
                        { "ce-id", eventId }
                    }
                }
            };

            await function.HandleAsync(context);
            Assert.Equal(eventId, EventIdRememberingFunction.LastEventId);
        }

        [Fact]
        public async Task TargetFunction_EventFunction_Generic()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            HostingInternals.AddServicesForFunctionTarget(services, typeof(StorageCloudEventFunction));
            var provider = services.BuildServiceProvider();
            var function = provider.GetRequiredService<IHttpFunction>();

            Assert.Equal(typeof(StorageCloudEventFunction), provider.GetRequiredService<HostingInternals.FunctionTypeProvider>().FunctionType);
            var eventId = Guid.NewGuid().ToString();
            var context = new DefaultHttpContext
            {
                Request =
                {
                    ContentType = "application/json",
                    Body = new MemoryStream(Encoding.UTF8.GetBytes("{\"name\": \"testdata\"}")),
                    Headers =
                    {
                        { "ce-specversion", "1.0" },
                        { "ce-source", "test" },
                        { "ce-type", "test" },
                        { "ce-id", eventId }
                    }
                },
            };

            await function.HandleAsync(context);
            Assert.Equal(eventId, StorageCloudEventFunction.LastEventId);
            Assert.Equal("testdata", StorageCloudEventFunction.LastData.Name);
        }

        [Fact]
        public void FindDefaultFunctionType_NoFunctionTypes() =>
            Assert.Throws<ArgumentException>(() => HostingInternals.FindDefaultFunctionType(
                typeof(FunctionTargetTest),
                typeof(FunctionsEnvironmentVariablesConfigurationSourceTest),
                typeof(AbstractHttpFunction), // Abstract, doesn't count
                typeof(INamedHttpFunction) // Interface, doesn't count
                ));

        [Fact]
        public void FindDefaultFunctionType_MultipleFunctionTypes() =>
            Assert.Throws<ArgumentException>(() => HostingInternals.FindDefaultFunctionType(
                typeof(DefaultFunction),
                typeof(EventIdRememberingFunction)));

        [Fact]
        public void FindDefaultFunctionType_SingleFunctionType()
        {
            var expected = typeof(DefaultFunction);
            var actual = HostingInternals.FindDefaultFunctionType(
                typeof(FunctionTargetTest),
                typeof(FunctionsEnvironmentVariablesConfigurationSourceTest),
                typeof(AbstractHttpFunction), // Abstract, doesn't count
                typeof(INamedHttpFunction), // Interface, doesn't count
                typeof(DefaultFunction));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FindDefaultFunctionType_TypedCloudEventFunction()
        {
            var expected = typeof(StorageCloudEventFunction);
            var actual = HostingInternals.FindDefaultFunctionType(typeof(StorageCloudEventFunction));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FindDefaultFunctionType_MultipleCloudEventTypesFunction() =>
            Assert.Throws<ArgumentException>(() => HostingInternals.FindDefaultFunctionType(
                typeof(MultipleCloudEventTypes)));


        public class DefaultFunction : IHttpFunction
        {
            public Task HandleAsync(HttpContext context) => Task.CompletedTask;
        }

        public class EventIdRememberingFunction : ICloudEventFunction
        {
            /// <summary>
            /// Horrible way of testing which function was actually used: remember the last event ID.
            /// Currently we only have a single test using this; if we have more, we'll need to disable
            /// parallelism.
            /// </summary>
            public static string LastEventId { get; private set; }

            public Task HandleAsync(CloudEvent cloudEvent, CancellationToken cancellationToken)
            {
                LastEventId = cloudEvent.Id;
                return Task.CompletedTask;
            }
        }

        public class StorageCloudEventFunction : ICloudEventFunction<StorageObjectData>
        {
            public static string LastEventId { get; set; }
            public static StorageObjectData LastData { get; set; }

            public Task HandleAsync(CloudEvent cloudEvent, StorageObjectData data, CancellationToken cancellationToken)
            {
                LastEventId = cloudEvent.Id;
                LastData = data;
                return Task.CompletedTask;
            }
        }

        public class NonFunction
        {
        }

        public class NonInstantiableFunction : IHttpFunction
        {
            private NonInstantiableFunction()
            {
            }

            public Task HandleAsync(HttpContext context) => Task.CompletedTask;
        }

        // Just a sample interface extending a function interface.
        public interface INamedHttpFunction : IHttpFunction
        {
            public string Name { get; }
        }

        public abstract class AbstractHttpFunction : IHttpFunction
        {
            public abstract Task HandleAsync(HttpContext context);
        }

        public class MultipleCloudEventTypes : ICloudEventFunction<string>, ICloudEventFunction<object>
        {
            public Task HandleAsync(CloudEvent cloudEvent, string data, CancellationToken cancellationToken)
                => Task.CompletedTask;
            public Task HandleAsync(CloudEvent cloudEvent, object data, CancellationToken cancellationToken)
                => Task.CompletedTask;
        }
    }
}
