using System.Collections.Generic;
using TMPro;
using YIUIFramework;

namespace ET.Client
{
    public partial class RedDotPanelComponent: Entity,IYIUIEvent<OnClickParentListEvent>,IYIUIEvent<OnClickChildListEvent>,IYIUIEvent<OnClickItemEvent>
    {
        public YIUILoopScroll<RedDotData, RedDotDataItemComponent>   m_SearchScroll;
        public List<RedDotData>                                      m_CurrentDataList      = new List<RedDotData>();
        public Dictionary<int, ERedDotKeyType>                       m_AllDropdownSearchDic = new Dictionary<int, ERedDotKeyType>();
        public List<TMP_Dropdown.OptionData>                         m_DropdownOptionData   = new List<TMP_Dropdown.OptionData>();
        public RedDotData                                            m_InfoData;
        public YIUILoopScroll<RedDotStack, RedDotStackItemComponent> m_StackScroll;
    }
    
    public struct OnClickParentListEvent
    {
        public RedDotData Data;
    }
    
    public struct OnClickChildListEvent
    {
        public RedDotData Data;
    }
    
    public struct OnClickItemEvent
    {
        public RedDotData Data;
    }
}