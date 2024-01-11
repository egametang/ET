namespace ET.Client
{
    public static partial class YIUIMgrComponentSystem
    {
        public static bool ActiveSelf(this YIUIMgrComponent self, string panelName)
        {
            var info = self.GetPanelInfo(panelName);
            return info?.ActiveSelf ?? false;
        }

        public static bool ActiveSelf<T>(this YIUIMgrComponent self) where T : Entity
        {
            var info = self.GetPanelInfo<T>();
            return info?.ActiveSelf ?? false;
        }
    }
}