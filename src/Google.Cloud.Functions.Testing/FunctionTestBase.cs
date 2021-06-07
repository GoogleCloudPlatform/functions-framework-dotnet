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

using CloudNative.CloudEvents;
using CloudNative.CloudEvents.SystemTextJson;
using Google.Cloud.Functions.Hosting;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Google.Cloud.Functions.Testing
{
    /// <summary>
    /// Base class for test for functions. This class is designed
    /// for use in test frameworks that dispose of test classes
    /// automatically. (This includes NUnit and xUnit.) This class
    /// creates a <see cref="FunctionTestServer{TFunction}"/> on construction,
    /// and disposes of the server when the instance of this class is disposed.
    /// Additionally, convenience methods to make requests to the server and
    /// access log entries are provided.
    /// </summary>
    public abstract class FunctionTestBase<TFunction> : IDisposable
    {
        private static readonly CloudEventFormatter s_defaultEventFormatter = new JsonEventFormatter();
        private static readonly ContentType s_applicationJsonContentType = new ContentType("application/json");

        /// <summary>
        /// The server which will invoke <typeparamref name="TFunction"/>
        /// when a request is made.
        /// </summary>
        public FunctionTestServer Server { get; }

        /// <summary>
        /// Constructs a new instance using the specified server. Note that
        /// this effectively takes ownership of the given server, which will be
        /// disposed when the FunctionTestBase instance is disposed.
        /// </summary>
        /// <param name="server">The function test server, which is expected to have a
        /// function target of <typeparamref name="TFunction"/>.</param>
        protected FunctionTestBase(FunctionTestServer server)
        {
            Server = server;
        }

        /// <summary>
        /// Constructs a new instance using a new server constructed with default settings,
        /// with startup classes specified by <see cref="FunctionsStartupAttribute"/> attributes
        /// within the class hierarchy of the actual test class, or the test assembly.
        /// </summary>
        protected FunctionTestBase()
        {
            Server = new FunctionTestServer<TFunction>(GetType());
        }

        /// <summary>
        /// Executes a simple HTTP GET request against an arbitrary URI, and returns
        /// the text of the response after validating that it indicates success. For
        /// more complex scenarios, use <see cref="ExecuteHttpRequestAsync(HttpRequestMessage, Action{HttpResponseMessage})"/>.
        /// </summary>
        /// <param name="uri">The URI to request, or "sample-uri" by default.</param>
        public async Task<string> ExecuteHttpGetRequestAsync(string uri = "sample-uri")
        {
            using var client = Server.CreateClient();
            using var response = await client.GetAsync(uri);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Executes the given HTTP request against the server, calling a synchronous validator against the response.
        /// </summary>
        public async Task ExecuteHttpRequestAsync(HttpRequestMessage request, Action<HttpResponseMessage> validator)
        {
            using var client = Server.CreateClient();
            using var response = await client.SendAsync(request);
            validator(response);
        }

        /// <summary>
        /// Executes the given HTTP request against the server, calling an asynchronous validator against the response.
        /// </summary>
        public async Task ExecuteHttpRequestAsync(HttpRequestMessage request, Func<HttpResponseMessage, Task> validator)
        {
            using var client = Server.CreateClient();
            using var response = await client.SendAsync(request);
            await validator(response);
        }

        /// <summary>
        /// Executes the function with the specified CloudEvent, which is converted
        /// to an HTTP POST request using structured encoding and the specified event formatter,
        /// or a default formatter if <paramref name="formatter"/> is null.
        /// This method asserts that the request completed successfully.
        /// </summary>
        public async Task ExecuteCloudEventRequestAsync(CloudEvent cloudEvent, CloudEventFormatter? formatter = null)
        {
            var dataFormatter = cloudEvent.Data is null ? null : CloudEventFormatterAttribute.CreateFormatter(cloudEvent.Data.GetType());
            formatter = formatter ?? dataFormatter ?? s_defaultEventFormatter;
            var bytes = formatter.EncodeStructuredModeMessage(cloudEvent, out var contentType);
            contentType ??= s_applicationJsonContentType;
            var mediaContentType = new MediaTypeHeaderValue(contentType.MediaType) { CharSet = contentType.CharSet };
            var request = new HttpRequestMessage
            {
                // Any URI 
                RequestUri = new Uri("uri", UriKind.Relative),
                // CloudEvent headers
                Content = new ReadOnlyMemoryContent(bytes) { Headers = { ContentType = mediaContentType } },
                Method = HttpMethod.Post
            };

            using var client = Server.CreateClient();
            using var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Convenience method to test CloudEvents by supplying only the most important aspects.
        /// This method simply constructs a CloudEvent from the parameters and delegates to
        /// <see cref="ExecuteCloudEventRequestAsync(CloudEvent, CloudEventFormatter)"/>.
        /// </summary>
        /// <param name="eventType">The CloudEvent type.</param>
        /// <param name="data">The data to populate the CloudEvent with. If the type of the event data
        /// has <see cref="CloudEventFormatterAttribute"/> applied to it, the corresponding formatter will be used.
        /// </param>
        /// <param name="source">The source URI for the CloudEvent, or null to use a default of "//test-source".</param>
        /// <param name="subject">The subject of the CloudEvent, or null if no subject is required.</param>
        /// <typeparam name="T">The type of the event data. This is used to find the appropriate event formatter.</typeparam>
        public Task ExecuteCloudEventRequestAsync<T>(string eventType, T? data, Uri? source = null, string? subject = null)
            where T : class
        {
            var formatter = CloudEventFormatterAttribute.CreateFormatter(typeof(T));
            var cloudEvent = new CloudEvent
            {
                Type = eventType,
                Source = source ?? new Uri("//test-source", UriKind.RelativeOrAbsolute),
                Id = Guid.NewGuid().ToString(),
                Subject = subject,
                Data = data
            };
            return ExecuteCloudEventRequestAsync(cloudEvent, formatter);
        }

        /// <summary>
        /// Returns a list of the log entries for the category associated with the function.
        /// </summary>
        /// <returns>A list of log entries. The returned value may refer to an empty list,
        /// but is never null.</returns>
        public List<TestLogEntry> GetFunctionLogEntries() => Server.GetFunctionLogEntries();

        /// <summary>
        /// Disposes of the test server.
        /// </summary>
        public void Dispose() => Server.Dispose();
    }
}
