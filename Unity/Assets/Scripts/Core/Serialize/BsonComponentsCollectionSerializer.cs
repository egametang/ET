using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

namespace ET
{
    public class BsonComponentsCollectionSerializer: IBsonSerializer
    {
        public object Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            ComponentsCollection componentsCollection = new();
            IBsonSerializer<Entity> bsonSerializer = BsonSerializer.LookupSerializer<Entity>();
            IBsonReader bsonReader = context.Reader;
            bsonReader.ReadStartArray();
            while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
            {
                Entity entity = bsonSerializer.Deserialize(context);
                componentsCollection.Add(Entity.GetLongHashCodeByType(entity.GetType()), entity);
            }
            bsonReader.ReadEndArray();

            return componentsCollection;
        }

        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
        {
            IBsonWriter bsonWriter = context.Writer;
            bsonWriter.WriteStartArray();
            ComponentsCollection componentsCollection = (ComponentsCollection)value;

            IBsonSerializer<Entity> bsonSerializer = BsonSerializer.LookupSerializer<Entity>();
            foreach ((long _, Entity entity) in componentsCollection)
            {
                if (entity is not ISerializeToEntity)
                {
                    continue;
                }
                bsonSerializer.Serialize(context, entity);
            }
            bsonWriter.WriteEndArray();
        }

        public System.Type ValueType { get; }
    }
}