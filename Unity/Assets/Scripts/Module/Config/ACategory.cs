using System;
using System.Collections.Generic;
using System.Linq;

namespace ETModel
{
	public abstract class ACategory : Object
	{
		public abstract Type ConfigType { get; }
		public abstract IConfig GetOne();
		public abstract IConfig[] GetAll();
		public abstract IConfig TryGet(int type);
	}

	/// <summary>
	/// 管理该所有的配置
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class ACategory<T> : ACategory where T : IConfig
	{
		protected Dictionary<long, IConfig> dict;

		public override void BeginInit()
		{
			this.dict = new Dictionary<long, IConfig>();

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

		public override IConfig TryGet(int type)
		{
			IConfig t;
			if (!this.dict.TryGetValue(type, out t))
			{
				return null;
			}
			return t;
		}

		public override IConfig[] GetAll()
		{
			return this.dict.Values.ToArray();
		}

		public override IConfig GetOne()
		{
			return this.dict.Values.First();
		}
	}
}