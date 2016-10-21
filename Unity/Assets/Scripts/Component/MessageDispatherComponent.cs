using System;
using System.Collections.Generic;
using System.Reflection;
using Base;
using Object = Base.Object;

namespace Model
{
	[ObjectEvent]
	public class MessageHandlerComponentEvent : ObjectEvent<MessageDispatherComponent>, ILoader, IAwake<string>
	{
		public void Load()
		{
			this.GetValue().Load();
		}

		public void Awake(string appType)
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

		private string AppType;
		private Dictionary<ushort, List<Action<Entity, MessageInfo>>> handlers;
		private Dictionary<ushort, Action<Entity, MessageInfo>> rpcHandlers;
		private Dictionary<Type, MessageAttribute> messageOpcode { get; set; } = new Dictionary<Type, MessageAttribute>();
		
		public void Awake(string appType)
		{
			this.AppType = appType;
			this.Load();
		}

		public void Load()
		{
			this.handlers = new Dictionary<ushort, List<Action<Entity, MessageInfo>>>();
			this.rpcHandlers = new Dictionary<ushort, Action<Entity, MessageInfo>>();
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
					if (messageHandlerAttribute.AppType != this.AppType)
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
			return this.messageOpcode[type].Opcode;
		}

		public void RegisterHandler<Message>(ushort opcode, Action<Entity, Message> action) where Message: AMessage
		{
			if (!this.handlers.ContainsKey(opcode))
			{
				this.handlers.Add(opcode, new List<Action<Entity, MessageInfo>>());
			}
			List<Action<Entity, MessageInfo>> actions = this.handlers[opcode];

			actions.Add((entity, messageInfo) =>
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

				action(entity, message);
			});
		}
		
		public void RegisterRpcHandler<Request, Response>(ushort opcode, Action<Entity, Request, Action<Response>> action) 
			where Request: ARequest 
			where Response: AResponse
		{
			if (this.rpcHandlers.ContainsKey(opcode))
			{
				Log.Error($"rpc消息不能注册两次! opcode: {opcode}");
				return;
			}
			this.rpcHandlers.Add(opcode, (entity, messageInfo) =>
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

				action(entity, request, response =>
					{
						entity.GetComponent<MessageComponent>().Reply(messageInfo.RpcId, response); 
					} 
				);
			});
		}


		public void Handle(Entity entity, ushort opcode, byte[] messageBytes, int offset)
		{
			List<Action<Entity, MessageInfo>> actions;
			if (!this.handlers.TryGetValue(opcode, out actions))
			{
				Log.Error($"消息 {opcode} 没有处理");
				return;
			}

			foreach (var ev in actions)
			{
				try
				{
					ev(entity, new MessageInfo { MessageBytes = messageBytes, Offset = offset, Count = messageBytes.Length - offset });
				}
				catch (Exception e)
				{
					Log.Error(e.ToString());
				}
			}
		}

		public void HandleRpc(Entity entity, ushort opcode, byte[] messageBytes, int offset, uint rpcId)
		{
			Action<Entity, MessageInfo> action;
			if (!this.rpcHandlers.TryGetValue(opcode, out action))
			{
				Log.Error($"Rpc消息 {opcode} 没有处理");
				return;
			}

			try
			{
				action(entity, new MessageInfo { MessageBytes = messageBytes, Offset = offset, Count = messageBytes.Length - offset, RpcId = rpcId });
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