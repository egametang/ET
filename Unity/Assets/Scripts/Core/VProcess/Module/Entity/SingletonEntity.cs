using System;

namespace ET
{
    public class SingletonEntity<T>: Entity where T : SingletonEntity<T>
    {
        public static T Instance
        {
            get
            {
                return VProcess.Instance.GetInstance<T>();
            }
        }

        public SingletonEntity()
        {
            VProcess.Instance.AddInstance((T)this);
        }

        public override void Dispose()
        {
            base.Dispose();

            VProcess.Instance.RemoveInstance(typeof(T));
        }
    }
}