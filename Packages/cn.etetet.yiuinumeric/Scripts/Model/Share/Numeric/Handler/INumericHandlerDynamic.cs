using System;

namespace ET
{
    public interface INumericHandlerDynamic
    {
        ETTask Run(Entity watcherEntity, Entity changeEntity, NumericChange data);
    }

    public interface INumericHandlerDynamic<T2> where T2 : Entity
    {
    }

    public interface INumericHandlerDynamic<T2, T3> : IClassEvent<T3> where T2 : Entity where T3 : struct
    {
    }

    public interface INumericHandlerDynamicSystem<T1, T2, T3> : ISystemType, INumericHandlerDynamic where T1 : Entity where T2 : Entity where T3 : struct
    {
    }

    [EntitySystem]
    public abstract class NumericHandlerDynamicSystem<T1, T2, T3> : ClassEventSystem<T1, T3>, INumericHandlerDynamicSystem<T1, T2, T3>
    where T1 : Entity, INumericHandlerDynamic<T2, T3> where T2 : Entity where T3 : struct
    {
        Type ISystemType.Type()
        {
            return typeof(T1);
        }

        Type ISystemType.SystemType()
        {
            return typeof(INumericHandlerDynamic<T2>);
        }

        protected override void Handle(Entity e, T3 t)
        {
            throw new NotImplementedException();
        }

        public async ETTask Run(Entity watcherEntity, Entity changeEntity, NumericChange data)
        {
            await this.Run((T1)watcherEntity, (T2)changeEntity, data);
        }

        protected abstract ETTask Run(T1 self, T2 entity, NumericChange data);
    }

    /* 案例

    1. 需要监听的组件 Component : INumericHandlerDynamic<Unit, NumericChange>
    泛型1: 数值组件挂在谁身上,
    泛型2: 固定NumericChange  写其他的无效
    (固定了为什么还要写一下能不能省略)
    (没办法省略 设计上需要)
    [ComponentOf(typeof(Unit))]
    public class NumericHandlerDynamicDemoComponent : Entity, IAwake, INumericHandlerDynamic<Unit, NumericChange>
    {
    }


    代表动态监听目标数值变化
    会把变化的消息直接发往对应的监听者中
    泛型1 = 监听的那个类 比如某个UI的component  消息来了就会直接发到这个实体类
    泛型2 = 数值系统挂载的父级是谁   一般是unit  player这种
    泛型3 = 固定NumericChange  写其他的无效
    当触发时 就会直接调用到对应的实例中
    一般用于UI刷新 等等
    [NumericHandlerDynamic(SceneType.Current, NumericType.AOI0)]
    [FriendOf(typeof(NumericHandlerDynamicDemoComponent))]
    public class UnitNumericChangeEventHandler_AOI0 : NumericHandlerDynamicSystem<NumericHandlerDynamicDemoComponent, Unit, NumericChange>
    {
        protected override async ETTask Run(NumericHandlerDynamicDemoComponent self, Unit entity, NumericChange data)
        {
            //注意这里来的动态消息是 任意Unit 都会来
            //如果你想要监听指定的某个Unit
            //1 提前存一下然后判断2个是不是相同 如果是则XX
            //2 使用动态监听中的定向监听功能
            Log.Error($"收到动态数值监听: {self.TestValue} {data.GetNumericTypeEnum()} {data.GetAsInt()}");
            await ETTask.CompletedTask;
        }
    }

     */
}
