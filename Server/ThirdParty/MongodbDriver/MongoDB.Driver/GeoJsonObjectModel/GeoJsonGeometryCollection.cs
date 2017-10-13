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
    /// Represents a GeoJson GeometryCollection object.
    /// </summary>
    /// <typeparam name="TCoordinates">The type of the coordinates.</typeparam>
    [BsonSerializer(typeof(GeoJsonGeometryCollectionSerializer<>))]
    public class GeoJsonGeometryCollection<TCoordinates> : GeoJsonGeometry<TCoordinates> where TCoordinates : GeoJsonCoordinates
    {
        // private fields
        private ReadOnlyCollection<GeoJsonGeometry<TCoordinates>> _geometries;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="GeoJsonGeometryCollection{TCoordinates}"/> class.
        /// </summary>
        /// <param name="geometries">The geometries.</param>
        public GeoJsonGeometryCollection(IEnumerable<GeoJsonGeometry<TCoordinates>> geometries)
            : this(null, geometries)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeoJsonGeometryCollection{TCoordinates}"/> class.
        /// </summary>
        /// <param name="args">The additional args.</param>
        /// <param name="geometries">The geometries.</param>
        public GeoJsonGeometryCollection(GeoJsonObjectArgs<TCoordinates> args, IEnumerable<GeoJsonGeometry<TCoordinates>> geometries)
            : base(args)
        {
            if (geometries == null)
            {
                throw new ArgumentNullException("geometries");
            }

            var geometriesArray = geometries.ToArray();
            if (geometriesArray.Contains(null))
            {
                throw new ArgumentException("One of the geometries is null.", "geometries");
            }

            _geometries = new ReadOnlyCollection<GeoJsonGeometry<TCoordinates>>(geometriesArray);
        }

        // public properties
        /// <summary>
        /// Gets the geometries.
        /// </summary>
        public ReadOnlyCollection<GeoJsonGeometry<TCoordinates>> Geometries
        {
            get { return _geometries; }
        }

        /// <summary>
        /// Gets the type of the GeoJson object.
        /// </summary>
        public override GeoJsonObjectType Type
        {
            get { return GeoJsonObjectType.GeometryCollection; }
        }
    }
}
