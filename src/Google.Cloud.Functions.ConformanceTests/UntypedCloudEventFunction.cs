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
using CloudNative.CloudEvents.SystemTextJson;
using Google.Cloud.Functions.Framework;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

// Note: no namespace, to make it simpler to run as part of conformance testing
public class UntypedCloudEventFunction : ICloudEventFunction
{
    public async Task HandleAsync(CloudEvent cloudEvent, CancellationToken cancellationToken)
    {
        // Write out a structured JSON representation of the CloudEvent
        var formatter = new JsonEventFormatter();
        var bytes = formatter.EncodeStructuredModeMessage(cloudEvent, out var contentType);
        await File.WriteAllBytesAsync("function_output.json", bytes);
    }
}
