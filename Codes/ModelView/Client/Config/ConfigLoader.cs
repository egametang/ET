using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    [Callback(CallbackType.GetAllConfigBytes)]
    public class GetAllConfigBytes: IAction<Dictionary<string, byte[]>>
    {
        public void Handle(Dictionary<string, byte[]> output)
        {
            Dictionary<string, UnityEngine.Object> keys = ResourcesComponent.Instance.GetBundleAll("config.unity3d");

            foreach (var kv in keys)
            {
                TextAsset v = kv.Value as TextAsset;
                string key = kv.Key;
                output[key] = v.bytes;
            }
        }
    }
    
    [Callback(CallbackType.GetOneConfigBytes)]
    public class GetOneConfigBytes: IFunc<string, byte[]>
    {
        public byte[] Handle(string configName)
        {
            TextAsset v = ResourcesComponent.Instance.GetAsset("config.unity3d", configName) as TextAsset;
            return v.bytes;
        }
    }
}