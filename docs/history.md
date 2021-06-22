# Version History

## 1.0.0 (relesaed 2021-06-22)

- Update to Google.Events.Protobuf 1.0.0 and CloudNative.CloudEvents 2.0.0
- Update the Source CloudEvent attribute for Firebase RTDB events

## 1.0.0-beta05 (released 2021-06-07)

- Update to release candidate of CloudNative.CloudEvents package,
  which removes the need for a Google.Events package at all
- Minor changes to conversions to CloudEvents (with a few more coming)
- Dispose of the host instead of the TestServer in FunctionTestServer (fixes #182)
- Templates now include `appsettings*.json` for deployment (fixes #192 and #201)
- Add support for scopes (fixes #193)

## 1.0.0-beta04 (released 2020-11-19)

- Actually improve the F# templates.

Release 1.0.0-beta03 did not include the new F# templates as
expected, due to confusion between templates and examples.

## 1.0.0-beta03 (released 2020-11-19)

- Improvements to F# templates
- Improved Firebase event conversions. (There will be more changes, but these are going in the right direction.)
- Support for Firebase RemoteConfig update events
- Improved templates for Visual Studio (from version 16.8.0 onwards, with the feature enabled in options)
- TestLogEntry now overrides ToString() in a useful way
- All assemblies are now signed (due to dependency updates, particularly to use CloudNative.CloudEvents 2.0.0-beta.1)

## 1.0.0-beta02 (released 2020-10-20)

Changes around startup classes:

- FunctionsStartupAttribute can now be applied to classes as well
  as assemblies. The Hosting package detects attributes that have
  been applied to the target function type, and its base types.
- FunctionsStartupAttribute can now be applied to test assemblies
  and test classes to specify replacement startup classes;
  FunctionTestBase will automatically use this to determine
  startup classes from the test class, making it easier to fake
  out dependencies.

## 1.0.0-beta01 (released 2020-10-14)

No API changes; just dependencies, and first beta release.

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
