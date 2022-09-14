#!/bin/bash

# Environment variables:
# - COMMITTISH_OVERRIDE: The commit to actually build the release from, if not the one that has been checked out
# - SKIP_NUGET_PUSH: If non-empty, the push to nuget.org is skipped

set -e

SCRIPT=$(readlink -f "$0")
SCRIPT_DIR=$(dirname "$SCRIPT")

cd $SCRIPT_DIR/..

# Make sure secrets are loaded in a well known location before running releasetool
source $SCRIPT_DIR/populatesecrets.sh
populate_all_secrets

NUGET_API_KEY="$(cat "$SECRETS_LOCATION"/google-cloud-nuget-api-key)"

# Make sure we have the most recent version of pip, then install other packages.
python -m pip install --require-hashes -r pip-requirements.txt
python -m pip install --require-hashes -r requirements.txt
python -m releasetool publish-reporter-script > /tmp/publisher-script

# The publish reporter script uses "python3" which doesn't exist on Windows.
# Work out what we should use instead.
# Try to detect Python 3. It's quite different between Windows and Linux.
if which python > /dev/null && python --version 2>&1 | grep -q "Python 3"; then declare -r PYTHON3=python
elif which py > /dev/null && py -3 --version 2>&1 | grep -q "Python 3"; then declare -r PYTHON3="py -3"
elif which python3 > /dev/null && python3 --version 2>&1 | grep -q "Python 3"; then declare -r PYTHON3=python3
else
  echo "Unable to detect Python 3 installation."
  exit 1
fi

# Fix up the publish reporter script using $PYTHON3. We assume this won't
# be harmful within sed - at the moment it's always "python", "py -3" or "python3".
sed -i "s/python3/$PYTHON3/g" /tmp/publisher-script

source /tmp/publisher-script

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
  for pkg in *.nupkg
  do
    dotnet nuget push -s https://api.nuget.org/v3/index.json -k $NUGET_API_KEY $pkg
  done
  cd ../../..
else
  echo "Skipping NuGet push"
fi
