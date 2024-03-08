using System;
using System.Reflection;

namespace ET
{
    public class SceneTypeSingleton: Singleton<SceneTypeSingleton>, ISingletonAwake<Type>
    {
        public static bool IsSame(int a, int b)
        {
            if (a == b)
            {
                return true;
            }

            if (a == 0)
            {
                return true;
            }

            if (b == 0)
            {
                return true;
            }

            return false;
        }
        
        private readonly DoubleMap<int, string> sceneNames = new();

        public void Awake(Type type)
        {
            FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (FieldInfo fieldInfo in fieldInfos)
            {
                if (fieldInfo.FieldType != typeof(int))
                {
                    continue;
                }
                this.sceneNames.Add((int)fieldInfo.GetValue(null), fieldInfo.Name);	
            }
        }
		
        public string GetSceneName(int sceneType)
        {
            return this.sceneNames.GetValueByKey(sceneType);
        }
		
        public int GetSceneType(string sceneName)
        {
            return this.sceneNames.GetKeyByValue(sceneName);
        }
    }
}