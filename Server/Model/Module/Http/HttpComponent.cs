using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace ETModel
{
    public delegate Task RequestDelegate(HttpListenerContext context);

    /// <summary>
    /// http请求分发器
    /// </summary>
    public class HttpComponent : Component
    {
        private enum ServiceState
        {
            None, // 长久,没请求都是同个对象
            Transient // 瞬时,每次请求都是一个新的.
        }

        private class ServiceInfo
        {
            public ServiceState ServiceState { get; set; } = ServiceState.Transient;

            public Type type;

            public object obj; // 用于保存长久的服务
        }

        public AppType appType;
        public HttpListener listener;
        public HttpConfig HttpConfig;

        // 中间件
        private List<Func<RequestDelegate, RequestDelegate>> _middlewares = new List<Func<RequestDelegate, RequestDelegate>>();

        // 服务,IOC容器.
        private Dictionary<string, ServiceInfo> _services = new Dictionary<string, ServiceInfo>();

        // 中间件调用入口
        private RequestDelegate _middlewareEntry = null;

        public void Awake()
        {
            StartConfig startConfig = Game.Scene.GetComponent<StartConfigComponent>().StartConfig;
            this.appType = startConfig.AppType;
            this.HttpConfig = startConfig.GetComponent<HttpConfig>();

            this.Load();
        }

        public void Load()
        {
            // 初始化中间件的调用链
            _middlewareEntry = context =>
            {
                return Task.CompletedTask;
            };

            _middlewares.Reverse();

            foreach (var func in _middlewares)
            {
                _middlewareEntry = func.Invoke(_middlewareEntry);
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

                this.Accept();
            }
            catch (HttpListenerException e)
            {
                if (e.ErrorCode == 5)
                {
                    throw new Exception($"CMD管理员中输入:netsh http add urlacl url=http://*:8080/ user=Everyone   |http server error: {e.ErrorCode}", e);
                }
                else
                {
                    throw new Exception($"http server error: {e.ErrorCode}", e);
                }
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
                await _middlewareEntry.Invoke(context); // 调用中间件
                context.Response.Close();
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
            this._middlewares.Clear();
            this._services.Clear();
            this._middlewareEntry = null;
        }

        /// <summary>
        /// 根据参数,获取服务数组.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public object[] GetServicesByParameters(ParameterInfo[] parameters)
        {
            object[] args = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfo info = parameters[i];

                if (info.ParameterType == typeof(HttpComponent)) // 注入HttpComponent
                {
                    args[i] = this;
                    continue;
                }

                if (_services.TryGetValue(info.ParameterType.Name, out ServiceInfo serviceInfo))
                {

                    if (serviceInfo.ServiceState == ServiceState.Transient)
                    {
                        args[i] = CreateInstance(serviceInfo.type);
                    }
                    else
                    {
                        args[i] = serviceInfo.obj;
                    }
                }
            }
            return args;
        }

        /// <summary>
        /// 创建实例并在构造函数中注入依赖对象
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public object CreateInstance(Type t)
        {
            var constructors = t.GetConstructors();
            if (constructors.Length != 1)
            {
                Log.Error($"{t.Name} 服务只能有1个构造函数.");
                return null;
            }
            else
            {
                object[] args = GetServicesByParameters(constructors[0].GetParameters());
                return Activator.CreateInstance(t, args);
            }
        }


        public void Run(Func<HttpListenerContext, RequestDelegate, Task> action)
        {
            _middlewares.Add((next) =>
            {
                return context =>
                {
                    Task task = action(context, next);
                    if (task == null)
                        task = Task.CompletedTask;
                    return task;
                };
            });
        }

        public void Run(string path, Func<HttpListenerContext, RequestDelegate, Task> action)
        {
            _middlewares.Add((next) =>
            {
                return context =>
                {
                    if (context.Request.Url.AbsolutePath == path)
                    {
                        Task task = action(context, next);
                        if (task == null)
                            task = Task.CompletedTask;
                        return task;
                    }
                    return next(context);
                };
            });
        }

        public void Use(Func<RequestDelegate, RequestDelegate> middleware)
        {
            _middlewares.Add(middleware);
        }

        public void Use<T>()
        {
            Type type = typeof(T);

            ConstructorInfo[] constructorInfos = type.GetConstructors();

            if (constructorInfos.Length != 1)
            {
                Log.Error($"{type.Name} 中间件只能有1个构造函数.");
                return;
            }

            MethodInfo methodInfo = type.GetMethod("Invoke");

            Func<RequestDelegate, RequestDelegate> func = new Func<RequestDelegate, RequestDelegate>(next =>
            {
                object obj = CreateInstance(type);
                return context =>
                {
                    return (Task)methodInfo.Invoke(obj, new object[] { context });
                };
            });
            _middlewares.Add(func);
        }

        public void AddService<T>()
        {
            Type type = typeof(T);
            if (!_services.ContainsKey(type.Name))
            {
                _services.Add(type.Name, new ServiceInfo
                {
                    ServiceState = ServiceState.None,
                    type = type,
                    obj = CreateInstance(type)
                });
            }
            else
            {
                Log.Error($"{type.Name} 此服务已经注册.");
            }
        }

        public void AddServiceTransient<T>()
        {
            Type type = typeof(T);
            if (!_services.ContainsKey(type.Name))
            {
                _services.Add(type.Name, new ServiceInfo
                {
                    ServiceState = ServiceState.Transient,
                    type = type,
                });
            }
            else
            {
                Log.Error($"{type.Name} 此服务已经注册.");
            }
        }
    }
}