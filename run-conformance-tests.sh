#!/bin/bash

# Runs the conformance tests from
# https://github.com/GoogleCloudPlatform/functions-framework-conformance

# That repository is included as a submodule already, under
# functions-framework-conformance

# This script assumes the submodule is already up-to-date, but not
# necessarily built. It assumes that "go" is already in the path.

set -e

rm -rf tmp/conformance-test-output
mkdir -p tmp/conformance-test-output

# Build the conformance test framework
echo "Building conformance test framework"
(cd functions-framework-conformance/client && go build)

# Build the conformance functions up-front
echo "Building conformance functions"
dotnet build -nologo -clp:NoSummary -v quiet -c Release src/Google.Cloud.Functions.ConformanceTests

# Run the tests
run_test() {
  TEST_TYPE=$1
  TEST_FUNCTION=$2
  echo "Running '$TEST_TYPE' test with function '$TEST_FUNCTION'"
  (cd tmp/conformance-test-output && \
   mkdir $TEST_TYPE && \
   cd $TEST_TYPE && \
   ../../../functions-framework-conformance/client/client.exe \
     -type $TEST_TYPE \
     -cmd "dotnet run --no-build -p ../../../src/Google.Cloud.Functions.ConformanceTests -c Release -- $TEST_FUNCTION" \
  )
}

run_test http HttpFunction
# TODO: use "cloudevent" instead of "ce" once the framework has been fixed
# (It assumes fall-through in case statements right now.)
run_test ce UntypedCloudEventFunction

echo "Tests complete"
