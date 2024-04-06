
using UnityEditor;

namespace ET
{
    [CustomEditor(typeof(EntityMonoBehaviour))]
    public class EntityMonoBehaviourEditor : Editor
    {
        //[MenuItem("ET/Test _F8", false)]
        static void MenuItemOfCompile()
        {
            Unit unit = new Unit();
        }
        
        public override void OnInspectorGUI()
        {
            EntityMonoBehaviour entityMonoBehaviour = (EntityMonoBehaviour)target;
        }
    }
}