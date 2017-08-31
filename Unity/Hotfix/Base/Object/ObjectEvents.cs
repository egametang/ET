using System.Collections.Generic;

namespace Hotfix
{
	public sealed class ObjectEvents
	{
		private static ObjectEvents instance;

		private Queue<Disposer> loads = new Queue<Disposer>();
		private Queue<Disposer> loads2 = new Queue<Disposer>();

		private Queue<Disposer> updates = new Queue<Disposer>();
		private Queue<Disposer> updates2 = new Queue<Disposer>();

		private Queue<Disposer> lateUpdates = new Queue<Disposer>();
		private Queue<Disposer> lateUpdates2 = new Queue<Disposer>();

		private Queue<Disposer> frameUpdates = new Queue<Disposer>();
		private Queue<Disposer> frameUpdates2 = new Queue<Disposer>();

		public static ObjectEvents Instance
		{
			get
			{
				return instance ?? (instance = new ObjectEvents());
			}
		}

		public void Add(Disposer disposer)
		{
			if (disposer is ILoad)
			{
				loads.Enqueue(disposer);
			}

			if (disposer is IUpdate)
			{
				updates.Enqueue(disposer);
			}

			if (disposer is ILateUpdate)
			{
				lateUpdates.Enqueue(disposer);
			}
		}

		public void Load()
		{
			while (this.loads.Count > 0)
			{
				Disposer disposer = this.loads.Dequeue();
				if (disposer.Id == 0)
				{
					continue;
				}

				this.loads2.Enqueue(disposer);

				((ILoad)disposer).Load();
			}

			ObjectHelper.Swap(ref this.loads, ref this.loads2);
		}

		public void Update()
		{
			while (this.updates.Count > 0)
			{
				Disposer disposer = this.updates.Dequeue();
				if (disposer.Id == 0)
				{
					continue;
				}

				this.updates2.Enqueue(disposer);

				((IUpdate)disposer).Update();
			}

			ObjectHelper.Swap(ref this.updates, ref this.updates2);
		}

		public void LateUpdate()
		{
			while (this.lateUpdates.Count > 0)
			{
				Disposer disposer = this.lateUpdates.Dequeue();
				if (disposer.Id == 0)
				{
					continue;
				}

				this.lateUpdates2.Enqueue(disposer);

				((ILateUpdate)disposer).LateUpdate();
			}

			ObjectHelper.Swap(ref this.lateUpdates, ref this.lateUpdates2);
		}
	}
}