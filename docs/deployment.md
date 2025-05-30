# Deployment

## Deploying locally

If your project depends on the Google.Cloud.Functions.Hosting
package, your function can run locally either by starting it through
Visual Studio or by running `dotnet run` from the command line. If
your project contains multiple functions, you must specify which one
you want to host either via command line arguments or environment
variables. The port to serve on can also be specified via the
command line or environment variables.

See the [README](../README.md) for configuration details.

(If you aren't using the hosting package, your deployment procedure
may be slightly different. See the [packages
documentation](packages.md) for details about what the hosting
package does, and considerations for writing functions which don't
use it.)

## Deploying to Cloud Run

Running the Functions Framework is equivalent to running a regular
ASP.NET Core application as far as infrastructure is concerned. This
means you can run functions on Cloud Run with ease.

There are detailed instructions for C# available as part of the
Cloud Run documentation:

- [Quickstart for C#](https://cloud.google.com/run/docs/quickstarts/build-and-deploy#c)
- [Codelab for C#](https://codelabs.developers.google.com/codelabs/cloud-run-hello-csharp/index.html)

The instructions apply to functions in exactly the same way as any
other Cloud Run application, with the same Dockerfile configuration.
The `PORT` environment variable will be supplied by Cloud Run. If
your project contains multiple functions, the simplest approach is
to specify an environment variable in your Dockerfile, with a line
like this:

```text
ENV FUNCTION_TARGET HelloFunctions.Function
```

## Deploying to Google Cloud Functions

Deployment to Google Cloud Functions is easy with the `gcloud`
command line. See the [general deployment
documentation](https://cloud.google.com/functions/docs/deploying/filesystem)
for more detailed information; this page is only intended to give simple
"getting started" instructions. In general, deploying a .NET
Functions Framework function follows the same procedure as deploying
a function written in every language. The .NET-specific aspects are:

- The runtime should be specified as `dotnet8`
- The entry point should be specified as the name of the function
  type, including namespace (e.g. `HelloFunctions.Function`)

The command line options are used to specify how the function is
triggered.

### Redeploying a previous 1st-generation function

The `dotnet8` runtime is only supported in Cloud Run functions,
formerly known as "Cloud Functions (2nd gen)".

Using `gcloud functions deploy` for *new* functions uses this
environment by default. If you need to redeploy a function which was
previously deployed on "Cloud Functions (1st gen)" you have two
options:

- (Recommended) Delete the 1st gen function, and then deploy as
  normal.
- Specify the `--gen2` flag to explicitly request 2nd generation.
  This will leave the existing 1st gen function in place, deploying
  a new Cloud Run function alongside it.

### HTTP functions

HTTP functions are deployed using `--trigger-http`. For example:

```text
gcloud functions deploy hello-functions --runtime dotnet8 --trigger-http --entry-point HelloFunctions.Function
```

On successful deployment, details of the deployed function will be
displayed, including the URL. Visit the URL in a browser to invoke
your function.

### CloudEvent functions

When you deploy a function listening for a particular event, you
have to specify the *event trigger* as part of the deployment
(instead of `--trigger-http` as above).

**Sample triggers**

Replace any ***bold italic*** parts with your project ID, bucket name and so forth.

Trigger type             | Payload type          | Sample command line options
------------------------ | --------------------- | --------------------------
Cloud Storage operation  | StorageObjectData     | --trigger-event google.storage.object.finalize --trigger-resource ***my-gcs-bucket***
Pub/Sub message          | MessagePublishedData  | --trigger-topic ***my-pubsub-topic-id***
Firestore event          | DocumentEventData     | --trigger-event providers/cloud.firestore/eventTypes/document.write --trigger-resource 'projects/***my-project***/databases/(default)/documents/***my-collection***/{document}'

> **Notes for the Firestore trigger**:  
> - The quotes around the Firestore resource are to avoid having to escape the parentheses in `(default)`.
> - The `{document}` part at the end intentionally has braces. You can change the `document` part if you wish;
>   it's just the wildcard name.
> - At the time of writing, the wildcards extracted into `event.params` [as described in the
>   documentation](https://cloud.google.com/functions/docs/calling/cloud-firestore) are currently
>   available in the `FirestoreEvent` class via the `Wildcards` property. This is subject to change,
>   as it's inconsitent with other Functions Frameworks.

### Triggering an HTTP function with a Cloud Pub/Sub push subscription

HTTP functions can work as the endpoints for [Cloud Pub/Sub push
subscriptions](https://cloud.google.com/pubsub/docs/push). If your
function implements `ICloudEventFunction` or
`ICloudEventFunction<MessagePublishedData>`, the Functions Framework
will adapt the incoming HTTP request to present it to your function
as a CloudEvent, as if you had deployed via `--trigger-topic`. The
requests for push subscriptions do not contain topic data, but if
you create the push subcription with a URL of
`https://<your-function>/projects/<project-id>/topics/<topic-id>`
then the Functions Framework will infer the topic name from the path
of the HTTP request. If the topic name cannot be inferred
automatically, a topic name of
`projects/unknown-project!/topics/unknown-topic!` will be used in
the CloudEvent.

> Note: the Functions Framework code to adapt the incoming HTTP
> request works with the [Cloud Pub/Sub
> Emulator](https://cloud.google.com/pubsub/docs/emulator), but
> some information is not included in emulator push subscription
> requests.

### Deploying a function with a local project dependency

Real world functions are often part of a larger application which
will usually contain common code for data types and common business
logic. If your function depends on another project via a local
project reference (a `<ProjectReference>` element in your .csproj
file), the source code for that project must also be included when
deploying to Google Cloud Functions. Additionally, you need to
specify which project contains the function you wish to deploy.

In a typical directory structure where all projects sit side-by-side
within one top-level directory, this will mean you need to deploy
from that top-level directory instead of from the function's project
directory. You also need to use the `--set-build-env-vars` command
line flag to specify the `GOOGLE_BUILDABLE` build-time environment
variable. This tells the Google Cloud Functions deployment process
the path to the project to build and deploy. Note that the
`GOOGLE_BUILDABLE` environment variable value is case-sensitive,
and should match the directory and file names used. Additionally,
note that the value is the path to the project, *not* a .NET assembly
name or namespace. (In many cases these will be the same, but not
always.)

When deploying a function with multiple projects, it's important to
make sure you have a suitable
[.gcloudignore](https://cloud.google.com/sdk/gcloud/reference/topic/gcloudignore)
file, so that you only upload the code that you want to. In
particular, you should almost always include `bin/` and `obj/` in the
`.gcloudignore` file so that you don't upload your locally-built
binaries.

See [the multi-project example
documentation](examples.md#multiprojectfunction-and-multiprojectdependency)
for a sample deployment command line, as well as sample projects.

### Deploying a function with an internal NuGet dependency

Similar to the above description, real world functions may depend on
internally-hosted NuGet packages. While the Google Cloud Functions build
system doesn't have access to the internal packages, they can be fetched
(as .nupkg files) as part of a pre-deployment step, included in a subdirectory
of the function's source directory, and referenced via a `nuget.config` file.

Unlike the example with multiple projects, the deployment command line
doesn't need to be changed for this. It would be feasible to combine the two
techniques, using a directory containing all the packages used by all the local
projects.

See [the local NuGet packages example
documentation](examples.md#localnugetpackagefunction-and-localnugetpackagecode)
for more details.
