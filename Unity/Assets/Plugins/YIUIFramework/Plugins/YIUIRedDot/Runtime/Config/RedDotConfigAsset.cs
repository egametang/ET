using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

namespace YIUIFramework
{
    //[CreateAssetMenu(fileName = "RedDotConfigAsset", menuName = "YIUI/RedDot/RedDotConfigAsset", order = 2)]
    [LabelText("红点关系配置资源")]
    public class RedDotConfigAsset : SerializedScriptableObject
    {
        [OdinSerialize]
        [ReadOnly]
        [ShowInInspector]
        internal Dictionary<ERedDotKeyType, RedDotConfigData> m_AllRedDotConfigDic =
            new Dictionary<ERedDotKeyType, RedDotConfigData>();

        public IReadOnlyDictionary<ERedDotKeyType, RedDotConfigData> AllRedDotConfigDic => m_AllRedDotConfigDic;

        public RedDotConfigData GetConfigData(ERedDotKeyType key)
        {
            m_AllRedDotConfigDic.TryGetValue(key, out var value);
            return value;
        }
    }
}