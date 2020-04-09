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

namespace MyFunction

open Google.Cloud.Functions.Framework
open System.Threading.Tasks

type Function() =
    interface ICloudEventFunction with
        /// <summary>
        /// Logic for your function goes here. Note that a Cloud Event function just consumes an event;
        /// it doesn't provide any response.
        /// </summary>
        /// <param name="cloudEvent">The Cloud Event your function should respond to.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        member this.HandleAsync cloudEvent =
            printfn "Cloud event information:"
            printfn "ID: %s" cloudEvent.Id
            printfn "Source: %A" cloudEvent.Source
            printfn "Type: %s" cloudEvent.Type
            printfn "Subject: %s" cloudEvent.Subject
            printfn "DataSchema: %A" cloudEvent.DataSchema
            printfn "DataContentType: %A" cloudEvent.DataContentType
            printfn "SpecVersion: %A" cloudEvent.SpecVersion
            printfn "Data: %A" cloudEvent.Data
            Task.CompletedTask
