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
using MongoDB.Bson.IO;

namespace MongoDB.Bson.Serialization
{
    /// <summary>
    /// An interface implemented by a polymorphic serializer.
    /// </summary>
    public interface IBsonPolymorphicSerializer
    {
        /// <summary>
        /// Gets a value indicating whether this serializer's discriminator is compatible with the object serializer.
        /// </summary>
        /// <value>
        /// <c>true</c> if this serializer's discriminator is compatible with the object serializer; otherwise, <c>false</c>.
        /// </value>
        bool IsDiscriminatorCompatibleWithObjectSerializer { get; }
    }
}
