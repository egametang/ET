
namespace Component.Config
{
	public class GlobalConfig: IType
	{
		public int Type { get; set; }
	}

	[ConfigAttribute]
	public class GlobalCategory: ConfigCategory<GlobalConfig>
	{
	}
}
