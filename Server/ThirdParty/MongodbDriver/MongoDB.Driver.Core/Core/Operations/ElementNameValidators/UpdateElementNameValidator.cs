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
    /// Represents an element name validator for update operations.
    /// </summary>
    public class UpdateElementNameValidator : IElementNameValidator
    {
        // private static fields
        private static readonly UpdateElementNameValidator __instance = new UpdateElementNameValidator();

        // public static fields
        /// <summary>
        /// Gets a pre-created instance of an UpdateElementNameValidator.
        /// </summary>
        /// <value>
        /// The pre-created instance.
        /// </value>
        public static UpdateElementNameValidator Instance
        {
            get { return __instance; }
        }

        // methods
        /// <inheritdoc/>
        public IElementNameValidator GetValidatorForChildContent(string elementName)
        {
            return NoOpElementNameValidator.Instance;
        }

        /// <inheritdoc/>
        public bool IsValidElementName(string elementName)
        {
            return elementName.Length > 0 && elementName[0] == '$';
        }
    }
}
