#!/bin/bash

# Runs the conformance tests from
# https://github.com/GoogleCloudPlatform/functions-framework-conformance

# That repository is included as a submodule already, under
# functions-framework-conformance

# This script checks that the submodule is already present, but
# does not assume it has already been built.
# It assumes that "go" is already in the path.

set -e

rm -rf tmp/conformance-test-output
mkdir -p tmp/conformance-test-output

if [[ ! -f functions-framework-conformance/README.md ]]
then
  echo "Conformance test repo not found. Init and update submodules."
  exit 1
fi

# Build the conformance test framework
echo "Building conformance test framework"
(cd functions-framework-conformance/client && go build)

# Build the conformance functions up-front
echo "Building conformance functions"
dotnet build -nologo -clp:NoSummary -v quiet -c Release src/Google.Cloud.Functions.ConformanceTests

CLIENT_BINARY=functions-framework-conformance/client/client
if [[ $OSTYPE =~ ^win* || $OSTYPE =~ ^msys* || $OSTYPE =~ ^cygwin* ]]
then
  CLIENT_BINARY=${CLIENT_BINARY}.exe
fi

# Run the Functions Framework once as a "warm-up", killing it after 5 seconds.
# This is necessary on MacOS for non-obvious reasons;
# it's possible that the conformance test runner just expects it to be
# ready a little bit earlier than it is.
# TODO: Remove this when we can.
echo "Running Functions Framework for 5 seconds as a warm-up step."
dotnet src/Google.Cloud.Functions.ConformanceTests/bin/Release/netcoreapp3.1/Google.Cloud.Functions.ConformanceTests.dll HttpFunction &
DOTNETPID=$!
sleep 5
kill $DOTNETPID

# Note: we run the DLL directly rather than using "dotnet run" as this
# responds more correctly to being killed by the conformance test runner
# on Linux.
DOTNET_DLL=src/Google.Cloud.Functions.ConformanceTests/bin/Release/netcoreapp3.1/Google.Cloud.Functions.ConformanceTests.dll

echo "Using conformance test runner binary: $CLIENT_BINARY"
echo "Using Functions Framework test binary: $DOTNET_DLL"

# Run the tests
run_test() {
  TEST_TYPE=$1
  TEST_FUNCTION=$2
  echo "Running '$TEST_TYPE' test with function '$TEST_FUNCTION'"
  (cd tmp/conformance-test-output && \
   mkdir $TEST_TYPE && \
   cd $TEST_TYPE && \
   ../../../$CLIENT_BINARY \
     -buildpacks=false \
     -type=$TEST_TYPE \
     -cmd="dotnet ../../../$DOTNET_DLL $TEST_FUNCTION" \
  )
}

run_test http HttpFunction
run_test cloudevent UntypedCloudEventFunction

echo "Tests completed successfully"
