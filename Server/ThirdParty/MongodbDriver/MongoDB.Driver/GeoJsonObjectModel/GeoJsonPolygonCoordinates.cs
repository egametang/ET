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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GeoJsonObjectModel.Serializers;

namespace MongoDB.Driver.GeoJsonObjectModel
{
    /// <summary>
    /// Represents the coordinates of a GeoJson Polygon object.
    /// </summary>
    /// <typeparam name="TCoordinates">The type of the coordinates.</typeparam>
    [BsonSerializer(typeof(GeoJsonPolygonCoordinatesSerializer<>))]
    public class GeoJsonPolygonCoordinates<TCoordinates> where TCoordinates : GeoJsonCoordinates
    {
        // private fields
        private GeoJsonLinearRingCoordinates<TCoordinates> _exterior;
        private ReadOnlyCollection<GeoJsonLinearRingCoordinates<TCoordinates>> _holes;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="GeoJsonPolygonCoordinates{TCoordinates}"/> class.
        /// </summary>
        /// <param name="exterior">The exterior.</param>
        public GeoJsonPolygonCoordinates(GeoJsonLinearRingCoordinates<TCoordinates> exterior)
            : this(exterior, new GeoJsonLinearRingCoordinates<TCoordinates>[0])
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeoJsonPolygonCoordinates{TCoordinates}"/> class.
        /// </summary>
        /// <param name="exterior">The exterior.</param>
        /// <param name="holes">The holes.</param>
        public GeoJsonPolygonCoordinates(GeoJsonLinearRingCoordinates<TCoordinates> exterior, IEnumerable<GeoJsonLinearRingCoordinates<TCoordinates>> holes)
        {
            if (exterior == null)
            {
                throw new ArgumentNullException("exterior");
            }
            if (holes == null)
            {
                throw new ArgumentNullException("holes");
            }

            var holesArray = holes.ToArray();
            if (holesArray.Contains(null))
            {
                throw new ArgumentException("One of the holes is null.", "holes");
            }

            _exterior = exterior;
            _holes = new ReadOnlyCollection<GeoJsonLinearRingCoordinates<TCoordinates>>(holesArray);
        }

        // public properties
        /// <summary>
        /// Gets the exterior.
        /// </summary>
        public GeoJsonLinearRingCoordinates<TCoordinates> Exterior
        {
            get { return _exterior; }
        }

        /// <summary>
        /// Gets the holes.
        /// </summary>
        public ReadOnlyCollection<GeoJsonLinearRingCoordinates<TCoordinates>> Holes
        {
            get { return _holes; }
        }
    }
}
