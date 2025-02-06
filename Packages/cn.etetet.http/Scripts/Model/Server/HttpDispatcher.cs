using System;
using System.Collections.Generic;

namespace ET.Server
{
    [CodeProcess]
    public class HttpDispatcher: Singleton<HttpDispatcher>, ISingletonAwake
    {
        private readonly Dictionary<string, Dictionary<int, IHttpHandler>> dispatcher = new();
        
        public void Awake()
        {
            HashSet<Type> types = CodeTypes.Instance.GetTypes(typeof (HttpHandlerAttribute));

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
                
                dict.Add(httpHandlerAttribute.SceneType, ihttpHandler);
            }
        }

        public IHttpHandler Get(int sceneType, string path)
        {
            return this.dispatcher[path][sceneType];
        }
    }
}