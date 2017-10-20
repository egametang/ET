/* Copyright 2010-2014 MongoDB Inc.
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
    /// Represents a GeoJson Geometry object.
    /// </summary>
    /// <typeparam name="TCoordinates">The type of the coordinates.</typeparam>
    [BsonSerializer(typeof(GeoJsonGeometrySerializer<>))]
    public abstract class GeoJsonGeometry<TCoordinates> : GeoJsonObject<TCoordinates> where TCoordinates : GeoJsonCoordinates
    {
        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="GeoJsonGeometry{TCoordinates}"/> class.
        /// </summary>
        /// <param name="args">The additional args.</param>
        protected GeoJsonGeometry(GeoJsonObjectArgs<TCoordinates> args)
            : base(args)
        {
        }
    }
}
