using System;
using System.Collections.Generic;
using System.Net;

namespace ET
{
    public class HttpComponentAwakeSystem : AwakeSystem<HttpComponent, string>
    {
        public override void Awake(HttpComponent self, string address)
        {
            try
            {
                self.Load();
                
                self.Listener = new HttpListener();

                foreach (string s in address.Split(';'))
                {
                    if (s.Trim() == "")
                    {
                        continue;
                    }
                    self.Listener.Prefixes.Add(s);
                }

                self.Listener.Start();

                self.Accept().Coroutine();
            }
            catch (HttpListenerException e)
            {
                throw new Exception($"请现在cmd中运行: netsh http add urlacl url=http://*:你的address中的端口/ user=Everyone, address: {address}", e);
            }
        }
    }

    [ObjectSystem]
    public class HttpComponentLoadSystem: LoadSystem<HttpComponent>
    {
        public override void Load(HttpComponent self)
        {
            self.Load();
        }
    }

    [ObjectSystem]
    public class HttpComponentDestroySystem: DestroySystem<HttpComponent>
    {
        public override void Destroy(HttpComponent self)
        {
            self.Listener.Stop();
            self.Listener.Close();
        }
    }
    
    [FriendClass(typeof(HttpComponent))]
    public static class HttpComponentSystem
    {
        public static void Load(this HttpComponent self)
        {
            self.dispatcher = new Dictionary<string, IHttpHandler>();

            List<Type> types = EventSystem.Instance.GetTypes(typeof (HttpHandlerAttribute));

            SceneType sceneType = self.GetParent<Scene>().SceneType;

            foreach (Type type in types)
            {
                object[] attrs = type.GetCustomAttributes(typeof(HttpHandlerAttribute), false);
                if (attrs.Length == 0)
                {
                    continue;
                }

                HttpHandlerAttribute httpHandlerAttribute = (HttpHandlerAttribute)attrs[0];

                if (httpHandlerAttribute.SceneType != sceneType)
                {
                    continue;
                }

                object obj = Activator.CreateInstance(type);

                IHttpHandler ihttpHandler = obj as IHttpHandler;
                if (ihttpHandler == null)
                {
                    throw new Exception($"HttpHandler handler not inherit IHttpHandler class: {obj.GetType().FullName}");
                }
                self.dispatcher.Add(httpHandlerAttribute.Path, ihttpHandler);
            }
        }
        
        public static async ETTask Accept(this HttpComponent self)
        {
            long instanceId = self.InstanceId;
            while (self.InstanceId == instanceId)
            {
                try
                {
                    HttpListenerContext context = await self.Listener.GetContextAsync();
                    self.Handle(context).Coroutine();
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public static async ETTask Handle(this HttpComponent self, HttpListenerContext context)
        {
            try
            {
                IHttpHandler handler;
                if (self.dispatcher.TryGetValue(context.Request.Url.AbsolutePath, out handler))
                {
                    await handler.Handle(self.Domain, context);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            context.Request.InputStream.Dispose();
            context.Response.OutputStream.Dispose();
        }
    }
}