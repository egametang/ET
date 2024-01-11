using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

namespace YIUIFramework
{
    [HideLabel]
    [HideReferenceObjectPicker]
    [Serializable]
    public class RedDotConfigData
    {
        [LabelText("Key")]
        [ShowInInspector]
        [OdinSerialize]
        public ERedDotKeyType Key { get; internal set; } = ERedDotKeyType.None;

        [LabelText("所有父级列表")]
        [ShowInInspector]
        [OdinSerialize]
        public List<ERedDotKeyType> ParentList { get; internal set; } = new List<ERedDotKeyType>();

        [LabelText("是否允许开关提示")]
        [ShowInInspector]
        [OdinSerialize]
        public bool SwitchTips { get; internal set; } = true; //true = 玩家可开关 false = 不可开关 (永久提示)

        internal RedDotConfigData()
        {
        }
    }
}