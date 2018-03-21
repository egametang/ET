using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.IO;

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

    // 处理方法
    private Dictionary<MethodInfo, IHttpHandler> handlersMapping;

    // Get处理
    private Dictionary<string, MethodInfo> getHandlers;
    private Dictionary<string, MethodInfo> postHandlers;

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
      this.handlersMapping = new Dictionary<MethodInfo, IHttpHandler>();
      this.getHandlers = new Dictionary<string, MethodInfo>();
      this.postHandlers = new Dictionary<string, MethodInfo>();

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

        LoadMethod(type, httpHandlerAttribute, ihttpHandler);
      }
    }

    public void LoadMethod(Type type, HttpHandlerAttribute httpHandlerAttribute, IHttpHandler httpHandler)
    {
      // 扫描这个类里面的方法
      MethodInfo[] methodInfos = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod | BindingFlags.Instance);
      foreach (var method in methodInfos)
      {
        object[] getAttrs = method.GetCustomAttributes(typeof(GetAttribute), false);
        if (getAttrs.Length != 0)
        {
          GetAttribute get = (GetAttribute)getAttrs[0];

          string path = method.Name;
          if (!string.IsNullOrEmpty(get.Path))
            path = get.Path;

          getHandlers.Add(httpHandlerAttribute.Path + path, method);
          Log.Debug($"add handler[{httpHandler.ToString()}.{method.Name}] path {httpHandlerAttribute.Path + path}");
        }

        object[] postAttrs = method.GetCustomAttributes(typeof(PostAttribute), false);
        if (postAttrs.Length != 0)
        {
          // Post处理方法
          PostAttribute post = (PostAttribute)postAttrs[0];

          string path = method.Name;
          if (!string.IsNullOrEmpty(post.Path))
            path = post.Path;

          postHandlers.Add(httpHandlerAttribute.Path + path, method);
          Log.Debug($"add handler[{httpHandler.ToString()}.{method.Name}] path {httpHandlerAttribute.Path + path}");
        }

        if (getAttrs.Length == 0 && postAttrs.Length == 0) continue;

        handlersMapping.Add(method, httpHandler);
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
        InvokeHandler(context);
        context.Response.Close();
      }
    }

    /// <summary>
    /// 调用处理方法
    /// </summary>
    /// <param name="context"></param>
    private void InvokeHandler(HttpListenerContext context)
    {
      context.Response.StatusCode = 404;

      MethodInfo methodInfo = null;
      IHttpHandler httpHandler = null;
      string postbody = "";
      if (context.Request.HttpMethod == "GET")
      {
        this.getHandlers.TryGetValue(context.Request.Url.AbsolutePath, out methodInfo);
        if (methodInfo != null)
          this.handlersMapping.TryGetValue(methodInfo, out httpHandler);
      }
      else if (context.Request.HttpMethod == "POST")
      {
        this.postHandlers.TryGetValue(context.Request.Url.AbsolutePath, out methodInfo);
        if (methodInfo != null)
        {
          this.handlersMapping.TryGetValue(methodInfo, out httpHandler);

          using (StreamReader sr = new StreamReader(context.Request.InputStream))
          {
            postbody = sr.ReadToEnd();
          }
        }
      }
      else
      {
        context.Response.StatusCode = 405;
      }

      if (httpHandler != null)
      {
        object[] args = InjectParameters(context, methodInfo, postbody);

        // 自动把返回值，以json方式响应。
        object resp = methodInfo?.Invoke(httpHandler, args);
        if (resp != null)
        {
          using (StreamWriter sw = new StreamWriter(context.Response.OutputStream))
          {
            if (resp.GetType() == typeof(string))
            {
              sw.Write(resp.ToString());
            }
            else
            {
              sw.Write(JsonHelper.ToJson(resp));
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
        }
        else if (item.ParameterType == typeof(HttpListenerResponse))
        {
          args[i] = context.Response;
        }
        else
        {
          try
          {
            if (context.Request.HttpMethod == "POST") //TODO 扩展一些，Http Entity 自动转换 的功能
            {
              if (item.Name == "postBody") // 约定参数名称为postBody,只传string类型。本来是byte[]，有需求可以改。
              {
                args[i] = postbody;
              }
              else if (item.ParameterType.IsClass && item.ParameterType != typeof(string) && !string.IsNullOrEmpty(postbody))
              {
                object entity = JsonHelper.FromJson(item.ParameterType, postbody);
                args[i] = entity;
              }
            }
            else if (context.Request.HttpMethod == "GET")
            {
              string query = context.Request.QueryString[item.Name];
              if (query != null)
              {
                object value = Convert.ChangeType(query, item.ParameterType);
                args[i] = value;
              }
            }
            else
            {
              args[i] = null;
            }
          }
          catch (Exception e)
          {
            Log.Debug(e.ToString());
            args[i] = null;
          }
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