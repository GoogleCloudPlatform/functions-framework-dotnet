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

using Google.Events.Protobuf.Cloud.Audit.V1;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using Xunit;

namespace Google.Events.Protobuf.Tests
{
    // If we create any other methods in StructExtensions, we might want a nested class for each method.
    // For now, this is simplest.
    public class StructExtensionsGetValueTest
    {
        // Effectively a smoke test
        [Fact]
        public void AuditLogDataStructs()
        {
            var json = TestResourceHelper.LoadJson("Cloud.Audit.V1.auditlog1.json");
            var auditLog = AuditLogData.Parser.ParseJson(json);

            var request = auditLog.Request;
            var response = auditLog.Response;
            var serviceData = auditLog.ServiceData;

            Assert.Equal("my-gcp-project-id", request.GetValue("resource"));
            Assert.Equal(new List<object>(), request.GetValue("policy/bindings"));

            Assert.Equal("user:user@example.com", response.GetValue("bindings[0]/members[0]"));

            Assert.Equal("ADD", serviceData.GetValue("policyDelta/bindingDeltas[0]/action"));
        }

        [Fact]
        public void TopLevelValue()
        {
            var @struct = OneValueStruct("x", Value.ForString("y"));
            var value = @struct.GetValue("x");
            Assert.Equal("y", value);
        }

        [Fact]
        public void ChildValue()
        {
            var childStruct = OneValueStruct("x", Value.ForString("y"));
            var @struct = OneValueStruct("child", Value.ForStruct(childStruct));
            var value = @struct.GetValue("child/x");
            Assert.Equal("y", value);
        }

        [Fact]
        public void GrandchildValue()
        {
            var grandchildStruct = OneValueStruct("x", Value.ForString("y"));
            var childStruct = OneValueStruct("grandchild", Value.ForStruct(grandchildStruct));
            var @struct = OneValueStruct("child", Value.ForStruct(childStruct));
            var value = @struct.GetValue("child/grandchild/x");
            Assert.Equal("y", value);
        }

        [Fact]
        public void ListIndex()
        {
            var list = Value.ForList(Value.ForString("x"), Value.ForNumber(2.0));
            var @struct = OneValueStruct("list", list);
            Assert.Equal("x", @struct.GetValue("list[0]"));
            Assert.Equal(2.0, @struct.GetValue("list[1]"));
        }

        [Fact]
        public void ListValue()
        {
            var list = Value.ForList(Value.ForString("x"), Value.ForNumber(2.0));
            var @struct = OneValueStruct("list", list);
            var value = @struct.GetValue("list");
            Assert.Equal(new List<object> { "x", 2.0 }, value);
        }

        [Fact]
        public void StructValue()
        {
            var childStruct = OneValueStruct("x", Value.ForString("y"));
            var @struct = OneValueStruct("child", Value.ForStruct(childStruct));
            var value = @struct.GetValue("child");
            Assert.Equal(new Dictionary<string, object> { { "x", "y" }}, value);
        }

        [Fact]
        public void BooleanValue()
        {
            var @struct = new Struct
            {
                Fields =
                {
                    { "t", Value.ForBool(true) },
                    { "f", Value.ForBool(false) },
                }
            };
            Assert.Equal(true, @struct.GetValue("t"));
            Assert.Equal(false, @struct.GetValue("f"));
        }

        [Fact]
        public void NullValue()
        {
            var @struct = OneValueStruct("x", Value.ForNull());
            Assert.Null(@struct.GetValue("x"));
        }

        [Theory]
        [InlineData("")]
        [InlineData("x/missing")]
        [InlineData("missing")]
        [InlineData("x[0]")]
        [InlineData("missing[0]")]
        [InlineData("list[-1]")]
        [InlineData("list[2]")]
        [InlineData("list[abc]")]
        [InlineData("list[1")]
        [InlineData("list1]")]
        [InlineData("list]1[")]
        [InlineData("list[1]]")]
        [InlineData("child//x2")]
        [InlineData("child/x2/missing")]
        public void BadPaths(string path)
        {
            var @struct = new Struct
            {
                Fields =
                {
                    { "x", Value.ForString("y") },
                    { "list", Value.ForList(Value.ForString("x"), Value.ForNumber(2.0)) },
                    { "child", Value.ForStruct(OneValueStruct("x2", Value.ForString("y2"))) }
                }
            };
            Assert.ThrowsAny<ArgumentException>(() => @struct.GetValue(path));
        }

        private static Struct OneValueStruct(string name, Value value) =>
            new Struct { Fields = { { name, value } } };
    }
}
