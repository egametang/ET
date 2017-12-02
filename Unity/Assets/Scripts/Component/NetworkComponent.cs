﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Model
{
	public abstract class NetworkComponent : Component
	{
		private AService Service;

		public AppType AppType;

		private readonly Dictionary<long, Session> sessions = new Dictionary<long, Session>();

		public IMessagePacker MessagePacker { get; set; }

		public IMessageDispatcher MessageDispatcher { get; set; }

		public void Awake(NetworkProtocol protocol)
		{
			switch (protocol)
			{
				case NetworkProtocol.TCP:
					this.Service = new TService();
					break;
				case NetworkProtocol.UDP:
					this.Service = new UService();
					break;
				case NetworkProtocol.KCP:
					this.Service = new KService();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public void Awake(NetworkProtocol protocol, IPEndPoint ipEndPoint)
		{
			try
			{
				switch (protocol)
				{
					case NetworkProtocol.TCP:
						this.Service = new TService(ipEndPoint);
						break;
					case NetworkProtocol.UDP:
						this.Service = new UService(ipEndPoint);
						break;
					case NetworkProtocol.KCP:
						this.Service = new KService(ipEndPoint);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				this.StartAccept();
			}
			catch (Exception e)
			{
				throw new Exception($"{ipEndPoint}", e);
			}
		}

		private async void StartAccept()
		{
			while (true)
			{
				if (this.Id == 0)
				{
					return;
				}

				await this.Accept();
			}
		}

		public virtual async Task<Session> Accept()
		{
			AChannel channel = await this.Service.AcceptChannel();
			Session session = new Session(this, channel);
			channel.ErrorCallback += (c, e) => { this.Remove(session.Id); };
			this.sessions.Add(session.Id, session);
			return session;
		}

		public virtual void Remove(long id)
		{
			Session session;
			if (!this.sessions.TryGetValue(id, out session))
			{
				return;
			}
			this.sessions.Remove(id);
			session.Dispose();
		}

		public Session Get(long id)
		{
			Session session;
			this.sessions.TryGetValue(id, out session);
			return session;
		}

		/// <summary>
		/// 创建一个新Session
		/// </summary>
		public virtual Session Create(IPEndPoint ipEndPoint)
		{
			try
			{
				AChannel channel = this.Service.ConnectChannel(ipEndPoint);
				Session session = new Session(this, channel);
				channel.ErrorCallback += (c, e) => { this.Remove(session.Id); };
				this.sessions.Add(session.Id, session);
				return session;
			}
			catch (Exception e)
			{
				Log.Error(e.ToString());
				return null;
			}
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
			if (this.Id == 0)
			{
				return;
			}

			base.Dispose();

			foreach (Session session in this.sessions.Values.ToArray())
			{
				session.Dispose();
			}

			this.Service.Dispose();
		}
	}
}