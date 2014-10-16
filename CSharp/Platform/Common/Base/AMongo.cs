namespace Common.Base
{
    /// <summary>
    /// 需要序列化并且需要存储 Key Value 数据的类继承此抽象类
    /// </summary>
    public abstract class AMongo: Object, IMongo
    {
        public virtual void BeginInit()
        {
        }

        public virtual void EndInit()
        {
        }
    }
}