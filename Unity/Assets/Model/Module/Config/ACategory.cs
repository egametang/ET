using System;
using System.Collections.Generic;
using System.Linq;

namespace ET
{
	public abstract class ACategory : Object
	{
		public abstract Type ConfigType { get; }
	}

	/// <summary>
	/// 管理该所有的配置
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class ACategory<T> : ACategory where T : IConfig
	{
		protected Dictionary<long, T> dict;

		public override void BeginInit()
		{
			this.dict = new Dictionary<long, T>();

			string configStr = ConfigHelper.GetText(typeof(T).Name);

			foreach (string str in configStr.Split(new[] { "\n" }, StringSplitOptions.None))
			{
				try
				{
					string str2 = str.Trim();
					if (str2 == "")
					{
						continue;
					}
					T t = ConfigHelper.ToObject<T>(str2);
					this.dict.Add(t.Id, t);
				}
				catch (Exception e)
				{
					throw new Exception($"parser json fail: {str}", e);
				}
			}
		}

		public override Type ConfigType
		{
			get
			{
				return typeof(T);
			}
		}

		public override void EndInit()
		{
		}

		public T Get(int id)
		{
			T t;
			if (!this.dict.TryGetValue(id, out t))
			{
				throw new Exception($"not found config: {typeof(T)} id: {id}");
			}
			return t;
		}

		public Dictionary<long, T> GetAll()
		{
			return this.dict;
		}

		public T GetOne()
		{
			return this.dict.Values.First();
		}
	}
}