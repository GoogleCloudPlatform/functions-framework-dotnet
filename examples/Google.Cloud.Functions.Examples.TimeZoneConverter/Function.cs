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
using Google.Cloud.Functions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using NodaTime.Text;
using NodaTime.TimeZones;
using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Google.Cloud.Functions.Examples.TimeZoneConverter
{
    /// <summary>
    /// Startup class to provide the NodaTime built-in TZDB (IANA) date time zone provider
    /// as a dependency to <see cref="Function"/>.
    /// </summary>
    public class Startup : FunctionsStartup
    {
        public override void ConfigureServices(WebHostBuilderContext context, IServiceCollection services) =>
            services.AddSingleton(DateTimeZoneProviders.Tzdb);
    }

    /// <summary>
    /// Function to convert between two time zones. Query parameters, all required:
    /// <list type="bullet">
    ///   <item><c>fromZone</c>: The ID of the time zone to convert from</item>
    ///   <item><c>toZone</c>: The ID of the time zone to convert to</item>
    ///   <item><c>when</c>: The local date/time to convert. This is treated as being in "fromZone", and then converted to "toZone".
    ///   Format: extended ISO yyyy-MM-ddTHH:mm:ss with optional subsecond digits.</item>
    /// </list>
    /// </summary>
    [FunctionsStartup(typeof(Startup))]
    public sealed class Function : IHttpFunction
    {
        private static readonly JsonSerializerOptions s_serializerOptions = new JsonSerializerOptions
        { 
            IgnoreNullValues = true,
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };
        private static readonly LocalDateTimePattern LocalPattern = LocalDateTimePattern.ExtendedIso;

        private readonly IDateTimeZoneProvider _timeZoneProvider;

        /// <summary>
        /// Constructs a converter using the given time zone provider and source.
        /// </summary>
        /// <param name="provider">The time zone provider to use.</param>
        public Function(IDateTimeZoneProvider provider) =>
            _timeZoneProvider = provider;

        /// <summary>
        /// Performs the function invocation, responding to the input in the request and providing
        /// output in the response.
        /// </summary>
        public Task HandleAsync(HttpContext context)
        {
            var result = MaybeConvert(context.Request);
            if (result is null)
            {
                // TODO: More detailed error handling, including messages.
                // See https://github.com/GoogleCloudPlatform/functions-framework-dotnet/issues/37
                context.Response.StatusCode = 400;
                return Task.CompletedTask;
            }
            else
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = StatusCodes.Status200OK;
                return JsonSerializer.SerializeAsync(context.Response.Body, result, s_serializerOptions);
            }
        }

        /// <summary>
        /// Extracts the parameters from the request, and performs a conversion if everything is value.
        /// </summary>
        /// <param name="request">The request to extract parameters from.</param>
        /// <returns>The conversion result, or null if the parameters were missing or invalid.</returns>
        private ConversionResult? MaybeConvert(HttpRequest request)
        {
            string? whenParameter = request.Query["when"].FirstOrDefault();
            string? fromZoneParameter = request.Query["fromZone"].FirstOrDefault();
            string? toZoneParameter = request.Query["toZone"].FirstOrDefault();

            // TODO: More detailed error handling, including messages.
            // See https://github.com/GoogleCloudPlatform/functions-framework-dotnet/issues/37

            // Check that we have all the parameters we expect.
            if (fromZoneParameter is null || toZoneParameter is null || whenParameter is null)
            {
                return null;
            }

            // Check that they all map to correct time zones and we can parse the "when" value as a LocalDateTime
            return
                _timeZoneProvider.GetZoneOrNull(fromZoneParameter) is DateTimeZone fromZone &&
                _timeZoneProvider.GetZoneOrNull(toZoneParameter) is DateTimeZone toZone &&
                LocalPattern.Parse(whenParameter) is { Success: true, Value: var when }
                ? Convert(when, fromZone, toZone)
                : null;
        }

        /// <summary>
        /// Performs the actual conversion, with validated inputs. (A "valid local date/time that happens to be skipped
        /// in the zone we're mapping from" still counts as skipped.)
        /// </summary>
        /// <param name="when">The parsed value from the "when" query parameter</param>
        /// <param name="fromZone">The time zone corresponding to the ID in the "fromZone" query parameter</param>
        /// <param name="toZone">The time zone corresponding to the ID in the "toZone" query parameter</param>
        /// <returns>The result of the conversion</returns>
        private ConversionResult Convert(LocalDateTime when, DateTimeZone fromZone, DateTimeZone toZone)
        {
            var mapping = fromZone.MapLocal(when);
            return mapping.Count switch
            {
                0 => ConvertSkipped(),
                1 => ConvertUnambiguous(),
                2 => ConvertAmbiguousInput(),
                _ => throw new InvalidOperationException($"{mapping.Count} mappings - bug in NodaTime?")
            };

            ConversionResult ConvertSkipped()
            {
                var shiftedForward = Resolvers.ReturnForwardShifted(when, fromZone, mapping.EarlyInterval, mapping.LateInterval);
                var converted = shiftedForward.WithZone(toZone).LocalDateTime;
                return CreateResult(converted, ConversionType.SkippedInputForwardShiftedResult);
            }

            ConversionResult ConvertUnambiguous()
            {
                var converted = mapping.First().WithZone(toZone).LocalDateTime;
                return CreateResult(converted, ConversionType.Unambiguous);
            }

            ConversionResult ConvertAmbiguousInput()
            {
                var earlyConverted = mapping.First().WithZone(toZone).LocalDateTime;
                var lateConverted = mapping.Last().WithZone(toZone).LocalDateTime;
                return CreateResult(
                    // While it would be very odd for the result of converting the earlier instant to be
                    // later (in local time) than the result of the converting the later instant, it's theoretically possible.
                    LocalDateTime.Min(earlyConverted, lateConverted),
                    earlyConverted == lateConverted ? ConversionType.AmbiguousInputUnambiguousResult : ConversionType.AmbiguousInputAmbiguousResult);
            }

            ConversionResult CreateResult(LocalDateTime converted, ConversionType conversionType) =>
                new ConversionResult(
                    _timeZoneProvider.VersionId,
                    LocalPattern.Format(when), fromZone.Id, toZone.Id,
                    LocalPattern.Format(converted), conversionType);
        }
    }
}
