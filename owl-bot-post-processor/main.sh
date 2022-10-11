#!/bin/bash
# Copyright 2022 Google LLC
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

# Run this script from the root directory of the repository, like this:
#    $ ./owlbot-post-processor/main.sh

# This script ensures that all version information is kept in sync and properly
# formatted. OwlBot will run this on every pull request, and modify the PR to
# include any changes.

set -ex

# While this is currently extremely simple, we expect this script to
# eventually have additional responsibilities.
./update-project-references.sh
