//------------------------------------------------------------
// Author: 亦亦
// Mail: 379338943@qq.com
// Data: 2023年2月12日
//------------------------------------------------------------

namespace ET.Client
{
    /// <summary>
    /// UI面板组件
    /// </summary>
    [FriendOf(typeof (YIUIPanelComponent))]
    [EntitySystemOf(typeof (YIUIPanelComponent))]
    public static partial class YIUIPanelComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this YIUIPanelComponent self)
        {
            self.InitPanelViewData();
        }
    }
}