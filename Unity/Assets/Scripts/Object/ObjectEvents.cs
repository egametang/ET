using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Base;

#if !SERVER
using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Runtime.Enviorment;
using UnityEngine;
#endif

namespace Model
{
	[Flags]
	public enum EntityEventType
	{
		Awake = 1,
		Awake1 = 2,
		Awake2 = 4,
		Awake3 = 8,
		Update = 16,
		Load = 32,
		LateUpdate = 64
	}
	

	public class EntityTypeInfo
	{
		private readonly Dictionary<EntityEventType, IStaticMethod> infos = new Dictionary<EntityEventType, IStaticMethod>();

		public void Add(EntityEventType type, IStaticMethod methodInfo)
		{
			try
			{
				this.infos.Add(type, methodInfo);
			}
			catch (Exception e)
			{
				throw new Exception($"Add EntityEventType MethodInfo Error: {type}", e);
			}
		}

		public IStaticMethod Get(EntityEventType type)
		{
			IStaticMethod methodInfo;
			this.infos.TryGetValue(type, out methodInfo);
			return methodInfo;
		}

		public EntityEventType[] GetEntityEventTypes()
		{
			return this.infos.Keys.ToArray();
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			foreach (EntityEventType disposerEventType in this.infos.Keys.ToArray())
			{
				sb.Append($"{disposerEventType} {this.infos[disposerEventType].Name} ");
			}
			return sb.ToString();
		}
	}

	public sealed class ObjectEvents
	{
		private static ObjectEvents instance;

		private readonly Dictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>();

		private readonly Dictionary<EntityEventType, HashSet<Disposer>> disposers = new Dictionary<EntityEventType, HashSet<Disposer>>();

		private readonly HashSet<Disposer> addDisposers = new HashSet<Disposer>();
		private readonly HashSet<Disposer> removeDisposers = new HashSet<Disposer>();

		private Dictionary<int, EntityTypeInfo> eventInfo;
		private Dictionary<Type, int> typeToEntityEventId;

#if !SERVER
		private ILRuntime.Runtime.Enviorment.AppDomain appDomain;
#endif

		public ObjectEvents()
		{
			foreach (EntityEventType t in Enum.GetValues(typeof (EntityEventType)))
			{
				this.disposers.Add(t, new HashSet<Disposer>());
			}
		}

		public static ObjectEvents Instance
		{
			get
			{
				return instance ?? (instance = new ObjectEvents());
			}
		}

		public void Reset()
		{
			instance = null;
		}

		public void Register(string name, Assembly assembly)
		{
			this.assemblies[name] = assembly;

			LoadAssemblyInfo();

			this.Load();
		}

