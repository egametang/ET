using System;
using System.Collections.Generic;
using System.Net;

namespace ET
{
	public abstract class NetworkComponent : Entity
	{
		protected AService Service;

		public readonly Dictionary<long, Session> Sessions = new Dictionary<long, Session>();

		public IMessagePacker MessagePacker { get; set; }

		public IMessageDispatcher MessageDispatcher { get; set; }

		public void Awake(NetworkProtocol protocol)
		{
			switch (protocol)
			{
				case NetworkProtocol.KCP:
					this.Service = new KService() { Parent = this };
					break;
				case NetworkProtocol.TCP:
					this.Service = new TService() { Parent = this };
					break;
				case NetworkProtocol.WebSocket:
					this.Service = new WService() { Parent = this };
					break;
			}
		}

		public void Awake(NetworkProtocol protocol, string address)
		{
			try
			{
				IPEndPoint ipEndPoint;
				switch (protocol)
				{
					case NetworkProtocol.KCP:
						ipEndPoint = NetworkHelper.ToIPEndPoint(address);
						this.Service = new KService(ipEndPoint, (channel)=> { this.OnAccept(channel); }) { Parent = this };
						break;
					case NetworkProtocol.TCP:
						ipEndPoint = NetworkHelper.ToIPEndPoint(address);
						this.Service = new TService(ipEndPoint, (channel)=> { this.OnAccept(channel); }) { Parent = this };
						break;
					case NetworkProtocol.WebSocket:
						string[] prefixs = address.Split(';');
						this.Service = new WService(prefixs, (channel)=> { this.OnAccept(channel); }) { Parent = this };
						break;
				}
			}
			catch (Exception e)
			{
				throw new Exception($"NetworkComponent Awake Error {address}", e);
			}
		}

		public int Count
		{
			get { return this.Sessions.Count; }
		}

		public virtual Session OnAccept(AChannel channel)
		{
			Session session = EntityFactory.CreateWithParent<Session, AChannel>(this, channel);
			this.Sessions.Add(session.Id, session);
			channel.Start();
			return session;
		}

		public virtual void Remove(long id)
		{
			Session session;
			if (!this.Sessions.TryGetValue(id, out session))
			{
				return;
			}
			this.Sessions.Remove(id);
			session.Dispose();
		}

		public Session Get(long id)
		{
			Session session;
			this.Sessions.TryGetValue(id, out session);
			return session;
		}

		/// <summary>
		/// 创建一个新Session
		/// </summary>
		public Session Create(IPEndPoint ipEndPoint)
		{
			AChannel channel = this.Service.ConnectChannel(ipEndPoint);
			Session session = EntityFactory.CreateWithParent<Session, AChannel>(this, channel);
			this.Sessions.Add(session.Id, session);
			channel.Start();
			return session;
		}
		
		/// <summary>
		/// 创建一个新Session
		/// </summary>
		public Session Create(string address)
		{
			AChannel channel = this.Service.ConnectChannel(address);
			Session session = EntityFactory.CreateWithParent<Session, AChannel>(this, channel);
			this.Sessions.Add(session.Id, session);
			channel.Start();
			return session;
		}

		public void Update()
		{
			if (this.Service == null)
			{
				return;
			}
			this.Service.Update();
		}

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			base.Dispose();

			this.Service.Dispose();
		}
	}
}