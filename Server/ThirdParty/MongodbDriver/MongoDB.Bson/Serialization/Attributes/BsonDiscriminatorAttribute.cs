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
    /// Specifies the discriminator and related options for a class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class BsonDiscriminatorAttribute : Attribute, IBsonClassMapAttribute
    {
        // private fields
        private string _discriminator;
        private bool _required;
        private bool _rootClass;

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonDiscriminatorAttribute class.
        /// </summary>
        public BsonDiscriminatorAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the BsonDiscriminatorAttribute class.
        /// </summary>
        /// <param name="discriminator">The discriminator.</param>
        public BsonDiscriminatorAttribute(string discriminator)
        {
            _discriminator = discriminator;
        }

        // public properties
        /// <summary>
        /// Gets the discriminator.
        /// </summary>
        public string Discriminator
        {
            get { return _discriminator; }
        }

        /// <summary>
        /// Gets or sets whether the discriminator is required.
        /// </summary>
        public bool Required
        {
            get { return _required; }
            set { _required = value; }
        }

        /// <summary>
        /// Gets or sets whether this is a root class.
        /// </summary>
        public bool RootClass
        {
            get { return _rootClass; }
            set { _rootClass = value; }
        }

        // public methods
        /// <summary>
        /// Applies a modification to the class map.
        /// </summary>
        /// <param name="classMap">The class map.</param>
        public void Apply(BsonClassMap classMap)
        {
            if (_discriminator != null)
            {
                classMap.SetDiscriminator(_discriminator);
            }
            classMap.SetDiscriminatorIsRequired(_required);
            classMap.SetIsRootClass(_rootClass);
        }
    }
}
