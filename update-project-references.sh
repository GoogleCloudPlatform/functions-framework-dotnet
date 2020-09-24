#!/bin/bash

set -e

VERSION=$(grep '<Version>.*</Version>' src/CommonProperties.xml  | sed -e 's/<\/\?Version>//g' | sed 's/ //g')

for proj in $(find src/Google.Cloud.Functions.Templates/templates -name '*proj')
do
  sed -i -e "s/Include=\"Google.Cloud.Functions.Hosting\" Version=\".*\"/Include=\"Google.Cloud.Functions.Hosting\" Version=\"$VERSION\"/g" $proj
done

for proj in $(find examples -name '*proj')
do
  sed -i -e "s/Include=\"Google.Cloud.Functions.Hosting\" Version=\".*\"/Include=\"Google.Cloud.Functions.Hosting\" Version=\"$VERSION\"/g" $proj
  sed -i -e "s/Include=\"Google.Cloud.Functions.Testing\" Version=\".*\"/Include=\"Google.Cloud.Functions.Testing\" Version=\"$VERSION\"/g" $proj
done
