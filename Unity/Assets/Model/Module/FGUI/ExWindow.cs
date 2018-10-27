/**
*  拓展窗体
*  
*  
* **/

using FairyGUI;

namespace ETModel
{
    public class ExWindow : Window
    {
        public event DoHideAnimationEvent doHideAnimationEvent;
        public event DoShowAnimationEvent doShowAnimationEvent;
        public event OnHideEvent onHideEvent;

        public ExWindow()
        {
        }
        protected override void DoHideAnimation()
        {
            if (doHideAnimationEvent != null)
            {
                doHideAnimationEvent();
            }
            base.DoHideAnimation();
        }
        protected override void DoShowAnimation()
        {
            if (doShowAnimationEvent != null)
            {
                doShowAnimationEvent();
            }
            base.DoShowAnimation();
        }

        protected override void OnHide()
        {
            if (onHideEvent != null)
            {
                onHideEvent();
            }
            base.OnHide();
        }
    }
}
