using System;
using System.ComponentModel;

namespace ET
{
    // 继承Object接口的用途是使类能序列化成Json格式

    // 继承IDisposable接口的用途是让你的类可以控制释放非托管资源的时间，比如文件流、数据库连接等。
    // 非托管资源是指不受垃圾回收器管理的资源，如果不及时释放，可能会造成内存泄漏或资源浪费。
    // IDisposable接口只有一个方法，就是Dispose()，你需要在这个方法中写清理非托管资源的逻辑

    // 继承ISupportInitialize接口的用途是让你的类可以在初始化时控制属性的设置顺序，比如在WinForm中的控件。
    // ISupportInitialize接口也只有两个方法，就是BeginInit()和EndInit()，你需要在这两个方法中写初始化的逻辑。

    public abstract class DisposeObject: Object, IDisposable, ISupportInitialize
    {
        // 定义一个虚方法Dispose，用于释放资源
        public virtual void Dispose()
        {
        }

        // 定义一个虚方法BeginInit，用于开始初始化对象
        public virtual void BeginInit()
        {
        }

        // 定义一个虚方法EndInit，用于结束初始化对象
        public virtual void EndInit()
        {
        }
    }
}