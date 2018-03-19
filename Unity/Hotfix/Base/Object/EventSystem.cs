using System;
using System.Collections.Generic;
using ETModel;

namespace ETHotfix
{
	public sealed class EventSystem
	{
		private readonly Dictionary<long, Component> allComponents = new Dictionary<long, Component>();

		private readonly Dictionary<string, List<IEvent>> allEvents = new Dictionary<string, List<IEvent>>();

		private readonly UnOrderMultiMap<Type, IAwakeSystem> awakeEvents = new UnOrderMultiMap<Type, IAwakeSystem>();

		private readonly UnOrderMultiMap<Type, IStartSystem> startEvents = new UnOrderMultiMap<Type, IStartSystem>();

		private readonly UnOrderMultiMap<Type, IDestroySystem> desdroyEvents = new UnOrderMultiMap<Type, IDestroySystem>();

		private readonly UnOrderMultiMap<Type, ILoadSystem> loadEvents = new UnOrderMultiMap<Type, ILoadSystem>();

		private readonly UnOrderMultiMap<Type, IUpdateSystem> updateEvents = new UnOrderMultiMap<Type, IUpdateSystem>();

		private readonly UnOrderMultiMap<Type, ILateUpdateSystem> lateUpdateEvents = new UnOrderMultiMap<Type, ILateUpdateSystem>();

		private Queue<long> updates = new Queue<long>();
		private Queue<long> updates2 = new Queue<long>();

		private readonly Queue<long> starts = new Queue<long>();

		private Queue<long> loaders = new Queue<long>();
		private Queue<long> loaders2 = new Queue<long>();

		private Queue<long> lateUpdates = new Queue<long>();
		private Queue<long> lateUpdates2 = new Queue<long>();

		public EventSystem()
		{
			Type[] types = ETModel.Game.Hotfix.GetHotfixTypes();
			foreach (Type type in types)
			{
				object[] attrs = type.GetCustomAttributes(typeof(ObjectSystemAttribute), false);

				if (attrs.Length == 0)
				{
					continue;
				}

				object obj = Activator.CreateInstance(type);

				IAwakeSystem objectSystem = obj as IAwakeSystem;
				if (objectSystem != null)
				{
					this.awakeEvents.Add(objectSystem.Type(), objectSystem);
				}

				IUpdateSystem updateSystem = obj as IUpdateSystem;
				if (updateSystem != null)
				{
					this.updateEvents.Add(updateSystem.Type(), updateSystem);
				}

				ILateUpdateSystem lateUpdateSystem = obj as ILateUpdateSystem;
				if (lateUpdateSystem != null)
				{
					this.lateUpdateEvents.Add(lateUpdateSystem.Type(), lateUpdateSystem);
				}

				IStartSystem startSystem = obj as IStartSystem;
				if (startSystem != null)
				{
					this.startEvents.Add(startSystem.Type(), startSystem);
				}

				IDestroySystem destroySystem = obj as IDestroySystem;
				if (destroySystem != null)
				{
					this.desdroyEvents.Add(destroySystem.Type(), destroySystem);
				}

				ILoadSystem loadSystem = obj as ILoadSystem;
				if (loadSystem != null)
				{
					this.loadEvents.Add(loadSystem.Type(), loadSystem);
				}
			}

			this.allEvents.Clear();
			foreach (Type type in types)
			{
				object[] attrs = type.GetCustomAttributes(typeof(EventAttribute), false);

				foreach (object attr in attrs)
				{
					EventAttribute aEventAttribute = (EventAttribute)attr;
					object obj = Activator.CreateInstance(type);
					IEvent iEvent = obj as IEvent;
					if (iEvent == null)
					{
						Log.Error($"{obj.GetType().Name} 没有继承IEvent");
					}
					this.RegisterEvent(aEventAttribute.Type, iEvent);

					// hotfix的事件也要注册到mono层，hotfix可以订阅mono层的事件
					Action<List<object>> action = list => { Handle(aEventAttribute.Type, list); };
					ETModel.Game.EventSystem.RegisterEvent(aEventAttribute.Type, new EventProxy(action));
				}
			}

			this.Load();
		}

