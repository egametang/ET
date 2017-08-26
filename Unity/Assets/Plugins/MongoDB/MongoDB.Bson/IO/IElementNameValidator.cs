/* Copyright 2010-2014 MongoDB Inc.
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

namespace MongoDB.Bson.IO
{
    /// <summary>
    /// Represents an element name validator. Used by BsonWriters when WriteName is called
    /// to determine if the element name is valid.
    /// </summary>
    public interface IElementNameValidator
    {
        /// <summary>
        /// Gets the validator to use for child content (a nested document or array).
        /// </summary>
        /// <param name="elementName">The name of the element.</param>
        /// <returns>The validator to use for child content.</returns>
        IElementNameValidator GetValidatorForChildContent(string elementName);

        /// <summary>
        /// Determines whether the element name is valid.
        /// </summary>
        /// <param name="elementName">The name of the element.</param>
        /// <returns>True if the element name is valid.</returns>
        bool IsValidElementName(string elementName);
    }
}
