namespace MyFunction

open CloudNative.CloudEvents
open Google.Cloud.Functions.Framework
open Google.Events.Protobuf.Cloud.Storage.V1
open System.Threading.Tasks

/// <summary>
/// A function that can be triggered in responses to changes in Google Cloud Storage.
/// The type argument (StorageObjectData in this case) determines how the event payload is deserialized.
/// The function must be deployed so that the trigger matches the expected payload type. (For example,
/// deploying a function expecting a StorageObject payload will not work for a trigger that provides
/// a FirestoreEvent.)
/// </summary>
type Function() =
    interface ICloudEventFunction<StorageObjectData> with
        /// <summary>
        /// Logic for your function goes here. Note that a Cloud Event function just consumes an event;
        /// it doesn't provide any response.
        /// </summary>
        /// <param name="cloudEvent">The Cloud Event your function should consume.</param>
        /// <param name="data">The deserialized data within the Cloud Event.</param>
        /// <param name="cancellationToken">A cancellation token that is notified if the request is aborted.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        member this.HandleAsync(cloudEvent, data, cancellationToken) =
            printfn "Storage object information:"
            printfn "  Name: %s" data.Name
            printfn "  Bucket: %s" data.Bucket
            printfn "  Size: %i" data.Size
            printfn "  Content type: %s" data.ContentType
            printfn "Cloud event information:"
            printfn "  ID: %s" cloudEvent.Id
            printfn "  Source: %O" cloudEvent.Source
            printfn "  Type: %s" cloudEvent.Type
            printfn "  Subject: %s" cloudEvent.Subject
            printfn "  DataSchema: %O" cloudEvent.DataSchema
            printfn "  DataContentType: %O" cloudEvent.DataContentType
            printfn "  Time: %s" (match Option.ofNullable cloudEvent.Time with
                                  | Some time -> time.ToUniversalTime().ToString "yyyy-MM-dd'T'HH:mm:ss.fff'Z'"
                                  | None -> "")
            printfn "  SpecVersion: %O" cloudEvent.SpecVersion

            // In this example, we don't need to perform any asynchronous operations, so we
            // just return an completed Task to conform to the interface.
            Task.CompletedTask
