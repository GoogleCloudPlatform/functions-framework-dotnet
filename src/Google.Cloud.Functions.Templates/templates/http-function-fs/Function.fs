namespace MyFunction

open Google.Cloud.Functions.Framework
open Microsoft.AspNetCore.Http

type Function() =
    interface IHttpFunction with
        /// <summary>
        /// Logic for your function goes here.
        /// </summary>
        /// <param name="context">The HTTP context, containing the request and the response.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        member this.HandleAsync context =
            async {
                do! context.Response.WriteAsync "Hello, Functions Framework." |> Async.AwaitTask
            } |> Async.StartAsTask :> _
