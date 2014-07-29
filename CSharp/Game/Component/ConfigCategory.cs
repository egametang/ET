using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Helper;

namespace Component
{
    public abstract class ConfigCategory<T>: ISupportInitialize, IConfigInitialize where T : IType
    {
        protected readonly Dictionary<int, T> dict = new Dictionary<int, T>();

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

        public void Init(string configsDir)
        {
            string path = Path.Combine(configsDir, this.ConfigName);

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

        public void BeginInit()
        {
        }

        public void EndInit()
        {
        }
    }
}