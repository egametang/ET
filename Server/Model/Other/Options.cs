#if SERVER
using CommandLine;
#endif

namespace Model
{
	public class Options
	{
		[Option("appId", Required = true)]
		public int AppId { get; set; }
		
		// 没啥用，主要是在查看进程信息能区分每个app.exe的类型
		[Option("appType", Required = true)]
		public AppType AppType { get; set; }

		[Option("config", Required = false)]
		public string Config { get; set; }
	}
}