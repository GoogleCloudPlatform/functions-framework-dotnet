# Microsoft.NET.Sdk.Web and launchSettings.json

You may well have reached this page due to a link in an exception,
while trying to start the Functions Framework Invoker.

If you start a project with an `Sdk` attribute of
"Microsoft.NET.Sdk.Web" in Visual Studio, it will create a
`launchSettings.json` file if one doesn't already exist, and pass a
single string, `%LAUNCHER_ARGS%` to the `Main` method.

That's useful for a regular ASP.NET Core application, but isn't
appropriate for a Functions Framework application using the
Google.Cloud.Functions.Framework.Invoker package which *always* uses
Kestrel, gets its port from the `PORT` environment variable etc.

There are three simple solutions to this issue. In order of
simplicity and preference:

- Change the `Sdk` attribute in the root element of your project
  file to "Microsoft.NET.Sdk", and delete any existing
  `launchSettings.json` file. At that point Visual Studio will
  treat it as a regular console application project. Using the
  Debugging property tab in Visual Studio will create a new
  `launchSettings.json`, but the way that file is used does not
  interfere with the Functions Framework Invoker.
- Add an element
  `<NoDefaultLaunchSettingsFile>true</NoDefaultLaunchSettingsFile>`
  within a `<PropertyGroup>` in your project file, and delete any
  existing `launchSettings.json` file. That will stop Visual Studio
  from creating a `launchSettings.json` file, so you won't experience
  this problem.
- Stop using the Functions Framework Invoker. If you want to run your
  function as part of a regular ASP.NET Core app, you can do so - and
  you *may* find it useful to still refer to the
  Google.Cloud.Functions.Framework package, just not the invoker.

There may be more complex options you wish to investigate if none
of the above options work in your situation. It would be useful if
you could [file an issue](https://github.com/GoogleCloudPlatform/functions-framework-dotnet/issues/new)
to provide some context, so we can help you find those options and
perhaps work around the problem within the framework in the future.
