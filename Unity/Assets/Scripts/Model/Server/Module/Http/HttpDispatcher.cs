using System;
using System.Collections.Generic;

namespace ET.Server
{
    public class HttpDispatcher: SingletonLock<HttpDispatcher>, ISingletonAwake
    {
        private readonly Dictionary<string, Dictionary<int, IHttpHandler>> dispatcher = new();
        
        public override void Load()
        {
            World.Instance.AddSingleton<HttpDispatcher>();
        }

        public void Awake()
        {
            HashSet<Type> types = EventSystem.Instance.GetTypes(typeof (HttpHandlerAttribute));

            foreach (Type type in types)
            {
                object[] attrs = type.GetCustomAttributes(typeof(HttpHandlerAttribute), false);
                if (attrs.Length == 0)
                {
                    continue;
                }

                HttpHandlerAttribute httpHandlerAttribute = (HttpHandlerAttribute)attrs[0];
                
                object obj = Activator.CreateInstance(type);

                IHttpHandler ihttpHandler = obj as IHttpHandler;
                if (ihttpHandler == null)
                {
                    throw new Exception($"HttpHandler handler not inherit IHttpHandler class: {obj.GetType().FullName}");
                }

                if (!this.dispatcher.TryGetValue(httpHandlerAttribute.Path, out var dict))
                {
                    dict = new Dictionary<int, IHttpHandler>();
                    this.dispatcher.Add(httpHandlerAttribute.Path, dict);
                }
                
                dict.Add((int)httpHandlerAttribute.SceneType, ihttpHandler);
            }
        }

        public IHttpHandler Get(SceneType sceneType, string path)
        {
            return this.dispatcher[path][(int)sceneType];
        }
    }
}