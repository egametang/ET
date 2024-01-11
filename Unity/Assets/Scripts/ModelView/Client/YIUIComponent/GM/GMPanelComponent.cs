using UnityEngine;
using YIUIFramework;

namespace ET.Client
{
    public partial class GMPanelComponent: Entity, IUpdate
    {
        public FloatPrefs _GMBtn_Pos_X = new("GMPanelComponent_GMBtn_Pos_X");
        public FloatPrefs _GMBtn_Pos_Y = new("GMPanelComponent_GMBtn_Pos_Y");
        public Vector2    _Offset;
        public Vector2    _LimitSize;
    }
}