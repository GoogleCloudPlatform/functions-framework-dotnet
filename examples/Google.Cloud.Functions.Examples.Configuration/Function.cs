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

using Google.Cloud.Functions.Framework;
using Google.Cloud.Functions.Invoker;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

// FunctionsStartupAttribute is applied to the assembly to tell the Functions Framework which startup
// types to load. If you have multiple startup classes, you can apply the attribute multiple times.
// The attribute must be applied to the assembly containing the function, but it can potentially refer
// to a startup class in a different assembly.
[assembly: FunctionsStartup(typeof(Google.Cloud.Functions.Examples.Configuration.Startup))]

namespace Google.Cloud.Functions.Examples.Configuration
{
    /// <summary>
    /// An options class to be bound from the configuration.
    /// </summary>
    public class DatabaseConnectionOptions
    {
        public const string ConfigurationSection = "DbConnection";

        public string Instance { get; set; }
        public string Database { get; set; }
    }

    /// <summary>
    /// A service connecting to a database, configured using <see cref="DatabaseConnectionOptions"/>.
    /// </summary>
    public class DatabaseService
    {
        public string Instance { get; }
        public string Database { get; }

        public DatabaseService(DatabaseConnectionOptions options) =>
            (Instance, Database) = (options.Instance, options.Database);
    }

    /// <summary>
    /// The startup class is provided with a host builder which exposes a service collection
    /// and configuration. This can be used to make additional dependencies available.
    /// In this case, we configure a <see cref="DatabaseService"/> based on the configuration.
    /// </summary>
    public class Startup : FunctionsStartup
    {
        public override void ConfigureServices(WebHostBuilderContext context, IServiceCollection services)
        {
            // Bind the connection options based on the current configuration.
            DatabaseConnectionOptions options = new DatabaseConnectionOptions();
            context.Configuration
                .GetSection(DatabaseConnectionOptions.ConfigurationSection)
                .Bind(options);

            // Build the database service from the connection options.
            DatabaseService database = new DatabaseService(options);

            // Add the database service to the service collection.
            services.AddSingleton(database);
        }
    }

    /// <summary>
    /// The actual Cloud Function using the DatabaseService dependency configured in
    /// Startup.
    /// </summary>
    public class Function : IHttpFunction
    {
        private readonly DatabaseService _database;

        public Function(DatabaseService database) =>
            _database = database;

        public async Task HandleAsync(HttpContext context)
        {
            await context.Response.WriteAsync($"Retrieving data from instance '{_database.Instance}', database '{_database.Database}'");
        }
    }
}
