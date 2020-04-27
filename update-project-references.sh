#!/bin/bash

set -e

VERSION=$(grep '<Version>.*</Version>' src/CommonProperties.xml  | sed -e 's/<\/\?Version>//g' | sed 's/ //g')

for proj in $(find src/Google.Cloud.Functions.Templates/templates -name '*proj')
do
  sed -i -e "s/Include=\"Google.Cloud.Functions.Invoker\" Version=\".*\"/Include=\"Google.Cloud.Functions.Invoker\" Version=\"$VERSION\"/g" $proj
done

for proj in $(find examples -name '*proj')
do
  sed -i -e "s/Include=\"Google.Cloud.Functions.Invoker\" Version=\".*\"/Include=\"Google.Cloud.Functions.Invoker\" Version=\"$VERSION\"/g" $proj
  sed -i -e "s/Include=\"Google.Cloud.Functions.Invoker.Testing\" Version=\".*\"/Include=\"Google.Cloud.Functions.Invoker.Testing\" Version=\"$VERSION\"/g" $proj
done
