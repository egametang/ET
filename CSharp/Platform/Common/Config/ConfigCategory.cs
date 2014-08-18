using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Common.Helper;

namespace Model
{
    public abstract class ConfigCategory<T>: ISupportInitialize where T : IType
    {
        protected readonly Dictionary<int, T> dict = new Dictionary<int, T>();

        protected ConfigCategory()
        {
            string path = Path.Combine(@"./Config/", this.ConfigName);

            if (!Directory.Exists(path))
            {
                throw new Exception(string.Format("not found config path: {0}", path));
            }

            foreach (var file in Directory.GetFiles(path))
            {
                var t = MongoHelper.FromJson<T>(File.ReadAllText(file));
                this.dict.Add(t.Type, t);
            }
        }

        public T this[int type]
        {
            get
            {
                return this.dict[type];
            }
        }

        public string ConfigName
        {
            get
            {
                return typeof (T).Name;
            }
        }

        public Dictionary<int, T> GetAll()
        {
            return this.dict;
        }

        public void BeginInit()
        {
        }

        public void EndInit()
        {
        }
    }
}