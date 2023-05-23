using UnityEngine;

namespace ET
{
    [ChildOf(typeof(WindowCoreData))]
    public class UIBaseWindow : Entity,IAwake, IDestroy
    {
        public bool IsPreLoad
        {
            get
            {
                return this.UIPrefabGameObject != null ;
            }
        }
        
        public Transform uiTransform
        {
            get
            {
                if (null != this.UIPrefabGameObject)
                {
                    return this.UIPrefabGameObject.transform;
                }
                return null;
            }
        }
        
        public WindowID WindowID
        {
            get
            {
                if (this.m_windowID == WindowID.WindowID_Invaild)
                {
                    Debug.LogError("window id is " + WindowID.WindowID_Invaild);
                }
                return m_windowID;
            }
            set { m_windowID = value; }
        }
        
        public bool IsInStackQueue
        {
            get;
            set;
        }
                
        
        public WindowID m_windowID           = WindowID.WindowID_Invaild;
        public GameObject UIPrefabGameObject = null;
        public WindowCoreData WindowData     = null;
    }
}