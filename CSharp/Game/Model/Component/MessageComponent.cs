using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Common.Base;

namespace Model
{
	public class MessageComponent: Component<World>, IAssemblyLoader
	{
		private Dictionary<ushort, List<IEventSync>> eventSyncs;
		private Dictionary<ushort, List<IEventAsync>> eventAsyncs;
		private Dictionary<ushort, Type> typeClassType;

		public void Load(Assembly assembly)
		{
			this.eventSyncs = new Dictionary<ushort, List<IEventSync>>();
			this.eventAsyncs = new Dictionary<ushort, List<IEventAsync>>();
			this.typeClassType = new Dictionary<ushort, Type>();

			ServerType serverType = World.Instance.Options.ServerType;

			Type[] types = assembly.GetTypes();
			foreach (Type t in types)
			{
				object[] attrs = t.GetCustomAttributes(typeof (MessageAttribute), false);
				if (attrs.Length == 0)
				{
					continue;
				}

				MessageAttribute messageAttribute = (MessageAttribute)attrs[0];
				if (!messageAttribute.Contains(serverType))
				{
					continue;
				}

				this.typeClassType[messageAttribute.Opcode] = messageAttribute.ClassType;

				object obj = Activator.CreateInstance(t);

				IEventSync iEventSync = obj as IEventSync;
				if (iEventSync != null)
				{
					if (!this.eventSyncs.ContainsKey(messageAttribute.Opcode))
					{
						this.eventSyncs.Add(messageAttribute.Opcode, new List<IEventSync>());
					}
					this.eventSyncs[messageAttribute.Opcode].Add(iEventSync);
					continue;
				}

				IEventAsync iEventAsync = obj as IEventAsync;
				if (iEventAsync != null)
				{
					if (!this.eventAsyncs.ContainsKey(messageAttribute.Opcode))
					{
						this.eventAsyncs.Add(messageAttribute.Opcode, new List<IEventAsync>());
					}
					this.eventAsyncs[messageAttribute.Opcode].Add(iEventAsync);
					continue;
				}

				throw new Exception(string.Format("message handler not inherit IEventSync or IEventAsync interface: {0}",
						obj.GetType().FullName));
			}
		}

		public Type GetClassType(ushort opcode)
		{
			return this.typeClassType[opcode];
		}

		public void Run(ushort opcode, Env env)
		{
			List<IEventSync> iEventSyncs = null;
			if (!this.eventSyncs.TryGetValue(opcode, out iEventSyncs))
			{
				throw new Exception(string.Format("no message handler, MessageAttribute: {0} opcode: {1}",
						typeof(MessageAttribute).Name, opcode));
			}

			foreach (IEventSync iEventSync in iEventSyncs)
			{
				iEventSync.Run(env);
			}
		}

		public async Task RunAsync(ushort opcode, Env env)
		{
			List<IEventSync> iEventSyncs = null;
			this.eventSyncs.TryGetValue(opcode, out iEventSyncs);

			List<IEventAsync> iEventAsyncs = null;
			this.eventAsyncs.TryGetValue(opcode, out iEventAsyncs);

			if (iEventSyncs == null && iEventAsyncs == null)
			{
				throw new Exception(string.Format("no message handler, MessageAttribute: {0} opcode: {1}",
						typeof(MessageAttribute).Name, opcode));
			}

			if (iEventSyncs != null)
			{
				foreach (IEventSync iEventSync in iEventSyncs)
				{
					iEventSync.Run(env);
				}
			}

			if (iEventAsyncs != null)
			{
				foreach (IEventAsync iEventAsync in iEventAsyncs)
				{
					await iEventAsync.RunAsync(env);
				}
			}
		}
	}
}