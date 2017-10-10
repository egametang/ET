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
    /// Represents the coordinates of a GeoJson MultiLineString object.
    /// </summary>
    /// <typeparam name="TCoordinates">The type of the coordinates.</typeparam>
    [BsonSerializer(typeof(GeoJsonMultiLineStringCoordinatesSerializer<>))]
    public class GeoJsonMultiLineStringCoordinates<TCoordinates> where TCoordinates : GeoJsonCoordinates
    {
        // private fields
        private ReadOnlyCollection<GeoJsonLineStringCoordinates<TCoordinates>> _lineStrings;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="GeoJsonMultiLineStringCoordinates{TCoordinates}"/> class.
        /// </summary>
        /// <param name="lineStrings">The line strings.</param>
        public GeoJsonMultiLineStringCoordinates(IEnumerable<GeoJsonLineStringCoordinates<TCoordinates>> lineStrings)
        {
            if (lineStrings == null)
            {
                throw new ArgumentNullException("lineStrings");
            }

            var lineStringsArray = lineStrings.ToArray();
            if (lineStringsArray.Contains(null))
            {
                throw new ArgumentException("One of the lineStrings is null.", "lineStrings");
            }

            _lineStrings = new ReadOnlyCollection<GeoJsonLineStringCoordinates<TCoordinates>>(lineStringsArray);
        }

        // public properties
        /// <summary>
        /// Gets the LineStrings.
        /// </summary>
        public ReadOnlyCollection<GeoJsonLineStringCoordinates<TCoordinates>> LineStrings
        {
            get { return _lineStrings; }
        }
    }
}
