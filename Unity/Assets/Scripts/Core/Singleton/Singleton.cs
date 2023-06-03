using System;

namespace ET
{
    // 定义一个接口 ISingleton，用于实现单例模式
    public interface ISingleton : IDisposable
    {
        // 注册单例对象
        void Register();

        // 销毁单例对象
        void Destroy();

        // 判断单例对象是否已经被销毁
        bool IsDisposed();
    }

    // 定义一个抽象类 Singleton<T>，继承自 ISingleton 接口，用于创建泛型的单例对象
    public abstract class Singleton<T> : ISingleton where T : Singleton<T>, new()
    {
        // 单例对象是否已经被销毁
        private bool isDisposed;

        // 存储单例对象的实例
        [StaticField]
        private static T instance;

        // 一个静态属性，用于获取或设置单例对象的实例
        public static T Instance
        {
            get
            {
                return instance;
            }
        }

        // 实现 ISingleton 接口的 Register 方法，用于注册单例对象
        void ISingleton.Register()
        {
            // 如果单例对象已经存在，则抛出异常
            if (instance != null)
            {
                throw new Exception($"singleton register twice! {typeof(T).Name}");
            }

            // 否则将当前对象赋值给单例对象
            instance = (T)this;
        }

        // 实现 ISingleton 接口的 Destroy 方法，用于销毁单例对象
        void ISingleton.Destroy()
        {
            // 如果单例对象已经被销毁，则直接返回
            if (this.isDisposed)
            {
                return;
            }

            // 否则将 isDisposed 设为 true，并调用 Dispose 方法释放资源，然后将单例对象设为 null
            this.isDisposed = true;

            instance.Dispose();
            instance = null;
        }

        // 实现 ISingleton 接口的 IsDisposed 方法，用于判断单例对象是否已经被销毁
        bool ISingleton.IsDisposed()
        {
            return this.isDisposed;
        }

        // 定义一个虚方法 Dispose，用于释放资源，可以在子类中重写
        public virtual void Dispose()
        {
        }
    }
}
