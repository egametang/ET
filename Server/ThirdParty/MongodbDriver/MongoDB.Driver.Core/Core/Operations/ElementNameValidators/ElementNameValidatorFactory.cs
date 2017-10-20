/* Copyright 2013-2015 MongoDB Inc.
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

using MongoDB.Bson.IO;

namespace MongoDB.Driver.Core.Operations.ElementNameValidators
{
    /// <summary>
    /// Represents a factory for element name validators based on the update type.
    /// </summary>
    public static class ElementNameValidatorFactory
    {
        /// <summary>
        /// Returns an element name validator for the update type.
        /// </summary>
        /// <param name="updateType">Type of the update.</param>
        /// <returns>An element name validator.</returns>
        public static IElementNameValidator ForUpdateType(UpdateType updateType)
        {
            switch (updateType)
            {
                case UpdateType.Replacement:
                    return CollectionElementNameValidator.Instance;
                case UpdateType.Update:
                    return UpdateElementNameValidator.Instance;
                default:
                    return new UpdateOrReplacementElementNameValidator();
            }
        }
    }
}
