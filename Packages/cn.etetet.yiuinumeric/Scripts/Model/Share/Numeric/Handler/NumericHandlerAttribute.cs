using System;

namespace ET
{
    //数值特性1.0 版本 可以响应到对应类型的数值变化
    [AttributeUsage(AttributeTargets.Class)]
    public class NumericHandlerAttribute : BaseAttribute
    {
        public int SceneType { get; }

        public int NumericType { get; }

        //监听指定的数值类型 一次只能监听一个 有多个监听就要写多个
        public NumericHandlerAttribute(int sceneType, ENumericType numericType)
        {
            this.SceneType   = sceneType;
            this.NumericType = (int)numericType;
        }

        //监听所有数值类型 注意这种监听所有一般用于服务器等大的分发之类的
        //一般情况下都不会使用这个方式
        public NumericHandlerAttribute(int sceneType)
        {
            this.SceneType   = sceneType;
            this.NumericType = 0;
        }
    }
}