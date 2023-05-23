using UnityEngine;

namespace ET
{
    
    [ObjectSystem]
    public class UIBaseWindowAwakeSystem : AwakeSystem<UIBaseWindow>
    {
        protected override void Awake(UIBaseWindow self)
        {
            self.WindowData = self.AddChild<WindowCoreData>();
            self.IsInStackQueue = false;
        }
    }
    
    [ObjectSystem]
    public class UIBaseWindowDestroySystem : DestroySystem<UIBaseWindow>
    {
        protected override void Destroy(UIBaseWindow self)
        {
            self.WindowData?.Dispose();
            self.WindowID = WindowID.WindowID_Invaild;
            self.IsInStackQueue = false;
            if (self.UIPrefabGameObject != null)
            {
                GameObject.Destroy(self.UIPrefabGameObject);
                self.UIPrefabGameObject = null;
            }
        }
    }
    
    
    
    public  static class UIBaseWindowSystem  
    {
        public static void SetRoot(this UIBaseWindow self, Transform rootTransform)
        {
            if(self.uiTransform == null)
            {
                Log.Error($"uibaseWindows {self.WindowID} uiTransform is null!!!");
                return;
            }
            if(rootTransform == null)
            {
                Log.Error($"uibaseWindows {self.WindowID} rootTransform is null!!!");
                return;
            }
            self.uiTransform.SetParent(rootTransform, false);
            self.uiTransform.transform.localScale = Vector3.one;
        }
    }
}
