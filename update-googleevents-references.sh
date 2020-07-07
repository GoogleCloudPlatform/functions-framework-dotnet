#!/bin/bash

set -e

if [[ -z "$1" ]]
then
  echo 'Please specify the Google.Events.* version to update to.'
  exit 1
fi

VERSION=$1

for proj in $(find src -name '*proj') $(find examples -name '*proj')
do
  sed -i -e "s/Include=\"Google\.Events\" Version=\".*\"/Include=\"Google.Events\" Version=\"$VERSION\"/g" $proj
  sed -i -e "s/Include=\"Google\.Events\.SystemTextJson\" Version=\".*\"/Include=\"Google.Events.SystemTextJson\" Version=\"$VERSION\"/g" $proj
  sed -i -e "s/Include=\"Google\.Events\.Protobuf\" Version=\".*\"/Include=\"Google.Events.Protobuf\" Version=\"$VERSION\"/g" $proj
done
