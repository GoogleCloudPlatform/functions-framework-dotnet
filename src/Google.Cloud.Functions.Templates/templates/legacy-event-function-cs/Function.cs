using Google.Cloud.Functions.Framework.LegacyEvents;
using System;
using System.Threading.Tasks;

namespace MyFunction
{
    /// <summary>
    /// A function that can be triggered in responses to changes in Google Cloud Storage.
    /// The type argument (StorageObject in this case) determines how the event payload is deserialized.
    /// The event must be deployed so that the trigger matches the expected payload type. (For example,
    /// deploying a function expecting a StorageObject payload will not work for a trigger that provides
    /// a FirestoreEvent.)
    /// </summary>
    public class Function : ILegacyEventFunction<StorageObject>
    {
        /// <summary>
        /// Logic for your function goes here. Note that a Legacy Event function just consumes an event;
        /// it doesn't provide any response.
        /// </summary>
        /// <param name="data">The Cloud Event your function should respond to.</param>
        /// <param name="context">The Cloud Event your function should respond to.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task HandleAsync(StorageObject data, Context context)
        {
            Console.WriteLine("Context:");
            Console.WriteLine($"  ID: {context.Id}");
            Console.WriteLine($"  Type: {context.Type}");
            Console.WriteLine($"  Timestamp: {context.Timestamp:yyyy-MM-dd'T'HH:mm:ss.fff'Z'}");
            Console.WriteLine($"  Resource name: {context.Resource.Name}");
            Console.WriteLine($"  Resource service: {context.Resource.Service}");
            Console.WriteLine($"  Resource type: {context.Resource.Type}");
            Console.WriteLine("Storage object:");
            Console.WriteLine($"  Name: {data.Name}");
            Console.WriteLine($"  Bucket: {data.Bucket}");
            Console.WriteLine($"  Size: {data.Size}");
            Console.WriteLine($"  Content type: {data.ContentType}");

            // In this example, we don't need to perform any asynchronous operations, so the
            // method doesn't need to be declared async.
            return Task.CompletedTask;
        }
    }
}
