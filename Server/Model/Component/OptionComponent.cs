using System;
using CommandLine;

namespace Model
{
	[EntityEvent(EntityEventId.OptionComponent)]
	public class OptionComponent : Component
	{
		public Options Options { get; } = new Options();

		private void Awake(string[] args)
		{
			if (!Parser.Default.ParseArguments(args, this.Options))
			{
				throw new Exception($"命令行格式错误!");
			}
		}
	}
}
