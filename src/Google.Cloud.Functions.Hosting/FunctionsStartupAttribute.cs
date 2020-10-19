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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Google.Cloud.Functions.Hosting
{
    /// <summary>
    /// Assembly and class attribute to specify a <see cref="FunctionsStartup"/> to use
    /// for additional configuration in the Functions Framework. To use multiple
    /// startup classes, specify this attribute multiple times. Both the assembly and
    /// the function type are queried for this attribute when determining the functions startup
    /// classes to use.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
    public class FunctionsStartupAttribute : Attribute
    {
        /// <summary>
        /// The Type of the <see cref="FunctionsStartup"/> class to register.
        /// </summary>
        public Type StartupType { get; set; }

        /// <summary>
        /// The ordering of application of the provider type relative to others
        /// specified by other attributes. Configurers specified in attributes
        /// with lower order numbers are invoked before those with higher order numbers.
        /// Defaults to 0.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="startupType">The Type of the <see cref="FunctionsStartup"/> class to register.</param>
        public FunctionsStartupAttribute(Type startupType) =>
            StartupType = startupType;

        /// <summary>
        /// Returns the functions startup classes found in the attributes of <paramref name="assembly"/> and
        /// <paramref name="target"/>, ordered by <see cref="Order"/> and then by the full name of the type.
        /// </summary>
        /// <param name="assembly">The assembly to inspect for attributes.</param>
        /// <param name="target">Optional type to query for attributes. If this is null, only
        /// startup classes from <paramref name="assembly"/> are returned.
        /// </param>
        /// <returns>A (possibly empty) sequence of the startup types detected via attributes.</returns>
        public static IEnumerable<Type> GetStartupTypes(Assembly assembly, Type? target)
        {
            var fromAssembly = assembly.GetCustomAttributes<FunctionsStartupAttribute>();
            var fromType = target?.GetCustomAttributes<FunctionsStartupAttribute>(inherit: true) ?? Enumerable.Empty<FunctionsStartupAttribute>();
            return fromAssembly.Concat(fromType)
                .OrderBy(attr => attr.Order)
                .ThenBy(attr => attr.StartupType.FullName)
                .Select(attr => attr.StartupType)
                .ToList();
        }
    }
}
