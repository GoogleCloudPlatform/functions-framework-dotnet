#!/bin/bash

set -e

REPO_ROOT=$(git rev-parse --show-toplevel)

# This script is designed to be used in the buildpack validation
# hook. Its purpose is to provide a function which can be built
# using a buildpack, but with the latest (potentially unreleased)
# version of the Functions Framework.

# Steps:
#
# - Clear out tmp
# - Copy the conformance test function source code
# - Create a nupkg subdirectory under that
# - Build and pack the Functions Framework into nupkg using a
#   version number of 999.0.0.
# - Create a nuget.config to refer to nupkg (as a relative directory)
# - Modify the csproj file to:
#   - Remove the local project reference
#   - Remove the imports
#   - Add a package reference

rm -rf $REPO_ROOT/tmp
mkdir $REPO_ROOT/tmp

TMP_TESTS=$REPO_ROOT/tmp/Google.Cloud.Functions.ConformanceTests

cp -r $REPO_ROOT/src/Google.Cloud.Functions.ConformanceTests $REPO_ROOT/tmp
rm -rf $TMP_TESTS/bin
rm -rf $TMP_TESTS/obj
mkdir $TMP_TESTS/nupkg

dotnet pack $REPO_ROOT/src \
  -o $TMP_TESTS/nupkg \
  -p:Version=999.0.0

mv $TMP_TESTS/standalone-nuget.config $TMP_TESTS/nuget.config

sed -i /Import/d $TMP_TESTS/Google.Cloud.Functions.ConformanceTests.csproj

dotnet remove $TMP_TESTS/Google.Cloud.Functions.ConformanceTests.csproj \
    reference ../Google.Cloud.Functions.Hosting/Google.Cloud.Functions.Hosting.csproj
dotnet add $TMP_TESTS/Google.Cloud.Functions.ConformanceTests.csproj \
    package Google.Cloud.Functions.Hosting -v 999.0.0 --no-restore
