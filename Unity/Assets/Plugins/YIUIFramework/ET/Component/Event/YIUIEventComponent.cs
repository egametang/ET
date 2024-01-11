//------------------------------------------------------------
// Author: 亦亦
// Mail: 379338943@qq.com
// Data: 2023年2月12日
//------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// UI事件分发组件
    /// </summary>
    [ChildOf(typeof (YIUIMgrComponent))]
    public partial class YIUIEventComponent: Entity, IAwake, IDestroy
    {
        public static YIUIEventComponent Inst;

        public Dictionary<Type, Dictionary<string, List<YIUIEventInfo>>> _AllEventInfo;
    }
}