using System;
using System.Collections.Generic;
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace ETModel
{
    public class StructBsonSerialize<TValue> : StructSerializerBase<TValue> where TValue : struct
    {
        private readonly List<PropertyInfo> propertyInfo = new List<PropertyInfo>();
        
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TValue value)
        {
            Type nominalType = args.NominalType;
            var fields = nominalType.GetFields(BindingFlags.Instance | BindingFlags.Public);
            var propsAll = nominalType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            propertyInfo.Clear();
            foreach (var prop in propsAll)
            {
                if (prop.CanWrite)
                {
                    propertyInfo.Add(prop);
                }
            }

            var bsonWriter = context.Writer;

            bsonWriter.WriteStartDocument();

            foreach (var field in fields)
            {
                bsonWriter.WriteName(field.Name);
                BsonSerializer.Serialize(bsonWriter, field.FieldType, field.GetValue(value));
            }
            foreach (var prop in propertyInfo)
            {
                bsonWriter.WriteName(prop.Name);
                BsonSerializer.Serialize(bsonWriter, prop.PropertyType, prop.GetValue(value, null));
            }

            bsonWriter.WriteEndDocument();
        }

        public override TValue Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            //boxing is required for SetValue to work
            var obj = (object)(new TValue());
            var actualType = args.NominalType;
            var bsonReader = context.Reader;
            
            bsonReader.ReadStartDocument();
            
            while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
            {
                var name = bsonReader.ReadName(Utf8NameDecoder.Instance);
            
                var field = actualType.GetField(name);
                if (field != null)
                {
                    var value = BsonSerializer.Deserialize(bsonReader, field.FieldType);
                    field.SetValue(obj, value);
                    
                }
            
                var prop = actualType.GetProperty(name);
                if (prop != null)
                {
                    var value = BsonSerializer.Deserialize(bsonReader, prop.PropertyType);
                    prop.SetValue(obj, value, null);
                }
            }
            
            bsonReader.ReadEndDocument();
            
            return (TValue)obj;
        }
    }
}