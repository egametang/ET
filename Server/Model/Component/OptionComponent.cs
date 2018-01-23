using System;
using CommandLine;

namespace Model
{
	[ObjectSystem]
	public class OptionComponentSystem : ObjectSystem<OptionComponent>, IAwake<string[]>
	{
		public void Awake(string[] args)
		{
			this.Get().Awake(args);
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
