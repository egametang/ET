using System;
using CommandLine;

namespace ETModel
{
	[ObjectSystem]
	public class OptionComponentSystem : AwakeSystem<OptionComponent, string[]>
	{
		public override void Awake(OptionComponent self, string[] a)
		{
			self.Awake(a);
		}
	}
	
	public class OptionComponent : Component
	{
		public Options Options { get; set; }

		public void Awake(string[] args)
		{
			Parser.Default.ParseArguments<Options>(args)
				.WithNotParsed(error => throw new Exception($"命令行格式错误!"))
				.WithParsed(options => { Options = options; });
		}
	}
}
