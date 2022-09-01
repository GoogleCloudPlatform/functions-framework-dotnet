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

using Google.Cloud.Functions.Examples.LocalNuGetPackageCode;
using Google.Cloud.Functions.Framework;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Google.Cloud.Functions.Examples.LocalNuGetPackageFunction
{
    public class Function : IHttpFunction
    {
        public async Task HandleAsync(HttpContext context)
        {
            // The message comes from SampleLibrary, which is in a separate
            // nuget package. There's no local project dependency - just
            // a package reference. The .nupkg file is in the nuget
            // subdirectory of the function's source directory, which
            // is referred to in nuget.config.
            await context.Response.WriteAsync(SampleLibrary.Message);
        }
    }
}
