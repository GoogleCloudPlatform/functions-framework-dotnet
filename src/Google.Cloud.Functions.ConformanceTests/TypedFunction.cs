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

using Google.Cloud.Functions.Framework;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Dynamic;

// Note: no namespace, to make it simpler to run as part of conformance testing
public class TypedFunction : ITypedFunction<ConformanceRequest, ConformanceResponse>
{
    public Task<ConformanceResponse> HandleAsync(ConformanceRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(new ConformanceResponse { Payload = request });
}

public class ConformanceRequest
{
    [JsonPropertyName("message")]
    public string Message { get; set; }
}

public class ConformanceResponse
{
    [JsonPropertyName("payload")]
    public dynamic Payload { get; set; }
}
