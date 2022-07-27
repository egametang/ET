using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    [Callback(CallbackType.GetAllConfigBytes)]
    public class GetAllConfigBytes: IAction<ConfigComponent, Dictionary<string, byte[]>>
    {
        public void Handle(ConfigComponent configComponent, Dictionary<string, byte[]> output)
        {
            using (Game.Scene.AddComponent<ResourcesComponent>())
            {
                const string configBundleName = "config.unity3d";
                ResourcesComponent.Instance.LoadBundle(configBundleName);
                
                HashSet<Type> configTypes = Game.EventSystem.GetTypes(typeof (ConfigAttribute));
                foreach (Type configType in configTypes)
                {
                    TextAsset v = ResourcesComponent.Instance.GetAsset(configBundleName, configType.Name) as TextAsset;
                    output[configType.Name] = v.bytes;
                }
            }
        }
    }

    [Callback(CallbackType.GetOneConfigBytes)]
    public class GetOneConfigBytes: IFunc<string, byte[]>
    {
        public byte[] Handle(string configName)
        {
            //TextAsset v = ResourcesComponent.Instance.GetAsset("config.unity3d", configName) as TextAsset;
            //return v.bytes;
            throw new NotImplementedException("client cant use LoadOneConfig");
        }
    }
}