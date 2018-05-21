using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ETModel
{
	[ObjectSystem]
	public class AppManagerComponentStartSystem : StartSystem<AppManagerComponent>
	{
		public override void Start(AppManagerComponent self)
		{
			self.Start();
		}
	}

	public class AppManagerComponent: Component
	{
		private readonly Dictionary<int, Process> processes = new Dictionary<int, Process>();

		public void Start()
		{
			string[] ips = NetHelper.GetAddressIPs();
			StartConfig[] startConfigs = Game.Scene.GetComponent<StartConfigComponent>().GetAll();
			
			foreach (StartConfig startConfig in startConfigs)
			{
				if (!ips.Contains(startConfig.ServerIP) && startConfig.ServerIP != "*")
				{
					continue;
				}

				if (startConfig.AppType.Is(AppType.Manager))
				{
					continue;
				}

				StartProcess(startConfig.AppId);
			}

			this.WatchProcessAsync();
		}

		private void StartProcess(int appId)
		{
			OptionComponent optionComponent = Game.Scene.GetComponent<OptionComponent>();
			StartConfigComponent startConfigComponent = Game.Scene.GetComponent<StartConfigComponent>();
			string configFile = optionComponent.Options.Config;
			StartConfig startConfig = startConfigComponent.Get(appId);
#if __MonoCS__
			const string exe = @"dotnet";
			string arguments = $"App.dll --appId={startConfig.AppId} --appType={startConfig.AppType} --config={configFile}";
#else
			const string exe = @"dotnet";
			string arguments = $"App.dll --appId={startConfig.AppId} --appType={startConfig.AppType} --config={configFile}";
#endif

			Log.Info($"{exe} {arguments}");
			try
			{
				ProcessStartInfo info = new ProcessStartInfo { FileName = exe, Arguments = arguments, CreateNoWindow = true, UseShellExecute = true };

				Process process = Process.Start(info);
				this.processes.Add(startConfig.AppId, process);
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}

		/// <summary>
		/// 监控启动的进程,如果进程挂掉了,重新拉起
		/// </summary>
		private async void WatchProcessAsync()
		{
			long instanceId = this.InstanceId;
			
			while (true)
			{
				await Game.Scene.GetComponent<TimerComponent>().WaitAsync(5000);

				if (this.InstanceId != instanceId)
				{
					return;
				}

				foreach (int appId in this.processes.Keys.ToArray())
				{
					Process process = this.processes[appId];
					if (!process.HasExited)
					{
						continue;
					}
					this.processes.Remove(appId);
					process.Dispose();
					this.StartProcess(appId);
				}
			}
		}
	}
}