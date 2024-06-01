using UnityEngine;

namespace ET
{
    [CreateAssetMenu(menuName = "ET/PackageConfig", fileName = "PackageConfig", order = 0)]
    public class PackageConfig: ScriptableObject
    {
        public int Id;
        public bool CreatePackageTypeFile;
    }
}