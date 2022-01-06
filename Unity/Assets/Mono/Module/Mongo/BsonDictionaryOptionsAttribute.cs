using System;

namespace MongoDB.Bson.Serialization.Options
{
    /// <summary>
    /// Represents the representation to use for dictionaries.
    /// </summary>
    public enum DictionaryRepresentation
    {
        /// <summary>Represent the dictionary as a Document.</summary>
        Document,
        /// <summary>Represent the dictionary as an array of arrays.</summary>
        ArrayOfArrays,
        /// <summary>Represent the dictionary as an array of documents.</summary>
        ArrayOfDocuments,
    }
}

namespace MongoDB.Bson.Serialization.Attributes
{
    public class BsonDictionaryOptionsAttribute: Attribute
    {
        public BsonDictionaryOptionsAttribute(MongoDB.Bson.Serialization.Options.DictionaryRepresentation dictionaryRepresentation)
        {
            
        }
    }
}