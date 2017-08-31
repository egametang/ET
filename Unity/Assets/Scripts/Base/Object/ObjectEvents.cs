using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Model
{
	public sealed class ObjectEvents
	{
		private static ObjectEvents instance;


		private Queue<Disposer> loads = new Queue<Disposer>();
		private Queue<Disposer> loads2 = new Queue<Disposer>();

		private Queue<Disposer> starts = new Queue<Disposer>();

		private Queue<Disposer> updates = new Queue<Disposer>();
		private Queue<Disposer> updates2 = new Queue<Disposer>();

		private Queue<Disposer> lateUpdates = new Queue<Disposer>();
		private Queue<Disposer> lateUpdates2 = new Queue<Disposer>();
		
		public static ObjectEvents Instance
		{
			get
			{
				return instance ?? (instance = new ObjectEvents());
			}
		}

		public static void Close()
		{
			instance = null;
		}

		private readonly Dictionary<string, Assembly> dictionary = new Dictionary<string, Assembly>();

		public void Add(string name, Assembly assembly)
		{
			this.dictionary[name] = assembly;
			this.Load();
		}

		public void Remove(string name)
		{
			this.dictionary.Remove(name);
		}

		public Assembly[] GetAll()
		{
			return this.dictionary.Values.ToArray();
		}

		public Assembly Get(string name)
		{
			return this.dictionary[name];
		}

		public void Add(Disposer disposer)
		{
			if (disposer is ILoad)
			{
				this.loads.Enqueue(disposer);
			}

			if (disposer is IStart)
			{
				this.starts.Enqueue(disposer);
			}

			if (disposer is IUpdate)
			{
				this.updates.Enqueue(disposer);
			}

			if (disposer is ILateUpdate)
			{
				this.lateUpdates.Enqueue(disposer);
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

		private void Start()
		{
			while (this.starts.Count > 0)
			{
				Disposer disposer = this.starts.Dequeue();
				if (disposer.Id == 0)
				{
					continue;
				}

				((IStart)disposer).Start();
			}
		}

		public void Update()
		{
			this.Start();

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