using System;

namespace MongoDB.Bson.Serialization.Attributes
{
    public enum DictionaryRepresentation
    {
        Document,
        ArrayOfArrays,
        ArrayOfDocuments,
    }
    
    public class BsonDictionaryOptionsAttribute: Attribute
    {
        public BsonDictionaryOptionsAttribute(DictionaryRepresentation dictionaryRepresentation)
        {
            
        }
    }
}