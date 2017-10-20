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
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GeoJsonObjectModel.Serializers;

namespace MongoDB.Driver.GeoJsonObjectModel
{
    /// <summary>
    /// Represents a GeoJson Polygon object.
    /// </summary>
    /// <typeparam name="TCoordinates">The type of the coordinates.</typeparam>
    [BsonSerializer(typeof(GeoJsonPolygonSerializer<>))]
    public class GeoJsonPolygon<TCoordinates> : GeoJsonGeometry<TCoordinates> where TCoordinates : GeoJsonCoordinates
    {
        // private fields
        private GeoJsonPolygonCoordinates<TCoordinates> _coordinates;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="GeoJsonPolygon{TCoordinates}"/> class.
        /// </summary>
        /// <param name="coordinates">The coordinates.</param>
        public GeoJsonPolygon(GeoJsonPolygonCoordinates<TCoordinates> coordinates)
            : this(null, coordinates)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeoJsonPolygon{TCoordinates}"/> class.
        /// </summary>
        /// <param name="args">The additional args.</param>
        /// <param name="coordinates">The coordinates.</param>
        public GeoJsonPolygon(GeoJsonObjectArgs<TCoordinates> args, GeoJsonPolygonCoordinates<TCoordinates> coordinates)
            : base(args)
        {
            if (coordinates == null)
            {
                throw new ArgumentNullException("coordinates");
            }

            _coordinates = coordinates;
        }

        // public properties
        /// <summary>
        /// Gets the coordinates.
        /// </summary>
        public GeoJsonPolygonCoordinates<TCoordinates> Coordinates
        {
            get { return _coordinates; }
        }

        /// <summary>
        /// Gets the type of the GeoJson object.
        /// </summary>
        public override GeoJsonObjectType Type
        {
            get { return GeoJsonObjectType.Polygon; }
        }
    }
}
