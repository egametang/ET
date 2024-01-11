using Sirenix.OdinInspector;

namespace YIUIFramework
{
    [LabelText("事件参数类型")]
    public enum EUIEventParamType
    {
        [LabelText("ParamVo 泛型类")]
        ParamVo = 0,

        [LabelText("System.Object")]
        Object = 1,

        [LabelText("Bool")]
        Bool = 2,

        [LabelText("String")]
        String = 3,

        [LabelText("Int")]
        Int = 4,

        [LabelText("Float")]
        Float = 5,

        [LabelText("Vector3")]
        Vector3 = 6,

        [LabelText("List_Int")]
        List_Int = 7,

        [LabelText("List_Long")]
        List_Long = 8,

        [LabelText("List_String")]
        List_String = 9,

        [LabelText("Long")]
        Long = 10,

        [LabelText("Uint")]
        Uint = 11,

        [LabelText("Ulong")]
        Ulong = 12,

        [LabelText("Double")]
        Double = 13,

        [LabelText("Vector2")]
        Vector2 = 14,

        [LabelText("Unity.GameObject")]
        UnityGameObject = 15,
    }
}