using System.IO;

namespace Base
{
	[ObjectEvent]
	public class GlobalConfigComponentEvent : ObjectEvent<GlobalConfigComponent>, IAwake
	{
		public void Awake()
		{
			this.GetValue().Awake();
		}
	}

	/// <summary>
	/// 全局配置
	/// </summary>
	public class GlobalConfigComponent : Component
	{
		public GlobalProto GlobalProto;
		
		public void Awake()
		{
		{
#if UNITY_EDITOR
			string filePath = @"./GlobalProto.txt";
#else
			string filePath = @"../GlobalProto.txt";
#endif
			if (!File.Exists(filePath))
			{
				throw new ConfigException("没有找到配置文件GlobalProto.txt");
			}
			this.GlobalProto = MongoHelper.FromJson<GlobalProto>(File.ReadAllLines(filePath)[0]);
		}
	}
	}
}