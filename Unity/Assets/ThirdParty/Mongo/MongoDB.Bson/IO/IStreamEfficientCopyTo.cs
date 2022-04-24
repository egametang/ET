/* Copyright 2019-present MongoDB Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System.IO;

namespace MongoDB.Bson.IO
{
    /// <summary>
    /// Represents the effective CopyTo method.
    /// </summary>
    public interface IStreamEfficientCopyTo
    {
        /// <summary>
        /// Copy the current stream to the destination without making unnecessary copies of the bytes.
        /// </summary>
        /// <param name="destination">The destination stream.</param>
        void EfficientCopyTo(Stream destination);
    }
}
