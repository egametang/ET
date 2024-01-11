namespace ET.Client
{
    public static partial class YIUIPanelComponentSystem
    {
        public static void Close(this YIUIPanelComponent self, bool tween = true, bool ignoreElse = false)
        {
            self.CloseAsync(tween, ignoreElse).Coroutine();
        }

        public static async ETTask<bool> CloseAsync(this YIUIPanelComponent self, bool tween = true, bool ignoreElse = false)
        {
            return await YIUIMgrComponent.Inst.ClosePanelAsync(self.UIBase.UIName, tween, ignoreElse);
        }

        public static void Home<T>(this YIUIPanelComponent self, bool tween = true) where T : Entity
        {
            YIUIMgrComponent.Inst.HomePanel<T>(tween).Coroutine();
        }

        public static void Home(this YIUIPanelComponent self, string homeName, bool tween = true)
        {
            YIUIMgrComponent.Inst.HomePanel(homeName, tween).Coroutine();
        }
    }
}