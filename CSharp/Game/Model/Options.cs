using System.Text;
using CommandLine;
using Common.Network;

namespace Common.Base
{
	public class Options
	{
		[Option('h', "host", Required = true, HelpText = "Host.")]
		public string Host { get; set; }

		[Option('p', "port", Required = true, HelpText = "Port.")]
		public int Port { get; set; }

		[Option("protocol", Required = true, HelpText = "Protocol, tcp or udp.")]
		public NetworkProtocol Protocol { get; set; }

		[Option('v', null, HelpText = "Print details during execution.")]
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
