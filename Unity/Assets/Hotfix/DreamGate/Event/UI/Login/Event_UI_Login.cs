using ETModel;

namespace ETHotfix
{
    [Event(EventIdType.UI_Login)]
    public class Event_UI_Login : AEvent
    {
        public override void Run()
        {
            FGUI.Open<UI_Login>();
        }
    }
}
