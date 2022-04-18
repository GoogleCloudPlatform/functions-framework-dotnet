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

generate OpenFunction.Examples.SimpleHttpFunction gcf-http 'c#'
generate OpenFunction.Examples.SimpleEventFunction gcf-event 'c#'
generate OpenFunction.Examples.SimpleUntypedEventFunction gcf-untyped-event 'c#'
generate OpenFunction.Examples.FSharpHttpFunction gcf-http 'f#'
generate OpenFunction.Examples.FSharpEventFunction gcf-event 'f#'
generate OpenFunction.Examples.FSharpUntypedEventFunction gcf-untyped-event 'f#'
generate OpenFunction.Examples.VbHttpFunction gcf-http 'vb'
generate OpenFunction.Examples.VbEventFunction gcf-event 'vb'
generate OpenFunction.Examples.VbUntypedEventFunction gcf-untyped-event 'vb'
