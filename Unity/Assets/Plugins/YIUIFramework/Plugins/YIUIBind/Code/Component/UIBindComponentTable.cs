//------------------------------------------------------------
// Author: 亦亦
// Mail: 379338943@qq.com
// Data: 2023年2月12日
//------------------------------------------------------------

using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using Logger = YIUIFramework.Logger;

namespace YIUIFramework
{
    //[DetailedInfoBox("UI 组件表 点击展开详细介绍", @"李胜扬")]
    [LabelText("UI 组件表")]
    [AddComponentMenu("YIUIBind/★★★UI Component Table 组件表★★★")]
    public sealed partial class UIBindComponentTable:SerializedMonoBehaviour
    {
        [OdinSerialize]
        [LabelText("所有绑定数据 最终数据")]
        [ReadOnly]
        [PropertyOrder(-9)]
        private Dictionary<string,Component> m_AllBindDic = new Dictionary<string,Component>();
        public IReadOnlyDictionary<string, Component> AllBindDic => m_AllBindDic;

        private Component FindComponent(string comName)
        {
            m_AllBindDic.TryGetValue(comName, out var value);
            if (value == null)
            {
                Logger.LogErrorContext(this,$" {name} 组件表中没有这个组件 {comName}");
            }
            return value;
        }
        
        public T FindComponent<T>(string comName) where T :Component
        {
            return (T)FindComponent(comName);
        }
    }
}