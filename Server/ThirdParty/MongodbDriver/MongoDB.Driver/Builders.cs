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

namespace MongoDB.Driver
{
    /// <summary>
    /// A static helper class containing various builders.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    public static class Builders<TDocument>
    {
        private static FilterDefinitionBuilder<TDocument> __filter = new FilterDefinitionBuilder<TDocument>();
        private static IndexKeysDefinitionBuilder<TDocument> __index = new IndexKeysDefinitionBuilder<TDocument>();
        private static ProjectionDefinitionBuilder<TDocument> __projection = new ProjectionDefinitionBuilder<TDocument>();
        private static SortDefinitionBuilder<TDocument> __sort = new SortDefinitionBuilder<TDocument>();
        private static UpdateDefinitionBuilder<TDocument> __update = new UpdateDefinitionBuilder<TDocument>();

        /// <summary>
        /// Gets a <see cref="FilterDefinitionBuilder{TDocument}"/>.
        /// </summary>
        public static FilterDefinitionBuilder<TDocument> Filter
        {
            get { return __filter; }
        }

        /// <summary>
        /// Gets an <see cref="IndexKeysDefinitionBuilder{TDocument}"/>.
        /// </summary>
        public static IndexKeysDefinitionBuilder<TDocument> IndexKeys
        {
            get { return __index; }
        }

        /// <summary>
        /// Gets a <see cref="ProjectionDefinitionBuilder{TDocument}"/>.
        /// </summary>
        public static ProjectionDefinitionBuilder<TDocument> Projection
        {
            get { return __projection; }
        }

        /// <summary>
        /// Gets a <see cref="SortDefinitionBuilder{TDocument}"/>.
        /// </summary>
        public static SortDefinitionBuilder<TDocument> Sort
        {
            get { return __sort; }
        }

        /// <summary>
        /// Gets an <see cref="UpdateDefinitionBuilder{TDocument}"/>.
        /// </summary>
        public static UpdateDefinitionBuilder<TDocument> Update
        {
            get { return __update; }
        }
    }
}
