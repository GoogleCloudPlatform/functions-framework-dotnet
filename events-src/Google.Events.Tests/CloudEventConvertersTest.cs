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
using System;
using System.Net.Mime;
using Xunit;

namespace Google.Events.Tests
{
    public class CloudEventConvertersTest
    {
        [Fact]
        public void ConvertEventData_Valid()
        {
            var evt = new CloudEvent("type", new Uri("//source")) { Data = "some text" };
            var data = CloudEventConverters.ConvertCloudEventData<EventData>(evt);
            Assert.Equal("some text", data.Text);
        }

        [Fact]
        public void PopulateCloudEvent_Valid()
        {
            var evt = new CloudEvent("type", new Uri("//source"));
            var data = new EventData { Text = "some text" };
            CloudEventConverters.PopulateCloudEvent(evt, data);
            Assert.Equal("some text", evt.Data);
        }

        [Fact]
        public void GetDataConverter_Cached()
        {
            var converter1 = CloudEventConverters.GetDataConverter<EventData>();
            var converter2 = CloudEventConverters.GetDataConverter<EventData>();
            Assert.Same(converter1, converter2);
        }

        [Fact]
        public void GetDataConverter_NoCtor() =>
            Assert.Throws<InvalidOperationException>(() => CloudEventConverters.GetDataConverter<NoCtorData>());

        [Fact]
        public void GetDataConverter_DoesntImplementInterface() =>
            Assert.Throws<InvalidOperationException>(() => CloudEventConverters.GetDataConverter<NoImplementationData>());

        [Fact]
        public void GetDataConverter_WrongTargetType() =>
            Assert.Throws<InvalidOperationException>(() => CloudEventConverters.GetDataConverter<WrongTargetTypeData>());

        [Fact]
        public void GetDataConverter_NoAttribute() =>
            Assert.Throws<InvalidOperationException>(() => CloudEventConverters.GetDataConverter<NoAttributeData>());

        [CloudEventDataConverter(typeof(EventDataConverter))]
        private class EventData
        {
            public string Text { get; set; }
        }

        private class EventDataConverter : ICloudEventDataConverter<EventData>
        {
            public EventData ConvertEventData(CloudEvent cloudEvent) =>
                new EventData { Text = (string) cloudEvent.Data };

            public void PopulateCloudEvent(CloudEvent cloudEvent, EventData data)
            {
                cloudEvent.DataContentType = new ContentType("text/plain");
                cloudEvent.Data = data.Text;
            }
        }

        [CloudEventDataConverter(typeof(NoCtorConverter))]
        private class NoCtorData
        {
        }

        private class NoCtorConverter : ICloudEventDataConverter<NoCtorData>
        {
            // This constructor won't be found.
            private NoCtorConverter()
            {
            }

            public NoCtorData ConvertEventData(CloudEvent cloudEvent) => null;

            public void PopulateCloudEvent(CloudEvent cloudEvent, NoCtorData data)
            {
            }
        }

        [CloudEventDataConverter(typeof(NoImplementationConverter))]
        private class NoImplementationData
        {
        }

        private class NoImplementationConverter
        {
        }

        [CloudEventDataConverter(typeof(WrongTargetTypeConverter))]
        private class WrongTargetTypeData
        {
        }

        private class WrongTargetTypeConverter : ICloudEventDataConverter<string>
        {
            public string ConvertEventData(CloudEvent cloudEvent) => null;

            public void PopulateCloudEvent(CloudEvent cloudEvent, string data)
            {
            }
        }

        private class NoAttributeData
        {
        }
    }
}
