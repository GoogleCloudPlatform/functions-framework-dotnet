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

namespace Google.Cloud.Functions.Invoker
{
    /// <summary>
    /// Assembly attribute to specify a <see cref="FunctionsStartup"/> to use
    /// for additional configuration in the Functions Framework. To use multiple
    /// startup classes, specify this attribute multiple times.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class FunctionsStartupAttribute : Attribute
    {
        /// <summary>
        /// 
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
    }
}
