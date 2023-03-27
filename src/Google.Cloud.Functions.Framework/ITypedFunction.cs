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

using System.Threading;
using System.Threading.Tasks;

namespace Google.Cloud.Functions.Framework;

/// <summary>
/// A typed function accepting a structured request and asynchronously 
/// returning a structured response.
/// </summary>
/// <typeparam name="TRequest">Type of the request object.</typeparam>
/// <typeparam name="TResponse">Type of the response object.</typeparam>
public interface ITypedFunction<TRequest, TResponse>
{
    /// <summary>
    /// Asynchronously handles an incoming request, expected to return 
    /// a structured response.
    /// </summary>
    /// <param name="request">The request payload, deserialized from the incoming request.</param>
    /// <param name="cancellationToken">A cancellation token which indicates if the request is aborted.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken);
}
