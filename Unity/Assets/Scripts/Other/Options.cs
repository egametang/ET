using System;
using System.Text;
using Base;
#if SERVER
using CommandLine;
#endif

namespace Model
{
	public class Options: ICloneable
	{
#if SERVER
		[Option("id", Required = true, HelpText = "Id.")]
#endif
		public int Id { get; set; }

#if SERVER
		[Option("IP", Required = false, HelpText = "进程运行的服务器ip.")]
#endif
		public string IP { get; set; }

#if SERVER
		[Option("appType", Required = true, HelpText = "AppType: realm gate")]
#endif
		public string AppType { get; set; }

#if SERVER
		[Option("protocol", Required = false, HelpText = "Protocol, tcp or udp.", DefaultValue = NetworkProtocol.UDP)]
#endif
		public NetworkProtocol Protocol { get; set; }

#if SERVER
		[Option("host", Required = true, HelpText = "Host.")]
#endif
		public string Host { get; set; }

#if SERVER
		[Option("port", Required = true, HelpText = "Port.")]
#endif
		public int Port { get; set; }

#if SERVER
		[Option("gateHost", Required = false, HelpText = "GateHost.")]
#endif
		public string GateHost { get; set; }

#if SERVER
		[Option("gatePort", Required = false, HelpText = "GatePort.")]
#endif
		public int GatePort { get; set; }

#if SERVER
		[Option('v', HelpText = "Print details during execution.")]
#endif
		public bool Verbose { get; set; }

#if SERVER
		[HelpOption]
#endif
		public string GetUsage()
		{
			// this without using CommandLine.Text
			StringBuilder usage = new StringBuilder();
			usage.AppendLine("Quickstart Application 1.0");
			usage.AppendLine("Read user manual for usage instructions...");
			return usage.ToString();
		}

		public object Clone()
		{
			return MongoHelper.FromBson<Options>(MongoHelper.ToBson(this));
		}
	}
}