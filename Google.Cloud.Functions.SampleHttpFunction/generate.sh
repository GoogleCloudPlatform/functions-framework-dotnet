#!/bin/bash

set -e

rm *.cs *.csproj
rm -rf bin obj

# Create the project and source files
dotnet new gcf-http

# Change the package reference to a project reference
dotnet remove package Google.Cloud.Functions.Invoker
dotnet add reference ../Google.Cloud.Functions.Invoker

# Explicitly import the MSBuild targets (not necessary for NuGet packages)
sed -i '2 i \ \ <Import Project="../Google.Cloud.Functions.Invoker/targets/Google.Cloud.Functions.Invoker.targets"/>' *.csproj
sed -i '3 i \ \ <Import Project="../Google.Cloud.Functions.Invoker/targets/Google.Cloud.Functions.Invoker.props"/>' *.csproj

# Disable packing warning, to keep Travis warning-free
sed -i 's/<PropertyGroup>/<PropertyGroup>\n    <WarnOnPackingNonPackableProject>False<\/WarnOnPackingNonPackableProject>/g' *.csproj
