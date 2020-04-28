#!/bin/bash

set -e

generate() {
  declare -r DIRECTORY=$1
  declare -r TEMPLATE_NAME=$2
  declare -r LANG=$3
  
  echo "Generating $DIRECTORY"

  rm -rf $DIRECTORY
  mkdir $DIRECTORY
  cd $DIRECTORY
  
  # Create the project and source files
  dotnet new $TEMPLATE_NAME -lang $LANG > /dev/null
  
  # Add copyright notices
  for source in *.??
  do
    mv $source $source.tmp
    cat ../copyright.txt $source.tmp > $source
    # Convert comment format in VB
    if [[ "$LANG" == "vb" ]]
    then
      sed -i -e "s/^\/\//\'/g" $source
    fi
    rm $source.tmp
  done
  
  cd ..
}

generate Google.Cloud.Functions.Examples.SimpleHttpFunction gcf-http 'c#'
generate Google.Cloud.Functions.Examples.SimpleEventFunction gcf-event 'c#'
generate Google.Cloud.Functions.Examples.SimpleLegacyEventFunction gcf-legacy-event 'c#'
generate Google.Cloud.Functions.Examples.FSharpHttpFunction gcf-http 'f#'
generate Google.Cloud.Functions.Examples.FSharpEventFunction gcf-event 'f#'
generate Google.Cloud.Functions.Examples.FSharpLegacyEventFunction gcf-legacy-event 'f#'
generate Google.Cloud.Functions.Examples.VbHttpFunction gcf-http 'vb'
generate Google.Cloud.Functions.Examples.VbEventFunction gcf-event 'vb'
generate Google.Cloud.Functions.Examples.VbLegacyEventFunction gcf-legacy-event 'vb'
