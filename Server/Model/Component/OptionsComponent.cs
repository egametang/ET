using System;
using System.Text;
using Base;
using CommandLine;

namespace Model
{
	public class Options
	{
		[Option("appType", Required = true, HelpText = "AppType: realm gate")]
		public string AppType { get; set; }

		[Option("name", Required = true, HelpText = "Name.")]
		public string Name { get; set; }

		[Option("protocol", Required = false, HelpText = "Protocol, tcp or udp.", DefaultValue = NetworkProtocol.UDP)]
		public NetworkProtocol Protocol { get; set; }

		[Option("host", Required = true, HelpText = "Host.")]
		public string Host { get; set; }

		[Option("port", Required = true, HelpText = "Port.")]
		public int Port { get; set; }

		[Option("gateHost", Required = false, HelpText = "GateHost.")]
		public string GateHost { get; set; }

		[Option("gatePort", Required = false, HelpText = "GatePort.")]
		public int GatePort { get; set; }

		[Option('v', HelpText = "Print details during execution.")]
		public bool Verbose { get; set; }

		[HelpOption]
		public string GetUsage()
		{
			// this without using CommandLine.Text
			StringBuilder usage = new StringBuilder();
			usage.AppendLine("Quickstart Application 1.0");
			usage.AppendLine("Read user manual for usage instructions...");
			return usage.ToString();
		}
	}

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
