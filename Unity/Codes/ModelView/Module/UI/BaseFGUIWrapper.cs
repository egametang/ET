using Cysharp.Threading.Tasks;
using ET.Enums;
using FairyGUI;
using System;
using System.Reflection;
using UnityEngine;

namespace ET
{
    public abstract class BaseFGUIWrapper : MonoBehaviour
    {
        public string Name { get; set; }
        public GComponent Root { get; private set; }
        public FGUILayer Layer { get; private set; }
        #region Apis
        public void Init(GComponent root, FGUILayer layer, bool show = true)
        {
            Root = root;
            Layer = layer;
            Root.sortingOrder = (int)Layer * 10;
            BindRoot(root);
            OnCreate();
            if (show)
            {
                Show();
            }
        }
        public void Show()
        {
            Root.visible = true;
            OnShow();
        }
        public void Hide()
        {
            Root.visible = false;
            OnHide();
        }
        public void Dispose()
        {
            OnDispose();
            Root.Dispose();
        }
        #endregion

        #region Protected Apis
        protected void CloseSelf()
        {
            Dispose();
        }
        #endregion

        #region Life Cycles
        protected virtual void OnCreate()
        {

        }
        protected virtual void OnShow()
        {

        }
        protected virtual void OnHide()
        {

        }
        protected virtual void OnDispose()
        {

        }
        #endregion

        #region BindRoot

        private void BindRoot(GComponent root)
        {
            Type type = GetType();
            //遍历type对象的字段
            foreach (FieldInfo fieldInfo in type.GetFields())
            {
                var fguiFieldAttr = fieldInfo.GetCustomAttribute(typeof(FGUIFieldAttribute), false);
                var fguiWrapperAttr = fieldInfo.GetCustomAttribute(typeof(FGUIWrappedViewAttribute), false);
                if (fguiWrapperAttr != null)
                {
                    FGUIWrappedViewAttribute attribute = (FGUIWrappedViewAttribute)fguiWrapperAttr;
                    string name = fieldInfo.Name;
                    GComponent com = root;

                    if (!string.IsNullOrEmpty(attribute.name))  //如果标签的名称名称不为空，则获取标签名称
                    {
                        name = attribute.name;
                    }

                    if (!string.IsNullOrEmpty(attribute.path))  //如果标签的路径信息不为空，则获取标签路径
                    {
                        string path = attribute.path.Replace('\\', '/');
                        string[] pathSplit = path.Split('/');
                        GObject temp = com;

                        foreach (var comName in pathSplit)
                        {
                            com = temp.asCom;
                            if (com == null)
                            {
                                Log.Error($"找不到组件{comName}，不存在或者不是组件类型。 {path} , {type.Name}");
                                break;
                            }
                            temp = com.GetChild(comName);
                        }

                        if (temp == null)
                        {
                            Log.Error($"找不到组件,不存在或者不是组件类型。 {path}, {type.Name}");
                        }
                        else
                        {
                            com = temp.asCom;
                            if (com == null)
                            {
                                Log.Error($"找不到组件,不存在或者不是组件类型。 {path}, {type.Name}");
                                continue;
                            }
                        }
                    }

                    GObject gobj = com.GetChild(name);
                    if (gobj == null)
                    {
                        Log.Error($"找不到元件{name},不存在。, {type.Name}");
                        continue;
                    }

                    GComponent childCom = gobj.asCom;
                    if (gobj == null)
                    {
                        Log.Error($"找不到组件{name},不存在或者不是组件类型。, {type.Name}");
                        continue;
                    }
                    BaseFGUIWrapper customWrapper = childCom.displayObject.gameObject.GetOrAddComponent(fieldInfo.FieldType) as BaseFGUIWrapper;
                    customWrapper.Init(childCom, Layer);
                    fieldInfo.SetValue(this, childCom);
                }
                else if (fguiFieldAttr != null)
                {
                    FGUIFieldAttribute attribute = (FGUIFieldAttribute)fguiFieldAttr;
                    string name = fieldInfo.Name;
                    GComponent com = root;

                    if (!string.IsNullOrEmpty(attribute.name))
                    {
                        name = attribute.name;
                    }

                    if (!string.IsNullOrEmpty(attribute.path))
                    {
                        string path = attribute.path.Replace('\\', '/');
                        string[] pathSplit = path.Split('/');
                        GObject temp = com;
                        foreach (var comName in pathSplit)
                        {
                            com = temp.asCom;
                            if (com == null)
                            {
                                Log.Error($"找不到组件{comName}，不存在或者不是组件类型。 {path}, {type.Name}");
                                break;
                            }
                            temp = com.GetChild(comName);
                        }

                        if (temp == null)
                        {
                            Log.Error($"找不到组件,不存在或者不是组件类型。 {path}, {type.Name}");
                        }
                        else
                        {
                            com = temp.asCom;
                            if (com == null)
                            {
                                Log.Error($"找不到组件,不存在或者不是组件类型。 {path}, {type.Name}");
                                continue;
                            }
                        }
                    }
                    if (fieldInfo.FieldType == typeof(Controller))
                    {
                        Controller ctrl = com.GetController(name);
                        if (ctrl == null)
                        {
                            Log.Error($"找不到控制器{name},不存在或者不是控制器类型。, {type.Name}");
                            continue;
                        }
                        fieldInfo.SetValue(this, ctrl);
                    }
                    else if (fieldInfo.FieldType == typeof(Transition))
                    {
                        Transition tran = com.GetTransition(name);
                        if (tran == null)
                        {
                            Log.Error($"找不到动效{name},不存在或者不是动效类型。, {type.Name}");
                            continue;
                        }
                        fieldInfo.SetValue(this, tran);
                    }
                    else if (fieldInfo.FieldType == typeof(GComponent))
                    {
                        GObject gobj = com.GetChild(name);
                        if (gobj == null)
                        {
                            Log.Error($"找不到元件{name},不存在, {type.Name}");
                            continue;
                        }
                        GComponent c = gobj.asCom;
                        if (gobj == null)
                        {
                            Log.Error($"找不到组件{name},不存在或者不是组件类型, {type.Name}");
                            continue;
                        }
                        fieldInfo.SetValue(this, c);
                    }
                    else
                    {
                        GObject gObj = com.GetChild(name);
                        if (gObj != null)
                        {
                            if (gObj.GetType() != fieldInfo.FieldType)
                            {
                                Log.Error($"{type.Name}的{name}绑定失败,字段名{fieldInfo.Name},字段类型:{fieldInfo.FieldType.Name},组件类型{gObj.GetType().Name}。");
                            }
                            else
                            {
                                fieldInfo.SetValue(this, gObj);
                            }
                        }
                        else
                        {
                            Log.Error($"找不到元件{name},可能不存在, {type.Name}");
                            continue;
                        }
                    }
                }
            }
        }
        #endregion
    }
}
