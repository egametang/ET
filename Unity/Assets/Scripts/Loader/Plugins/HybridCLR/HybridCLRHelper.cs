using ET.Client;
using HybridCLR;
using UnityEngine;

namespace ET
{
    public static class HybridCLRHelper
    {
        public static void Load()
        {
            foreach (string s in AOTGenericReferences.PatchedAOTAssemblyList)
            {
                UnityEngine.Object o = ResourcesComponent.Instance.GetAssets($"Assets/Bundles/AotDlls/{s}.bytes");
                TextAsset textAsset = o as TextAsset;
                RuntimeApi.LoadMetadataForAOTAssembly(textAsset.bytes, HomologousImageMode.SuperSet);
            }
        }
    }
}