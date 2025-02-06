using System;
using System.Collections.Generic;
#if DOTNET || UNITY_STANDALONE
using System.Threading.Tasks;
#endif

namespace ET
{
    /// <summary>
    /// ConfigLoader会扫描所有的有ConfigAttribute标签的配置,加载进来
    /// </summary>
    public class ConfigLoader : Singleton<ConfigLoader>, ISingletonAwake
    {
        public struct GetAllConfigBytes
        {
        }

        public struct GetOneConfigBytes
        {
            public string ConfigName;
        }

        public void Awake()
        {
        }

        public async ETTask Reload(Type configType)
        {
            GetOneConfigBytes getOneConfigBytes = new() { ConfigName = configType.Name };
            byte[] oneConfigBytes = await EventSystem.Instance.Invoke<GetOneConfigBytes, ETTask<byte[]>>(getOneConfigBytes);
            LoadOneConfig(configType, oneConfigBytes);
            ConfigProcess();
        }

        public async ETTask LoadAsync()
        {
            Dictionary<Type, byte[]> configBytes = await EventSystem.Instance.Invoke<GetAllConfigBytes, ETTask<Dictionary<Type, byte[]>>>(new GetAllConfigBytes());

#if DOTNET || UNITY_STANDALONE
            using ListComponent<Task> listTasks = ListComponent<Task>.Create();

            foreach (Type type in configBytes.Keys)
            {
                byte[] oneConfigBytes = configBytes[type];
                Task task = Task.Run(() => LoadOneConfig(type, oneConfigBytes));
                listTasks.Add(task);
            }

            await Task.WhenAll(listTasks.ToArray());
#else
            foreach (Type type in configBytes.Keys)
            {
                LoadOneConfig(type, configBytes[type]);
            }
#endif

            ConfigProcess();
        }

        private static void LoadOneConfig(Type configType, byte[] oneConfigBytes)
        {
            object category = MongoHelper.Deserialize(configType, oneConfigBytes, 0, oneConfigBytes.Length);
            ASingleton singleton = category as ASingleton;
            World.Instance.AddSingleton(singleton);
        }

        private void ConfigProcess()
        {
            var hashSet = CodeTypes.Instance.GetTypes(typeof (ConfigProcessAttribute));
            foreach (Type type in hashSet)
            {
                object obj = Activator.CreateInstance(type);
                ((ISingletonAwake)obj).Awake();
                World.Instance.AddSingleton((ASingleton)obj);
            }
        }
    }
}