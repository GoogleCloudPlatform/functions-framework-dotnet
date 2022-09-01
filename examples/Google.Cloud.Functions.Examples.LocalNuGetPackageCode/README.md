This directory contains code that is used within
Google.Cloud.Functions.Examples.LocalNuGetPackageFunction.

The intention is to simulate the common situation where a company
has a local NuGet source containing all its own packages, and wishes
to deploy a function that depends on one of those packages.

The .nupkg file corresponding to this code is in the function
directory. Normally this wouldn't be checked into source control of
course, but that makes it easier to clone this repository and deploy
the sample function immediately. In normal practice the package
would be built as part of the normal build and deployment process, or
potentially fetched from the corporate package source if it had
already been built.