		public static void Handle(string type, List<object> param)
		{
			switch (param.Count)
			{
				case 0:
					Game.EventSystem.Run(type);
					break;
				case 1:
					Game.EventSystem.Run(type, param[0]);
					break;
				case 2:
					Game.EventSystem.Run(type, param[0], param[1]);
					break;
				case 3:
					Game.EventSystem.Run(type, param[0], param[1], param[2]);
					break;
			}
		}
		
		public void RegisterEvent(string eventId, IEvent e)
		{
			if (!this.allEvents.ContainsKey(eventId))
			{
				this.allEvents.Add(eventId, new List<IEvent>());
			}
			this.allEvents[eventId].Add(e);
		}

		public void Add(Component component)
		{
			this.allComponents.Add(component.InstanceId, component);

			Type type = component.GetType();

			if (this.loadEvents.ContainsKey(type))
			{
				this.loaders.Enqueue(component.InstanceId);
			}

			if (this.updateEvents.ContainsKey(type))
			{
				this.updates.Enqueue(component.InstanceId);
			}

			if (this.startEvents.ContainsKey(type))
			{
				this.starts.Enqueue(component.InstanceId);
			}

			if (this.lateUpdateEvents.ContainsKey(type))
			{
				this.lateUpdates.Enqueue(component.InstanceId);
			}
		}

		public void Remove(long instanceId)
		{
			this.allComponents.Remove(instanceId);
		}

