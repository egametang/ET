using System.Text;
using CommandLine;
using Common.Network;

namespace Model
{
	public class Options
	{
		[Option('s', "serverType", Required = true, HelpText = "ServerType.")]
		public ServerType ServerType { get; set; }

		[Option('n', "name", Required = true, HelpText = "Name.")]
		public string Name { get; set; }

		[Option('h', "gateHost", Required = false, HelpText = "GateHost.")]
		public string GateHost { get; set; }

		[Option('p', "gatePort", Required = false, HelpText = "GatePort.")]
		public int GatePort { get; set; }

		[Option('h', "host", Required = true, HelpText = "Host.")]
		public string Host { get; set; }

		[Option('p', "port", Required = true, HelpText = "Port.")]
		public int Port { get; set; }

		[Option("protocol", Required = true, HelpText = "Protocol, tcp or udp.")]
		public NetworkProtocol Protocol { get; set; }

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
}