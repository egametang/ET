namespace ET.Client
{
    //实体组件通用的System标记，必须指定类型
    [EntitySystemOf(typeof(Computer))]
    public static partial class ComputerSystem
    {
        [EntitySystem]
        private static void Awake(this Computer self)
        {
            Log.Debug("Computer Awake");
        }

        [EntitySystem]
        private static void Update(this Computer self)
        {
            Log.Debug("Computer Update");
        }

        [EntitySystem]
        private static void Destroy(this Computer self)
        {
            Log.Debug("Computer Destroy");
        }

        //自己编写的给外部调用的测试方法
        public static void Open(this Computer self)
        {
            Log.Debug("Computer Open");
        }
    }
}