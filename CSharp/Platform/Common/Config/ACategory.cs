using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Common.Helper;

namespace Common.Config
{
    public abstract class ACategory<T>: ICategory where T : AConfig
    {
        protected readonly Dictionary<int, T> dict = new Dictionary<int, T>();

        public virtual void BeginInit()
        {
            string path = Path.Combine(@"../../Config/", typeof(T).Name);

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

        public Type ConfigType
        {
            get
            {
                return typeof (T);
            }
        }

        public virtual void EndInit()
        {
        }

        public T this[int type]
        {
            get
            {
                return this.dict[type];
            }
        }

        public T[] GetAll()
        {
            return this.dict.Values.ToArray();
        }
    }
}