using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ET
{
    public static class LoadConfigHelper
    {
        public static void GetAllConfigBytes(Dictionary<string, byte[]> output)
        {
            Dictionary<string, UnityEngine.Object> keys = ResourcesComponent.Instance.GetBundleAll("config.unity3d");

            foreach (var kv in keys)
            {
                TextAsset v = kv.Value as TextAsset;
                string key = kv.Key;
                output[key] = v.bytes;
            }
        }
        
        public static byte[] GetOneConfigBytes(string configName)
        {
            TextAsset v = ResourcesComponent.Instance.GetAsset("config.unity3d", configName) as TextAsset;
            return v.bytes;
        }
    }
}