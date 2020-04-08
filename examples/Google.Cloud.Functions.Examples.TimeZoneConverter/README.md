# TimeZoneConverter example function

This function uses the [Noda Time](https://nodatime.org) library to
perform time zone conversions based on the [IANA time zone
database](https://www.iana.org/time-zones).

## Parameters

All input is provided via the following query parameters:

- `fromZone`: The ID of the time zone to convert from
- `toZone`: The ID of the time zone to convert to
- `when`: The local date/time to convert. This is treated as being in "fromZone", and then converted to "toZone".

Time zone IDs should be valid IANA IDs, e.g. "Europe/London",
"Europe/Paris", "America/Los_Angeles".

Local date time values - both in the query and in the result - use
ISO-8601 extended format with optional subsecond precision to the
nanosecond, e.g.

- 2020-04-09T08:10:15 (just to the second)
- 2020-04-09T08:10:15.123 (to the millisecond)
- 2020-04-09T08:10:15.123456 (to the microsecond)
- 2020-04-09T08:10:15.123456789 (to the nanosecond)

## Results

The result is returned as a JSON object with the following fields:

- `data_version`: the data version returned from Noda Time, e.g. "TZDB: 2019b (mapping: 14742)"
  (Only the "2019b" part is relevant here; a later version of this function may remove the other parts.)
- `input`: The input local date/time provided to the function as the `when` parameter
- `from_zone`: The "from" time zone provided to the function as the `fromZone` parameter
- `to_zone`: The "to" time zone provided to the function as the `toZone` parameter
- `result`: The output local date/time - this is the core of the result of the conversion
- `conversion_type`: Time zone conversions are usually unambiguous, but changes in offset (e.g. due to
  daylight saving transitions) can mean that the input either doesn't exist at all in the "from" zone,
  or occurs twice. This field will have one of four values:
  - `"Unambiguous"`: The input was unambiguous, mapping to a single instant in time.
  - `"AmbiguousInputAmbiguousResult"`: The input was ambiguous, usually due to occurring within a "fall back" daylight saving transition.
    The conversion to the target time zone of each value did not resolve this ambiguity. The result is the earlier of the results.
  - `"AmbiguousInputUnambiguousResult"`: The input was ambiguous, usually due to occurring within a "fall back" daylight saving transition.
    However, after converting both possible instants to the target time zone, the results are the same.
    This is usually due to converting between time zones which observe the same daylight saving transitions.
  - `"SkippedInputForwardShiftedResult"`: The input was skipped, usually due to occurring within a "spring forward" daylight saving transition.
    The result is provided by shifting the input value by the length of the "gap" in local time (usually one hour).

## Examples

To try this function out, run it locally on port 8080 and use the following URLs:

- http://localhost:8080/?when=2020-04-09T08:50:00.123&fromZone=Europe/London&toZone=Europe/Paris
  This converts an unambiguous input from London to Paris.
- http://localhost:8080/?when=2020-03-29T01:50:00.123&fromZone=Europe/London&toZone=Europe/Paris
  This convers an input which is skipped in London due to "spring forward in March", converting it to Paris.
- http://localhost:8080/?when=2020-10-25T01:50:00.123&fromZone=Europe/London&toZone=Europe/Paris
  This converts an input which is ambiguous in London due to "fall back in October" in London, but converting to Paris which "falls back" at the same time- so the result is unambiguous.
- http://localhost:8080/?when=2020-10-25T01:50:00.123&fromZone=Europe/London&toZone=America/Los_Angeles
  This converts an input which is ambiguous in London, but converting to the Los Angeles time zone which "falls back" in November - so the result is also ambiguous.
