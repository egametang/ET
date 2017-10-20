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

using System.Collections.Generic;
using MongoDB.Bson.IO;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.Operations.ElementNameValidators
{
    /// <summary>
    /// Represents an element name validator that checks that element names are valid for MongoDB collections.
    /// </summary>
    public class CollectionElementNameValidator : IElementNameValidator
    {
        // private static fields
        private static readonly List<string> __exceptions = new List<string> { "$db", "$ref", "$id", "$code", "$scope" };
        private static readonly CollectionElementNameValidator __instance = new CollectionElementNameValidator();

        // public static fields
        /// <summary>
        /// Gets a pre-created instance of a CollectionElementNameValidator.
        /// </summary>
        /// <value>
        /// The pre-created instance.
        /// </value>
        public static CollectionElementNameValidator Instance
        {
            get { return __instance; }
        }

        // methods
        /// <inheritdoc/>
        public IElementNameValidator GetValidatorForChildContent(string elementName)
        {
            return this;
        }

        /// <inheritdoc/>
        public bool IsValidElementName(string elementName)
        {
            Ensure.IsNotNull(elementName, nameof(elementName));

            if (elementName.Length == 0)
            {
                // the server seems to allow empty element names, but in 1.x we did not
                return false;
            }

            if (elementName.IndexOf('.') != -1)
            {
                return false;
            }

            if (elementName[0] == '$' && !__exceptions.Contains(elementName))
            {
                return false;
            }

            return true;
        }
    }
}
