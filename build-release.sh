#!/bin/bash

if [[ -z "$1" ]]
then
  echo "This script does not take any args"
  exit 1
fi

set -e

./build.sh

