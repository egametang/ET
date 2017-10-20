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
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GeoJsonObjectModel.Serializers;

namespace MongoDB.Driver.GeoJsonObjectModel
{
    /// <summary>
    /// Represents the coordinates of a GeoJson linear ring.
    /// </summary>
    /// <typeparam name="TCoordinates">The type of the coordinates.</typeparam>
    [BsonSerializer(typeof(GeoJsonLinearRingCoordinatesSerializer<>))]
    public class GeoJsonLinearRingCoordinates<TCoordinates> : GeoJsonLineStringCoordinates<TCoordinates> where TCoordinates : GeoJsonCoordinates
    {
        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="GeoJsonLinearRingCoordinates{TCoordinates}"/> class.
        /// </summary>
        /// <param name="positions">The positions.</param>
        public GeoJsonLinearRingCoordinates(IEnumerable<TCoordinates> positions)
            : base(positions)
        {
            if (Positions.Count < 4)
            {
                throw new ArgumentException("A linear ring requires at least 4 positions.", "positions");
            }
            if (!Positions.First().Equals(Positions.Last()))
            {
                throw new ArgumentException("The first and last positions in a linear ring must be equal.", "positions");
            }
        }
    }
}
