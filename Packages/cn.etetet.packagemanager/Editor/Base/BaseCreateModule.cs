#if ODIN_INSPECTOR
using Sirenix.OdinInspector;

namespace ET.PackageManager.Editor
{
    /// <summary>
    /// 基类 自动创建模块
    /// 由其他模块继承后实现
    /// </summary>
    [HideReferenceObjectPicker]
    public abstract class BaseCreateModule
    {
        public virtual void Initialize()
        {
        }

        public virtual void OnDestroy()
        {
        }
    }
}
#endif
