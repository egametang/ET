#if SERVER
using CommandLine;
#endif

namespace ETModel
{
	public class Options
	{
		[Option("appId", Required = false, Default = 1)]
		public int AppId { get; set; }
		
		// 没啥用，主要是在查看进程信息能区分每个app.exe的类型
		[Option("appType", Required = false, Default = AppType.Manager)]
		public AppType AppType { get; set; }

		[Option("config", Required = false, Default = "../Config/StartConfig/LocalAllServer.txt")]
		public string Config { get; set; }
	}
}