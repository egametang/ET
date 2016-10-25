using System;
using System.Collections.Generic;
using System.Reflection;
using Base;
using Object = Base.Object;

namespace Model
{
	[ObjectEvent]
	public class MessageHandlerComponentEvent : ObjectEvent<MessageDispatherComponent>, ILoader, IAwake<AppType>
	{
		public void Load()
		{
			this.GetValue().Load();
		}

		public void Awake(AppType appType)
		{
			this.GetValue().Awake(appType);
		}
	}

	
	/// <summary>
	/// 消息分发组件
	/// </summary>
	public class MessageDispatherComponent: Component, IMessageDispather
	{
		private class MessageInfo
		{
			public byte[] MessageBytes;
			public int Offset;
			public int Count;
			public uint RpcId;
		}

		private AppType AppType;
		private Dictionary<ushort, List<Action<Session, MessageInfo>>> handlers;
		private Dictionary<ushort, Action<Session, MessageInfo>> rpcHandlers;
		private Dictionary<Type, MessageAttribute> messageOpcode { get; set; } = new Dictionary<Type, MessageAttribute>();
		
		public void Awake(AppType appType)
		{
			this.AppType = appType;
			this.Load();
		}

		public void Load()
		{
			this.handlers = new Dictionary<ushort, List<Action<Session, MessageInfo>>>();
			this.rpcHandlers = new Dictionary<ushort, Action<Session, MessageInfo>>();
			this.messageOpcode = new Dictionary<Type, MessageAttribute>();

			Assembly[] assemblies = Object.ObjectManager.GetAssemblies();

			foreach (Assembly assembly in assemblies)
			{
				Type[] types = assembly.GetTypes();
				foreach (Type type in types)
				{
					object[] attrs = type.GetCustomAttributes(typeof(MessageAttribute), false);
					if (attrs.Length == 0)
					{
						continue;
					}

					MessageAttribute messageAttribute = (MessageAttribute)attrs[0];
					this.messageOpcode[type] = messageAttribute;
				}
			}

			foreach (Assembly assembly in assemblies)
			{
				Type[] types = assembly.GetTypes();
				foreach (Type type in types)
				{
					object[] attrs = type.GetCustomAttributes(typeof(MessageHandlerAttribute), false);
					if (attrs.Length == 0)
					{
						continue;
					}

					MessageHandlerAttribute messageHandlerAttribute = (MessageHandlerAttribute)attrs[0];
					if (!messageHandlerAttribute.Contains(this.AppType))
					{
						continue;
					}

					object obj = Activator.CreateInstance(type);

					IMRegister iMRegister = obj as IMRegister;
					if (iMRegister == null)
					{
						throw new Exception($"message handler not inherit AMEvent or AMRpcEvent abstract class: {obj.GetType().FullName}");
					}
					iMRegister.Register(this);
				}
			}
		}

		public ushort GetOpcode(Type type)
		{
			MessageAttribute messageAttribute;
			if (!this.messageOpcode.TryGetValue(type, out messageAttribute))
			{
				throw new Exception($"查找Opcode失败: {type.Name}");
			}
			return messageAttribute.Opcode;
		}

		public void RegisterHandler<Message>(ushort opcode, Action<Session, Message> action) where Message: AMessage
		{
			if (!this.handlers.ContainsKey(opcode))
			{
				this.handlers.Add(opcode, new List<Action<Session, MessageInfo>>());
			}
			List<Action<Session, MessageInfo>> actions = this.handlers[opcode];

			actions.Add((session, messageInfo) =>
			{
				Message message;
				try
				{
                    message = MongoHelper.FromBson<Message>(messageInfo.MessageBytes, messageInfo.Offset, messageInfo.Count);
					Log.Info(MongoHelper.ToJson(message));
                }
			    catch (Exception ex)
			    {
			        throw new Exception("解释消息失败:" + opcode, ex);
			    }

				action(session, message);
			});
		}
		
		public void RegisterRpcHandler<Request, Response>(ushort opcode, Action<Session, Request, Action<Response>> action) 
			where Request: ARequest 
			where Response: AResponse
		{
			if (this.rpcHandlers.ContainsKey(opcode))
			{
				Log.Error($"rpc消息不能注册两次! opcode: {opcode}");
				return;
			}
			this.rpcHandlers.Add(opcode, (session, messageInfo) =>
			{
				Request request;
				try
				{
					request = MongoHelper.FromBson<Request>(messageInfo.MessageBytes, messageInfo.Offset, messageInfo.Count);
					Log.Info(MongoHelper.ToJson(request));
				}
				catch (Exception ex)
				{
					throw new Exception("解释消息失败:" + opcode, ex);
				}

				action(session, request, response =>
					{
						session.Reply(messageInfo.RpcId, response); 
					} 
				);
			});
		}


		public void Handle(Session session, ushort opcode, byte[] messageBytes, int offset)
		{
			List<Action<Session, MessageInfo>> actions;
			if (!this.handlers.TryGetValue(opcode, out actions))
			{
				Log.Error($"消息 {opcode} 没有处理");
				return;
			}

			foreach (var ev in actions)
			{
				try
				{
					ev(session, new MessageInfo { MessageBytes = messageBytes, Offset = offset, Count = messageBytes.Length - offset });
				}
				catch (Exception e)
				{
					Log.Error(e.ToString());
				}
			}
		}

		public void HandleRpc(Session session, ushort opcode, byte[] messageBytes, int offset, uint rpcId)
		{
			Action<Session, MessageInfo> action;
			if (!this.rpcHandlers.TryGetValue(opcode, out action))
			{
				Log.Error($"Rpc消息 {opcode} 没有处理");
				return;
			}

			try
			{
				action(session, new MessageInfo { MessageBytes = messageBytes, Offset = offset, Count = messageBytes.Length - offset, RpcId = rpcId });
			}
			catch (Exception e)
			{
				Log.Error(e.ToString());
			}
		}


		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}

			base.Dispose();
		}
	}
}