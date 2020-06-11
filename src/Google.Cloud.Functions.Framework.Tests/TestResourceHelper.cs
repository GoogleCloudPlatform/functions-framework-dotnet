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
using System.IO;

namespace Google.Cloud.Functions.Framework.Tests
{
    /// <summary>
    /// Helper methods to make it simpler to load embedded resources.
    /// </summary>
    internal class TestResourceHelper
    {
        public static Stream LoadResource(System.Type type, string relativeResourceName)
        {
            // In order to make diagnostics simpler, we load the whole resource into a MemoryStream
            // and return that.
            MemoryStream ret = new MemoryStream();
            using (var resource = type.Assembly.GetManifestResourceStream(type, relativeResourceName))
            {
                if (resource is null)
                {
                    throw new ArgumentException($"Resource {relativeResourceName} not found");
                }
                resource.CopyTo(ret);
            }
            ret.Position = 0;
            return ret;
        }

        public static Stream LoadResource<T>(string relativeResourceName) =>
            LoadResource(typeof(T), relativeResourceName);

        public static Stream LoadResource(object caller, string relativeResourceName) =>
            LoadResource(caller.GetType(), relativeResourceName);
    }
}
