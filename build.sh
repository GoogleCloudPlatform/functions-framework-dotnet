#!/bin/bash

set -e

# Make sure that SourceLink uses the GitHub repo, even if that's not where
# our origin remote points at.
git remote add github https://github.com/GoogleCloudPlatform/functions-framework-dotnet.git
export GitRepositoryRemoteName=github

export ContinuousIntegrationBuild=true
export Configuration=Release
# When building examples, build against the version in this
# repo rather than against NuGet; this allows us to make breaking
# changes.
export LocalFunctionsFramework=true

echo Building...
dotnet build -nologo -clp:NoSummary -v quiet src
dotnet build -nologo -clp:NoSummary -v quiet examples

echo Testing...
dotnet test -nologo --no-build -v quiet src
dotnet test -nologo --no-build -v quiet examples

echo Packing...
rm -rf nupkg
dotnet pack -nologo -v quiet src -o $PWD/nupkg

echo Created packages:
ls nupkg

# Remove the github remote so that if there are multiple iterations
# against the same clone, the "git remote add" earlier will work.
git remote remove github