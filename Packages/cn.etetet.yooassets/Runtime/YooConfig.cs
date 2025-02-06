using UnityEngine;
using YooAsset;

namespace ET
{
    [CreateAssetMenu(menuName = "ET/YooConfig", fileName = "YooConfig", order = 0)]
    public class YooConfig: ScriptableObject
    {
        public EPlayMode EPlayMode;
        public string Url;
    }
}