Imports CloudNative.CloudEvents
Imports Google.Cloud.Functions.Framework
Imports System.Threading

Public Class CloudFunction
    Implements ICloudEventFunction

    ''' <summary>
    ''' Logic for your function goes here. Note that a CloudEvent function just consumes an event;
    ''' it doesn't provide any response.
    ''' </summary>
    ''' <param name="cloudEvent">The CloudEvent your function should consume.</param>
    ''' <param name="cancellationToken">A cancellation token that is notified if the request is aborted.</param>
    ''' <returns>A task representing the asynchronous operation.</returns>
    Public Function HandleAsync(cloudEvent As CloudEvent, cancellationToken As CancellationToken) As Task _
        Implements ICloudEventFunction.HandleAsync
        Console.WriteLine("CloudEvent information:")
        Console.WriteLine($"ID: {cloudEvent.Id}")
        Console.WriteLine($"Source: {cloudEvent.Source}")
        Console.WriteLine($"Type: {cloudEvent.Type}")
        Console.WriteLine($"Subject: {cloudEvent.Subject}")
        Console.WriteLine($"DataSchema: {cloudEvent.DataSchema}")
        Console.WriteLine($"DataContentType: {cloudEvent.DataContentType}")
        Console.WriteLine($"Time: {cloudEvent.Time?.ToUniversalTime():yyyy-MM-dd'T'HH:mm:ss.fff'Z'}")
        Console.WriteLine($"SpecVersion: {cloudEvent.SpecVersion}")
        Console.WriteLine($"Data: {cloudEvent.Data}")

        ' In this example, we don't need to perform any asynchronous operations, so the
        ' function doesn't need to be declared as Async.
        Return Task.CompletedTask
    End Function
End Class
