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

Imports Google.Cloud.Functions.Framework
Imports Microsoft.AspNetCore.Http

Public Class CloudFunction
    Implements IHttpFunction

    ''' <summary>
    ''' Logic for your function goes here.
    ''' </summary>
    ''' <param name="context">The HTTP context, containing the request and the response.</param>
    ''' <returns>A task representing the asynchronous operation.</returns>
    Public Async Function HandleAsync(context As HttpContext) As Task Implements IHttpFunction.HandleAsync
        Await context.Response.WriteAsync("Hello VB function!")
    End Function
End Class
