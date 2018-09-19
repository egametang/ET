#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace ETModel
{
    /// <summary>
    /// ET组件可视化
    /// </summary>
    public class ECSView : MonoBehaviour
    {
        #region Static Parts
        /// <summary>
        /// 组件与其对应可视化对象map
        /// </summary>
        private static DoubleMap<object, ECSView> _dic = new DoubleMap<object, ECSView>();

        private static Transform root;
        /// <summary>
        /// 可视化对象根节点
        /// </summary>
        private static Transform Root
        {
            get
            {
                if (root == null)
                {
                    root = new GameObject("ETViewRoot").transform;
                    DontDestroyOnLoad(root);
                }
                return root;
            }
        }
        private static Transform pool;
        /// <summary>
        /// 组件放入Pool的可视化根节点
        /// </summary>
        private static Transform Pool
        {
            get
            {
                if (pool == null)
                {
                    pool = new GameObject("Pool").transform;
                    pool.parent = Root;
                }
                return pool;
            }
        }
        /// <summary>
        /// 创建组件的可视化节点
        /// </summary>
        /// <param name="self"></param>
        public static void CreateView(object self)
        {
            if (!Define.IsEditorMode)
                return;
            if (_dic.ContainsKey(self))
                return;
            ECSView view = new GameObject(self.GetType().ToString()).AddComponent<ECSView>();
            view.Component = self;
            _dic.Add(self, view);
            SetParent(self);
        }
        /// <summary>
        /// 销毁组件的可视化节点
        /// </summary>
        /// <param name="self"></param>
        public static void DestroyView(object self)
        {
            if (!Define.IsEditorMode)
                return;
            if (_dic.ContainsKey(self))
            {
                ECSView view = _dic.GetValueByKey(self);
                if (view != null)
                    DestroyImmediate(view.gameObject);
                _dic.RemoveByKey(self);
            }
        }
        /// <summary>
        /// 根据组件获取可视化节点
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static ECSView GetView(object self)
        {
            if (!Define.IsEditorMode)
                return null;
            if (_dic.ContainsKey(self))
                return _dic.GetValueByKey(self);
            return null;
        }
        /// <summary>
        /// 根据可视化节点获取其组件
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static object GetComponent(ECSView self)
        {
            if (!Define.IsEditorMode)
                return null;
            if (_dic.ContainsValue(self))
                return _dic.GetKeyByValue(self);
            return null;
        }
        /// <summary>
        /// 放入Pool操作，修改可视化节点到Pool节点下
        /// </summary>
        /// <param name="self"></param>
        public static void ReturnPool(object self)
        {
            if (!Define.IsEditorMode)
                return;
            if (self == null)
                return;
            ECSView selfView = GetView(self);
            if (selfView == null)
            {
                _dic.RemoveByKey(self);
                return;
            }
            selfView.transform.parent = Pool;
        }
        /// <summary>
        /// 设置可视化父对象
        /// </summary>
        /// <param name="self"></param>
        /// <param name="parent"></param>
        public static void SetParent(object self, object parent = null)
        {
            if (!Define.IsEditorMode)
                return;
            if (self == null)
                return;
            ECSView selfView = GetView(self);
            if (selfView == null)
            {
                _dic.RemoveByKey(self);
                return;
            }
            ECSView parentView = GetView(parent);
            if (parentView != null)
                selfView.transform.parent = parentView.transform;
            else
                selfView.transform.parent = Root;
        }

        #endregion

        /// <summary>
        /// 该可视化节点对应的组件，对其属性显示到Inspector视图内
        /// </summary>
        public object Component;
    }

#if UNITY_EDITOR
    [InitializeOnLoad]
    public class MyHierarchyEditor
    {
        static MyHierarchyEditor()
        {
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
        }

        private static void OnHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        {
            GameObject obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (obj == null)
                return;
            if (obj.GetComponent<ECSView>() != null)
            {
                GUIStyle style = new GUIStyle(){
                    padding ={ left =EditorStyles.label.padding.left-1, top = EditorStyles.label.padding.top },
                    normal ={ textColor =Color.red }
                };
                GUI.Box(selectionRect, GUIContent.none);
                GUI.Label(selectionRect, obj.name, style);
            }
        }
    }
#endif
}
