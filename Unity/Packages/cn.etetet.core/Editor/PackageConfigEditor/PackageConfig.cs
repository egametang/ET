using System.IO;
using UnityEditor;
using UnityEngine;

namespace ET
{
    [CreateAssetMenu(menuName = "ET/PackageConfig", fileName = "PackageConfig", order = 0)]
    public class PackageConfig: ScriptableObject
    {
        public int Id;
        public string Name;
        public bool CreatePackageTypeFile;
    }
}