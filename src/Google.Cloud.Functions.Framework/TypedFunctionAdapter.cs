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
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Google.Cloud.Functions.Framework;

/// <summary>
/// An adapter to implement an HTTP Function based on a <see cref="ITypedFunction{TRequest, TResponse}"/>,
/// with built-in request deserialization and response serialization.
/// </summary>
public sealed class TypedFunctionAdapter<TRequest, TResult> : IHttpFunction
{
    private readonly ITypedFunction<TRequest, TResult> _function;
    private readonly IHttpRequestReader<TRequest> _requestReader;
    private readonly IHttpResponseWriter<TResult> _responseWriter;
    private readonly ILogger _logger;

    /// <summary>
    /// Constructs a new instance based on the given TypedFunction.
    /// </summary>
    public TypedFunctionAdapter(
        ITypedFunction<TRequest, TResult> function,
        IHttpRequestReader<TRequest> requestReader,
        IHttpResponseWriter<TResult> responseWriter,
        ILogger<TypedFunctionAdapter<TRequest, TResult>> logger)
    {
        _function = Preconditions.CheckNotNull(function, nameof(function));
        _requestReader = Preconditions.CheckNotNull(requestReader, nameof(requestReader));
        _responseWriter = Preconditions.CheckNotNull(responseWriter, nameof(responseWriter));
        _logger = Preconditions.CheckNotNull(logger, nameof(logger));
    }

    /// <summary>
    /// Handles an HTTP request by extracting the CloudEvent from it, deserializing the data, and passing
    /// both the event and the data to the original CloudEvent Function.
    /// The request fails if it does not contain a CloudEvent.
    /// </summary>
    /// <param name="context">The HTTP context containing the request and response.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task HandleAsync(HttpContext context)
    {
        TRequest data;
        try
        {
            data = await _requestReader.ReadRequestAsync(context.Request);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        TResult res = await _function.HandleAsync(data, context.RequestAborted);
        try
        {
            await _responseWriter.WriteResponseAsync(context.Response, res);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            return;
        }
    }
}
