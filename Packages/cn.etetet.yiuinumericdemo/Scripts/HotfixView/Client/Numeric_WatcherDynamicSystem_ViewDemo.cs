namespace ET.Client
{
    [Event(SceneType.Current)]
    public class AfterUnitCreate_HandlerDynamicDemo : AEvent<Scene, AfterUnitCreate>
    {
        protected override async ETTask Run(Scene scene, AfterUnitCreate args)
        {
            Unit unit = args.Unit;
            unit.AddComponent<NumericHandlerDynamicDemoComponent>();
            await ETTask.CompletedTask;
        }
    }

    [EntitySystemOf(typeof(NumericHandlerDynamicDemoComponent))]
    [FriendOf(typeof(NumericHandlerDynamicDemoComponent))]
    public static partial class NumericHandlerDynamicDemoComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ET.NumericHandlerDynamicDemoComponent self)
        {
            self.TestValue = "测试流程 动态数值监听";
        }

        [NumericHandlerDynamic(SceneType.Current, ENumericType.AOI0)]
        [FriendOf(typeof(NumericHandlerDynamicDemoComponent))]
        public class UnitNumericChangeEventHandler_AOI0 : NumericHandlerDynamicSystem<NumericHandlerDynamicDemoComponent, Unit, NumericChange>
        {
            protected override async ETTask Run(NumericHandlerDynamicDemoComponent self, Unit entity, NumericChange data)
            {
                //注意这里来的动态消息是 任意Unit 都会来
                //如果你想要监听指定的某个Unit
                //1 提前存一下然后判断2个是不是相同 如果是则XX
                //2 使用动态监听中的定向监听功能
                Log.Error($"收到动态数值监听: {self.TestValue} {data.GetNumericTypeEnum()} {data.GetAsFloat()}");
                await ETTask.CompletedTask;
            }
        }

        [NumericHandlerDynamic(SceneType.Current, ENumericType.AOI0, 1)]
        [FriendOf(typeof(NumericHandlerDynamicDemoComponent))]
        public class UnitNumericChangeEventHandler_ParentAOI0 : NumericHandlerDynamicSystem<NumericHandlerDynamicDemoComponent, Unit, NumericChange>
        {
            protected override async ETTask Run(NumericHandlerDynamicDemoComponent self, Unit entity, NumericChange data)
            {
                //精准响应
                //NumericHandlerDynamic 特性中的第三个参数 = 这个Unit是我这个componet的多少级Parent
                //则只有这个Unit改变了才会有事件通知
                //测试场景中如果有多个Unit 你就能体会出 这个监听与上面那个监听有什么不同了
                Log.Error($"收到动态数值精准监听: {self.TestValue} {data.GetNumericTypeEnum()} {data.GetAsFloat()}");
                await ETTask.CompletedTask;
            }
        }
    }
}
