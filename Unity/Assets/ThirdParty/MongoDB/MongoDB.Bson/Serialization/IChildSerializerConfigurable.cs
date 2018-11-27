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

namespace MongoDB.Bson.Serialization
{
    // this interface is public so custom serializers can choose to implement it
    // but typically you would choose to implement this interface explicitly
    // these methods support forwarding attributes to child serializers and wouldn't normally be public

    /// <summary>
    /// Represents a serializer that has a child serializer that configuration attributes can be forwarded to.
    /// </summary>
    public interface IChildSerializerConfigurable
    {
        /// <summary>
        /// Gets the child serializer.
        /// </summary>
        /// <value>
        /// The child serializer.
        /// </value>
        IBsonSerializer ChildSerializer { get; }

        /// <summary>
        /// Returns a serializer that has been reconfigured with the specified child serializer.
        /// </summary>
        /// <param name="childSerializer">The child serializer.</param>
        /// <returns>The reconfigured serializer.</returns>
        IBsonSerializer WithChildSerializer(IBsonSerializer childSerializer);
    }
}
