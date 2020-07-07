# Version History

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
