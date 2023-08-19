using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;

namespace ET.Client
{
    public static class AssetsBundleHelper
    {
        public static async ETTask InitializeAsync()
        {
            IResourceLocator resourceLocator = await Addressables.InitializeAsync().Task;
            long downloadSize = await Addressables.GetDownloadSizeAsync(resourceLocator.Keys).Task;
            Log.Info($"download size: {downloadSize}");
        }
        
        public static async ETTask LoadCodeAsync()
        {
            List<string> codes = new List<string>()
            {
                "Model.dll",
                "Model.pdb",
                "Hotfix.dll",
                "Hotfix.pdb",
            };
            
            List<ETTask> tasks = new List<ETTask>(codes.Count);
            foreach (string s in codes)
            {
                tasks.Add(LoadOneDll($"Assets/Bundles/Code/{s}.bytes"));
            }
            await ETTaskHelper.WaitAll(tasks);
        }
        
        public static async ETTask LoadAotDllAsync()
        {
            if (Define.EnableIL2CPP)
            {
                List<ETTask> tasks = new List<ETTask>(AOTGenericReferences.PatchedAOTAssemblyList.Count);
                foreach (string s in AOTGenericReferences.PatchedAOTAssemblyList)
                {
                    tasks.Add(LoadOneDll($"Assets/Bundles/AotDlls/{s}.bytes"));
                }

                await ETTaskHelper.WaitAll(tasks);
            }
        }
        
        private static async ETTask LoadOneDll(string s)
        {
            await ResourcesComponent.Instance.LoadAssetAsync(s);
        }
    }
}