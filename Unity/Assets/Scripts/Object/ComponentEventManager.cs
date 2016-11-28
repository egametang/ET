using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Base;

namespace Model
{
	[Flags]
	public enum DisposerEventType
	{
		Awake = 1,
		Awake1 = 2,
		Awake2 = 4,
		Awake3 = 8,
		Update = 16,
		Load = 32,
	}

	public class DisposerTypeInfo
	{
		private readonly Dictionary<DisposerEventType, MethodInfo> infos = new Dictionary<DisposerEventType, MethodInfo>();

		public void Add(DisposerEventType type, MethodInfo methodInfo)
		{
			try
			{
				this.infos.Add(type, methodInfo);
			}
			catch (Exception e)
			{
				throw new Exception($"Add DisposerEventType MethodInfo Error: {type}", e);
			}
		}

		public MethodInfo Get(DisposerEventType type)
		{
			MethodInfo methodInfo;
			this.infos.TryGetValue(type, out methodInfo);
			return methodInfo;
		}

		public DisposerEventType[] GetDisposerEvent2Types()
		{
			return this.infos.Keys.ToArray();
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			foreach (DisposerEventType disposerEventType in this.infos.Keys.ToArray())
			{
				sb.Append($"{disposerEventType} {this.infos[disposerEventType].Name} ");
			}
			return sb.ToString();
		}
	}

	public sealed class ComponentEventManager
	{
		private readonly Dictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>();

		private readonly Dictionary<DisposerEventType, HashSet<Disposer>> disposers = new Dictionary<DisposerEventType, HashSet<Disposer>>();

		private Dictionary<Type, DisposerTypeInfo> eventInfo;

		public ComponentEventManager()
		{
			foreach (DisposerEventType t in Enum.GetValues(typeof(DisposerEventType)))
			{
				this.disposers.Add(t, new HashSet<Disposer>());
			}
		}

		public void Register(string name, Assembly assembly)
		{
			this.eventInfo = new Dictionary<Type, DisposerTypeInfo>();

			this.assemblies[name] = assembly;
			
			foreach (Assembly ass in this.assemblies.Values)
			{
				Type[] types = ass.GetTypes();
				foreach (Type type in types)
				{
					object[] attrs = type.GetCustomAttributes(typeof(ComponentEventAttribute), true);
					if (attrs.Length == 0)
					{
						continue;
					}

					ComponentEventAttribute componentEventAttribute = attrs[0] as ComponentEventAttribute;

					Type type2 = componentEventAttribute.ClassType;

					if (!this.eventInfo.ContainsKey(type2))
					{
						this.eventInfo.Add(type2, new DisposerTypeInfo());
					}

					foreach (MethodInfo methodInfo in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly))
					{
						int n = methodInfo.GetParameters().Length;
						if (methodInfo.IsStatic)
						{
							--n;
						}
						string sn = n > 0? $"{methodInfo.Name}{n}" : methodInfo.Name;
						foreach (string s in Enum.GetNames(typeof(DisposerEventType)))
						{
							if (s != sn)
							{
								continue;
							}
							DisposerEventType t = EnumHelper.FromString<DisposerEventType>(s);
							this.eventInfo[type2].Add(t, methodInfo);
							break;
						}
					}
				}
			}

			this.Load();
		}

		public void Add(Disposer disposer)
		{
			DisposerTypeInfo disposerTypeInfo;
			if (!this.eventInfo.TryGetValue(disposer.GetType(), out disposerTypeInfo))
			{
				return;
			}

			foreach (DisposerEventType disposerEvent2Type in disposerTypeInfo.GetDisposerEvent2Types())
			{
				this.disposers[disposerEvent2Type].Add(disposer);
			}
		}

		public void Remove(Disposer disposer)
		{
			DisposerTypeInfo disposerTypeInfo;
			if (!this.eventInfo.TryGetValue(disposer.GetType(), out disposerTypeInfo))
			{
				return;
			}

			foreach (DisposerEventType disposerEvent2Type in disposerTypeInfo.GetDisposerEvent2Types())
			{
				this.disposers[disposerEvent2Type].Remove(disposer);
			}
		}

		public Assembly GetAssembly(string name)
		{
			return this.assemblies[name];
		}

		public Assembly[] GetAssemblies()
		{
			return this.assemblies.Values.ToArray();
		}

		private void Load()
		{
			HashSet<Disposer> list;
			if (!this.disposers.TryGetValue(DisposerEventType.Update, out list))
			{
				return;
			}
			foreach (Disposer disposer in list)
			{
				DisposerTypeInfo disposerTypeInfo = this.eventInfo[disposer.GetType()];
				disposerTypeInfo.Get(DisposerEventType.Load).Run(disposer);
			}
		}

		public void Awake(Disposer disposer)
		{
			DisposerTypeInfo disposerTypeInfo = this.eventInfo[disposer.GetType()];
			disposerTypeInfo.Get(DisposerEventType.Awake)?.Run(disposer);
		}

		public void Awake(Disposer disposer, object p1)
		{
			DisposerTypeInfo disposerTypeInfo = this.eventInfo[disposer.GetType()];
			disposerTypeInfo.Get(DisposerEventType.Awake1)?.Run(disposer, p1);
		}

		public void Awake(Disposer disposer, object p1, object p2)
		{
			DisposerTypeInfo disposerTypeInfo = this.eventInfo[disposer.GetType()];
			disposerTypeInfo.Get(DisposerEventType.Awake2)?.Run(disposer, p1, p2 );
		}

		public void Awake(Disposer disposer, object p1, object p2, object p3)
		{
			DisposerTypeInfo disposerTypeInfo = this.eventInfo[disposer.GetType()];
			disposerTypeInfo.Get(DisposerEventType.Awake3)?.Run(disposer, p1, p2, p3 );
		}

		public void Update()
		{
			HashSet<Disposer> list;
			if (!this.disposers.TryGetValue(DisposerEventType.Update, out list))
			{
				return;
			}
			foreach (Disposer disposer in list)
			{
				DisposerTypeInfo disposerTypeInfo = this.eventInfo[disposer.GetType()];
				disposerTypeInfo.Get(DisposerEventType.Update).Run(disposer);
			}
		}

		public string MethodInfo()
		{
			StringBuilder sb = new StringBuilder();
			foreach (Type type in this.eventInfo.Keys.ToArray())
			{
				sb.Append($"{type.Name} {this.eventInfo[type]}\n");
			}
			return sb.ToString();
		}
	}
}