namespace ET
{
    public interface INumericHandler
    {
        ETTask Run(Entity changeEntity, NumericChange args);
    }

    [EntitySystem]
    public abstract class NumericHandlerSystem<T> : SystemObject, INumericHandler where T : Entity
    {
        async ETTask INumericHandler.Run(Entity self, NumericChange data)
        {
            await this.Run((T)self, data);
        }

        protected abstract ETTask Run(T self, NumericChange data);
    }

    /* 案例

    [NumericHandler(SceneType.All, NumericType.NumericTest0)]
    public class NumericHandlerDemo : NumericHandlerSystem<Unit>
    {
        protected override async ETTask Run(Unit self, NumericChange data)
        {
            Log.Error($"{data.GetNumericTypeEnum()} {data.GetAsInt()}");
            await ETTask.CompletedTask;
        }
    }

    [NumericHandler(SceneType.All, NumericType.NumericTest0)]
    标记特性 有改变时就会有通知
    1 SceneType = SceneType.None || All  表示不关心场景类型 其他则只有对应场景发送来的通知才会接收
      不建议使用All || None 你应该明确的监听某个场景的数值
    2 NumericType.NumericTest0 表示监听的数值类型
      如果不传或者填0 表示监听这个类型下的所有数值 但是注意 这个监听所有与监听指定不要有操作冲突

    NumericHandlerSystem<PlayerComponent>
    继承 NumericHandlerSystem  泛型 = PlayerComponent  继承的类必须是继承了Entity的类
    所以现在可以监听任类的数值变化 不止于 Unit

    所以原则上不要使用AEvent<Scene, NumericChange>了
    所有数值监听都应该使用案例中的方式
    */
}