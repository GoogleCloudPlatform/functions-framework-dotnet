These are extra examples to see if the existing Functions Framework
and CloudEvent integration paths can easily meet the use cases we
want to support, or if additional functionality is required.

## Testing the functions

Start the desired function (e.g. set it as the startup project and
hit Run in Visual Studio, or cd into the directory and run `dotnet
run`) then use curl to fire requests. There are sample requests in
this directory:

- `custom-payload-request.json` for all the custom payload examples
- Google payloads for the GoogleTypeDetection example:
  - `google-storage-request.json`
  - `google-pubsub-request.json`
  - `google-firestore-request.json`

Sample curl command:

```sh
curl -X POST -H 'Content-Type:application/cloudevents' http://localhost:8080 -d @custom-payload-request.json 
```

## Examples in this directory

### CustomPayload1

This is for a customer-defined payload, where the customer is able
and willing to add the `[CloudEventFormatter]` attribute which
enables the Functions Framework (and anything else using the SDK) to
"know" how to deserialize.

In this case, we use
CloudNative.CloudEvents.SystemTextJson.JsonEventFormatter, which
uses System.Text.Json to deserialize. We need to add attributes to
the payload properties as System.Text.Json is case-sensitive, but
that's not CloudEvent-specific at all.

### CustomPayload2 

This is for a third-party-defined payload, where the customer is
unable or unwilling to add the `[CloudEventFormatter]` attribute.
Instead, they specify a startup class (see [the repo
docs](../docs/customization.md) for more info) to inject a
`CloudEventFormatter` for the Functions Framework to use.

In this case, we've added a reference to
CloudNative.CloudEvents.NewtonsoftJson and used the
JsonEventFormatter from that package. That's case-insensitive, so
the payload class is really just a POCO with no extra attributes.

Note that although the payload is in the same project in the example
(just for simplicity), it could easily be in a third-party package.

### CustomPayload3

This is for a third-party-defined payload where the third-party
doesn't want to add a CloudEvents reference in their main package
for whatever reason, but is happy to have a separate package to
enable simple integration with the Functions Framework. This
separate package depends on the Functions Framework, and provides
the same kind of startup class as in CustomPayload2 - the customer
just needs to refer to that startup class rather than writing it
themselves.

(The code in this example would be spread across three separate
packages in reality.)

### GoogleTypeDetection

This is an example of a function intended to handle any Google event
type, with automatic deserializing to the "right" kind of object.

The GoogleEventFormatter code would be added to
Google.Events.Protobuf, with the dictionary part being generated (C#
makes this easy via partial classes).

The customer code requiring a startup class isn't ideal, although
it's really not very much code. We can't add that class to either
the Functions Framework or the Google.Events.Protobuf package as
they don't know about each other (at the moment). We *could* create
an additional package just to contain that startup code... or we
could try to work out another approach.

(One possible option would be to create a "marker" IGoogleEvent
interface, make all the Google event classes implement it. That
could be decorated with the CloudEventFormatter attribute to let it
all just work. I'm not sure how I feel about that idea, but we could
test it.)

## Supporting code

### LoggingFunction: used to log requests

This is just an HTTP function that logs the headers and body of a
request.