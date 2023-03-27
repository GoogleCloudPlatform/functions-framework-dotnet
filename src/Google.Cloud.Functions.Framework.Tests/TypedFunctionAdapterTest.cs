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

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Google.Cloud.Functions.Framework.Tests;

public class TypedFunctionAdapterTest
{
    [Fact]
    public async Task InvalidRequest_400BadRequestError()
    {
        var adapter = new TypedFunctionAdapter<string, int>(
            new TestTypedFunction(),
            new AlwaysFailsToParseRequestReader(),
            new Int32ResponseWriter(),
            new NullLogger<TypedFunctionAdapter<string, int>>()
        );
        var context = new DefaultHttpContext();
        await adapter.HandleAsync(context);
        Assert.Equal(400, context.Response.StatusCode);
    }

    [Fact]
    public async Task TestRequest_ExecutesFunction()
    {
        var payload = "Hello World!";

        var adapter = new TypedFunctionAdapter<string, int>(
            new TestTypedFunction(),
            new StringRequestReader(),
            new Int32ResponseWriter(),
            new NullLogger<TypedFunctionAdapter<string, int>>()
        );

        var context = new DefaultHttpContext
        {
            Request =
        {
            ContentType = "text/plain",
            Body = new MemoryStream(Encoding.UTF8.GetBytes(payload)),
        },
            Response =
        {
            Body = new MemoryStream(),
        }
        };

        await adapter.HandleAsync(context);
        Assert.Equal(200, context.Response.StatusCode);
        context.Response.Body.Position = 0;
        // The function counts the number of characters in the payload,
        // and returns a text representation of that number.
        Assert.Equal("12", new StreamReader(context.Response.Body).ReadToEnd());
    }

    class TestTypedFunction : ITypedFunction<string, int>
    {
        public Task<int> HandleAsync(string request, CancellationToken cancellationToken) =>
            Task.FromResult(request.Length);
    }

    class StringRequestReader : IHttpRequestReader<string>
    {
        public Task<string> ReadRequestAsync(HttpRequest request) =>
            new StreamReader(request.Body).ReadToEndAsync();
    }

    class Int32ResponseWriter : IHttpResponseWriter<int>
    {
        public Task WriteResponseAsync(HttpResponse httpResponse, int functionResponse) =>
            httpResponse.WriteAsync(functionResponse.ToString(CultureInfo.InvariantCulture));
    }

    class AlwaysFailsToParseRequestReader : IHttpRequestReader<string>
    {
        public Task<string> ReadRequestAsync(HttpRequest request) =>
            Task.FromException<string>(new Exception("Injected parse failure"));
    }
}
