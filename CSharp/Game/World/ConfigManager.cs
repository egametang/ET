using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Helper;

namespace World
{
	public class ConfigManager<T> : ISupportInitialize, IConfigInitialize where T : IType
	{
		protected readonly Dictionary<int, T> dict = new Dictionary<int, T>();

		public T this[int type]
		{
			get
			{
				return dict[type];
			}
		}

		public Dictionary<int, T> GetAll()
		{
			return this.dict;
		}

		public void Init(string dir)
		{
			foreach (var file in Directory.GetFiles(dir))
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
