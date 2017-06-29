using System;
using CommandLine;

namespace Model
{
	[ObjectEvent]
	public class OptionComponentEvent : ObjectEvent<OptionComponent>, IAwake<string[]>
	{
		public void Awake(string[] args)
		{
			this.Get().Awake(args);
		}
	}
	
	public class OptionComponent : Component
	{
		public Options Options { get; } = new Options();

		public void Awake(string[] args)
		{
			if (!Parser.Default.ParseArguments(args, this.Options))
			{
				throw new Exception($"命令行格式错误!");
			}
		}
	}
}
