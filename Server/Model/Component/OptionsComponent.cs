using System;
using Base;
using CommandLine;

namespace Model
{
	[ObjectEvent]
	public class OptionsComponentEvent : ObjectEvent<OptionsComponent>, IAwake<string[]>
	{
		public void Awake(string[] args)
		{
			this.GetValue().Awake(args);
		}
	}

	public class OptionsComponent: Component
	{
		public Options Options = new Options();

		public void Awake(string[] args)
		{
			if (!Parser.Default.ParseArguments(args, this.Options))
			{
				throw new Exception($"命令行格式错误!");
			}
		}
	}
}
