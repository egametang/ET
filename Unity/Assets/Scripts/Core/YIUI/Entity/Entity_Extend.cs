using System;

namespace ET
{
    /// <summary>
    /// 为YIUI扩展的
    /// 主要是无法使用泛型的创建 用于YIUI添加能保证一定是Entity
    /// </summary>
    public partial class Entity
    {
        public Entity AddYIUIChild(Type childType, bool isFromPool = false)
        {
            var component = Create(childType, isFromPool);
            component.Id     = IdGenerater.Instance.GenerateId();
            component.Parent = this;
            EntitySystemSingleton.Instance.Awake(component);
            return component;
        }

        public Entity AddYIUIChild<A>(Type childType, A a, bool isFromPool = false)
        {
            var component = Create(childType, isFromPool);
            component.Id     = IdGenerater.Instance.GenerateId();
            component.Parent = this;
            EntitySystemSingleton.Instance.Awake(component,a);
            return component;
        }
        
        //自定义扩展 获取不到就报错 不管你log是true 还是false
        //不想报错就别调用这个方法
        public K GetComponent<K>(bool log) where K : Entity
        {
            if (this.components == null)
            {
                return null;
            }
            
            Entity component;
            if (!this.components.TryGetValue(this.GetLongHashCode(typeof (K)), out component))
            {
                Log.Error($"{this.GetType().Name} 目标没有这个组件 {typeof (K).Name}");
                return default;
            }
            
            // 如果有IGetComponent接口，则触发GetComponentSystem
            if (this is IGetComponentSys)
            {
                EntitySystemSingleton.Instance.GetComponentSys(this, typeof(K));
            }

            return (K) component;
        }
    }
}