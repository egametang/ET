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

using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GeoJsonObjectModel.Serializers;

namespace MongoDB.Driver.GeoJsonObjectModel
{
    /// <summary>
    /// Represents a GeoJson named coordinate reference system.
    /// </summary>
    [BsonSerializer(typeof(GeoJsonNamedCoordinateReferenceSystemSerializer))]
    public class GeoJsonNamedCoordinateReferenceSystem : GeoJsonCoordinateReferenceSystem
    {
        // private fields
        private string _name;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="GeoJsonNamedCoordinateReferenceSystem"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public GeoJsonNamedCoordinateReferenceSystem(string name)
        {
            _name = name;
        }

        // public properties
        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Gets the type of the GeoJson coordinate reference system.
        /// </summary>
        public override string Type
        {
            get { return "name"; }
        }
    }
}
