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
using Google.Events.SystemTextJson.Cloud.PubSub.V1;
using System;
using Xunit;

namespace Google.Events.SystemTextJson.Tests
{
    public class JsonCloudEventDataConverterTest
    {
        [Fact]
        public void ConvertEventData()
        {
            var evt = new CloudEvent("type", new Uri("//source")) { Data = "{ \"data\": \"aGVsbG8ganNvbg==\" }" };
            var converter = new JsonCloudEventDataConverter<PubsubMessageData>();
            var result = converter.ConvertEventData(evt);
            Assert.Equal("hello json", result.TextData);
        }

        [Fact]
        public void PopulateCloudEvent()
        {
            var evt = new CloudEvent("type", new Uri("//source"));
            var data = new PubsubMessageData { TextData = "hello json" };
            var converter = new JsonCloudEventDataConverter<PubsubMessageData>();
            converter.PopulateCloudEvent(evt, data);
            Assert.Equal("{\"attributes\":{},\"data\":\"aGVsbG8ganNvbg==\"}", evt.Data);
        }
    }
}
