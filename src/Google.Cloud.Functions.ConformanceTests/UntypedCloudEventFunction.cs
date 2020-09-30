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
using Google.Cloud.Functions.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// Note: no namespace, to make it simpler to run as part of conformance testing
public class UntypedCloudEventFunction : ICloudEventFunction
{
    public async Task HandleAsync(CloudEvent cloudEvent, CancellationToken cancellationToken)
    {
        // This is an example of how CloudEvents are unfortunately inconsistent when it
        // comes to Data. Is it "pre-serialized" or not? We prevent parsing date/time values,
        // so that we can get as close to the original JSON as possible.
        var settings = new JsonSerializerSettings { DateParseHandling = DateParseHandling.None };
        cloudEvent.Data = JsonConvert.DeserializeObject<JObject>((string) cloudEvent.Data, settings);

        // Work around https://github.com/cloudevents/sdk-csharp/issues/59
        var attributes = cloudEvent.GetAttributes();
        var attributeKeys = attributes.Keys.ToList();
        foreach (var key in attributeKeys)
        {
            var lowerKey = key.ToLowerInvariant();
            var value = attributes[key];
            attributes.Remove(key);
            attributes[lowerKey] = value;
        }

        // Write out a structured JSON representation of the CloudEvent
        var formatter = new JsonEventFormatter();
        var bytes = formatter.EncodeStructuredEvent(cloudEvent, out var contentType);
        await File.WriteAllBytesAsync("function_output.json", bytes);
    }
}
