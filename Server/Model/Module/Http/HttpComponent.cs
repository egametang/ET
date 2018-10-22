using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

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

	[ObjectSystem]
	public class HttpComponentComponentStartSystem : StartSystem<HttpComponent>
	{
		public override void Start(HttpComponent self)
		{
			self.Start();
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

		// 处理方法
		private Dictionary<MethodInfo, IHttpHandler> handlersMapping;

		// Get处理
		private Dictionary<string, MethodInfo> getHandlers;
		private Dictionary<string, MethodInfo> postHandlers;

		public void Awake()
		{
			StartConfig startConfig = StartConfigComponent.Instance.StartConfig;
			this.appType = startConfig.AppType;
			this.HttpConfig = startConfig.GetComponent<HttpConfig>();

			this.Load();
		}

		public void Load()
		{
			this.dispatcher = new Dictionary<string, IHttpHandler>();
			this.handlersMapping = new Dictionary<MethodInfo, IHttpHandler>();
			this.getHandlers = new Dictionary<string, MethodInfo>();
			this.postHandlers = new Dictionary<string, MethodInfo>();

			List<Type> types = Game.EventSystem.GetTypes(typeof(HttpHandlerAttribute));

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

				LoadMethod(type, httpHandlerAttribute, ihttpHandler);
			}
		}

		public void Start()
		{
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

				this.Accept().NoAwait();
			}
			catch (HttpListenerException e)
			{
				throw new Exception($"http server error: {e.ErrorCode}", e);
			}
		}

		public void LoadMethod(Type type, HttpHandlerAttribute httpHandlerAttribute, IHttpHandler httpHandler)
		{
			// 扫描这个类里面的方法
			MethodInfo[] methodInfos = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod | BindingFlags.Instance);
			foreach (MethodInfo method in methodInfos)
			{
				object[] getAttrs = method.GetCustomAttributes(typeof(GetAttribute), false);
				if (getAttrs.Length != 0)
				{
					GetAttribute get = (GetAttribute)getAttrs[0];

					string path = method.Name;
					if (!string.IsNullOrEmpty(get.Path))
					{
						path = get.Path;
					}

					getHandlers.Add(httpHandlerAttribute.Path + path, method);
					//Log.Debug($"add handler[{httpHandler}.{method.Name}] path {httpHandlerAttribute.Path + path}");
				}

				object[] postAttrs = method.GetCustomAttributes(typeof(PostAttribute), false);
				if (postAttrs.Length != 0)
				{
					// Post处理方法
					PostAttribute post = (PostAttribute)postAttrs[0];

					string path = method.Name;
					if (!string.IsNullOrEmpty(post.Path))
					{
						path = post.Path;
					}

					postHandlers.Add(httpHandlerAttribute.Path + path, method);
					//Log.Debug($"add handler[{httpHandler}.{method.Name}] path {httpHandlerAttribute.Path + path}");
				}

				if (getAttrs.Length == 0 && postAttrs.Length == 0)
				{
					continue;
				}

				handlersMapping.Add(method, httpHandler);
			}
		}

		public async ETVoid Accept()
		{
			long instanceId = this.InstanceId;
			
			while (true)
			{
				if (this.InstanceId != instanceId)
				{
					return;
				}

				HttpListenerContext context = await this.listener.GetContextAsync();
				await InvokeHandler(context);
				context.Response.Close();
			}
		}

		/// <summary>
		/// 调用处理方法
		/// </summary>
		/// <param name="context"></param>
		private async ETTask InvokeHandler(HttpListenerContext context)
		{
			context.Response.StatusCode = 404;

			MethodInfo methodInfo = null;
			IHttpHandler httpHandler = null;
			string postbody = "";
			switch (context.Request.HttpMethod)
			{
				case "GET":
					this.getHandlers.TryGetValue(context.Request.Url.AbsolutePath, out methodInfo);
					if (methodInfo != null)
					{
						this.handlersMapping.TryGetValue(methodInfo, out httpHandler);
					}
					break;
				case "POST":
					this.postHandlers.TryGetValue(context.Request.Url.AbsolutePath, out methodInfo);
					if (methodInfo != null)
					{
						this.handlersMapping.TryGetValue(methodInfo, out httpHandler);

						using (StreamReader sr = new StreamReader(context.Request.InputStream))
						{
							postbody = sr.ReadToEnd();
						}
					}
					break;
				default:
					context.Response.StatusCode = 405;
					break;
			}

			if (httpHandler != null)
			{
				object[] args = InjectParameters(context, methodInfo, postbody);

				// 自动把返回值，以json方式响应。
				object resp = methodInfo.Invoke(httpHandler, args);
				object result = resp;
				if (resp is ETTask t)
				{
					await t;
					result = t.GetType().GetProperty("Result").GetValue(t, null);
				}

				if (result != null)
				{
					using (StreamWriter sw = new StreamWriter(context.Response.OutputStream))
					{
						if (result.GetType() == typeof(string))
						{
							sw.Write(result.ToString());
						}
						else
						{
							sw.Write(JsonHelper.ToJson(result));
						}
					}
				}
			}
		}

		/// <summary>
		/// 注入参数
		/// </summary>
		/// <param name="context"></param>
		/// <param name="methodInfo"></param>
		/// <param name="postbody"></param>
		/// <returns></returns>
		private static object[] InjectParameters(HttpListenerContext context, MethodInfo methodInfo, string postbody)
		{
			context.Response.StatusCode = 200;
			ParameterInfo[] parameterInfos = methodInfo.GetParameters();
			object[] args = new object[parameterInfos.Length];
			for (int i = 0; i < parameterInfos.Length; i++)
			{
				ParameterInfo item = parameterInfos[i];

				if (item.ParameterType == typeof(HttpListenerRequest))
				{
					args[i] = context.Request;
					continue;
				}

				if (item.ParameterType == typeof(HttpListenerResponse))
				{
					args[i] = context.Response;
					continue;
				}

				try
				{
					switch (context.Request.HttpMethod)
					{
						case "POST":
							if (item.Name == "postBody") // 约定参数名称为postBody,只传string类型。本来是byte[]，有需求可以改。
							{
								args[i] = postbody;
							}
							else if (item.ParameterType.IsClass && item.ParameterType != typeof(string) && !string.IsNullOrEmpty(postbody))
							{
								object entity = JsonHelper.FromJson(item.ParameterType, postbody);
								args[i] = entity;
							}

							break;
						case "GET":
							string query = context.Request.QueryString[item.Name];
							if (query != null)
							{
								object value = Convert.ChangeType(query, item.ParameterType);
								args[i] = value;
							}

							break;
						default:
							args[i] = null;
							break;
					}
				}
				catch (Exception e)
				{
					Log.Error(e);
					args[i] = null;
				}
			}

			return args;
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