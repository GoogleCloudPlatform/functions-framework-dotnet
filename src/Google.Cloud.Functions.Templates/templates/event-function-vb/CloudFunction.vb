' Copyright 2020, Google LLC
'
' Licensed under the Apache License, Version 2.0 (the "License");
' you may Not use this file except In compliance With the License.
' You may obtain a copy of the License at
'
'     https://www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law Or agreed to in writing, software
' distributed under the License Is distributed on an "AS IS" BASIS,
' WITHOUT WARRANTIES Or CONDITIONS OF ANY KIND, either express Or implied.
' See the License for the specific language governing permissions And
' limitations under the License.

Imports CloudNative.CloudEvents
Imports Google.Cloud.Functions.Framework

Public Class CloudFunction
    Implements ICloudEventFunction

    ''' <summary>
    ''' Logic for your function goes here. Note that a Cloud Event function just consumes an event;
    ''' it doesn't provide any response.
    ''' </summary>
    ''' <param name="cloudEvent">The Cloud Event your function should respond to.</param>
    ''' <returns>A task representing the asynchronous operation.</returns>
    Public Function HandleAsync(cloudEvent As CloudEvent) As Task Implements ICloudEventFunction.HandleAsync
        Console.WriteLine("Cloud event information:")
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
