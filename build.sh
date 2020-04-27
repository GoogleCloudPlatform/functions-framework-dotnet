#!/bin/bash

set -e

export ContinuousIntegrationBuild=true
export Configuration=Release

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
