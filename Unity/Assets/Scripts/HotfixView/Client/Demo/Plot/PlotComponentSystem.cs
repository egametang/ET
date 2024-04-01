namespace ET.Client
{
    [EntitySystemOf(typeof(PlotComponent))]
    [FriendOf(typeof(PlotComponent))]
    public static partial class PlotComponentSystem
    {
        [EntitySystem]
        private static void Awake(this PlotComponent self)
        {
            
        }

        public static async ETTask Play(this PlotComponent self, int tableId)
        {
            //var table = xxx;
            //switch (table.Type)
            //{
                //case 1:
                    //self.PlayChat(tableId);
                    //break;
            //}
        }

        private static async ETTask PlayChat(this PlotComponent self,int tableId)
        {
            await EventSystem.Instance.PublishAsync(self.Root(),new Chat(){TableId = tableId});
        }
        
        private static async ETTask PlayAnimation(this PlotComponent self,int tableId)
        {
            await EventSystem.Instance.PublishAsync(self.Root(),new Chat(){TableId = tableId});
        }
        
        [EntitySystem]
        private static void Destroy(this PlotComponent self)
        {
        }
    }
}