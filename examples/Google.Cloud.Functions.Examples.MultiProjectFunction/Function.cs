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

using Google.Cloud.Functions.Examples.MultiProjectDependency;
using Google.Cloud.Functions.Framework;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Google.Cloud.Functions.Examples.MultiProjectFunction
{
    /// <summary>
    /// Function demonstrating how to use a project dependency from the function
    /// project to a class library. When deploying to Google Cloud Functions, the
    /// class library code still needs to be uploaded as part of the build, and the
    /// GOOGLE_BUILDABLE build-time environment variable needs to be set to the
    /// path to the function project.
    /// </summary>
    public class Function : IHttpFunction
    {
        public async Task HandleAsync(HttpContext context)
        {
            var logic = new BusinessLogic();
            var result = logic.PerformGeneralBusinessLogic();
            await context.Response.WriteAsync($"Result: {result}");
        }
    }
}
