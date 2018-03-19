using System;
using System.Collections.Generic;
using ETModel;

namespace ETHotfix
{
	public sealed class EventSystem
	{
		private readonly Dictionary<long, Component> allComponents = new Dictionary<long, Component>();

		private readonly Dictionary<string, List<IEvent>> allEvents = new Dictionary<string, List<IEvent>>();

		private readonly UnOrderMultiMap<Type, AAwakeSystem> awakeEvents = new UnOrderMultiMap<Type, AAwakeSystem>();

		private readonly UnOrderMultiMap<Type, AStartSystem> startEvents = new UnOrderMultiMap<Type, AStartSystem>();

		private readonly UnOrderMultiMap<Type, ALoadSystem> loadEvents = new UnOrderMultiMap<Type, ALoadSystem>();

		private readonly UnOrderMultiMap<Type, AUpdateSystem> updateEvents = new UnOrderMultiMap<Type, AUpdateSystem>();

		private readonly UnOrderMultiMap<Type, ALateUpdateSystem> lateUpdateEvents = new UnOrderMultiMap<Type, ALateUpdateSystem>();

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

				AAwakeSystem objectSystem = obj as AAwakeSystem;
				if (objectSystem != null)
				{
					this.awakeEvents.Add(objectSystem.Type(), objectSystem);
				}

				AUpdateSystem aUpdateSystem = obj as AUpdateSystem;
				if (aUpdateSystem != null)
				{
					this.updateEvents.Add(aUpdateSystem.Type(), aUpdateSystem);
				}

				ALateUpdateSystem aLateUpdateSystem = obj as ALateUpdateSystem;
				if (aLateUpdateSystem != null)
				{
					this.lateUpdateEvents.Add(aLateUpdateSystem.Type(), aLateUpdateSystem);
				}

				AStartSystem aStartSystem = obj as AStartSystem;
				if (aStartSystem != null)
				{
					this.startEvents.Add(aStartSystem.Type(), aStartSystem);
				}

				ALoadSystem aLoadSystem = obj as ALoadSystem;
				if (aLoadSystem != null)
				{
					this.loadEvents.Add(aLoadSystem.Type(), aLoadSystem);
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
			List<AAwakeSystem> iAwakeSystems = this.awakeEvents[component.GetType()];
			if (iAwakeSystems == null)
			{
				return;
			}

			foreach (AAwakeSystem aAwakeSystem in iAwakeSystems)
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
				iAwake.Run(component);
			}
		}

		public void Awake<P1>(Component component, P1 p1)
		{
			List<AAwakeSystem> iAwakeSystems = this.awakeEvents[component.GetType()];
			if (iAwakeSystems == null)
			{
				return;
			}
			
			foreach (AAwakeSystem aAwakeSystem in iAwakeSystems)
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
				iAwake.Run(component, p1);
			}
		}

		public void Awake<P1, P2>(Component component, P1 p1, P2 p2)
		{
			List<AAwakeSystem> iAwakeSystems = this.awakeEvents[component.GetType()];
			if (iAwakeSystems == null)
			{
				return;
			}

			foreach (AAwakeSystem aAwakeSystem in iAwakeSystems)
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
				iAwake.Run(component, p1, p2);
			}
		}

		public void Awake<P1, P2, P3>(Component component, P1 p1, P2 p2, P3 p3)
		{
			List<AAwakeSystem> iAwakeSystems = this.awakeEvents[component.GetType()];
			if (iAwakeSystems == null)
			{
				return;
			}

			foreach (AAwakeSystem aAwakeSystem in iAwakeSystems)
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
				iAwake.Run(component, p1, p2, p3);
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
				
				List<ALoadSystem> aLoadSystems = this.loadEvents[component.GetType()];
				if (aLoadSystems == null)
				{
					continue;
				}

				this.loaders2.Enqueue(instanceId);

				foreach (ALoadSystem aLoadSystem in aLoadSystems)
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

				List<AStartSystem> aStartSystems = this.startEvents[component.GetType()];
				if (aStartSystems == null)
				{
					continue;
				}

				foreach (AStartSystem aStartSystem in aStartSystems)
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
				
				List<AUpdateSystem> aUpdateSystems = this.updateEvents[component.GetType()];
				if (aUpdateSystems == null)
				{
					continue;
				}

				this.updates2.Enqueue(instanceId);

				foreach (AUpdateSystem aUpdateSystem in aUpdateSystems)
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
				
				List<ALateUpdateSystem> aLateUpdateSystems = this.lateUpdateEvents[component.GetType()];
				if (aLateUpdateSystems == null)
				{
					continue;
				}

				this.lateUpdates2.Enqueue(instanceId);

				foreach (ALateUpdateSystem aLateUpdateSystem in aLateUpdateSystems)
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