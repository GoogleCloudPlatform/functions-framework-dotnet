#!/bin/bash

set -e

VERSION=$(grep '<Version>.*</Version>' src/CommonProperties.xml  | sed -e 's/<\/\?Version>//g' | sed 's/ //g')

for proj in $(find src/OpenFunction.Templates/templates -name '*proj')
do
  sed -i -e "s/Include=\"OpenFunction.Hosting\" Version=\".*\"/Include=\"OpenFunction.Hosting\" Version=\"$VERSION\"/g" $proj
done

for proj in $(find examples -name '*proj')
do
  sed -i -e "s/Include=\"OpenFunction.Hosting\" Version=\".*\"/Include=\"OpenFunction.Hosting\" Version=\"$VERSION\"/g" $proj
  sed -i -e "s/Include=\"OpenFunction.Testing\" Version=\".*\"/Include=\"OpenFunction.Testing\" Version=\"$VERSION\"/g" $proj
done
