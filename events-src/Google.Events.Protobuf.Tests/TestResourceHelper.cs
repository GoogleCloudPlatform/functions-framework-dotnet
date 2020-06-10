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
using System.Text;

namespace Google.Events.Protobuf.Tests
{
    /// <summary>
    /// Helper methods to make it simpler to load embedded resources.
    /// </summary>
    internal static class TestResourceHelper
    {
        public static string LoadJson(string resourceName)
        {
            MemoryStream ret = new MemoryStream();
            var type = typeof(TestResourceHelper);
            using (var resource = type.Assembly.GetManifestResourceStream(type, resourceName))
            {
                if (resource is null)
                {
                    throw new ArgumentException($"Resource {resourceName} not found");
                }
                resource.CopyTo(ret);
            }
            return Encoding.UTF8.GetString(ret.ToArray());
        }
    }
}
