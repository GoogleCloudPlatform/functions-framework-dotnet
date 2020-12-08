// Copyright 2020, Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Google.Cloud.Functions.Framework
{
    /// <summary>
    /// A dynamic function, which is expected to have either a Handle or a HandleAsync
    /// method. The relevant method is invoked automatically with arguments populated via
    /// dependency injection.
    /// </summary>
    public interface IDynamicFunction
    {
    }
}
