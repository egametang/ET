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
			StartConfig[] startConfigs = Game.Scene.GetComponent<StartConfigComponent>().GetAll();
			foreach (StartConfig startConfig in startConfigs)
			{
				if (!ips.Contains(startConfig.IP))
				{
					continue;
				}

				if (startConfig.Options.AppType == AppType.Manager)
				{
					continue;
				}

				string arguments = $"--id={startConfig.Options.Id} --appType={startConfig.Options.AppType}";

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