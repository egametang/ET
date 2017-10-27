using System;
using System.Collections.Generic;
using System.Reflection;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Intepreter;

namespace Model
{
	/// <summary>
	/// 用来抹平ILRuntime跟mono层差异
	/// </summary>
	public interface IMessageMethod
	{
		void Run(AMessage a);
	}

	public class IMessageMonoMethod : IMessageMethod
	{
		private readonly IMHandler iMHandler;

		public IMessageMonoMethod(IMHandler iMHandler)
		{
			this.iMHandler = iMHandler;
		}

		public void Run(AMessage a)
		{
			this.iMHandler.Handle(a);
		}
	}

	public class IMessageILMethod : IMessageMethod
	{
		private readonly ILRuntime.Runtime.Enviorment.AppDomain appDomain;
		private readonly ILTypeInstance instance;
		private readonly IMethod method;
		private readonly object[] param;

		public IMessageILMethod(Type type, string methodName)
		{
			appDomain = Init.Instance.AppDomain;
			this.instance = this.appDomain.Instantiate(type.FullName);
			this.method = this.instance.Type.GetMethod(methodName);
			int n = this.method.ParameterCount;
			this.param = new object[n];
		}

		public void Run(AMessage a)
		{
			this.param[0] = a;
			this.appDomain.Invoke(this.method, this.instance, param);
		}
	}


	[ObjectEvent]
	public class MessageDispatherComponentEvent : ObjectEvent<MessageDispatherComponent>, IAwake, ILoad
	{
		public void Awake()
		{
			this.Get().Awake();
		}

		public void Load()
		{
			this.Get().Load();
		}
	}

	public static class Opcode2Name
	{
		private static Dictionary<int, string> _init = new Dictionary<int, string>();
		public static string GetName(int code)
		{
			if (_init.Count == 0)
			{
				Type type = typeof(Opcode);
				FieldInfo[] fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
				foreach (FieldInfo field in fields)
				{
					if (!field.IsStatic)
					{
						continue;
					}
					int codeID = (ushort)field.GetValue(null);
					if (_init.ContainsKey(codeID))
					{
						Log.Warning($"重复的Opcode:{codeID}");
						continue;
					}
					_init.Add(codeID, field.Name);
				}
			}
			return _init[code];
		}
	}

	/// <summary>
	/// 消息分发组件
	/// </summary>
	public class MessageDispatherComponent : Component
	{
		private Dictionary<ushort, List<IMessageMethod>> handlers;


		public void Awake()
		{
			this.Load();
		}

		public void Load()
		{
			handlers = new Dictionary<ushort, List<IMessageMethod>>();

			Type[] types = DllHelper.GetMonoTypes();

			foreach (Type type in types)
			{
				object[] attrs = type.GetCustomAttributes(typeof(MessageHandlerAttribute), false);
				if (attrs.Length == 0)
				{
					continue;
				}
				MessageHandlerAttribute messageHandlerAttribute = (MessageHandlerAttribute)attrs[0];
				IMHandler iMHandler = (IMHandler)Activator.CreateInstance(type);
				if (!this.handlers.ContainsKey(messageHandlerAttribute.Opcode))
				{
					this.handlers.Add(messageHandlerAttribute.Opcode, new List<IMessageMethod>());
				}
				this.handlers[messageHandlerAttribute.Opcode].Add(new IMessageMonoMethod(iMHandler));
			}

			// hotfix dll
			Type[] hotfixTypes = DllHelper.GetHotfixTypes();
			foreach (Type type in hotfixTypes)
			{
				object[] attrs = type.GetCustomAttributes(typeof(MessageHandlerAttribute), false);
				if (attrs.Length == 0)
				{
					continue;
				}
				MessageHandlerAttribute messageHandlerAttribute = (MessageHandlerAttribute)attrs[0];
#if ILRuntime
				IMessageMethod iMessageMethod = new IMessageILMethod(type, "Run");
#else
				IMHandler iMHandler = (IMHandler)Activator.CreateInstance(type);
				IMessageMethod iMessageMethod = new IMessageMonoMethod(iMHandler);
#endif
				if (!this.handlers.ContainsKey(messageHandlerAttribute.Opcode))
				{
					this.handlers.Add(messageHandlerAttribute.Opcode, new List<IMessageMethod>());
				}
				this.handlers[messageHandlerAttribute.Opcode].Add(iMessageMethod);
			}
		}

		public void Handle(MessageInfo messageInfo)
		{
			List<IMessageMethod> actions;
			if (!this.handlers.TryGetValue(messageInfo.Opcode, out actions))
			{
				Log.Error($"消息 {Opcode2Name.GetName(messageInfo.Opcode)}({messageInfo.Opcode}) 没有处理");
				return;
			}

			foreach (IMessageMethod ev in actions)
			{
				try
				{
					ev.Run(messageInfo.Message);
				}
				catch (Exception e)
				{
					Log.Error(e.ToString());
				}
			}
		}

		public override void Dispose()
		{
			if (Id == 0)
			{
				return;
			}

			base.Dispose();
		}
	}
}