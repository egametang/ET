using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

namespace YIUIFramework
{
    //[CreateAssetMenu(fileName = "RedDotKeyAsset", menuName = "YIUI/RedDot/RedDotKeyAsset", order = 1)]
    [LabelText("红点枚举配置资源")]
    public class RedDotKeyAsset : SerializedScriptableObject
    {
        [OdinSerialize]
        [ReadOnly]
        [ShowInInspector]
        internal Dictionary<int, RedDotKeyData> m_AllRedDotDic = new Dictionary<int, RedDotKeyData>();

        public IReadOnlyDictionary<int, RedDotKeyData> AllRedDotDic => m_AllRedDotDic;
    }
}