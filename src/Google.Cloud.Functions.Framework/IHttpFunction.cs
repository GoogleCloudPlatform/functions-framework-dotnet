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

using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Google.Cloud.Functions.Framework
{
    /// <summary>
    /// A simple HTTP function, which populates the <see cref="HttpContext.Response"/>.
    /// </summary>
    public interface IHttpFunction
    {
        /// <summary>
        /// Asynchronously handles the request in <see cref="HttpContext.Request"/>,
        /// populating <see cref="HttpContext.Response"/> to indicate the result.
        /// </summary>
        /// <param name="context">The HTTP context containing the request and response.</param>
        /// <returns>A task to indicate when the request is complete.</returns>
        Task HandleAsync(HttpContext context);
    }
}
