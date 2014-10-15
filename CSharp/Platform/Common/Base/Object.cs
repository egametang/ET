using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Common.Base
{
    public class Object
    {
        [BsonId]
        public ObjectId Guid { get; protected set; }

        [BsonElement]
        [BsonIgnoreIfNull]
        public Dictionary<string, object> Values;

        protected Object()
        {
            this.Guid = ObjectId.GenerateNewId();
        }

        protected Object(ObjectId guid)
        {
            this.Guid = guid;
        }

        public object this[string key]
        {
            set
            {
                if (this.Values == null)
                {
                    this.Values = new Dictionary<string, object>();
                }
                this.Values[key] = value;
            }
            get
            {
                return this.Values[key];
            }
        }

        public T Get<T>(string key)
        {
            if (!this.Values.ContainsKey(key))
            {
                return default(T);
            }
            return (T) this.Values[key];
        }

        public T Get<T>()
        {
            return this.Get<T>(typeof (T).Name);
        }

        public void Set(string key, object obj)
        {
            if (this.Values == null)
            {
                this.Values = new Dictionary<string, object>();
            }
            this.Values[key] = obj;
        }

        public void Set<T>(T obj)
        {
            if (this.Values == null)
            {
                this.Values = new Dictionary<string, object>();
            }
            this.Values[typeof (T).Name] = obj;
        }

        public bool Contain(string key)
        {
            return this.Values.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            bool ret = this.Values.Remove(key);
            if (this.Values.Count == 0)
            {
                this.Values = null;
            }
            return ret;
        }
    }
}