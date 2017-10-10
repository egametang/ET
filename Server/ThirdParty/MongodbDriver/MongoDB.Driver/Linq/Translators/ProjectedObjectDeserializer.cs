/* Copyright 2015 MongoDB Inc.
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

using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Driver.Linq.Translators
{
    internal sealed class ProjectedObjectDeserializer : SerializerBase<ProjectedObject>, IBsonDocumentSerializer
    {
        private readonly Dictionary<string, BsonSerializationInfo> _deserializationMap;

        public ProjectedObjectDeserializer(IEnumerable<BsonSerializationInfo> deserializationInfo)
        {
            _deserializationMap = deserializationInfo.Distinct().ToDictionary(x => x.ElementName, x => x);
        }

        public override ProjectedObject Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            context.Reader.GetCurrentBsonType();
            var obj = ReadDocument(context, null, null, new ProjectedObject());
            return obj;
        }

        public bool TryGetMemberSerializationInfo(string memberName, out BsonSerializationInfo serializationInfo)
        {
            return _deserializationMap.TryGetValue(memberName, out serializationInfo);
        }

        // private methods
        private string BuildElementName(string prefix, string name)
        {
            if (prefix == null)
            {
                return name;
            }

            return prefix + "." + name;
        }

        private List<object> ReadArray(BsonDeserializationContext context, string currentKey)
        {
            var bsonReader = context.Reader;
            bsonReader.ReadStartArray();
            var array = new List<object>();
            BsonType bsonType;
            while ((bsonType = bsonReader.ReadBsonType()) != BsonType.EndOfDocument)
            {
                if (bsonType == BsonType.Document)
                {
                    array.Add(ReadDocument(context, currentKey, null, new ProjectedObject()));
                }
                else if (bsonType == BsonType.Array)
                {
                    array.Add(ReadArray(context, currentKey));
                }
                else
                {
                    // we should never get here because we, presumably, have only pulled back things we know about...
                    throw new MongoInternalException("Unexpected field.");
                }
            }
            bsonReader.ReadEndArray();
            return array;
        }

        private ProjectedObject ReadDocument(BsonDeserializationContext context, string currentKey, string scopeKey, ProjectedObject currentObject)
        {
            var bsonReader = context.Reader;
            bsonReader.ReadStartDocument();
            BsonType bsonType;
            while ((bsonType = bsonReader.ReadBsonType()) != BsonType.EndOfDocument)
            {
                var name = bsonReader.ReadName(Utf8NameDecoder.Instance);
                var newCurrentKey = BuildElementName(currentKey, name);
                var newScopeKey = BuildElementName(scopeKey, name);
                BsonSerializationInfo serializationInfo;
                if (_deserializationMap.TryGetValue(newCurrentKey, out serializationInfo))
                {
                    var value = serializationInfo.Serializer.Deserialize(context, new BsonDeserializationArgs { NominalType = serializationInfo.NominalType });
                    currentObject.Add(newScopeKey, value);
                }
                else
                {
                    if (bsonType == BsonType.Document)
                    {
                        // we are going to read nested documents into the same documentStore to keep them flat, optimized for lookup
                        ReadDocument(context, newCurrentKey, newScopeKey, currentObject);
                    }
                    else if (bsonType == BsonType.Array)
                    {
                        currentObject.Add(name, ReadArray(context, newCurrentKey));
                    }
                    else
                    {
                        bsonReader.SkipValue();
                    }
                }
            }
            bsonReader.ReadEndDocument();
            return currentObject;
        }
    }
}
