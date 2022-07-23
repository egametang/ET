using System;
using FairyGUI.Utils;

namespace FairyGUI
{
    public class ControllerAction
    {
        public enum ActionType
        {
            PlayTransition,
            ChangePage
        }

        public string[] fromPage;
        public string[] toPage;

        public static ControllerAction CreateAction(ActionType type)
        {
            switch (type)
            {
                case ActionType.PlayTransition:
                    return new PlayTransitionAction();

                case ActionType.ChangePage:
                    return new ChangePageAction();
            }
            return null;
        }

        public ControllerAction()
        {
        }

        public void Run(Controller controller, string prevPage, string curPage)
        {
            if ((fromPage == null || fromPage.Length == 0 || Array.IndexOf(fromPage, prevPage) != -1)
                && (toPage == null || toPage.Length == 0 || Array.IndexOf(toPage, curPage) != -1))
                Enter(controller);
            else
                Leave(controller);
        }

        virtual protected void Enter(Controller controller)
        {

        }

        virtual protected void Leave(Controller controller)
        {

        }

        virtual public void Setup(ByteBuffer buffer)
        {
            int cnt;

            cnt = buffer.ReadShort();
            fromPage = new string[cnt];
            for (int i = 0; i < cnt; i++)
                fromPage[i] = buffer.ReadS();

            cnt = buffer.ReadShort();
            toPage = new string[cnt];
            for (int i = 0; i < cnt; i++)
                toPage[i] = buffer.ReadS();
        }
    }
}
