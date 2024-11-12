namespace ET
{
    [NumericHandler(SceneType.Current, 0)]
    public class UnitNumericChangeEventHandler_Client_All : NumericHandlerSystem<Unit>
    {
        protected override async ETTask Run(Unit self, NumericChange data)
        {
            //因为是任意值 所以你无法确定目标是什么类型的数值 这里只能获取原生值
            //如果想获取准确值可以判断后获取
            Log.Info($"客户端: 监听任意数值的改变 ID:[{self.Id}] [{data.GetNumericType()}] Odl:[{data.GetSourceValueOld()}] New:[{data.GetSourceValue()}]");
            await ETTask.CompletedTask;
        }
    }

    [NumericHandler(SceneType.Current, ENumericType.Speed0)]
    public class UnitNumericChangeEventHandler_Client_Speed0 : NumericHandlerSystem<Unit>
    {
        protected override async ETTask Run(Unit self, NumericChange data)
        {
            Log.Error($"客户端: 监听到{self.Id} 的速度变化为{data.GetAsFloat()}");

            {
                //案例 只是想说 任意Entity都可以监听到数值变化
                //下面的 NumericHandlerSystem<Scene> 就可以监听到变化
                var numeric = self.Scene().GetComponent<NumericDataComponent>();
                if (numeric == null)
                {
                    numeric = self.Scene().AddComponent<NumericDataComponent>();
                }

                numeric.Set(ENumericType.Speed1, data.GetAsFloat());
            }

            await ETTask.CompletedTask;
        }
    }

    [NumericHandler(SceneType.Current, ENumericType.Speed0)]
    public class UnitNumericChangeEventHandler_Scene_Speed0 : NumericHandlerSystem<Scene>
    {
        protected override async ETTask Run(Scene self, NumericChange data)
        {
            Log.Error($"客户端:场景的数值变化 监听到{self.Id} 的速度变化为{data.GetAsFloat()}");
            await ETTask.CompletedTask;
        }
    }

    [NumericHandler(SceneType.Map, ENumericType.Speed0)]
    public class UnitNumericChangeEventHandler_Server_Speed0 : NumericHandlerSystem<Unit>
    {
        protected override async ETTask Run(Unit self, NumericChange data)
        {
            Log.Error($"服务器: 监听到{self.Id} 的速度变化为{data.GetAsFloat()}");
            await ETTask.CompletedTask;
        }
    }
}