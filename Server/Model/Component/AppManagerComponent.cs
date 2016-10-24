using System;
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
				if (!ips.Contains(startConfig.IP) && startConfig.IP != "*")
				{
					continue;
				}

				if (startConfig.Options.AppType == AppType.Manager)
				{
					continue;
				}


#if __MonoCS__
				const string exe = @"mono";
				string arguments = $"App.exe --id={startConfig.Options.Id} --appType={startConfig.Options.AppType}";
#else
				const string exe = @"App.exe";
				string arguments = $"--id={startConfig.Options.Id} --appType={startConfig.Options.AppType}";
#endif

				Log.Info($"{exe} {arguments}");
				try
				{
					ProcessStartInfo info = new ProcessStartInfo
					{
						FileName = exe,
						Arguments = arguments,
						CreateNoWindow = true,
						UseShellExecute = true
					};

					Process process = Process.Start(info);
					this.processes.Add(process.Id, process);
				}
				catch (Exception e)
				{
					Log.Error(e.ToString());
				}
			}
		}
    }
}