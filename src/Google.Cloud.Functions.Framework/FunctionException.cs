// Copyright 2020, Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Net;

namespace Google.Cloud.Functions.Framework
{
    // More decisions and considerations:
    // - Another message to be set on the response? (As content? Something else?)
    // - Prohibit success status codes?
    // - What happens if we've already started writing a streaming response? (Is that feasible?)
    // - What's the best way of controlling the logging?
    // - Use an int instead of HttpStatusCode, as per HttpContext.Response.StatusCode?
    
    /// <summary>
    /// Exception to provide a convenient way of aborting a function with a specific HTTP status code.
    /// When a function is executed via Google.Cloud.Functions.Invoker package, this exception is caught
    /// at the top level, the message is logged, and the response status code is set accordingly.
    /// This exception should generally not be caught within function code.
    /// </summary>
    public class FunctionException : Exception
    {
        /// <summary>
        /// The status code to set in the response.
        /// </summary>
        public HttpStatusCode StatusCode { get; }

        /// <summary>
        /// Constructs an instance with the given status code and message. The message (if any) will be logged,
        /// but not included in the response.
        /// If the calling code has already logged the error, use a null message to avoid duplicate logging.
        /// </summary>
        /// <param name="statusCode">The status code to set in the response.</param>
        /// <param name="message">The message to log, or null if no logging is required.</param>
        public FunctionException(HttpStatusCode statusCode, string? message) : this(statusCode, message, null)
        {
        }

        /// <summary>
        /// Constructs an instance with the given status code, message (if any) and exception (if any).
        /// The message and exception will be logged, but not included in the response.
        /// If the calling code has already logged the error, use a null message to avoid duplicate logging.
        /// </summary>
        /// <param name="statusCode">The status code to set in the response.</param>
        /// <param name="message">The message to log, or null if no logging is required.</param>
        /// <param name="innerException">An exception causing this one. This will be logged.</param>
        public FunctionException(HttpStatusCode statusCode, string? message, Exception? innerException)
            : base(message, innerException)
        {
            StatusCode = statusCode;
        }
    }
}
