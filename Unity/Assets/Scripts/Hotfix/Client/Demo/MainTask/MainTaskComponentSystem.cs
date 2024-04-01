namespace ET.Client
{
    [EntitySystemOf(typeof(MainTaskComponent))]
    [FriendOf(typeof(MainTaskComponent))]
    public static partial class MainTaskComponentSystem
    {
        [EntitySystem]
        private static void Awake(this MainTaskComponent self,int tableId,int progress)
        {
            self.TableId = tableId;
            self.UpdateProgress(progress);
        }

        public static void UpdateTaskId(this MainTaskComponent self, int tableId)
        {
            self.TableId = tableId;
            EventSystem.Instance.Publish(self.Root(), new MainTaskStart());
        }

        public static void UpdateProgress(this MainTaskComponent self, int progress)
        {
            self.Progress = progress;
            self.CheckProgress();
        }

        private static void CheckProgress(this MainTaskComponent self)
        {
            MainTaskConfig table = self.Table;
            if (self.Progress >= 1)
            {
                if (table.AutoComplete == 1)
                {
                    self.UpdateTaskId(self.TableId + 1);
                    return;
                }

                //标记为已完成
                self.Status = 1;
            }
        }

        [EntitySystem]
        private static void Destroy(this MainTaskComponent self)
        {
        }
    }

    public struct MainTaskStatusUpdate
    {
    }

    public struct MainTaskStart
    {
    }
}