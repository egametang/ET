using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using Common.Base;

namespace Model
{
	public class World: Entity<World>
	{
		private static readonly World instance = new World();

		private Assembly assembly;

		public static World Instance
		{
			get
			{
				return instance;
			}
		}

		private List<IRunner> iRunners;

		private bool isStop;

		private World()
		{
		}

		public void Load()
		{
			this.assembly = Assembly.Load(File.ReadAllBytes(@"./Controller.dll"));
			this.iRunners = new List<IRunner>();

			foreach (Component<World> component in this.GetComponents())
			{
				IAssemblyLoader assemblyLoader = component as IAssemblyLoader;
				if (assemblyLoader != null)
				{
					assemblyLoader.Load(this.assembly);
				}
				
				IRunner runner = component as IRunner;
				if (runner != null)
				{
					this.iRunners.Add(runner);
				}
			}
		}

		public void Start()
		{
			while (!isStop)
			{
				Thread.Sleep(1);
				foreach (IRunner runner in this.iRunners)
				{
					runner.Run();
				}
			}
		}

		public void Stop()
		{
			isStop = true;
		}
	}
}