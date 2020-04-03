#!/bin/bash

set -e

export Configuration=Release

dotnet build src
dotnet test src
dotnet build examples
dotnet pack src
