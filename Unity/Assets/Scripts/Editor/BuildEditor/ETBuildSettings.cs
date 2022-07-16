using UnityEditor;
using UnityEngine;

namespace ET
{
    public class ETBuildSettings : ScriptableObject
    {
        public bool clearFolder = false;
        public bool isBuildExe = false;
        public bool isContainAB = false;
        public BuildType buildType = BuildType.Release;
        public BuildAssetBundleOptions buildAssetBundleOptions = BuildAssetBundleOptions.None;
    }
}
