using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Common.Base;
using Common.Helper;

namespace Model
{
	public class MessageComponent: Component<World>, IAssemblyLoader
	{
		private Dictionary<Opcode, Func<byte[], byte[]>> events;
		private Dictionary<Opcode, Func<byte[], Task<byte[]>>> eventsAsync;

		public void Load(Assembly assembly)
		{
			this.events = new Dictionary<Opcode, Func<byte[], byte[]>>();
			this.eventsAsync = new Dictionary<Opcode, Func<byte[], Task<byte[]>>>();

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
				
				object obj = Activator.CreateInstance(t);

				IRegister iRegister = obj as IRegister;
				if (iRegister != null)
				{
					iRegister.Register();
				}

				throw new Exception(
					string.Format("message handler not inherit IRegister interface: {0}", 
						obj.GetType().FullName));
			}
		}

		public void Register<T, R>(Func<T, R> func)
		{
			Opcode opcode = (Opcode) Enum.Parse(typeof (Opcode), typeof (T).Name);
			events.Add(opcode, messageBytes =>
			{
				T t = MongoHelper.FromBson<T>(messageBytes, 6);
				R k = func(t);
				return MongoHelper.ToBson(k);
			});
		}

		public void RegisterAsync<T, R>(Func<T, Task<R>> func)
		{
			Opcode opcode = (Opcode)Enum.Parse(typeof(Opcode), typeof(T).Name);
			eventsAsync.Add(opcode, async messageBytes =>
			{
				T t = MongoHelper.FromBson<T>(messageBytes, 6);
				R r = await func(t);
				return MongoHelper.ToBson(r);
			});
		}

		public async Task<byte[]> RunAsync(Opcode opcode, byte[] messageBytes)
		{
			Func<byte[], byte[]> func = null;
			if (this.events.TryGetValue(opcode, out func))
			{
				return func(messageBytes);
			}

			Func<byte[], Task<byte[]>> funcAsync = null;
			if (this.eventsAsync.TryGetValue(opcode, out funcAsync))
			{
				return await funcAsync(messageBytes);
			}
			throw new GameException(string.Format("not found opcode handler: {0}", opcode));
		}
	}
}