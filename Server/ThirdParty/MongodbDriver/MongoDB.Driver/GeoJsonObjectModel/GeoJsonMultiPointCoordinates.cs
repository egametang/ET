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
    /// Represents the coordinates of a GeoJson MultiPoint object.
    /// </summary>
    /// <typeparam name="TCoordinates">The type of the coordinates.</typeparam>
    [BsonSerializer(typeof(GeoJsonMultiPointCoordinatesSerializer<>))]
    public class GeoJsonMultiPointCoordinates<TCoordinates> where TCoordinates : GeoJsonCoordinates
    {
        // private fields
        private ReadOnlyCollection<TCoordinates> _positions;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="GeoJsonMultiPointCoordinates{TCoordinates}"/> class.
        /// </summary>
        /// <param name="positions">The positions.</param>
        public GeoJsonMultiPointCoordinates(IEnumerable<TCoordinates> positions)
        {
            if (positions == null)
            {
                throw new ArgumentNullException("positions");
            }

            var positionsArray = positions.ToArray();
            if (positionsArray.Contains(null))
            {
                throw new ArgumentException("One of the positions is null.", "positions");
            }

            _positions = new ReadOnlyCollection<TCoordinates>(positionsArray);
        }

        // public properties
        /// <summary>
        /// Gets the positions.
        /// </summary>
        public ReadOnlyCollection<TCoordinates> Positions
        {
            get { return _positions; }
        }
    }
}
