using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace ETModel
{
	[ObjectSystem]
	public class AppManagerComponentAwakeSystem : AwakeSystem<AppManagerComponent>
	{
		public override void Awake(AppManagerComponent self)
		{
			self.Awake();
		}
	}

	public class AppManagerComponent: Component
	{
		private readonly Dictionary<int, Process> processes = new Dictionary<int, Process>();

		public void Awake()
		{
			string[] ips = NetHelper.GetAddressIPs();
			StartConfig[] startConfigs = StartConfigComponent.Instance.GetAll();
			
			foreach (StartConfig startConfig in startConfigs)
			{
				Game.Scene.GetComponent<TimerComponent>().WaitAsync(100);
				
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

			this.WatchProcessAsync().NoAwait();
		}

		private void StartProcess(int appId)
		{
			OptionComponent optionComponent = Game.Scene.GetComponent<OptionComponent>();
			StartConfigComponent startConfigComponent = StartConfigComponent.Instance;
			string configFile = optionComponent.Options.Config;
			StartConfig startConfig = startConfigComponent.Get(appId);
			const string exe = "dotnet";
			string arguments = $"App.dll --appId={startConfig.AppId} --appType={startConfig.AppType} --config={configFile}";

			Log.Info($"{exe} {arguments}");
			try
			{
				bool useShellExecute = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
				ProcessStartInfo info = new ProcessStartInfo { FileName = exe, Arguments = arguments, CreateNoWindow = true, UseShellExecute = useShellExecute };

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
		private async ETVoid WatchProcessAsync()
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