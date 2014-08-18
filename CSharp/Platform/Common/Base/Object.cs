using System.Collections.Generic;
using MongoDB.Bson;

namespace Common.Base
{
    public class Object
    {
        public ObjectId Id { get; set; }

        public Dictionary<string, object> Dict { get; private set; }

        protected Object()
        {
            this.Id = ObjectId.GenerateNewId();
            this.Dict = new Dictionary<string, object>();
        }

        public object this[string key]
        {
            set
            {
                this.Dict[key] = value;
            }
            get
            {
                return this.Dict[key];
            }
        }

        public T Get<T>(string key)
        {
            if (!this.Dict.ContainsKey(key))
            {
                return default(T);
            }
            return (T) this.Dict[key];
        }

        public T Get<T>()
        {
            return this.Get<T>(typeof (T).Name);
        }

        public void Set(string key, object obj)
        {
            this.Dict[key] = obj;
        }

        public void Set<T>(T obj)
        {
            this.Dict[typeof (T).Name] = obj;
        }

        public bool Contain(string key)
        {
            return this.Dict.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            return this.Dict.Remove(key);
        }
    }
}