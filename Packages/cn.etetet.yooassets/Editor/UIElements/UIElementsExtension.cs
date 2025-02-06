
namespace YooAsset.Editor
{

#if UNITY_2019
    public static partial class UnityEngine_UIElements_ListView_Extension
    {
        public static void ClearSelection(this UnityEngine.UIElements.ListView o)
        {
            o.selectedIndex = -1;
        }
    }
#endif

#if UNITY_2019 || UNITY_2020
    public static partial class UnityEngine_UIElements_ListView_Extension
    {
        public static void Rebuild(this UnityEngine.UIElements.ListView o)
        {
            o.Refresh();
        }
    }
#endif

}