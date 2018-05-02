using ETModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ETModel
{
    /// <summary>
    ///  注册Mvc中间件
    /// </summary>
    public static class MvcMiddlewareEx
    {
        public static void UseMvc(this HttpComponent self)
        {
            self.Use<MvcMiddleware>();
        }
    }

    /// <summary>
    /// Mvc中间件
    /// </summary>
    public class MvcMiddleware
    {
        private RequestDelegate _next;

        private Dictionary<string, IHttpHandler> _dispatcher;

        // 处理方法
        private Dictionary<MethodInfo, IHttpHandler> _handlersMapping = new Dictionary<MethodInfo, IHttpHandler>();

        // Get处理
        private Dictionary<string, MethodInfo> _getHandlers = new Dictionary<string, MethodInfo>();
        private Dictionary<string, MethodInfo> _postHandlers = new Dictionary<string, MethodInfo>();

        public MvcMiddleware(RequestDelegate next, HttpComponent httpComponent)
        {
            _next = next;

            _dispatcher = new Dictionary<string, IHttpHandler>();

            Type[] types = DllHelper.GetMonoTypes();

            foreach (Type type in types)
            {
                object[] attrs = type.GetCustomAttributes(typeof(HttpHandlerAttribute), false);
                if (attrs.Length == 0)
                {
                    continue;
                }

                HttpHandlerAttribute httpHandlerAttribute = (HttpHandlerAttribute)attrs[0];
                if (!httpHandlerAttribute.AppType.Is(httpComponent.appType))
                {
                    continue;
                }

                object obj = Activator.CreateInstance(type);

                IHttpHandler ihttpHandler = obj as IHttpHandler;
                if (ihttpHandler == null)
                {
                    throw new Exception($"HttpHandler handler not inherit IHttpHandler class: {obj.GetType().FullName}");
                }

                //this.dispatcher.Add(httpHandlerAttribute.Path, ihttpHandler);
                LoadMethod(type, httpHandlerAttribute, ihttpHandler);
            }
        }

        public Task Invoke(HttpListenerContext context)
        {
            return InvokeHandler(context);
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

                    _getHandlers.Add(httpHandlerAttribute.Path + path, method);
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

                    _postHandlers.Add(httpHandlerAttribute.Path + path, method);
                }

                if (getAttrs.Length == 0 && postAttrs.Length == 0)
                {
                    continue;
                }

                _handlersMapping.Add(method, httpHandler);
            }
        }

        /// <summary>
        /// 调用处理方法
        /// </summary>
        /// <param name="context"></param>
        private async Task InvokeHandler(HttpListenerContext context)
        {
            context.Response.StatusCode = 404;

            MethodInfo methodInfo = null;
            IHttpHandler httpHandler = null;
            string postbody = "";
            switch (context.Request.HttpMethod)
            {
                case "GET":
                    this._getHandlers.TryGetValue(context.Request.Url.AbsolutePath, out methodInfo);
                    if (methodInfo != null)
                    {
                        this._handlersMapping.TryGetValue(methodInfo, out httpHandler);
                    }
                    break;
                case "POST":
                    this._postHandlers.TryGetValue(context.Request.Url.AbsolutePath, out methodInfo);
                    if (methodInfo != null)
                    {
                        this._handlersMapping.TryGetValue(methodInfo, out httpHandler);

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
                if (resp is Task t)
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
    }
}