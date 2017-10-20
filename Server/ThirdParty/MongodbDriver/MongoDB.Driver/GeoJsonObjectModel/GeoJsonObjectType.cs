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

namespace MongoDB.Driver.GeoJsonObjectModel
{
    /// <summary>
    /// Represents the type of a GeoJson object.
    /// </summary>
    public enum GeoJsonObjectType
    {
        /// <summary>
        /// A Feature.
        /// </summary>
        Feature,

        /// <summary>
        /// A FeatureCollection.
        /// </summary>
        FeatureCollection,

        /// <summary>
        /// A GeometryCollection.
        /// </summary>
        GeometryCollection,

        /// <summary>
        /// A LineString.
        /// </summary>
        LineString,

        /// <summary>
        /// A MultiLineString.
        /// </summary>
        MultiLineString,

        /// <summary>
        /// A MultiPoint.
        /// </summary>
        MultiPoint,

        /// <summary>
        /// A MultiPolygon.
        /// </summary>
        MultiPolygon,

        /// <summary>
        /// A Point.
        /// </summary>
        Point,

        /// <summary>
        /// A Polygon.
        /// </summary>
        Polygon
    }
}
