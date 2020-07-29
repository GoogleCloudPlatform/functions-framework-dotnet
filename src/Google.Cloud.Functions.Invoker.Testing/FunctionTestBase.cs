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
using Microsoft.AspNetCore.Authorization.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Google.Cloud.Functions.Invoker.Testing
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
        private static readonly ICloudEventFormatter s_eventFormatter = new JsonEventFormatter();
        private static readonly ContentType s_applicationJsonContentType = new ContentType("application/json");

        /// <summary>
        /// The server which will invoke <typeparamref name="TFunction"/>
        /// when a request is made.
        /// </summary>
        public FunctionTestServer<TFunction> Server { get; }

        /// <summary>
        /// Constructs a new instance using the specified server. Note that
        /// this effectively takes ownership of the given server, which will be
        /// disposed when the FunctionTestBase instance is disposed.
        /// </summary>
        /// <param name="server">The function test server.</param>
        protected FunctionTestBase(FunctionTestServer<TFunction> server)
        {
            Server = server;
        }

        /// <summary>
        /// Constructs a new instance using a new server constructed with default settings.
        /// </summary>
        protected FunctionTestBase() : this(new FunctionTestServer<TFunction>())
        {
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
        /// Executes the given HTTP request against the server, calling the validators against the response.
        /// </summary>
        public async Task ExecuteHttpRequestAsync(HttpRequestMessage request, Action<HttpResponseMessage> validator)
        {
            using var client = Server.CreateClient();
            using var response = await client.SendAsync(request);
            validator(response);
        }

        /// <summary>
        /// Executes the function with the specified CloudEvent, which is converted
        /// to an HTTP POST request using structured encoding. This method asserts that the
        /// request completed successfully.
        /// </summary>
        public async Task ExecuteCloudEventRequestAsync(CloudEvent cloudEvent)
        {
            var bytes = s_eventFormatter.EncodeStructuredEvent(cloudEvent, out var contentType);
            contentType ??= s_applicationJsonContentType;
            var mediaContentType = new MediaTypeHeaderValue(contentType.MediaType) { CharSet = contentType.CharSet };
            var request = new HttpRequestMessage
            {
                // Any URI 
                RequestUri = new Uri("uri", UriKind.Relative),
                // CloudEvent headers
                Content = new ByteArrayContent(bytes) { Headers = { ContentType = mediaContentType } },
                Method = HttpMethod.Post
            };

            using var client = Server.CreateClient();
            using var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
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
