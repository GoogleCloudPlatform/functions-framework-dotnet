#!/bin/bash

set -e

# Script to automate the release process. Assumptions:
# - docs/history.md has already been updated (but not committed)
# - any Google.Events references have already been updated
# - the version number in *this* repo has not been updated

# Pass in the new version number as the sole command line argument

if [[ -z "$1" ]]
then
  echo 'Please specify the new version number.'
  exit 1
fi

OLD_VERSION=$(grep '<Version>.*</Version>' src/CommonProperties.xml  | sed -e 's/<\/\?Version>//g' | sed 's/ //g')
NEW_VERSION=$1

if [[ $OLD_VERSION == $NEW_VERSION ]]
then
  echo "Error: already at $OLD_VERSION. Either finish manually, or revert version changes."
  exit 1
fi

# Update CommonProperties.xml
sed -i -e "s/<Version>.*<\/Version>/<Version>$NEW_VERSION<\/Version>/g" src/CommonProperties.xml

# Update all references/templates
./update-project-references.sh

# Update the README instructions for template installation.
# The binary mode (-b) here is to preserve existing line endings.
# (At some point we should probably make all line endings consistent...)
sed -i -b -e "s/Templates::$OLD_VERSION/Templates::$NEW_VERSION/g" README.md

# Build the commit message up as a file for simplicity
mkdir -p tmp

echo "Release Functions Framework .NET packages version $NEW_VERSION" > tmp/commit.txt
echo "" >> tmp/commit.txt
echo "Changes since $OLD_VERSION:" >> tmp/commit.txt
# Skip the first three lines of docs/history.md (header, blank line, subheader for this release)
# Then take lines until we get to the next release, skipping that line with "head".
tail -n +4 docs/history.md | sed '/##/q' | head -n -1 >> tmp/commit.txt

# TODO: Automate finding the packages we're releasing
echo "Packages in this release:" >> tmp/commit.txt
echo "- Release Google.Cloud.Functions.Framework version $NEW_VERSION" >> tmp/commit.txt
echo "- Release Google.Cloud.Functions.Hosting version $NEW_VERSION" >> tmp/commit.txt
echo "- Release Google.Cloud.Functions.Templates version $NEW_VERSION" >> tmp/commit.txt
echo "- Release Google.Cloud.Functions.Testing version $NEW_VERSION" >> tmp/commit.txt

# Commit!
git commit -a -F tmp/commit.txt

echo "Created commit:"
git show -s

echo ""
echo "Changes committed updating from $OLD_VERSION to $NEW_VERSION. Please push to GitHub."
