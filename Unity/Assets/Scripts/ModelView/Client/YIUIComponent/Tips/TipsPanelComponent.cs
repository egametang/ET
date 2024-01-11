using System;
using System.Collections.Generic;
using YIUIFramework;

namespace ET.Client
{
    public partial class TipsPanelComponent: Entity, IYIUIOpen<Type, ParamVo>, IYIUIEvent<EventPutTipsView>
    {
        public Dictionary<Type, ObjAsyncCache<Entity>> _AllPool  = new();
        public int                                     _RefCount = 0;
    }

    /// <summary>
    /// 通用弹窗view关闭事件
    /// </summary>
    public struct EventPutTipsView
    {
        public Entity View;  //View实例
        public bool   Tween; //关闭时是否可触发动画
    }
}