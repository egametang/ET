using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Base;

namespace Model
{
	[ObjectEvent]
	public class AppManagerComponentEvent : ObjectEvent<AppManagerComponent>, IAwake
	{
		public void Awake()
		{
			this.GetValue().Awake();
		}
	}

	public class AppManagerComponent : Component
    {
		private readonly Dictionary<int, Process> processes = new Dictionary<int, Process>();

		public void Awake()
		{
			string[] ips = NetHelper.GetAddressIPs();
			CommandLines commandLines = Game.Scene.GetComponent<OptionsComponent>().AllOptions;
			foreach (Options options in commandLines.Options)
			{
				if (!ips.Contains(options.IP))
				{
					continue;
				}

				if (options.AppType == AppType.Manager)
				{
					continue;
				}

				string arguments = $"--appType={options.AppType} --id={options.Id} --Protocol={options.Protocol} --Host={options.Host} --Port={options.Port}";

				ProcessStartInfo info = new ProcessStartInfo(@"App.exe", arguments)
				{
					UseShellExecute = true,
					WorkingDirectory = @"..\Server\Bin\Debug"
				};
				Process process = Process.Start(info);
				this.processes.Add(process.Id, process);
			}
		}
    }
}