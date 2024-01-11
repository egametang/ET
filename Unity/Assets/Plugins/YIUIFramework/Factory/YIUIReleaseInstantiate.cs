using UnityEngine;

namespace YIUIFramework
{
    /// <summary>
    /// 一般的普通UI 被创建必须调用 UIFactory.Destroy 同步释放资源
    /// 但是有些人真的会忘记 这里额外增加一个mono脚本 摧毁时自动调用
    /// 缺点就是多了一个mono脚本 肯定消耗会变高的
    /// 如果你创建的对象 你知道什么时候摧毁 就不要使用这个了
    /// 如果你真的不知道什么时候移除 或者 不想管理 也接受多余消耗 可以挂载他
    /// UIBase 类无需
    /// </summary>
    public class YIUIReleaseInstantiate: MonoBehaviour
    {
        private void OnDestroy()
        {
            YIUIFactory.Destroy(gameObject);
        }
    }
}