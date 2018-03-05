using System;
using System.Collections.Generic;
using System.Linq;
using ETModel;

namespace ETHotfix
{
	public abstract class ACategory : Object
	{
		public abstract Type ConfigType { get; }
	}

	/// <summary>
	/// 管理该所有的配置
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class ACategory<T>: ACategory where T : AConfig
	{
		protected Dictionary<long, T> dict;

		public override void BeginInit()
		{
			this.dict = new Dictionary<long, T>();

			string configStr = ConfigHelper.GetText(typeof (T).Name);

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
				return typeof (T);
			}
		}

		public override void EndInit()
		{
		}

		public T this[long type]
		{
			get
			{
				T t;
				if (!this.dict.TryGetValue(type, out t))
				{
					throw new KeyNotFoundException($"{typeof (T)} 没有找到配置, key: {type}");
				}
				return t;
			}
		}

		public T TryGet(int type)
		{
			T t;
			if (!this.dict.TryGetValue(type, out t))
			{
				return null;
			}
			return t;
		}

		public T[] GetAll()
		{
			return this.dict.Values.ToArray();
		}

		public T GetOne()
		{
			return this.dict.Values.First();
		}
	}
}