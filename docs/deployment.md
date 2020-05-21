# Deployment

## Deploying locally

If your project depends on the Google.Cloud.Functions.Invoker
package, your function can run locally either by starting it through
Visual Studio or by running `dotnet run` from the command line. If
your project contains multiple functions, you must specify which one
you want to host either via command line arguments or environment
variables. The port to serve on can also be specified via the
command line or environment variables.

See the [README](../README.md) for configuration details.

(If you aren't using the invoker package, your deployment procedure
may be slightly different. See the [packages
documentation](packages.md) for details about what the invoker
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

> **NOTE:**  
> The `dotnet3` runtime is currently available on an "allow
> list" basis. Deployment will fail unless your project has been
> specifically included in the list. While there is no public sign-up
> available for this list, we expect to make that available soon,
> at which point this documentation will be updated with a sign-up
> link.

Deployment to Google Cloud Functions is easy with the `gcloud`
command line. See the [general deployment
documentation](https://cloud.google.com/functions/docs/deploying/filesystem)
for more detailed information; this page is only intended to give simple
"getting started" instructions. In general, deploying a .NET
Functions Framework function follows the same procedure as deploying
a function written in every language. The .NET-specific aspects are:

- The runtime should be specified as `dotnet3`
- The entry point should be specified as the name of the function
  type, including namespace (e.g. `HelloFunctions.Function`)

The command line options are used to specify how the function is
triggered.

### HTTP functions

HTTP functions are deployed using `--trigger-http`. For example:

```text
gcloud functions deploy hello-functions --runtime dotnet3 --trigger-http --entry-point HelloFunctions.Function
```

On successful deployment, details of the deployed function will be
displayed, including the URL. Visit the URL in a browser to invoke
your function.

### Cloud Event functions

When you deploy a function listening for a particular event, you
have to specify the *event trigger* as part of the deployment
(instead of `--trigger-http` as above).

**Sample triggers**

Replace any ***bold italic*** parts with your project ID, bucket name and so forth.

Trigger type             | Payload type   | Sample command line options
------------------------ | -------------- | --------------------------
Cloud Storage operation  | StorageObject  | --trigger-event google.storage.object.finalize --trigger-resource ***my-gcs-bucket***
Pub/Sub message          | PubSubMessage  | --trigger-topic ***my-pubsub-topic-id***
Firestore event          | FirestoreEvent | --trigger-event providers/cloud.firestore/eventTypes/document.write --trigger-resource 'projects/***my-project***/databases/(default)/documents/***my-collection***/{document}'

> **Notes for the Firestore trigger**:  
> - The quotes around the Firestore resource are to avoid having to escape the parentheses in `(default)`.
> - The `{document}` part at the end intentionally has braces. You can change the `document` part if you wish;
>   it's just the wildcard name.
> - At the time of writing, the wildcards extracted into `event.params` [as described in the
>   documentation](https://cloud.google.com/functions/docs/calling/cloud-firestore) are currently
>   available in the `FirestoreEvent` class via the `Wildcards` property. This is subject to change,
>   as it's inconsitent with other Functions Frameworks.

