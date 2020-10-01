# Version History

## 1.0.0-alpha13 (released 2020-10-01)

- Modify the Hosting package MSBuild targets to only create an entry point for
  consuming projects with an output type of Exe.

## 1.0.0-alpha12 (released 2020-09-25)

Just package renaming:

- Old Google.Cloud.Functions.Invoker package is now Google.Cloud.Functions.Hosting
- Old Google.Cloud.Functions.Invoker.Testing package is now Google.Cloud.Functions.Testing

## 1.0.0-alpha11 (released 2020-09-03)

- Fixed FunctionTestBase.ExecuteCloudEvent
- Fixed access to logs in nested/generic classes via FunctionTestServer

## 1.0.0-alpha10 (released 2020-09-02)

- Added more convenience methods in FunctionTestBase
- Format Firestore subject/source as we expect for native CloudEvents
- Create source URIs for CloudEvents using RelativeOrAbsolute

## 1.0.0-alpha09 (released 2020-08-25)

- Rearchitect [customization](customization.md) and hosting startup
- Add a FunctionTestBase class to simplify integration testing
- Suppressed Kestrel heartbeat warnings (which are expected given
  the "run occasionally" nature of Functions servers)
- Add public in-memory ILogger implementation for unit testing

## 1.0.0-alpha08 (released 2020-07-07)

- Added an in-memory logger provider to the test server
- Moved the event data classes out of the Functions Framework into
  `Google.Events.*`. We have a dependency on `Google.Events` but
  not on any specific serialization strategy.
- Changed the shape of the PubSub CloudEvent data to match the event
  on Cloud Run. (The message is wrapped in another class that will
  eventually contain the subscription name.)
- Separated out the bucket name (source) and object name (subject) for
  storage CloudEvents.

## 1.0.0-alpha07 (released 2020-06-02)

Latest version available when deploying to Google Cloud Functions
(`gcloud functions deploy`) became available via a restricted public
alpha.
