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

namespace Google.Cloud.Functions.Framework.Tests.GcfEvents
{
    /// <summary>
    /// Helpers for resources accessed cross multiple tests.
    /// </summary>
    internal static class GcfEventResources
    {
        internal static HttpContext CreateHttpContext(string resourceName) =>
            new DefaultHttpContext
            {
                Request =
                {
                    Body = TestResourceHelper.LoadResource(typeof(GcfEventResources), resourceName),
                    ContentType = "application/json"
                }
            };
    }
}
