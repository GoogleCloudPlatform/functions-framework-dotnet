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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Google.Cloud.Functions.Framework
{
    /// <summary>
    /// Function which delegates to an IDynamicFunction.
    /// </summary>
    public sealed class DynamicFunctionAdapter : IHttpFunction
    {
        // TODO: *Lots* of caching by type. We may not be able to avoid reflection for the final invocation,
        // but we can do a lot of lookups early.

        private readonly IDynamicFunction _function;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Constructs an instance that will execute the specified function, using the specified service provider for dependencies.
        /// </summary>
        public DynamicFunctionAdapter(IDynamicFunction function, IServiceProvider serviceProvider)
        {
            (_function, _serviceProvider) = (function, serviceProvider);
        }

        /// <summary>
        /// Implements the IHttpFunction contract by executing the Handle or HandleAsync method dynamically.
        /// </summary>
        public Task HandleAsync(HttpContext context)
        {
            // TODO: Adapt sync functions too.
            var method = _function.GetType().GetMethod("HandleAsync", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method is null)
            {
                throw new Exception("No HandleAsync method found");
            }
            if (!typeof(Task).IsAssignableFrom(method.ReturnType))
            {
                throw new Exception("Bad return type");
            }
            var arguments = method.GetParameters().Select(GetArgument).ToArray();

            var result = method.Invoke(_function, arguments);
            return (Task) result!;

            object GetArgument(ParameterInfo parameter)
            {
                // TODO: CloudEvent deserialization.
                if (parameter.ParameterType == typeof(HttpContext))
                {
                    return context;
                }
                if (parameter.ParameterType == typeof(ILogger))
                {
                    return _serviceProvider.GetRequiredService(typeof(ILogger<>).MakeGenericType(_function.GetType()));
                }
                return _serviceProvider.GetRequiredService(parameter.ParameterType);
            }
        }

    }
}
