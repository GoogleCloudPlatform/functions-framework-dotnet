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

namespace Google.Cloud.Functions.Examples.FSharpUntypedEventFunction

open Google.Cloud.Functions.Framework
open System.Threading.Tasks

type Function() =
    interface ICloudEventFunction with
        /// <summary>
        /// Logic for your function goes here. Note that a CloudEvent function just consumes an event;
        /// it doesn't provide any response.
        /// </summary>
        /// <param name="cloudEvent">The CloudEvent your function should consume.</param>
        /// <param name="cancellationToken">A cancellation token that is notified if the request is aborted.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        member this.HandleAsync(cloudEvent, cancellationToken) =
            printfn "CloudEvent information:"
            printfn "ID: %s" cloudEvent.Id
            printfn "Source: %A" cloudEvent.Source
            printfn "Type: %s" cloudEvent.Type
            printfn "Subject: %s" cloudEvent.Subject
            printfn "DataSchema: %A" cloudEvent.DataSchema
            printfn "DataContentType: %O" cloudEvent.DataContentType
            printfn "Time: %s" (match Option.ofNullable cloudEvent.Time with
                                | Some time -> time.ToUniversalTime().ToString "yyyy-MM-dd'T'HH:mm:ss.fff'Z'"
                                | None -> "")
            printfn "SpecVersion: %O" cloudEvent.SpecVersion
            printfn "Data: %A" cloudEvent.Data

            // In this example, we don't need to perform any asynchronous operations, so we
            // just return a completed Task to conform to the interface.
            Task.CompletedTask
