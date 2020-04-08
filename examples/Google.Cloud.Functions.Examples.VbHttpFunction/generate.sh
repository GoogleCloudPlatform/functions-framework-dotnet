#!/bin/bash

set -e

rm *.vb *.vbproj
rm -rf bin obj

# Create the project and source files
dotnet new gcf-http -lang vb

# Change the package reference to a project reference
dotnet remove package Google.Cloud.Functions.Invoker
dotnet add reference ../../src/Google.Cloud.Functions.Invoker

# Explicitly import the MSBuild targets (not necessary for NuGet packages)
sed -i '2 i \ \ <Import Project="../../src/Google.Cloud.Functions.Invoker/targets/Google.Cloud.Functions.Invoker.targets"/>' *.vbproj
sed -i '3 i \ \ <Import Project="../../src/Google.Cloud.Functions.Invoker/targets/Google.Cloud.Functions.Invoker.props"/>' *.vbproj
