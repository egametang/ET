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

		public Options Options { get; set; }

		private readonly List<IUpdate> iUpdates = new List<IUpdate>();

		private bool isStop;

		private World()
		{
		}

		public void Load()
		{
			this.assembly = Assembly.Load(File.ReadAllBytes(@"./Controller.dll"));

			foreach (Component<World> component in this.GetComponents())
			{
				IAssemblyLoader assemblyLoader = component as IAssemblyLoader;
				if (assemblyLoader != null)
				{
					assemblyLoader.Load(this.assembly);
				}
			}
		}

		public void Start()
		{
			Load();

			foreach (Component<World> component in this.GetComponents())
			{
				IUpdate update = component as IUpdate;
				if (update != null)
				{
					this.iUpdates.Add(update);
				}

				IStart start = component as IStart;
				if (start != null)
				{
					start.Start();
				}
			}

			// loop
			while (!isStop)
			{
				Thread.Sleep(1);
				foreach (IUpdate update in this.iUpdates)
				{
					update.Update();
				}
			}
		}

		public void Stop()
		{
			isStop = true;
		}
	}
}