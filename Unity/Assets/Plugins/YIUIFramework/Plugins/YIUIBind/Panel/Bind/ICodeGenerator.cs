#if UNITY_EDITOR
using System.Text;

namespace YIUIFramework
{
    /// <summary>
    /// 用于EDITOR时，使用Get取得数据列表
    /// 编译时，使用Get and GenCode来生成同类型代码
    /// 然后使用新生成代码里的Get来得到数据列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICodeGenerator<T>
    {
        T[] Get();

        /// <summary>
        /// 生成数据代码，仅能在editor里执行
        /// 在方法里，只要写单项的属性设置代码就行了。
        /// 而要设置的代码名默认叫"v";
        /// </summary>
        /// <param name="info"></param>
        /// <param name="sb">用于写代码的SB</param>
        /// <returns></returns>
        void WriteCode(T info, StringBuilder sb);

        /// <summary>
        /// 可能遇到他是个单例不是new的情况 所以这里可以重写
        /// </summary>
        /// <param name="info"></param>
        /// <param name="sb"></param>
        void NewCode(T info, StringBuilder sb);
    }
}
#endif