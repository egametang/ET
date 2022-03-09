using System;
using System.Collections.Generic;

namespace ET
{
	[ObjectSystem]
	public class NumericWatcherComponentAwakeSystem : AwakeSystem<NumericWatcherComponent>
	{
		public override void Awake(NumericWatcherComponent self)
		{
			NumericWatcherComponent.Instance = self;
			self.Awake();
		}
	}

	
	public class NumericWatcherComponentLoadSystem : LoadSystem<NumericWatcherComponent>
	{
		public override void Load(NumericWatcherComponent self)
		{
			self.Load();
		}
	}

	/// <summary>
	/// 监视数值变化组件,分发监听
	/// </summary>
	public class NumericWatcherComponent : Entity, IAwake, ILoad
	{
		public static NumericWatcherComponent Instance { get; set; }
		
		private Dictionary<int, List<INumericWatcher>> allWatchers;

		public void Awake()
		{
			this.Load();
		}

		public void Load()
		{
			this.allWatchers = new Dictionary<int, List<INumericWatcher>>();

			HashSet<Type> types = Game.EventSystem.GetTypes(typeof(NumericWatcherAttribute));
			foreach (Type type in types)
			{
				object[] attrs = type.GetCustomAttributes(typeof(NumericWatcherAttribute), false);

				foreach (object attr in attrs)
				{
					NumericWatcherAttribute numericWatcherAttribute = (NumericWatcherAttribute)attr;
					INumericWatcher obj = (INumericWatcher)Activator.CreateInstance(type);
					if (!this.allWatchers.ContainsKey(numericWatcherAttribute.NumericType))
					{
						this.allWatchers.Add(numericWatcherAttribute.NumericType, new List<INumericWatcher>());
					}
					this.allWatchers[numericWatcherAttribute.NumericType].Add(obj);
				}
			}
		}

		public void Run(EventType.NumbericChange args)
		{
			List<INumericWatcher> list;
			if (!this.allWatchers.TryGetValue(args.NumericType, out list))
			{
				return;
			}
			foreach (INumericWatcher numericWatcher in list)
			{
				numericWatcher.Run(args);
			}
		}
	}
}