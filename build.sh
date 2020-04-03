#!/bin/bash

set -e

export Configuration=Release

dotnet build src
dotnet test src
dotnet build examples
dotnet test examples
dotnet pack src
