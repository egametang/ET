using System;
using System.Collections.Generic;
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace ET
{
    public class StructBsonSerialize<TValue>: StructSerializerBase<TValue> where TValue : struct
    {
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TValue value)
        {
            Type nominalType = args.NominalType;

            IBsonWriter bsonWriter = context.Writer;

            bsonWriter.WriteStartDocument();

            FieldInfo[] fields = nominalType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (FieldInfo field in fields)
            {
                bsonWriter.WriteName(field.Name);
                BsonSerializer.Serialize(bsonWriter, field.FieldType, field.GetValue(value));
            }

            bsonWriter.WriteEndDocument();
        }

        public override TValue Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            //boxing is required for SetValue to work
            object obj = new TValue();
            Type actualType = args.NominalType;
            IBsonReader bsonReader = context.Reader;

            bsonReader.ReadStartDocument();

            while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
            {
                string name = bsonReader.ReadName(Utf8NameDecoder.Instance);

                FieldInfo field = actualType.GetField(name,BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (field != null)
                {
                    object value = BsonSerializer.Deserialize(bsonReader, field.FieldType);
                    field.SetValue(obj, value);
                }
            }

            bsonReader.ReadEndDocument();

            return (TValue) obj;
        }
    }
}
