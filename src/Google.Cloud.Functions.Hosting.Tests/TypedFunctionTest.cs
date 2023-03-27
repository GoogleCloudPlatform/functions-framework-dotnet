// Copyright 2023, Google LLC
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
using Google.Cloud.Functions.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Google.Cloud.Functions.Hosting.Tests;

/// <summary>
/// General test for typed functions, using a custom reader/writer specified in a startup,
/// and the built-in function testing.
/// </summary>
public class TypedFunctionTest : FunctionTestBase<TypedFunctionTest.TypedFunction>
{
    // Note: this constructor is currently required because we have assembly-wide
    // attributes in the test assembly. Those effectively override the function's
    // startup attributes - which is more reasonable in normal functions, where
    // the function is in its own assembly rather than in the test assembly.
    // We may want to revisit how startup classes are found, but doing so without
    // breaking existing users could be tricky.
    public TypedFunctionTest() : base(new FunctionTestServer<TypedFunction>())
    {
    }

    [Fact]
    public async Task FunctionExecuted()
    {
        var requestMessage = new HttpRequestMessage
        {
            // Any URI 
            RequestUri = new Uri("uri", UriKind.Relative),
            Content = new StringContent("{\"payload\":\"Test\"}"),
            Method = HttpMethod.Post
        };
        await ExecuteHttpRequestAsync(requestMessage, ValidateResponse);

        async Task ValidateResponse(HttpResponseMessage response)
        {
            string expectedJson = "{\"payload\":\"Echo: Test\"}";
            string actualJson = await response.Content.ReadAsStringAsync();
            Assert.Equal(expectedJson, actualJson);
        }
    }

    // These are all nested types to avoid polluting the namespace.
    [FunctionsStartup(typeof(Startup))]
    public class TypedFunction : ITypedFunction<EchoRequest, EchoResponse>
    {
        public Task<EchoResponse> HandleAsync(EchoRequest request, CancellationToken cancellationToken) =>
            Task.FromResult(new EchoResponse { Message = $"Echo: {request.Message}" });
    }

    public class Startup : FunctionsStartup
    {
        public override void ConfigureServices(WebHostBuilderContext context, IServiceCollection services)
        {
            services.AddSingleton<IHttpRequestReader<EchoRequest>>(new NewtonsoftJsonHttpRequestReader<EchoRequest>());
            services.AddSingleton<IHttpResponseWriter<EchoResponse>>(new NewtonsoftJsonHttpResponseWriter<EchoResponse>());
        }
    }

    // Note: these classes deliberately have different JSON property and C# property
    // names, specified only as Json.NET attributes, to prove that we're really using
    // the custom reader/writer.
    public class EchoRequest
    {
        [JsonProperty("payload")]
        public string Message { get; set; }
    }

    public class EchoResponse
    {
        [JsonProperty("payload")]
        public string Message { get; set; }
    }

    internal sealed class NewtonsoftJsonHttpRequestReader<TRequest> : IHttpRequestReader<TRequest>
    {
        public async Task<TRequest> ReadRequestAsync(HttpRequest httpRequest)
        {
            var json = await new StreamReader(httpRequest.Body).ReadToEndAsync();
            var request = JsonConvert.DeserializeObject<TRequest>(json);
            return request ?? throw new InvalidDataException("Deserialization of HTTP request resulted in null.");
        }
    }

    internal sealed class NewtonsoftJsonHttpResponseWriter<TResponse> : IHttpResponseWriter<TResponse>
    {
        public async Task WriteResponseAsync(HttpResponse httpResponse, TResponse functionResponse)
        {
            var json = JsonConvert.SerializeObject(functionResponse);
            await httpResponse.WriteAsync(json);
        }
    }
}
