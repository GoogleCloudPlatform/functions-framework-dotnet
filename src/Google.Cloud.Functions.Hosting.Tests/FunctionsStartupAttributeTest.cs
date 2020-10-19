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
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

[assembly: FunctionsStartup(typeof(Google.Cloud.Functions.Hosting.Tests.FunctionsStartupAttributeTest.TestStartup1), Order = 4)]
[assembly: FunctionsStartup(typeof(Google.Cloud.Functions.Hosting.Tests.FunctionsStartupAttributeTest.TestStartup2), Order = 2)]

namespace Google.Cloud.Functions.Hosting.Tests
{
    public class FunctionsStartupAttributeTest
    {
        // Startups used for attribute detection.
        public class TestStartup1 : FunctionsStartup { }
        public class TestStartup2 : FunctionsStartup { }
        public class TestStartup3 : FunctionsStartup { }

        [FunctionsStartup(typeof(TestStartup3), Order = 3)]
        public class TargetTypeWithAttribute
        {
        }

        // This is the only test that uses the assembly attribute detection.
        // Everything else specifies the startups explicitly.
        [Fact]
        public void GetStartupTypes_NoType()
        {
            var actualTypes = FunctionsStartupAttribute.GetStartupTypes(typeof(FunctionsStartupAttributeTest).Assembly, null);
            // TestStartup2 comes before TestStartup1 due to the Order properties.
            var expectedTypes = new[] { typeof(TestStartup2), typeof(TestStartup1) };
            Assert.Equal(expectedTypes, actualTypes);
        }

        [Fact]
        public void GetStartupTypes_WithType()
        {
            var actualTypes = FunctionsStartupAttribute.GetStartupTypes(typeof(FunctionsStartupAttributeTest).Assembly, typeof(TargetTypeWithAttribute));
            // Ordering is based on the Order properties
            var expectedTypes = new[] { typeof(TestStartup2), typeof(TestStartup3), typeof(TestStartup1) };
            Assert.Equal(expectedTypes, actualTypes);
        }
    }
}
