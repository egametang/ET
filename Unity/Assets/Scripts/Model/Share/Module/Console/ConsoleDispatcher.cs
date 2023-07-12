using System;
using System.Collections.Generic;

namespace ET
{
    public class ConsoleDispatcher: SingletonLock<ConsoleDispatcher>, ISingletonAwake
    {
        private readonly Dictionary<string, IConsoleHandler> handlers = new();
        
        public override void Load()
        {
            World.Instance.AddSingleton<ConsoleDispatcher>(true);
        }

        public void Awake()
        {
            HashSet<Type> types = EventSystem.Instance.GetTypes(typeof (ConsoleHandlerAttribute));

            foreach (Type type in types)
            {
                object[] attrs = type.GetCustomAttributes(typeof(ConsoleHandlerAttribute), false);
                if (attrs.Length == 0)
                {
                    continue;
                }

                ConsoleHandlerAttribute consoleHandlerAttribute = (ConsoleHandlerAttribute)attrs[0];

                object obj = Activator.CreateInstance(type);

                IConsoleHandler iConsoleHandler = obj as IConsoleHandler;
                if (iConsoleHandler == null)
                {
                    throw new Exception($"ConsoleHandler handler not inherit IConsoleHandler class: {obj.GetType().FullName}");
                }
                this.handlers.Add(consoleHandlerAttribute.Mode, iConsoleHandler);
            }
        }

        public IConsoleHandler Get(string key)
        {
            return this.handlers[key];
        }
    }
}