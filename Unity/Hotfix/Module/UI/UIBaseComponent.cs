using ETModel;

namespace ETHotfix
{
    public abstract class UIBaseComponent : Component
    {
        protected string Type;
        private bool IsOpened;
        private bool IsClearning;

        public virtual void Show()
        {
            IsOpened = true;
            GetParent<UI>().GameObject.SetActive(true);
        }

        public virtual void Close()
        {
            IsOpened = false;
            GetParent<UI>().GameObject.SetActive(false);
            AutoClean(30);
        }

        private async void AutoClean(int second)
        {
            //延迟30秒内仍没被打开则回收到对象池 - 1000 = 1s
            if (IsClearning) return;
            IsClearning = true;
            await ETModel.Game.Scene.GetComponent<TimerComponent>().WaitAsync(second * 1000);
            IsClearning = false;
            if (!string.IsNullOrEmpty(Type) && !IsOpened) Game.Scene.GetComponent<UIComponent>().Remove(Type);
        }
    }
}
