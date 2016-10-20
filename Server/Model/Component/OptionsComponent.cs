using System;
using System.IO;
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
		public CommandLines AllOptions = new CommandLines();

		public Options Options = new Options();

		public void Awake(string[] args)
		{
			string s = File.ReadAllText("./CommandLineConfig.txt");
			this.AllOptions = MongoHelper.FromJson<CommandLines>(s);

			if (!Parser.Default.ParseArguments(args, this.Options))
			{
				throw new Exception($"命令行格式错误!");
			}
		}
	}
}
