using CommandLine;

namespace ETModel
{
	public class Options: Entity
	{
		[Option("process", Required = false, Default = 1)]
		public int Process { get; set; }
	}
}