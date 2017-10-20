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
    /// Represents a GeoJson bounding box.
    /// </summary>
    /// <typeparam name="TCoordinates">The type of the coordinates.</typeparam>
    [BsonSerializer(typeof(GeoJsonBoundingBoxSerializer<>))]
    public class GeoJsonBoundingBox<TCoordinates> where TCoordinates : GeoJsonCoordinates
    {
        // private fields
        private TCoordinates _max;
        private TCoordinates _min;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="GeoJsonBoundingBox{TCoordinates}"/> class.
        /// </summary>
        /// <param name="min">The min.</param>
        /// <param name="max">The max.</param>
        public GeoJsonBoundingBox(TCoordinates min, TCoordinates max)
        {
            if (min == null)
            {
                throw new ArgumentNullException("min");
            }
            if (max == null)
            {
                throw new ArgumentNullException("max");
            }

            _min = min;
            _max = max;
        }

        // public properties
        /// <summary>
        /// Gets the max.
        /// </summary>
        public TCoordinates Max
        {
            get { return _max; }
        }

        /// <summary>
        /// Gets the min.
        /// </summary>
        public TCoordinates Min
        {
            get { return _min; }
        }
    }
}
