namespace MyFunction

open Google.Cloud.Functions.Framework.LegacyEvents
open System.Threading.Tasks

/// <summary>
/// A function that can be triggered in responses to changes in Google Cloud Storage.
/// The type argument (StorageObject in this case) determines how the event payload is deserialized.
/// The event must be deployed so that the trigger matches the expected payload type. (For example,
/// deploying a function expecting a StorageObject payload will not work for a trigger that provides
/// a FirestoreEvent.)
/// </summary>
type Function() =
    interface ILegacyEventFunction<StorageObject> with
        /// <summary>
        /// Logic for your function goes here. Note that a Legacy Event function just consumes an event;
        /// it doesn't provide any response.
        /// </summary>
        /// <param name="payload">The payload of the legacy event.</param>
        /// <param name="context">The metadata of the event (timestamp etc).</param>
        /// <param name="cancellationToken">A cancellation token that is notified if the request is aborted.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        member this.HandleAsync(payload, context, cancellationToken) =
            printfn "Context:"
            printfn "  ID: %s" context.Id
            printfn "  Type: %s" context.Type
            printfn "  Resource name: %s" context.Resource.Name
            printfn "  Resource service: %s" context.Resource.Service
            printfn "  Resource type: %s" context.Resource.Type
            printfn "Storage object:"
            printfn "  Name: %s" payload.Name
            printfn "  Bucket: %s" payload.Bucket
            printfn "  Size: %A" payload.Size
            printfn "  Content type: %s" payload.ContentType
            Task.CompletedTask
