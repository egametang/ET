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
    /// Specifies the known types for this class (the derived classes).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
    public class BsonKnownTypesAttribute : Attribute, IBsonClassMapAttribute
    {
        // private fields
        private Type[] _knownTypes;

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonKnownTypesAttribute class.
        /// </summary>
        /// <param name="knownTypes">One or more known types.</param>
        public BsonKnownTypesAttribute(params Type[] knownTypes)
        {
            _knownTypes = knownTypes;
        }

        /// <summary>
        /// Initializes a new instance of the BsonKnownTypesAttribute class.
        /// </summary>
        /// <param name="knownType">A known types.</param>
        public BsonKnownTypesAttribute(Type knownType)
        {
            _knownTypes = new Type[] { knownType };
        }

        // public properties
        /// <summary>
        /// Gets a list of the known types.
        /// </summary>
        public Type[] KnownTypes
        {
            get { return _knownTypes; }
        }

        // public methods
        /// <summary>
        /// Applies a modification to the class map.
        /// </summary>
        /// <param name="classMap">The class map.</param>
        public void Apply(BsonClassMap classMap)
        {
            foreach (var type in _knownTypes)
            {
                classMap.AddKnownType(type);
            }
        }
    }
}