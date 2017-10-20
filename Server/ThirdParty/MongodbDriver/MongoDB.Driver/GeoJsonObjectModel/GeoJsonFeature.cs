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

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GeoJsonObjectModel.Serializers;

namespace MongoDB.Driver.GeoJsonObjectModel
{
    /// <summary>
    /// Represents a GeoJson Feature object.
    /// </summary>
    /// <typeparam name="TCoordinates">The type of the coordinates.</typeparam>
    [BsonSerializer(typeof(GeoJsonFeatureSerializer<>))]
    public class GeoJsonFeature<TCoordinates> : GeoJsonObject<TCoordinates> where TCoordinates : GeoJsonCoordinates
    {
        // private fields
        private GeoJsonGeometry<TCoordinates> _geometry;
        private BsonValue _id;
        private BsonDocument _properties;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="GeoJsonFeature{TCoordinates}"/> class.
        /// </summary>
        /// <param name="geometry">The geometry.</param>
        public GeoJsonFeature(GeoJsonGeometry<TCoordinates> geometry)
            : this(null, geometry)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeoJsonFeature{TCoordinates}"/> class.
        /// </summary>
        /// <param name="args">The additional args.</param>
        /// <param name="geometry">The geometry.</param>
        public GeoJsonFeature(GeoJsonFeatureArgs<TCoordinates> args, GeoJsonGeometry<TCoordinates> geometry)
            : base(args)
        {
            _geometry = geometry;

            if (args != null)
            {
                _id = args.Id;
                _properties = args.Properties;
            }
        }

        // public properties
        /// <summary>
        /// Gets the geometry.
        /// </summary>
        public GeoJsonGeometry<TCoordinates> Geometry
        {
            get { return _geometry; }
        }

        /// <summary>
        /// Gets the id.
        /// </summary>
        public BsonValue Id
        {
            get { return _id; }
        }

        /// <summary>
        /// Gets the properties.
        /// </summary>
        public BsonDocument Properties
        {
            get { return _properties; }
        }

        /// <summary>
        /// Gets the type of the GeoJson object.
        /// </summary>
        public override GeoJsonObjectType Type
        {
            get { return GeoJsonObjectType.Feature; }
        }
    }
}
