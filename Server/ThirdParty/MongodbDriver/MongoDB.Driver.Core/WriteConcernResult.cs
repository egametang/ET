/* Copyright 2010-2015 MongoDB Inc.
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

using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents the results of an operation performed with an acknowledged WriteConcern.
    /// </summary>
#if NET45
    [Serializable]
#endif
    public class WriteConcernResult
    {
        // fields
        private readonly BsonDocument _response;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="WriteConcernResult"/> class.
        /// </summary>
        /// <param name="response">The response.</param>
        public WriteConcernResult(BsonDocument response)
        {
            _response = Ensure.IsNotNull(response, nameof(response));
        }

        // properties
        /// <summary>
        /// Gets the number of documents affected.
        /// </summary>
        public long DocumentsAffected
        {
            get
            {
                BsonValue value;
                return _response.TryGetValue("n", out value) ? value.ToInt64() : 0;
            }
        }

        /// <summary>
        /// Gets whether the result has a LastErrorMessage.
        /// </summary>
        public bool HasLastErrorMessage
        {
            get { return _response.GetValue("err", false).ToBoolean(); }
        }

        /// <summary>
        /// Gets the last error message (null if none).
        /// </summary>
        public string LastErrorMessage
        {
            get
            {
                var err = _response.GetValue("err", false);
                return (err.ToBoolean()) ? err.ToString() : null;
            }
        }

        /// <summary>
        /// Gets the _id of an upsert that resulted in an insert.
        /// </summary>
        public BsonValue Upserted
        {
            get
            {
                return _response.GetValue("upserted", null);
            }
        }

        /// <summary>
        /// Gets whether the last command updated an existing document.
        /// </summary>
        public bool UpdatedExisting
        {
            get
            {
                var updatedExisting = _response.GetValue("updatedExisting", false);
                return updatedExisting.ToBoolean();
            }
        }

        /// <summary>
        /// Gets the wrapped result.
        /// </summary>
        public BsonDocument Response
        {
            get { return _response; }
        }
    }
}
