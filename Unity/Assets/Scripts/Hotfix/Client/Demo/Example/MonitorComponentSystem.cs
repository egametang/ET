namespace ET.Client
{
    [EntitySystemOf(typeof(MonitorComponent))]
    //数据修改友好标记，允许修改指定类型上的数据
    [FriendOf(typeof(ET.Client.MonitorComponent))]
    public static partial class MonitorComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Client.MonitorComponent self, int brightness)
        {
            Log.Debug("MonitorComponent Awake");
            
            //修改亮度
            self.Brightness = brightness;
        }
        
        [EntitySystem]
        private static void Destroy(this ET.Client.MonitorComponent self)
        {
            Log.Debug("MonitorComponent Destroy");
        }

        public static void ChangeBrightness(this MonitorComponent self, int value)
        {
            self.Brightness = value;
        }
    }
}