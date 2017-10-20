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

using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GeoJsonObjectModel.Serializers;

namespace MongoDB.Driver.GeoJsonObjectModel
{
    /// <summary>
    /// Represents a GeoJson linked coordinate reference system.
    /// </summary>
    [BsonSerializer(typeof(GeoJsonLinkedCoordinateReferenceSystemSerializer))]
    public class GeoJsonLinkedCoordinateReferenceSystem : GeoJsonCoordinateReferenceSystem
    {
        // private fields
        private string _href;
        private string _hrefType;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="GeoJsonLinkedCoordinateReferenceSystem"/> class.
        /// </summary>
        /// <param name="href">The href.</param>
        public GeoJsonLinkedCoordinateReferenceSystem(string href)
        {
            _href = href;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeoJsonLinkedCoordinateReferenceSystem"/> class.
        /// </summary>
        /// <param name="href">The href.</param>
        /// <param name="hrefType">Type of the href.</param>
        public GeoJsonLinkedCoordinateReferenceSystem(string href, string hrefType)
        {
            _href = href;
            _hrefType = hrefType;
        }

        // public properties
        /// <summary>
        /// Gets the href.
        /// </summary>
        public string HRef
        {
            get { return _href; }
        }

        /// <summary>
        /// Gets the type of the href.
        /// </summary>
        public string HRefType
        {
            get { return _hrefType; }
        }

        /// <summary>
        /// Gets the type of the GeoJson coordinate reference system.
        /// </summary>
        public override string Type
        {
            get { return "link"; }
        }
    }
}