		public void Awake(Component component)
		{
			List<IAwakeSystem> iAwakeSystems = this.awakeEvents[component.GetType()];
			if (iAwakeSystems == null)
			{
				return;
			}

			foreach (IAwakeSystem aAwakeSystem in iAwakeSystems)
			{
				if (aAwakeSystem == null)
				{
					continue;
				}
				
				IAwake iAwake = aAwakeSystem as IAwake;
				if (iAwake == null)
				{
					continue;
				}

				try
				{
					iAwake.Run(component);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}

		public void Awake<P1>(Component component, P1 p1)
		{
			List<IAwakeSystem> iAwakeSystems = this.awakeEvents[component.GetType()];
			if (iAwakeSystems == null)
			{
				return;
			}
			
			foreach (IAwakeSystem aAwakeSystem in iAwakeSystems)
			{
				if (aAwakeSystem == null)
				{
					continue;
				}
				
				IAwake<P1> iAwake = aAwakeSystem as IAwake<P1>;
				if (iAwake == null)
				{
					continue;
				}

				try
				{
					iAwake.Run(component, p1);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}

		public void Awake<P1, P2>(Component component, P1 p1, P2 p2)
		{
			List<IAwakeSystem> iAwakeSystems = this.awakeEvents[component.GetType()];
			if (iAwakeSystems == null)
			{
				return;
			}

			foreach (IAwakeSystem aAwakeSystem in iAwakeSystems)
			{
				if (aAwakeSystem == null)
				{
					continue;
				}
				
				IAwake<P1, P2> iAwake = aAwakeSystem as IAwake<P1, P2>;
				if (iAwake == null)
				{
					continue;
				}

				try
				{
					iAwake.Run(component, p1, p2);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}

		public void Awake<P1, P2, P3>(Component component, P1 p1, P2 p2, P3 p3)
		{
			List<IAwakeSystem> iAwakeSystems = this.awakeEvents[component.GetType()];
			if (iAwakeSystems == null)
			{
				return;
			}

			foreach (IAwakeSystem aAwakeSystem in iAwakeSystems)
			{
				if (aAwakeSystem == null)
				{
					continue;
				}
				
				IAwake<P1, P2, P3> iAwake = aAwakeSystem as IAwake<P1, P2, P3>;
				if (iAwake == null)
				{
					continue;
				}

				try
				{
					iAwake.Run(component, p1, p2, p3);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}

		public void Load()
		{
			while (this.loaders.Count > 0)
			{
				long instanceId = this.loaders.Dequeue();
				Component component;
				if (!this.allComponents.TryGetValue(instanceId, out component))
				{
					continue;
				}
				if (component.IsDisposed)
				{
					continue;
				}
				
				List<ILoadSystem> aLoadSystems = this.loadEvents[component.GetType()];
				if (aLoadSystems == null)
				{
					continue;
				}

				this.loaders2.Enqueue(instanceId);

				foreach (ILoadSystem aLoadSystem in aLoadSystems)
				{
					try
					{
						aLoadSystem.Run(component);
					}
					catch (Exception e)
					{
						Log.Error(e);
					}
				}
			}

			ObjectHelper.Swap(ref this.loaders, ref this.loaders2);
		}

		private void Start()
		{
			while (this.starts.Count > 0)
			{
				long instanceId = this.starts.Dequeue();
				Component component;
				if (!this.allComponents.TryGetValue(instanceId, out component))
				{
					continue;
				}

				List<IStartSystem> aStartSystems = this.startEvents[component.GetType()];
				if (aStartSystems == null)
				{
					continue;
				}

				foreach (IStartSystem aStartSystem in aStartSystems)
				{
					try
					{
						aStartSystem.Run(component);
					}
					catch (Exception e)
					{
						Log.Error(e);
					}
				}
			}
		}

		public void Desdroy(Component component)
		{
			List<IDestroySystem> iDestroySystems = this.desdroyEvents[component.GetType()];
			if (iDestroySystems == null)
			{
				return;
			}

			foreach (IDestroySystem aDestroySystem in iDestroySystems)
			{
				if (aDestroySystem == null)
				{
					continue;
				}

				try
				{
					aDestroySystem.Run(component);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}

		public void Update()
		{
			this.Start();
			
			while (this.updates.Count > 0)
			{
				long instanceId = this.updates.Dequeue();
				Component component;
				if (!this.allComponents.TryGetValue(instanceId, out component))
				{
					continue;
				}
				if (component.IsDisposed)
				{
					continue;
				}
				
				List<IUpdateSystem> aUpdateSystems = this.updateEvents[component.GetType()];
				if (aUpdateSystems == null)
				{
					continue;
				}

				this.updates2.Enqueue(instanceId);

				foreach (IUpdateSystem aUpdateSystem in aUpdateSystems)
				{
					try
					{
						aUpdateSystem.Run(component);
					}
					catch (Exception e)
					{
						Log.Error(e);
					}
				}
			}

			ObjectHelper.Swap(ref this.updates, ref this.updates2);
		}

		public void LateUpdate()
		{
			while (this.lateUpdates.Count > 0)
			{
				long instanceId = this.lateUpdates.Dequeue();
				Component component;
				if (!this.allComponents.TryGetValue(instanceId, out component))
				{
					continue;
				}
				if (component.IsDisposed)
				{
					continue;
				}
				
				List<ILateUpdateSystem> aLateUpdateSystems = this.lateUpdateEvents[component.GetType()];
				if (aLateUpdateSystems == null)
				{
					continue;
				}

				this.lateUpdates2.Enqueue(instanceId);

				foreach (ILateUpdateSystem aLateUpdateSystem in aLateUpdateSystems)
				{
					try
					{
						aLateUpdateSystem.Run(component);
					}
					catch (Exception e)
					{
						Log.Error(e);
					}
				}
			}

			ObjectHelper.Swap(ref this.lateUpdates, ref this.lateUpdates2);
		}

		public void Run(string type)
		{
			List<IEvent> iEvents;
			if (!this.allEvents.TryGetValue(type, out iEvents))
			{
				return;
			}
			foreach (IEvent iEvent in iEvents)
			{
				try
				{
					iEvent?.Handle();
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}

		public void Run<A>(string type, A a)
		{
			List<IEvent> iEvents;
			if (!this.allEvents.TryGetValue(type, out iEvents))
			{
				return;
			}
			foreach (IEvent iEvent in iEvents)
			{
				try
				{
					iEvent?.Handle(a);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}

		public void Run<A, B>(string type, A a, B b)
		{
			List<IEvent> iEvents;
			if (!this.allEvents.TryGetValue(type, out iEvents))
			{
				return;
			}
			foreach (IEvent iEvent in iEvents)
			{
				try
				{
					iEvent?.Handle(a, b);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}

		public void Run<A, B, C>(string type, A a, B b, C c)
		{
			List<IEvent> iEvents;
			if (!this.allEvents.TryGetValue(type, out iEvents))
			{
				return;
			}
			foreach (IEvent iEvent in iEvents)
			{
				try
				{
					iEvent?.Handle(a, b, c);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}
	}
}