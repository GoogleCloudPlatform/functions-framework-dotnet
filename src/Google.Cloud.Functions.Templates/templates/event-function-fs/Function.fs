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
        /// <param name="cancellationToken">A cancellation token that is notified if the request is aborted.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        member this.HandleAsync(cloudEvent, cancellationToken) =
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
