using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Common.Helper;

namespace Common.Config
{
    public abstract class ACategory<T>: ICategory where T : IConfig
    {
        protected readonly Dictionary<int, T> dict = new Dictionary<int, T>();

        public void BeginInit()
        {
            string path = Path.Combine(@"./Config/", this.Name);

            if (!Directory.Exists(path))
            {
                throw new Exception(string.Format("not found config path: {0}", path));
            }

            foreach (var file in Directory.GetFiles(path))
            {
                var t = MongoHelper.FromJson<T>(File.ReadAllText(file));
                this.dict.Add(t.Id, t);
            }
        }

        public void EndInit()
        {
        }

        public T this[int type]
        {
            get
            {
                return this.dict[type];
            }
        }

        public string Name
        {
            get
            {
                return typeof (T).Name;
            }
        }

        public T[] GetAll()
        {
            return this.dict.Values.ToArray();
        }
    }
}