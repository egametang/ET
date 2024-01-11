using Sirenix.OdinInspector;

namespace YIUIFramework
{
    [LabelText("类型")]

    //禁止修改ID
    //因为涉及到Mask 所以最高只有32个 请注意
    //@lsy
    public enum EUIBindDataType
    {
        [LabelText("Bool")]
        Bool = 0,

        [LabelText("String")]
        String = 1,

        [LabelText("Int")]
        Int = 2,

        [LabelText("Float")]
        Float = 3,

        [LabelText("Vector3")]
        Vector3 = 4,

        [LabelText("List_Int")]
        List_Int = 5,

        [LabelText("List_Long")]
        List_Long = 6,

        [LabelText("List_String")]
        List_String = 7,

        [LabelText("Long")]
        Long = 8,

        [LabelText("Uint")]
        Uint = 9,

        [LabelText("Ulong")]
        Ulong = 10,

        [LabelText("Double")]
        Double = 11,

        [LabelText("Vector2")]
        Vector2 = 12,

        [LabelText("Color")]
        Color = 13,
    }
}