		public void LoadAssemblyInfo()
		{
			this.eventInfo = new Dictionary<int, EntityTypeInfo>();
			this.typeToEntityEventId = new Dictionary<Type, int>();

			Type[] types = DllHelper.GetMonoTypes();
			List<string> allEntityType = Enum.GetNames(typeof(EntityEventType)).ToList();
			foreach (Type type in types)
			{
				object[] attrs = type.GetCustomAttributes(typeof(EntityEventAttribute), true);
				if (attrs.Length == 0)
				{
					continue;
				}

				EntityEventAttribute entityEventAttribute = attrs[0] as EntityEventAttribute;

				int entityEventId = entityEventAttribute.ClassType;

				this.typeToEntityEventId[type] = entityEventId;

				if (!this.eventInfo.ContainsKey(entityEventId))
				{
					this.eventInfo.Add(entityEventId, new EntityTypeInfo());
				}

				MethodInfo[] methodInfos = type.GetMethods(
					BindingFlags.Instance | BindingFlags.Static | BindingFlags.InvokeMethod | 
					BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly);
				foreach (MethodInfo methodInfo in methodInfos)
				{
					int n = methodInfo.GetParameters().Length;
					if (methodInfo.IsStatic)
					{
						--n;
					}

					string sn = n > 0 ? $"{methodInfo.Name}{n}" : methodInfo.Name;
					if (!allEntityType.Contains(sn))
					{
						continue;
					}

					EntityEventType t = EnumHelper.FromString<EntityEventType>(sn);
					this.eventInfo[entityEventId].Add(t, new MonoStaticMethod(methodInfo));
				}
			}

#if !SERVER
			if (this.appDomain == null)
			{
				return;
			}

			IType[] ilTypes = this.appDomain.LoadedTypes.Values.ToArray();
			foreach (IType itype in ilTypes)
			{
				Type type = itype.ReflectionType;
				object[] attrs = type.GetCustomAttributes(typeof(EntityEventAttribute), true);
				if (attrs.Length == 0)
				{
					continue;
				}

				EntityEventAttribute entityEventAttribute = attrs[0] as EntityEventAttribute;

				int entityEventId = entityEventAttribute.ClassType;

				if (!this.eventInfo.ContainsKey(entityEventId))
				{
					this.eventInfo.Add(entityEventId, new EntityTypeInfo());
				}

				foreach (IMethod methodInfo in itype.GetMethods())
				{
					int n = methodInfo.ParameterCount;
					if (methodInfo.IsStatic)
					{
						--n;
					}

					string sn = n > 0 ? $"{methodInfo.Name}{n}" : methodInfo.Name;
					if (!allEntityType.Contains(sn))
					{
						continue;
					}

					EntityEventType t = EnumHelper.FromString<EntityEventType>(sn);
					this.eventInfo[entityEventId].Add(t, new ILStaticMethod(methodInfo, n));
				}
			}
#endif
		}


#if !SERVER
		public void RegisterILRuntime()
		{
			appDomain = new ILRuntime.Runtime.Enviorment.AppDomain();
			ILRuntime.Runtime.Generated.CLRBindings.Initialize(appDomain);

			GameObject code = (GameObject)Resources.Load("Code");
			byte[] assBytes = code.Get<TextAsset>("Hotfix.dll").bytes;
			byte[] mdbBytes = code.Get<TextAsset>("Hotfix.pdb").bytes;

			using (MemoryStream fs = new MemoryStream(assBytes))
			using (MemoryStream p = new MemoryStream(mdbBytes))
			{
				appDomain.LoadAssembly(fs, p, new Mono.Cecil.Pdb.PdbReaderProvider());
			}
		}

		public void RegisterILAdapter()
		{
			Assembly assembly = ObjectEvents.Instance.GetAssembly("Model");

			foreach (Type type in assembly.GetTypes())
			{
				object[] attrs = type.GetCustomAttributes(typeof(ILAdapterAttribute), false);
				if (attrs.Length == 0)
				{
					continue;
				}
				object obj = Activator.CreateInstance(type);
				CrossBindingAdaptor adaptor = obj as CrossBindingAdaptor;
				if (adaptor == null)
				{
					continue;
				}
				appDomain.RegisterCrossBindingAdaptor(adaptor);
			}
		}

		public ILRuntime.Runtime.Enviorment.AppDomain AppDomain
		{
			get
			{
				return this.appDomain;
			}
		}
#endif

		private int GetEntityEventIdByType(Type type)
		{
			int entityEventId = 0;
			this.typeToEntityEventId.TryGetValue(type, out entityEventId);
			return entityEventId;
		}

		public void Add(Disposer disposer)
		{
			this.addDisposers.Add(disposer);
		}

		public void Remove(Disposer disposer)
		{
			this.removeDisposers.Add(disposer);
		}

		private void UpdateAddDisposer()
		{
			foreach (Disposer disposer in this.addDisposers)
			{
				EntityTypeInfo entityTypeInfo;
				if (!this.eventInfo.TryGetValue(this.GetEntityEventIdByType(disposer.GetType()), out entityTypeInfo))
				{
					continue;
				}

				foreach (EntityEventType disposerEvent2Type in entityTypeInfo.GetEntityEventTypes())
				{
					this.disposers[disposerEvent2Type].Add(disposer);
				}
			}
			this.addDisposers.Clear();
		}

