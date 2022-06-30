using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace ET.Client
{
    [Event(SceneType.Process)]
    public class LoadConfig: AEvent<Scene, EventType.LoadConfig>
    {
        protected override async ETTask Run(Scene entity, EventType.LoadConfig a)
        {
            List<string> paths = YooAssetProxy.GetAssetPathsByTag("ConfigList");

            Dictionary<string, UnityEngine.Object> keys = new Dictionary<string, UnityEngine.Object>();

            List<ETTask<AssetOperationHandle>> etTasks = new List<ETTask<AssetOperationHandle>>();
            foreach (var path in paths)
            {
                etTasks.Add(YooAssetProxy.LoadAssetAsync<TextAsset>(path));
            }

            await ETTaskHelper.WaitAll(etTasks);

            foreach (var etTask in etTasks)
            {
                keys[etTask.GetResult().AssetObject.name] = etTask.GetResult().GetAsset<TextAsset>();
            }
            
            foreach (var kv in keys)
            {
                TextAsset v = kv.Value as TextAsset;
                string key = kv.Key;
                a.configBytes[key] = v.bytes;
            }
        }
    }
}