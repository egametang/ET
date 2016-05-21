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

using System;
using System.Linq;

namespace MongoDB.Bson.Serialization.IdGenerators
{
    /// <summary>
    /// Represents an Id generator for Guids stored in BsonBinaryData values.
    /// </summary>
    public class BsonBinaryDataGuidGenerator : IIdGenerator
    {
        // private static fields
        private static BsonBinaryDataGuidGenerator __csharpLegacyInstance = new BsonBinaryDataGuidGenerator(GuidRepresentation.CSharpLegacy);
        private static BsonBinaryDataGuidGenerator __javaLegacyInstance = new BsonBinaryDataGuidGenerator(GuidRepresentation.JavaLegacy);
        private static BsonBinaryDataGuidGenerator __pythonLegacyInstance = new BsonBinaryDataGuidGenerator(GuidRepresentation.PythonLegacy);
        private static BsonBinaryDataGuidGenerator __standardInstance = new BsonBinaryDataGuidGenerator(GuidRepresentation.Standard);
        private static BsonBinaryDataGuidGenerator __unspecifiedInstance = new BsonBinaryDataGuidGenerator(GuidRepresentation.Unspecified);
        private static byte[] __emptyGuidBytes = Guid.Empty.ToByteArray();

        // private fields
        private GuidRepresentation _guidRepresentation;

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonBinaryDataGuidGenerator class.
        /// </summary>
        /// <param name="guidRepresentation">The GuidRepresentation to use when generating new Id values.</param>
        public BsonBinaryDataGuidGenerator(GuidRepresentation guidRepresentation)
        {
            _guidRepresentation = guidRepresentation;
        }

        // public static properties
        /// <summary>
        /// Gets an instance of BsonBinaryDataGuidGenerator for CSharpLegacy GuidRepresentation.
        /// </summary>
        public static BsonBinaryDataGuidGenerator CSharpLegacyInstance
        {
            get { return __csharpLegacyInstance; }
        }

        /// <summary>
        /// Gets an instance of BsonBinaryDataGuidGenerator for JavaLegacy GuidRepresentation.
        /// </summary>
        public static BsonBinaryDataGuidGenerator JavaLegacyInstance
        {
            get { return __javaLegacyInstance; }
        }

        /// <summary>
        /// Gets an instance of BsonBinaryDataGuidGenerator for PythonLegacy GuidRepresentation.
        /// </summary>
        public static BsonBinaryDataGuidGenerator PythonLegacyInstance
        {
            get { return __pythonLegacyInstance; }
        }

        /// <summary>
        /// Gets an instance of BsonBinaryDataGuidGenerator for Standard GuidRepresentation.
        /// </summary>
        public static BsonBinaryDataGuidGenerator StandardInstance
        {
            get { return __standardInstance; }
        }

        /// <summary>
        /// Gets an instance of BsonBinaryDataGuidGenerator for Unspecifed GuidRepresentation.
        /// </summary>
        public static BsonBinaryDataGuidGenerator UnspecifedInstance
        {
            get { return __unspecifiedInstance; }
        }

        // public static methods
        /// <summary>
        /// Gets the instance of BsonBinaryDataGuidGenerator for a GuidRepresentation.
        /// </summary>
        /// <param name="guidRepresentation">The GuidRepresentation.</param>
        /// <returns>The instance of BsonBinaryDataGuidGenerator for a GuidRepresentation.</returns>
        public static BsonBinaryDataGuidGenerator GetInstance(GuidRepresentation guidRepresentation)
        {
            switch (guidRepresentation)
            {
                case GuidRepresentation.CSharpLegacy: return __csharpLegacyInstance;
                case GuidRepresentation.JavaLegacy: return __javaLegacyInstance;
                case GuidRepresentation.PythonLegacy: return __pythonLegacyInstance;
                case GuidRepresentation.Standard: return __standardInstance;
                case GuidRepresentation.Unspecified: return __unspecifiedInstance;
                default: throw new ArgumentOutOfRangeException("guidRepresentation");
            }
        }

        // public methods
        /// <summary>
        /// Generates an Id for a document.
        /// </summary>
        /// <param name="container">The container of the document (will be a MongoCollection when called from the C# driver). </param>
        /// <param name="document">The document.</param>
        /// <returns>An Id.</returns>
        public object GenerateId(object container, object document)
        {
            return new BsonBinaryData(Guid.NewGuid(), _guidRepresentation);
        }

        /// <summary>
        /// Tests whether an Id is empty.
        /// </summary>
        /// <param name="id">The Id.</param>
        /// <returns>True if the Id is empty.</returns>
        public bool IsEmpty(object id)
        {
            if (id == null || ((BsonValue)id).IsBsonNull)
            {
                return true;
            }

            var idBsonBinaryData = (BsonBinaryData)id;
            var subType = idBsonBinaryData.SubType;
            if (subType != BsonBinarySubType.UuidLegacy && subType != BsonBinarySubType.UuidStandard)
            {
                throw new ArgumentOutOfRangeException("id", "The binary sub type of the id value passed to the BsonBinaryDataGuidGenerator IsEmpty method is not UuidLegacy or UuidStandard.");
            }
            return idBsonBinaryData.Bytes.SequenceEqual(__emptyGuidBytes);
        }
    }
}