		private void UpdateRemoveDisposer()
		{
			foreach (Disposer disposer in this.removeDisposers)
			{
				EntityTypeInfo entityTypeInfo;
				if (!this.eventInfo.TryGetValue(this.GetEntityEventIdByType(disposer.GetType()), out entityTypeInfo))
				{
					continue;
				}

				foreach (EntityEventType disposerEvent2Type in entityTypeInfo.GetEntityEventTypes())
				{
					this.disposers[disposerEvent2Type].Remove(disposer);
				}
			}
			this.removeDisposers.Clear();
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
			if (!this.disposers.TryGetValue(EntityEventType.Load, out list))
			{
				return;
			}
			foreach (Disposer disposer in list)
			{
				EntityTypeInfo entityTypeInfo = this.eventInfo[this.GetEntityEventIdByType(disposer.GetType())];
				entityTypeInfo.Get(EntityEventType.Load).Run(disposer);
			}
		}

		public void Awake(Disposer disposer)
		{
			EntityTypeInfo entityTypeInfo;
			if (!this.eventInfo.TryGetValue(this.GetEntityEventIdByType(disposer.GetType()), out entityTypeInfo))
			{
				return;
			}
			entityTypeInfo.Get(EntityEventType.Awake)?.Run(disposer);
		}

		public void Awake(Disposer disposer, object p1)
		{
			EntityTypeInfo entityTypeInfo;
			if (!this.eventInfo.TryGetValue(this.GetEntityEventIdByType(disposer.GetType()), out entityTypeInfo))
			{
				return;
			}
			entityTypeInfo.Get(EntityEventType.Awake1)?.Run(disposer, p1);
		}

		public void Awake(Disposer disposer, object p1, object p2)
		{
			EntityTypeInfo entityTypeInfo;
			if (!this.eventInfo.TryGetValue(this.GetEntityEventIdByType(disposer.GetType()), out entityTypeInfo))
			{
				return;
			}
			entityTypeInfo.Get(EntityEventType.Awake2)?.Run(disposer, p1, p2);
		}

		public void Awake(Disposer disposer, object p1, object p2, object p3)
		{
			EntityTypeInfo entityTypeInfo;
			if (!this.eventInfo.TryGetValue(this.GetEntityEventIdByType(disposer.GetType()), out entityTypeInfo))
			{
				return;
			}
			entityTypeInfo.Get(EntityEventType.Awake3)?.Run(disposer, p1, p2, p3);
		}

		public void Update()
		{
			UpdateAddDisposer();
			UpdateRemoveDisposer();

			HashSet<Disposer> list;
			if (!this.disposers.TryGetValue(EntityEventType.Update, out list))
			{
				return;
			}
			foreach (Disposer disposer in list)
			{
				try
				{
					if (this.removeDisposers.Contains(disposer))
					{
						continue;
					}
					EntityTypeInfo entityTypeInfo = this.eventInfo[this.GetEntityEventIdByType(disposer.GetType())];
					entityTypeInfo.Get(EntityEventType.Update).Run(disposer);
				}
				catch (Exception e)
				{
					Log.Error(e.ToString());
				}
			}
		}

		public void LateUpdate()
		{
			HashSet<Disposer> list;
			if (!this.disposers.TryGetValue(EntityEventType.LateUpdate, out list))
			{
				return;
			}
			foreach (Disposer disposer in list)
			{
				try
				{
					EntityTypeInfo entityTypeInfo = this.eventInfo[this.GetEntityEventIdByType(disposer.GetType())];
					entityTypeInfo.Get(EntityEventType.LateUpdate).Run(disposer);
				}
				catch (Exception e)
				{
					Log.Error(e.ToString());
				}
			}
		}
	}
}