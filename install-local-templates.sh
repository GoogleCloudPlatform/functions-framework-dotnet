#!/bin/sh

# This script builds a templates package using a date-based version, and installs it via "dotnet new -i"

REAL_VERSION=$(grep '<Version>.*</Version>' src/CommonProperties.xml  | sed -e 's/<\/\?Version>//g' | sed 's/ //g')

VERSION=$REAL_VERSION-$(date -u +%Y%m%d%H%M)

export Configuration=Release
dotnet pack -p:Version=$VERSION src/Google.Cloud.Functions.Templates

NUGET_SOURCE=$PWD/src/Google.Cloud.Functions.Templates/bin/Release/

# Come out of the functions-framework-dotnet directory to use the default SDK,
# which is what VS uses.
$(cd .. && dotnet new -i Google.Cloud.Functions.Templates::$VERSION --nuget-source=$NUGET_SOURCE)

echo "Installed local templates as version $VERSION"
