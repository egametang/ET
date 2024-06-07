using System;
using System.Reflection;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization.Attributes;

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
                BsonElementAttribute bsonElement = field.GetCustomAttribute<BsonElementAttribute>();
                if (bsonElement == null && !field.IsPublic)
                {
                    continue;
                }
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

            while (bsonReader.State != BsonReaderState.EndOfDocument)
            {
                switch (bsonReader.State)
                {
                    case BsonReaderState.Name:
                    {
                        string name = bsonReader.ReadName(Utf8NameDecoder.Instance);
                        FieldInfo field = actualType.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (field != null)
                        {
                            object value = BsonSerializer.Deserialize(bsonReader, field.FieldType);
                            field.SetValue(obj, value);
                        }

                        break;
                    }
                    case BsonReaderState.Type:
                    {
                        bsonReader.ReadBsonType();
                        break;
                    }
                    case BsonReaderState.Value:
                    {
                        bsonReader.SkipValue();
                        break;
                    }
                }
            }

            bsonReader.ReadEndDocument();

            return (TValue)obj;
        }
    }
}