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

namespace MongoDB.Driver.GeoJsonObjectModel
{
    /// <summary>
    /// Represents additional arguments for a GeoJson Feature object.
    /// </summary>
    /// <typeparam name="TCoordinates">The type of the coordinates.</typeparam>
    public class GeoJsonFeatureArgs<TCoordinates> : GeoJsonObjectArgs<TCoordinates> where TCoordinates : GeoJsonCoordinates
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public BsonValue Id { get; set; }

        /// <summary>
        /// Gets or sets the properties.
        /// </summary>
        public BsonDocument Properties { get; set; }
    }
}
