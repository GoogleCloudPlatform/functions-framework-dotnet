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

using Google.Cloud.Functions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Google.Cloud.Functions.Examples.MultipleFunctions
{

    public class Function2Startup : FunctionsStartup
    {
        public override void ConfigureServices(WebHostBuilderContext context, IServiceCollection services) =>
            services.AddSingleton(new Dependency<string>("text"));
    }

    [FunctionsStartup(typeof(Function2Startup))]
    public class Function2 : FunctionBase
    {
        public Function2(Dependency<int> dep1 = null, Dependency<string> dep2 = null, Dependency<Guid> dep3 = null, Dependency<decimal> dep4 = null)
            : base(dep1, dep2, dep3, dep4)
        {
        }
    }
}
