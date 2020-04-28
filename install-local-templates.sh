#!/bin/sh

# This script builds a templates package using a date-based version, and installs it via "dotnet new -i"

REAL_VERSION=$(grep '<Version>.*</Version>' src/CommonProperties.xml  | sed -e 's/<\/\?Version>//g' | sed 's/ //g')

VERSION=$REAL_VERSION-$(date -u +%Y%m%d%H%M)

export Configuration=Release
dotnet pack -p:Version=$VERSION src/Google.Cloud.Functions.Templates
dotnet new -i Google.Cloud.Functions.Templates::$VERSION \
  --nuget-source=src/Google.Cloud.Functions.Templates/bin/Release/

echo "Installed local templates as version $VERSION"
