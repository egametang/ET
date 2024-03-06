using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

namespace ET
{
    public class BsonChildrenCollectionSerializer: IBsonSerializer
    {
        public object Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            ChildrenCollection childrenCollection = ChildrenCollection.Create(true);
            IBsonSerializer<Entity> bsonSerializer = BsonSerializer.LookupSerializer<Entity>();
            var bsonReader = context.Reader;
            bsonReader.ReadStartArray();
            while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
            {
                Entity entity = bsonSerializer.Deserialize(context);
                entity.IsSerilizeWithParent = true;
                childrenCollection.Add(entity.Id, entity);
            }
            bsonReader.ReadEndArray();

            return childrenCollection;
        }

        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
        {
            IBsonWriter bsonWriter = context.Writer;
            bsonWriter.WriteStartArray();
            ChildrenCollection childrenCollection = (ChildrenCollection)value;

            IBsonSerializer<Entity> bsonSerializer = BsonSerializer.LookupSerializer<Entity>();
            foreach ((long _, Entity entity) in childrenCollection)
            {
                if (entity is ISerializeToEntity || entity.IsSerilizeWithParent)
                {
                    bsonSerializer.Serialize(context, entity);
                }
            }
            bsonWriter.WriteEndArray();
        }

        public System.Type ValueType { get; }
    }
}