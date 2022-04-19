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

generate OpenFunction.Examples.SimpleHttpFunction of-http 'c#'
generate OpenFunction.Examples.SimpleEventFunction of-event 'c#'
generate OpenFunction.Examples.SimpleUntypedEventFunction of-untyped-event 'c#'
generate OpenFunction.Examples.FSharpHttpFunction of-http 'f#'
generate OpenFunction.Examples.FSharpEventFunction of-event 'f#'
generate OpenFunction.Examples.FSharpUntypedEventFunction of-untyped-event 'f#'
generate OpenFunction.Examples.VbHttpFunction of-http 'vb'
generate OpenFunction.Examples.VbEventFunction of-event 'vb'
generate OpenFunction.Examples.VbUntypedEventFunction of-untyped-event 'vb'
