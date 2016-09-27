using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Base
{
	public enum NetChannelType
	{
		Login,
		Gate,
		Battle,
	}

	[ObjectEvent]
	public class MessageComponentEvent : ObjectEvent<MessageComponent>, ILoader, IAwake, IUpdate
	{
		public void Load()
		{
			MessageComponent component = this.GetValue();
			component.Load();
		}

		public void Awake()
		{
			this.GetValue().Awake();
		}

		public void Update()
		{
			this.GetValue().Update();
		}
	}
	
	/// <summary>
	/// 消息分发组件
	/// </summary>
	public class MessageComponent: Component<Scene>
	{
		private uint RpcId { get; set; }
		private Dictionary<Opcode, List<Action<byte[], int, int>>> events;
		private readonly Dictionary<uint, Action<byte[], int, int>> requestCallback = new Dictionary<uint, Action<byte[], int, int>>();
		private readonly Dictionary<Opcode, Action<byte[], int, int>> waitCallback = new Dictionary<Opcode, Action<byte[], int, int>>();
		private readonly Dictionary<NetChannelType, AChannel> channels = new Dictionary<NetChannelType, AChannel>();
		
		public void Awake()
		{
			this.Load();
		}

		public void Load()
		{
			this.events = new Dictionary<Opcode, List<Action<byte[], int, int>>>();

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
					if (messageAttribute.SceneType != this.Owner.SceneType)
					{
						continue;
					}

					object obj = Activator.CreateInstance(type);

					IMRegister<MessageComponent> iMRegister = obj as IMRegister<MessageComponent>;
					if (iMRegister == null)
					{
						throw new GameException($"message handler not inherit IEventSync or IEventAsync interface: {obj.GetType().FullName}");
					}
					iMRegister.Register(this);
				}
			}
		}

		public void Register<T>(Action<Scene, T> action)
		{
			Opcode opcode = EnumHelper.FromString<Opcode>(typeof (T).Name);
			if (!this.events.ContainsKey(opcode))
			{
				this.events.Add(opcode, new List<Action<byte[], int, int>>());
			}
			List<Action<byte[], int, int>> actions = this.events[opcode];
			actions.Add((messageBytes, offset, count) =>
			{
				T t;
				try
			    {
                    t = MongoHelper.FromBson<T>(messageBytes, offset, count);
                }
			    catch (Exception ex)
			    {
			        throw new GameException("解释消息失败:" + opcode, ex);
			    }

				if (OpcodeHelper.IsNeedDebugLogMessage(opcode))
				{
					Log.Debug(MongoHelper.ToJson(t));
				}

				action(this.Owner, t);
			});
		}

		public void Connect(NetChannelType channelType, string host, int port)
		{
			AChannel channel = Share.Scene.GetComponent<NetworkComponent>().ConnectChannel(host, port);
			this.channels[channelType] = channel;
		}

		public void Close(NetChannelType channelType)
		{
			AChannel channel = this.GetChannel(channelType);
			if (channel == null || channel.IsDisposed())
			{
				return;
			}
			this.channels.Remove(channelType);
			channel.Dispose();
		}

		public void Update()
		{
			foreach (AChannel channel in this.channels.Values.ToArray())
			{
				this.UpdateChannel(channel);
			}
		}

		private void UpdateChannel(AChannel channel)
		{
			if (channel.IsDisposed())
			{
				return;
			}

			while (true)
			{
				byte[] messageBytes = channel.Recv();
				if (messageBytes == null)
				{
					return;
				}

				if (messageBytes.Length < 6)
				{
					continue;
				}

				Opcode opcode = (Opcode)BitConverter.ToUInt16(messageBytes, 0);
				try
				{
					this.Run(opcode, messageBytes);
				}
				catch (Exception e)
				{
					Log.Error(e.ToString());
				}
			}
		}

		public void Run(Opcode opcode, byte[] messageBytes)
		{
			int offset = 0;
			uint flagUInt = BitConverter.ToUInt32(messageBytes, 2);
			bool isCompressed = (byte)(flagUInt >> 24) == 1;
			if (isCompressed) // 表示有压缩,需要解压缩
			{
				messageBytes = ZipHelper.Decompress(messageBytes, 6, messageBytes.Length - 6);
				offset = 0;
			}
			else
			{
				offset = 6;
			}
			uint rpcId = flagUInt & 0x0fff;
			this.RunDecompressedBytes(opcode, rpcId, messageBytes, offset);
		}

		public void RunDecompressedBytes(Opcode opcode, uint rpcId, byte[] messageBytes, int offset)
		{
			Action<byte[], int, int> action;
			if (this.requestCallback.TryGetValue(rpcId, out action))
			{
				this.requestCallback.Remove(rpcId);
				action(messageBytes, offset, messageBytes.Length - offset);
				return;
			}

			if (this.waitCallback.TryGetValue(opcode, out action))
			{
				this.waitCallback.Remove(opcode);
				action(messageBytes, offset, messageBytes.Length - offset);
				return;
			}

			List<Action<byte[], int, int>> actions;
			if (!this.events.TryGetValue(opcode, out actions))
			{
				if (this.Owner.SceneType == SceneType.Game)
				{
					Log.Error($"消息{opcode}没有处理");
				}
				return;
			}

			foreach (var ev in actions)
			{
				try
				{
					ev(messageBytes, offset, messageBytes.Length - offset);
				}
				catch (Exception e)
				{
					Log.Error(e.ToString());
				}
			}
		}

		/// <summary>
		/// Rpc调用,发送一个消息,等待返回一个消息
		/// </summary>
		/// <typeparam name="Response"></typeparam>
		/// <param name="request"></param>
		/// <returns></returns>
		public Task<Response> CallAsync<Response>(object request) where Response : IErrorMessage
		{
			this.Send(request, ++this.RpcId);

			var tcs = new TaskCompletionSource<Response>();
			this.requestCallback[this.RpcId] = (bytes, offset, count) =>
			{
				try
				{
					Response response = MongoHelper.FromBson<Response>(bytes, offset, count);
					Opcode opcode = EnumHelper.FromString<Opcode>(response.GetType().Name);
					if (OpcodeHelper.IsNeedDebugLogMessage(opcode))
					{
						Log.Debug(MongoHelper.ToJson(response));
					}
					if (response.ErrorMessage.errno != (int) ErrorCode.ERR_Success)
					{
						tcs.SetException(new RpcException((ErrorCode)response.ErrorMessage.errno,  response.ErrorMessage.msg.Utf8ToStr()));
						return;
					}
					tcs.SetResult(response);
				}
				catch (Exception e)
				{
					tcs.SetException(new GameException($"Rpc Error: {typeof(Response).FullName}", e));
				}
			};
			
			return tcs.Task;
		}

		/// <summary>
		/// 不发送消息,直接等待返回一个消息
		/// </summary>
		/// <typeparam name="Response"></typeparam>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task<Response> WaitAsync<Response>(CancellationToken cancellationToken) where Response : class
		{
			var tcs = new TaskCompletionSource<Response>();
			Opcode opcode = EnumHelper.FromString<Opcode>(typeof(Response).Name);
			this.waitCallback[opcode] = (bytes, offset, count) =>
			{
				try
				{
					Response response = MongoHelper.FromBson<Response>(bytes, offset, count);
					Opcode op = EnumHelper.FromString<Opcode>(response.GetType().Name);
					if (OpcodeHelper.IsNeedDebugLogMessage(op))
					{
						Log.Debug(MongoHelper.ToJson(response));
					}
					tcs.SetResult(response);
				}
				catch (Exception e)
				{
					tcs.SetException(new GameException($"Wait Error: {typeof(Response).FullName}", e));
				}
			};

			return tcs.Task;
		}

		/// <summary>
		/// 不发送消息,直接等待返回一个消息
		/// </summary>
		/// <typeparam name="Response"></typeparam>
		/// <returns></returns>
		public Task<Response> WaitAsync<Response>() where Response : class
		{
			var tcs = new TaskCompletionSource<Response>();
			Opcode opcode = EnumHelper.FromString<Opcode>(typeof(Response).Name);
			this.waitCallback[opcode] = (bytes, offset, count) =>
			{
				try
				{
					Response response = MongoHelper.FromBson<Response>(bytes, offset, count);
					Opcode op = EnumHelper.FromString<Opcode>(response.GetType().Name);
					if (OpcodeHelper.IsNeedDebugLogMessage(op))
					{
						Log.Debug(MongoHelper.ToJson(response));
					}
					tcs.SetResult(response);
				}
				catch (Exception e)
				{
					tcs.SetException(new GameException($"Wait Error: {typeof(Response).FullName}", e));
				}
			};

			return tcs.Task;
		}

		public AChannel GetChannel(NetChannelType channelType)
		{
			AChannel channel;
			this.channels.TryGetValue(channelType, out channel);
			return channel;
		}

		public void Send(object message)
		{
			this.Send(message, 0);
		}

		public bool IsChannelConnected(NetChannelType channelType)
		{
			AChannel channel = GetChannel(channelType);
			if (channel == null)
			{
				return false;
			}
			return true;
		}

		private void Send(object message, uint rpcId)
		{
			Opcode opcode = EnumHelper.FromString<Opcode>(message.GetType().Name);
			byte[] opcodeBytes = BitConverter.GetBytes((ushort)opcode);
			byte[] seqBytes = BitConverter.GetBytes(rpcId);
			byte[] messageBytes = MongoHelper.ToBson(message);

			NetChannelType channelType;
			if ((ushort)opcode > 7000 && (ushort)opcode < 8000)
			{
				channelType = NetChannelType.Login;
			}
			else if ((ushort)opcode > 0 && (ushort)opcode <= 1000)
			{
				channelType = NetChannelType.Battle;
			}
			else
			{
				channelType = NetChannelType.Gate;
			}

			AChannel channel = this.GetChannel(channelType);
			if (channel == null)
			{
				throw new GameException("game channel not found!");
			}

			channel.Send(new List<byte[]> { opcodeBytes, seqBytes, messageBytes });

			if (OpcodeHelper.IsNeedDebugLogMessage(opcode))
			{
				Log.Debug(MongoHelper.ToJson(message));
			}
		}

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}

			base.Dispose();

			foreach (AChannel channel in this.channels.Values.ToArray())
			{
				channel.Dispose();
			}
		}
	}
}