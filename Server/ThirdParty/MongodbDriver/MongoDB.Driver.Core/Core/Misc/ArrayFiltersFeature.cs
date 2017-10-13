﻿/* Copyright 2017 MongoDB Inc.
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

using MongoDB.Bson;
using System;
using System.Collections.Generic;

namespace MongoDB.Driver.Core.Misc
{
    /// <summary>
    /// Represents the arrayFilters feature.
    /// </summary>
    /// <seealso cref="MongoDB.Driver.Core.Misc.Feature" />
    public class ArrayFiltersFeature : Feature
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayFiltersFeature"/> class.
        /// </summary>
        /// <param name="name">The name of the feature.</param>
        /// <param name="firstSupportedVersion">The first server version that supports the feature.</param>
        public ArrayFiltersFeature(string name, SemanticVersion firstSupportedVersion)
            : base(name, firstSupportedVersion)
        {
        }

        /// <summary>
        /// Throws if arrayFilters value is not null and arrayFilters are not supported.
        /// </summary>
        /// <param name="serverVersion">The server version.</param>
        /// <param name="value">The value.</param>
        public void ThrowIfNotSupported(SemanticVersion serverVersion, IEnumerable<BsonDocument> value)
        {
            if (value != null && !base.IsSupported(serverVersion))
            {
                throw new NotSupportedException($"Server version {serverVersion} does not support arrayFilters.");
            }
        }
    }
}
