using ETModel;

namespace ETHotfix
{
    [Event(EventIdType.UI_CharacterCreation)]
    public class Event_UI_CharacterCreation : AEvent
    {
        public override void Run()
        {
            FGUI.Open<UI_CharacterCreation>();
        }
    }
}
