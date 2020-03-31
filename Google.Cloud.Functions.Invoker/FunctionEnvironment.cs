﻿// Copyright 2020, Google LLC
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

using Google.Cloud.Functions.Framework;
using Microsoft.AspNetCore.Http;
using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;

namespace Google.Cloud.Functions.Invoker
{
    /// <summary>
    /// Everything we need to know about a function we're trying to serve - but without
    /// serving it yet. The intention of the class is to make the "start a server" code trivial
    /// when it's presented with an instance of FunctionEnvironment, but for a FunctionEnvironment
    /// to be testable without starting a server.
    /// </summary>
    internal sealed class FunctionEnvironment
    {
        /// <summary>
        /// The delegate to execute for HTTP requests.
        /// </summary>
        public RequestDelegate RequestHandler { get; }

        /// <summary>
        /// The IP Address to listen on.
        /// </summary>
        public IPAddress Address { get; }

        /// <summary>
        /// The port to listen on.
        /// </summary>
        public int Port { get; }

        private FunctionEnvironment(RequestDelegate handler, IPAddress address, int port) =>
            (RequestHandler, Address, Port) = (handler, address, port);

        internal static FunctionEnvironment Create(Assembly functionAssembly, string[] commandLine, EnvironmentVariableProvider variableProvider) =>
            new Builder(functionAssembly, commandLine, variableProvider).Build();

        private class Builder
        {
            private readonly Assembly _functionAssembly;
            private readonly string[] _commandLine;
            private readonly EnvironmentVariableProvider _variableProvider;

            internal Builder(Assembly functionAssembly, string[] commandLine, EnvironmentVariableProvider variableProvider) =>
                (_functionAssembly, _commandLine, _variableProvider) = (functionAssembly, commandLine, variableProvider);

            internal FunctionEnvironment Build()
            {
                RequestDelegate handler = BuildHandler();
                int port = DeterminePort();
                return new FunctionEnvironment(handler, IPAddress.Any, port);
            }

            private RequestDelegate BuildHandler()
            {
                // TODO: Better command line parsing, e.g. --target=xyz --port=5000
                var target = _commandLine.FirstOrDefault() ?? _variableProvider[EntryPoint.FunctionTargetEnvironmentVariable];
                if (string.IsNullOrEmpty(target))
                {
                    throw new Exception("No target provided");
                }
                var type = _functionAssembly.GetType(target);
                if (type is null)
                {
                    throw new Exception($"Can't load target type '{target}'");
                }
                var instance = Activator.CreateInstance(type);
                return instance switch
                {
                    IHttpFunction function => context => function.HandleAsync(context),
                    ICloudEventFunction function => new CloudEventAdapter(function).HandleAsync,
                    _ => throw new Exception("Function doesn't support known interfaces")
                };
            }

            private int DeterminePort()
            {
                var environmentVariable = _variableProvider[EntryPoint.PortEnvironmentVariable];
                if (environmentVariable is object)
                {
                    if (!int.TryParse(environmentVariable, NumberStyles.None, CultureInfo.InvariantCulture, out var parsed))
                    {
                        throw new Exception($"Can't parse {EntryPoint.PortEnvironmentVariable} environment variable value '{environmentVariable}'");
                    }
                    return parsed;
                }
                // TODO: Parse args
                return 8080;
            }
        }
    }
}
