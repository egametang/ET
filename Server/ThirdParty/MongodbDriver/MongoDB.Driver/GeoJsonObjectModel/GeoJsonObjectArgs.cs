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
    /// Represents additional args provided when creating a GeoJson object.
    /// </summary>
    /// <typeparam name="TCoordinates">The type of the coordinates.</typeparam>
    public class GeoJsonObjectArgs<TCoordinates> where TCoordinates : GeoJsonCoordinates
    {
        /// <summary>
        /// Gets or sets the bounding box.
        /// </summary>
        public GeoJsonBoundingBox<TCoordinates> BoundingBox { get; set; }

        /// <summary>
        /// Gets or sets the coordinate reference system.
        /// </summary>
        public GeoJsonCoordinateReferenceSystem CoordinateReferenceSystem { get; set; }

        /// <summary>
        /// Gets or sets the extra members.
        /// </summary>
        public BsonDocument ExtraMembers { get; set; }
    }
}
