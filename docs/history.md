# Version History

## [2.2.1](https://github.com/GoogleCloudPlatform/functions-framework-dotnet/compare/Google.Cloud.Functions.Framework-2.2.0...Google.Cloud.Functions.Framework-2.2.1) (2024-05-02)


### Miscellaneous chores

* Update SBOM generator ([e067fa9](https://github.com/GoogleCloudPlatform/functions-framework-dotnet/commit/e067fa9dca51b7c607c4592e63117c116eec8112))

## [2.2.0](https://github.com/GoogleCloudPlatform/functions-framework-dotnet/compare/Google.Cloud.Functions.Framework-2.1.0...Google.Cloud.Functions.Framework-2.2.0) (2024-04-30)


### Bug Fixes

* Propagate framework scopes ([dedf36c](https://github.com/GoogleCloudPlatform/functions-framework-dotnet/commit/dedf36cd740276d73015146798ef149bf1f9a52e))
* **readme:** min dotnet version ([ed235ac](https://github.com/GoogleCloudPlatform/functions-framework-dotnet/commit/ed235ac72a4a781dba026bbf0e3f242609e30afe))


### Documentation

* Clarify GOOGLE_BUILDABLE meaning ([8ccefd0](https://github.com/GoogleCloudPlatform/functions-framework-dotnet/commit/8ccefd06ed3123b740c1e1245280baaca33157f2))
* Deployment instructions for .NET 8 ([dcbcfa0](https://github.com/GoogleCloudPlatform/functions-framework-dotnet/commit/dcbcfa0f24a35db351e5e5631040667dfe6ae5a9))
* Fix link in customization.md ([6f191c7](https://github.com/GoogleCloudPlatform/functions-framework-dotnet/commit/6f191c7704db5ae8673c180913b9537c3b9b024a))


### Miscellaneous chores

* release 2.2.0 ([#511](https://github.com/GoogleCloudPlatform/functions-framework-dotnet/issues/511)) ([170cb0c](https://github.com/GoogleCloudPlatform/functions-framework-dotnet/commit/170cb0c0fac78ce6e961b5d6a8218471b13c9519))

## [2.1.0](https://github.com/GoogleCloudPlatform/functions-framework-dotnet/compare/Google.Cloud.Functions.Framework-2.0.0...Google.Cloud.Functions.Framework-2.1.0) (2023-05-23)


### Features

* Implement strongly typed function signatures for dotnet ([bb812fb](https://github.com/GoogleCloudPlatform/functions-framework-dotnet/commit/bb812fb2a25d9727f616e10839b9e0304e2d5669))

## [2.0.0](https://github.com/GoogleCloudPlatform/functions-framework-dotnet/compare/Google.Cloud.Functions.Framework-2.0.0-beta01...Google.Cloud.Functions.Framework-2.0.0) (2023-02-06)

GA release targeting .NET 6.0.

## [2.0.0-beta01](https://github.com/GoogleCloudPlatform/functions-framework-dotnet/compare/Google.Cloud.Functions.Framework-1.1.0...Google.Cloud.Functions.Framework-2.0.0-beta01) (2022-11-08)


### Features

* Update all projects to target .NET 6.0 ([26cff19](https://github.com/GoogleCloudPlatform/functions-framework-dotnet/commit/26cff19ae4ccb8595ae4cbdf69fc87631f6de974))
* Use file-scoped namespaces in templates ([4bbce05](https://github.com/GoogleCloudPlatform/functions-framework-dotnet/commit/4bbce0519ae75dbfa14b9f04fb876f0f4faeb2ce))


### Documentation

* Update documentation to use dotnet6 runtime ([2a53c38](https://github.com/GoogleCloudPlatform/functions-framework-dotnet/commit/2a53c38fb86ac51e80d3c2bfec414ab3944fc9ca))


### Miscellaneous chores

* Update cryptography in pip requirements ([23367bb](https://github.com/GoogleCloudPlatform/functions-framework-dotnet/commit/23367bb94046f212bdbd813dd6e7b0ae79bf3da6))

## [1.1.0](https://github.com/GoogleCloudPlatform/functions-framework-dotnet/compare/Google.Cloud.Functions.Framework-1.0.0...Google.Cloud.Functions.Framework-1.1.0) (2022-10-18)


### Features

* Support Pub/Sub push notifications format (adapt to CloudEvent) ([af9f09d](https://github.com/GoogleCloudPlatform/functions-framework-dotnet/commit/af9f09d67f3f877c9796c8345273c7a06e114d1b)), closes [#234](https://github.com/GoogleCloudPlatform/functions-framework-dotnet/issues/234)
* Warn if the "unknown topic" resource name is used ([59bf0ed](https://github.com/GoogleCloudPlatform/functions-framework-dotnet/commit/59bf0ed957a543c3a59cb6ada58d8cc1518af4c1))


### Bug Fixes

* Change Firebase RTDB event types (.ref instead of .document) ([36200ed](https://github.com/GoogleCloudPlatform/functions-framework-dotnet/commit/36200ed1ff6ac820f7b969c8e44bb61c398927b1))
* Temporary workaround for conformance client output ([dc93d1e](https://github.com/GoogleCloudPlatform/functions-framework-dotnet/commit/dc93d1e5f90e9cfa9e5671d0828949275093b700))


### Documentation

* Add an example of deployment using a local NuGet package ([4452440](https://github.com/GoogleCloudPlatform/functions-framework-dotnet/commit/44524408cfc85f4eec2f6163866a3e6b6ef79010))
* Add missing backslash to sample deployment command line ([827b911](https://github.com/GoogleCloudPlatform/functions-framework-dotnet/commit/827b911bb10fe96a0092dd58189959b44d0a7795))
* update broken badge ([4f3476f](https://github.com/GoogleCloudPlatform/functions-framework-dotnet/commit/4f3476f660e0e1e6df8bf4c64385b27e5c2933c4))


### Dependencies

* Update CloudEvents SDK dependencies ([07fc94f](https://github.com/GoogleCloudPlatform/functions-framework-dotnet/commit/07fc94faa7b211b26920f2f28b4628548019faa0))

## 1.0.0 (released 2021-06-22)

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
