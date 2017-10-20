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
    /// Represents a GeoJson FeatureCollection.
    /// </summary>
    /// <typeparam name="TCoordinates">The type of the coordinates.</typeparam>
    [BsonSerializer(typeof(GeoJsonFeatureCollectionSerializer<>))]
    public class GeoJsonFeatureCollection<TCoordinates> : GeoJsonObject<TCoordinates> where TCoordinates : GeoJsonCoordinates
    {
        // private fields
        private ReadOnlyCollection<GeoJsonFeature<TCoordinates>> _features;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="GeoJsonFeatureCollection{TCoordinates}"/> class.
        /// </summary>
        /// <param name="features">The features.</param>
        public GeoJsonFeatureCollection(IEnumerable<GeoJsonFeature<TCoordinates>> features)
            : this(null, features)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeoJsonFeatureCollection{TCoordinates}"/> class.
        /// </summary>
        /// <param name="args">The additional args.</param>
        /// <param name="features">The features.</param>
        public GeoJsonFeatureCollection(GeoJsonObjectArgs<TCoordinates> args, IEnumerable<GeoJsonFeature<TCoordinates>> features)
            : base(args)
        {
            if (features == null)
            {
                throw new ArgumentNullException("features");
            }

            var featuresArray = features.ToArray();
            if (featuresArray.Contains(null))
            {
                throw new ArgumentException("One of the features is null.", "features");
            }

            _features = new ReadOnlyCollection<GeoJsonFeature<TCoordinates>>(featuresArray);
        }

        // public properties
        /// <summary>
        /// Gets the features.
        /// </summary>
        public ReadOnlyCollection<GeoJsonFeature<TCoordinates>> Features
        {
            get { return _features; }
        }

        /// <summary>
        /// Gets the type of the GeoJson object.
        /// </summary>
        public override GeoJsonObjectType Type
        {
            get { return GeoJsonObjectType.FeatureCollection; }
        }
    }
}
