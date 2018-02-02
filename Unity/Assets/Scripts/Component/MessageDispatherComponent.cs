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
		void Run(Session session, AMessage a);
	}

	public class IMessageMonoMethod : IMessageMethod
	{
		private readonly IMHandler iMHandler;

		public IMessageMonoMethod(IMHandler iMHandler)
		{
			this.iMHandler = iMHandler;
		}

		public void Run(Session session, AMessage a)
		{
			this.iMHandler.Handle(session, a);
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
			this.method = this.instance.Type.GetMethod(methodName, 2);
			int n = this.method.ParameterCount;
			this.param = new object[n];
		}

		public void Run(Session session, AMessage a)
		{
			this.param[0] = a;
			this.appDomain.Invoke(this.method, this.instance, param);
		}
	}


	[ObjectSystem]
	public class MessageDispatherComponentSystem : ObjectSystem<MessageDispatherComponent>, IAwake, ILoad
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
				IMessageMethod iMessageMethod = new IMessageILMethod(type, "Handle");
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

		public void Handle(Session session, MessageInfo messageInfo)
		{
			List<IMessageMethod> actions;
			if (!this.handlers.TryGetValue(messageInfo.Opcode, out actions))
			{
				Log.Error($"消息 {messageInfo.Opcode} 没有处理");
				return;
			}

			foreach (IMessageMethod ev in actions)
			{
				try
				{
					ev.Run(session, messageInfo.Message);
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