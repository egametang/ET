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

namespace MongoDB.Bson.Serialization.Attributes
{
    /// <summary>
    /// Specifies that this factory method should be used for creator-based deserialization.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class BsonFactoryMethodAttribute : Attribute, IBsonCreatorMapAttribute
    {
        // private fields
        private string[] _argumentNames;

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonFactoryMethodAttribute class.
        /// </summary>
        public BsonFactoryMethodAttribute()
        { }

        /// <summary>
        /// Initializes a new instance of the BsonFactoryMethodAttribute class.
        /// </summary>
        /// <param name="argumentNames">The names of the members that the creator argument values come from.</param>
        public BsonFactoryMethodAttribute(params string[] argumentNames)
        {
            _argumentNames = argumentNames;
        }

        // public properties
        /// <summary>
        /// Gets the names of the members that the creator arguments values come from.
        /// </summary>
        public string[] ArgumentNames
        {
            get { return _argumentNames; }
        }

        // public methods
        /// <summary>
        /// Applies a modification to the creator map.
        /// </summary>
        /// <param name="creatorMap">The creator map.</param>
        public void Apply(BsonCreatorMap creatorMap)
        {
            if (_argumentNames != null)
            {
                creatorMap.SetArguments(_argumentNames);
            }
        }
    }
}