Imports CloudNative.CloudEvents
Imports Google.Cloud.Functions.Framework
Imports Google.Events.SystemTextJson.Cloud.Storage.V1
Imports System.Threading

''' <summary>
''' A function that can be triggered in responses to changes in Google Cloud Storage.
''' The type argument (StorageObjectData in this case) determines how the event payload is deserialized.
''' The event must be deployed so that the trigger matches the expected payload type. (For example,
''' deploying a function expecting a StorageObject payload will not work for a trigger that provides
''' a FirestoreEvent.)
''' </summary>
Public Class CloudFunction
    Implements ICloudEventFunction(Of StorageObjectData)

    ''' <summary>
    ''' Logic for your function goes here. Note that a Cloud Event function just consumes an event;
    ''' it doesn't provide any response.
    ''' </summary>
    ''' <param name="cloudEvent">The Cloud Event your function should consume.</param>
    ''' <param name="data">The deserialized data within the Cloud Event.</param>
    ''' <param name="cancellationToken">A cancellation token that is notified if the request is aborted.</param>
    ''' <returns>A task representing the asynchronous operation.</returns>
    Public Function HandleAsync(cloudEvent As CloudEvent, data As StorageObjectData, cancellationToken As CancellationToken) As Task _
        Implements ICloudEventFunction(Of StorageObjectData).HandleAsync
        Console.WriteLine("Storage object information:")
        Console.WriteLine($"  Name: {data.Name}")
        Console.WriteLine($"  Bucket: {data.Bucket}")
        Console.WriteLine($"  Size: {data.Size}")
        Console.WriteLine($"  Content type: {data.ContentType}")
        Console.WriteLine("Cloud event information:")
        Console.WriteLine($"  ID: {cloudEvent.Id}")
        Console.WriteLine($"  Source: {cloudEvent.Source}")
        Console.WriteLine($"  Type: {cloudEvent.Type}")
        Console.WriteLine($"  Subject: {cloudEvent.Subject}")
        Console.WriteLine($"  DataSchema: {cloudEvent.DataSchema}")
        Console.WriteLine($"  DataContentType: {cloudEvent.DataContentType}")
        Console.WriteLine($"  Time: {cloudEvent.Time?.ToUniversalTime():yyyy-MM-dd'T'HH:mm:ss.fff'Z'}")
        Console.WriteLine($"  SpecVersion: {cloudEvent.SpecVersion}")
        ' In this example, we don't need to perform any asynchronous operations, so the
        ' function doesn't need to be declared as Async.
        Return Task.CompletedTask
    End Function
End Class
