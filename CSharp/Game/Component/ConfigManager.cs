using System.Collections.Generic;
using System.IO;
using Helper;

namespace Component
{
	public class ConfigManager<T> where T : IType
	{
		private readonly Dictionary<int, T> dict = new Dictionary<int, T>();

		public virtual void LoadConfig(string dir)
		{
			foreach (var file in Directory.GetFiles(dir))
			{
				var t = MongoHelper.FromJson<T>(File.ReadAllText(file));
				this.dict.Add(t.Type, t);
			}
		}

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
	}
}
