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

using System;

namespace MongoDB.Bson.Serialization
{
    /// <summary>
    /// A marker interface that represents serialization options.
    /// </summary>
    public interface IBsonSerializationOptions
    {
        /// <summary>
        /// Apply an attribute to these serialization options and modify the options accordingly.
        /// </summary>
        /// <param name="serializer">The serializer that these serialization options are for.</param>
        /// <param name="attribute">The serialization options attribute.</param>
        void ApplyAttribute(IBsonSerializer serializer, Attribute attribute);
        /// <summary>
        /// Clones the serialization options.
        /// </summary>
        /// <returns>A cloned copy of the serialization options.</returns>
        IBsonSerializationOptions Clone();
        /// <summary>
        /// Freezes the serialization options.
        /// </summary>
        /// <returns>The frozen serialization options.</returns>
        IBsonSerializationOptions Freeze();
    }
}
