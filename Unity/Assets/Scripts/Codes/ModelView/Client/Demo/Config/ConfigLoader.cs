using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    [Callback]
    public class GetAllConfigBytes: ACallbackHandler<ConfigComponent.GetAllConfigBytes, Dictionary<string, byte[]>>
    {
        public override Dictionary<string, byte[]> Handle(ConfigComponent.GetAllConfigBytes args)
        {
            Dictionary<string, byte[]> output = new Dictionary<string, byte[]>();
            using (Game.Scene.AddComponent<ResourcesComponent>())
            {
                const string configBundleName = "config.unity3d";
                ResourcesComponent.Instance.LoadBundle(configBundleName);
                
                HashSet<Type> configTypes = EventSystem.Instance.GetTypes(typeof (ConfigAttribute));
                foreach (Type configType in configTypes)
                {
                    TextAsset v = ResourcesComponent.Instance.GetAsset(configBundleName, configType.Name) as TextAsset;
                    output[configType.Name] = v.bytes;
                }
            }

            return output;
        }
    }
    
    [Callback]
    public class GetOneConfigBytes: ACallbackHandler<ConfigComponent.GetOneConfigBytes, byte[]>
    {
        public override byte[] Handle(ConfigComponent.GetOneConfigBytes args)
        {
            //TextAsset v = ResourcesComponent.Instance.GetAsset("config.unity3d", configName) as TextAsset;
            //return v.bytes;
            throw new NotImplementedException("client cant use LoadOneConfig");
        }
    }
}