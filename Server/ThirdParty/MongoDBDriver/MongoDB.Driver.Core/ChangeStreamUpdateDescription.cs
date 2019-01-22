/* Copyright 2017-present MongoDB Inc.
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

using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver.Core.Misc;
using MongoDB.Shared;

namespace MongoDB.Driver
{
    /// <summary>
    /// An UpdateDescription in a ChangeStreamDocument instance.
    /// </summary>
    public sealed class ChangeStreamUpdateDescription
    {
        // private fields
        private readonly string[] _removedFields;
        private readonly BsonDocument _updatedFields;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeStreamUpdateDescription" /> class.
        /// </summary>
        /// <param name="updatedFields">The updated fields.</param>
        /// <param name="removedFields">The removed fields.</param>
        public ChangeStreamUpdateDescription(
            BsonDocument updatedFields,
            string[] removedFields)
        {
            _updatedFields = Ensure.IsNotNull(updatedFields, nameof(updatedFields));
            _removedFields = Ensure.IsNotNull(removedFields, nameof(removedFields));
        }

        // public properties
        /// <summary>
        /// Gets the removed fields.
        /// </summary>
        /// <value>
        /// The removed fields.
        /// </value>
        public string[] RemovedFields => _removedFields;

        /// <summary>
        /// Gets the updated fields.
        /// </summary>
        /// <value>
        /// The updated fields.
        /// </value>
        public BsonDocument UpdatedFields => _updatedFields;

        // public methods
        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(ChangeStreamUpdateDescription))
            {
                return false;
            }

            var other = (ChangeStreamUpdateDescription)obj;
            return
                _removedFields.SequenceEqual(other._removedFields) &&
                _updatedFields.Equals(other._updatedFields);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return new Hasher()
                .HashElements(_removedFields)
                .Hash(_updatedFields)
                .GetHashCode();
        }
    }
}
