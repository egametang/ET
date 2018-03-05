using System;
using System.Collections.Generic;
using System.Net;

namespace ETModel
{
	[ObjectSystem]
	public class HttpComponentComponentAwakeSystem : AwakeSystem<HttpComponent>
	{
		public override void Awake(HttpComponent self)
		{
			self.Awake();
		}
	}

	[ObjectSystem]
	public class HttpComponentComponentLoadSystem : LoadSystem<HttpComponent>
	{
		public override void Load(HttpComponent self)
		{
			self.Load();
		}
	}

	/// <summary>
	/// http请求分发器
	/// </summary>
	public class HttpComponent : Component
	{
		public AppType appType;
		public HttpListener listener;
		public HttpConfig HttpConfig;
		public Dictionary<string, IHttpHandler> dispatcher;

		public void Awake()
		{
			StartConfig startConfig = Game.Scene.GetComponent<StartConfigComponent>().StartConfig;
			this.appType = startConfig.AppType;
			this.HttpConfig = startConfig.GetComponent<HttpConfig>();

			this.Load();

			try
			{
				this.listener = new HttpListener();

				if (this.HttpConfig.Url == null)
				{
					this.HttpConfig.Url = "";
				}
				foreach (string s in this.HttpConfig.Url.Split(';'))
				{
					if (s.Trim() == "")
					{
						continue;
					}
					this.listener.Prefixes.Add(s);
				}

				this.listener.Start();

				this.Accept();
			}
			catch (HttpListenerException e)
			{
				throw new Exception($"http server error: {e.ErrorCode}", e);
			}
		}

		public void Load()
		{
			this.dispatcher = new Dictionary<string, IHttpHandler>();

			Type[] types = DllHelper.GetMonoTypes();

			foreach (Type type in types)
			{
				object[] attrs = type.GetCustomAttributes(typeof(HttpHandlerAttribute), false);
				if (attrs.Length == 0)
				{
					continue;
				}

				HttpHandlerAttribute httpHandlerAttribute = (HttpHandlerAttribute)attrs[0];
				if (!httpHandlerAttribute.AppType.Is(this.appType))
				{
					continue;
				}

				object obj = Activator.CreateInstance(type);

				IHttpHandler ihttpHandler = obj as IHttpHandler;
				if (ihttpHandler == null)
				{
					throw new Exception($"HttpHandler handler not inherit IHttpHandler class: {obj.GetType().FullName}");
				}
				this.dispatcher.Add(httpHandlerAttribute.Path, ihttpHandler);
			}
		}

		public async void Accept()
		{
			while (true)
			{
				if (this.IsDisposed)
				{
					return;
				}

				HttpListenerContext context = await this.listener.GetContextAsync();

				IHttpHandler handler;
				if (this.dispatcher.TryGetValue(context.Request.Url.AbsolutePath, out handler))
				{
					handler.Handle(context);
				}

				//Log.Debug($"{context.Request.Url.AbsolutePath} {context.Request.Url.AbsoluteUri} {context.Request.Url.LocalPath} {context.Request.Url.IdnHost} " +
				//          $"{context.Request.Url.Host} {context.Request.Url.}");
			}
		}

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			base.Dispose();

			this.listener.Stop();
			this.listener.Close();
		}
	}
}