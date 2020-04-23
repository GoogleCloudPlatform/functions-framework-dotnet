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

  # Change the package reference to a project reference
  dotnet remove package Google.Cloud.Functions.Invoker > /dev/null
  dotnet add reference ../../src/Google.Cloud.Functions.Invoker > /dev/null

  # Explicitly import the MSBuild targets (not necessary for NuGet packages)
  sed -i '2 i \ \ <Import Project="../../src/Google.Cloud.Functions.Invoker/targets/Google.Cloud.Functions.Invoker.targets"/>' *.*proj
  sed -i '3 i \ \ <Import Project="../../src/Google.Cloud.Functions.Invoker/targets/Google.Cloud.Functions.Invoker.props"/>' *.*proj
  
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
generate Google.Cloud.Functions.Examples.VbHttpFunction gcf-http 'vb'
generate Google.Cloud.Functions.Examples.VbEventFunction gcf-event 'vb'
