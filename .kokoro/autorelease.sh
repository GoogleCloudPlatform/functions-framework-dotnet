#!/bin/bash

# Environment variables:
# - COMMITTISH_OVERRIDE: The commit to actually build the release from, if not the one that has been checked out
# - SKIP_NUGET_PUSH: If non-empty, the push to nuget.org is skipped

set -e

SCRIPT=$(readlink -f "$0")
SCRIPT_DIR=$(dirname "$SCRIPT")

cd $SCRIPT_DIR/..

# Make sure secrets are loaded in a well known location before running
# the release reporter script.
source $SCRIPT_DIR/populatesecrets.sh
populate_all_secrets

NUGET_API_KEY="$(cat "$SECRETS_LOCATION"/google-cloud-nuget-api-key)"

dotnet tool restore
source <(dotnet release-progress-reporter publish-reporter-script)

COMMITTISH=$COMMITTISH_OVERRIDE
if [[ $COMMITTISH_OVERRIDE = "" ]]
then
  COMMITTISH=HEAD
fi

TAG=$(git tag --points-at $COMMITTISH | head -n 1)

if [[ $TAG = "" ]]
then
  echo "Committish $COMMITTISH does not point at a tag. Aborting."
  exit 1
fi

echo "Building with tag $TAG"

# Build the release and run the tests.
./build-release.sh $TAG

if [[ $SKIP_NUGET_PUSH = "" ]]
then
  echo "Pushing NuGet packages"
  # Push the changes to nuget.
  cd ./tmp/release/nupkg
  
  # First generate all the SBOMs.
  for pkg in *.nupkg
  do
    dotnet generate-sbom $pkg
  done
  
  # Only start pushing to NuGet when SBOM generation has succeeded.
  for pkg in *.nupkg
  do
    dotnet nuget push -s https://api.nuget.org/v3/index.json -k $NUGET_API_KEY $pkg
  done
  cd ../../..
else
  echo "Skipping NuGet push"
fi
