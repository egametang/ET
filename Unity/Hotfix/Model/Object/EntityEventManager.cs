using System.Collections.Generic;

namespace Model
{
	public sealed class EntityEventManager
	{
		public Queue<Disposer> adds = new Queue<Disposer>();
		public Queue<Disposer> removes = new Queue<Disposer>();

		public HashSet<IUpdate> updates = new HashSet<IUpdate>();
		public HashSet<ILoad> loads = new HashSet<ILoad>();

		public void Add(Disposer disposer)
		{
			adds.Enqueue(disposer);
		}

		public void Remove(Disposer disposer)
		{
			removes.Enqueue(disposer);
		}

		public void Update()
		{
			while(adds.Count > 0)
			{
				Disposer disposer = adds.Dequeue();

				IUpdate update = disposer as IUpdate;
				if (update == null)
				{
					continue;
				}
				updates.Add(update);

				ILoad load = disposer as ILoad;
				if (load == null)
				{
					continue;
				}
				loads.Add(load);
			}

			while (removes.Count > 0)
			{
				Disposer disposer = removes.Dequeue();

				IUpdate update = disposer as IUpdate;
				if (update == null)
				{
					continue;
				}
				updates.Remove(update);

				ILoad load = disposer as ILoad;
				if (load == null)
				{
					continue;
				}
				loads.Remove(load);
			}


			foreach(IUpdate update in updates)
			{
				Disposer disposer = (Disposer)update;
				if (removes.Contains(disposer))
				{
					continue;
				}
				update.Update();
			}
		}

		public void Load()
		{
			foreach (ILoad load in loads)
			{
				Disposer disposer = (Disposer)load;
				if (removes.Contains(disposer))
				{
					continue;
				}
				load.Load();
			}
		}
	}
}