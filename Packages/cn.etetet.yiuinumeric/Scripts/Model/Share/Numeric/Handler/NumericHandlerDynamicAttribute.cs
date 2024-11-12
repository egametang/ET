using System;

namespace ET
{
    //数值特性2.0 版本 这个响应可以直接动态的触发到对应需要监听的类中
    //目前 1.0 2.0  只能同时响应1个数值 或者 全部  并不能指定范围 
    //如果你是要自己分发那就不要使用2.0功能 直接用1.0功能就可以了
    [AttributeUsage(AttributeTargets.Class)]
    public class NumericHandlerDynamicAttribute : BaseAttribute
    {
        //触发的场景限制
        //这个比较简单 就是对应的才会响应 None / All 就是无限制
        public int SceneType { get; }

        //触发的类型限制
        //对应类型改变才会响应
        public int NumericType { get; }

        //监听的父级层数限制 默认=0层
        //这个有比较多的功能和用法
        //0层代表 任意都响应 没有完全理解的情况下请使用0
        /*
            特殊举例: Unit 挂了数值组件  还挂了一个 UI对应的UI血条组件 >> UnitHPComponent 显示层的UI
                此时数值组件响应了  所以NumericChange中的_ChangeEntity; //NumericComponent.Parent = 这个Unit

                [NumericHandlerDynamic(SceneType.Current, NumericType.HP0, 1)]
                [FriendOf(typeof(Unit))]
                public class UnitHPComponent_UnitNumericChangeEventHandler_Diamond0 : NumericHandlerDynamicSystem<UnitHPComponent, Unit, NumericChange>
                {
                    protected override async ETTask Run(UnitHPComponent self, Unit entity, NumericChange data, NumericChange data)
                    {
                        await ETTask.CompletedTask;
                    }
                }

                解读上述案例:
                NumericHandlerDynamicSystem其实就是全局的静态事件监听写在哪里都可以的推荐还是写到对应的CompentSystem里面
                泛型1 = 响应事件的具体实体  肯定是存在的
                泛型2 = 那个数值组件的父级是谁    //NumericComponent.Parent = 这个Unit
                那就是这个System注册了数值改变事件 也就是当数值组件的父级是Unit时会触发

                当InvakeParentLayerCount <= 0时 代表无需检查父级 就是不需要精准响应
                任何Unit的数值改变都会来 则实现中需要自己对unit进行判断处理

                如果你满足结构 你的响应实体是挂在Unit下的 那么相对来说UnitHPComponent的Parent就是Unit 那么就有1层Parent
                如果你特性填 InvakeParentLayerCount = 1
                那么此消息就无需检查到底是哪个Unit的血量改变了一定是你的 好处我就不用判断这个血量改变是不是我的Unit才刷新了会少很多次响应
                (这个少响应很关键 首先是异步响应WaitAll 假设有非常多Unit单位 比如1W个 其中一个血量变了 这里就需要创建1W个Task 其中9999都是 if(是不是我这个Unit)然后retun)
                (那么这多余的都是消耗掉的 如果有这个精准响应者可以减少非常多的麻烦 所以能使用精准响应时一定要用)
                前提就是 UnitHPComponent的Parent就是Unit 或者parent.parent.parent 都可以只要在一条线上 填对应的parent层数即可 一般都是1
                (尽可能的吧这种响应与数值组件挂在同一条线上)
         */
        public int InvakeParentLayerCount { get; } //非0时精准响应

        /// <summary>
        /// 监听指定的数值类型
        /// </summary>
        /// <param name="sceneType">触发的场景限制</param>
        /// <param name="numericType">触发的类型限制</param>
        /// <param name="invakeParentLayerCount">监听的父级层数限制 默认=0层 全响应 没有完全理解的情况下请使用0 (非0时精准响应)</param>  
        public NumericHandlerDynamicAttribute(int sceneType, ENumericType numericType, int invakeParentLayerCount = 0)
        {
            this.SceneType              = sceneType;
            this.NumericType            = (int)numericType;
            this.InvakeParentLayerCount = invakeParentLayerCount;
        }

        //监听所有数值类型 注意这种监听所有一般用于服务器等大的分发之类的
        //一般情况下都不会使用这个方式
        public NumericHandlerDynamicAttribute(int sceneType)
        {
            this.SceneType   = sceneType;
            this.NumericType = 0;
        }
    }
}
