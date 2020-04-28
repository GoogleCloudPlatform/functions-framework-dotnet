Imports CloudNative.CloudEvents
Imports Google.Cloud.Functions.Framework.LegacyEvents
Imports System.Threading

''' <summary>
''' A function that can be triggered in responses to changes in Google Cloud Storage.
''' The type argument (StorageObject in this case) determines how the event payload is deserialized.
''' The event must be deployed so that the trigger matches the expected payload type. (For example,
''' deploying a function expecting a StorageObject payload will not work for a trigger that provides
''' a FirestoreEvent.)
''' </summary>
Public Class CloudFunction
    Implements ILegacyEventFunction(Of StorageObject)

    ''' <summary>
    ''' Logic for your function goes here. Note that a Legacy Event function just consumes an event;
    ''' it doesn't provide any response.
    ''' </summary>
    ''' <param name="payload">The payload of the legacy event.</param>
    ''' <param name="context">The metadata of the event (timestamp etc).</param>
    ''' <param name="cancellationToken">A cancellation token that is notified if the request is aborted.</param>
    ''' <returns>A task representing the asynchronous operation.</returns>
    Public Function HandleAsync(payload As StorageObject, context As Context, cancellationToken As CancellationToken) As Task _
        Implements ILegacyEventFunction(Of StorageObject).HandleAsync
        Console.WriteLine("Context:")
        Console.WriteLine($"  ID: {context.Id}")
        Console.WriteLine($"  Type: {context.Type}")
        Console.WriteLine($"  Timestamp: {context.Timestamp:yyyy-MM-dd'T'HH:mm:ss.fff'Z'}")
        Console.WriteLine($"  Resource name: {context.Resource.Name}")
        Console.WriteLine($"  Resource service: {context.Resource.Service}")
        Console.WriteLine($"  Resource type: {context.Resource.Type}")
        Console.WriteLine("Storage object:")
        Console.WriteLine($"  Name: {payload.Name}")
        Console.WriteLine($"  Bucket: {payload.Bucket}")
        Console.WriteLine($"  Size: {payload.Size}")
        Console.WriteLine($"  Content type: {payload.ContentType}")

        ' In this example, we don't need to perform any asynchronous operations, so the
        ' function doesn't need to be declared as Async.
        Return Task.CompletedTask
    End Function
End Class
