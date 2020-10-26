using CommandLine;

namespace ET
{
	public class Options: Entity
	{
		[Option("Process", Required = false, Default = 1)]
		public int Process { get; set; }
	}
}