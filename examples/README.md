The following projects are the result of creating new projects from
the templates:

- Google.Cloud.Functions.Examples.SimpleHttpFunction
- Google.Cloud.Functions.Examples.SimpleEventFunction
- Google.Cloud.Functions.Examples.FSharpHttpFunction
- Google.Cloud.Functions.Examples.FSharpEventFunction
- Google.Cloud.Functions.Examples.VbHttpFunction
- Google.Cloud.Functions.Examples.VbEventFunction

In each case, after creating the project, the following changes are
applied:

- Change the package reference for Google.Cloud.Functions.Invoker
  into a local project reference
- Import the MSBuild files from the Invoker project (these are
  imported automatically when the library is used as a package
  reference)
- Add a copyright notice to the source code
