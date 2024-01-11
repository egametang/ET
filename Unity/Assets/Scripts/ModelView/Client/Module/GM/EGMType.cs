using Sirenix.OdinInspector;

namespace ET.Client
{
    //GM命令分类
    //将会根据枚举生成页签
    //LabelText特性标记转换为显示名称
    public enum EGMType
    {
        [GMGroup("公共")]
        Common = 1,

        [GMGroup("案列")]
        Test = 999,
    }
}