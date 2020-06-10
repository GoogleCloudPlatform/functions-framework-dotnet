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

using CloudNative.CloudEvents;
using Google.Events.Protobuf.Cloud.Storage.V1;
using Google.Protobuf;
using System;
using System.Net.Mime;
using Xunit;

namespace Google.Events.Protobuf.Tests
{
    public class ProtobufCloudEventDataConverterTest
    {
        private static readonly ContentType JsonContentType = new ContentType("application/json");
        private static readonly ContentType ProtobufContentType = new ContentType("application/protobuf");

        [Fact]
        public void ConvertEventData_BadDataType()
        {
            var evt = new CloudEvent("type", new Uri("//source")) { Data = Guid.NewGuid() };
            var converter = new ProtobufCloudEventDataConverter<StorageObject>();
            Assert.Throws<ArgumentException>(() => converter.ConvertEventData(evt));
        }

        [Fact]
        public void ConvertEventData_Binary()
        {
            var original = new StorageObject { Bucket = "my-bucket" };
            var evt = new CloudEvent("type", new Uri("//source")) { Data = original.ToByteArray() };
            var converter = new ProtobufCloudEventDataConverter<StorageObject>();
            var result = converter.ConvertEventData(evt);
            Assert.Equal(original, result);
        }

        [Fact]
        public void ConvertEventData_Json()
        {
            var original = new StorageObject { Bucket = "my-bucket" };
            var evt = new CloudEvent("type", new Uri("//source")) { Data = "{ \"bucket\": \"my-bucket\" }" };
            var converter = new ProtobufCloudEventDataConverter<StorageObject>();
            var result = converter.ConvertEventData(evt);
            Assert.Equal(original, result);
        }

        [Fact]
        public void PopulateCloudEvent_DefaultToJson()
        {
            var data = new StorageObject { Bucket = "my-bucket" };
            var evt = new CloudEvent("type", new Uri("//source"));
            var converter = new ProtobufCloudEventDataConverter<StorageObject>();
            converter.PopulateCloudEvent(evt, data);
            Assert.Equal("{ \"bucket\": \"my-bucket\" }", evt.Data);
            Assert.Equal(JsonContentType, evt.DataContentType);
        }

        [Fact]
        public void PopulateCloudEvent_ExplicitlyJson()
        {
            var data = new StorageObject { Bucket = "my-bucket" };
            var evt = new CloudEvent("type", new Uri("//source")) { DataContentType = JsonContentType };
            var converter = new ProtobufCloudEventDataConverter<StorageObject>();
            converter.PopulateCloudEvent(evt, data);
            Assert.Equal("{ \"bucket\": \"my-bucket\" }", evt.Data);
            Assert.Equal(JsonContentType, evt.DataContentType);
        }

        [Fact]
        public void PopulateCloudEvent_ExplicitlyBinary()
        {
            var data = new StorageObject { Bucket = "my-bucket" };
            var evt = new CloudEvent("type", new Uri("//source")) { DataContentType = ProtobufContentType };
            var converter = new ProtobufCloudEventDataConverter<StorageObject>();
            converter.PopulateCloudEvent(evt, data);
            Assert.Equal(data.ToByteArray(), evt.Data);
            Assert.Equal(ProtobufContentType, evt.DataContentType);
        }

        [Fact]
        public void PopulateCloudEvent_UnknownDataContentType()
        {
            var data = new StorageObject { Bucket = "my-bucket" };
            var evt = new CloudEvent("type", new Uri("//source")) { DataContentType = new ContentType("application/unknown") };
            var converter = new ProtobufCloudEventDataConverter<StorageObject>();
            Assert.Throws<ArgumentException>(() => converter.PopulateCloudEvent(evt, data));
        }
    }
}
