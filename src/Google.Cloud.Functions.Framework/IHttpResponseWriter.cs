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
using System.Threading.Tasks;

namespace Google.Cloud.Functions.Framework;

/// <summary>
/// Responsible for writing a strongly typed response object to an
/// HTTP response.
/// </summary>
public interface IHttpResponseWriter<TResponse>
{
    /// <summary>
    /// Asynchronously writes a strongly typed response object to
    /// an HTTP response.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task WriteResponseAsync(HttpResponse httpResponse, TResponse functionResponse);
}