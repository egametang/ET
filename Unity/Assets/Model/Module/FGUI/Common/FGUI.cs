using System;
using FairyGUI;

namespace ETModel
{
    public static class FGUI
    {
        public static T Open<T>() where T : FGUIBase
        {
            return FGUIManagerComponent.Instance.ShowUIForms<T>() as T;
        }

        public static T GetUI<T>() where T : FGUIBase
        {
            return FGUIManagerComponent.Instance.GetUIForms<T>() as T;
        }

        public static T GetShowingUI<T>() where T : FGUIBase
        {
            return FGUIManagerComponent.Instance.GetShowingUIForms<T>() as T;
        }

        public static void Close(Type type)
        {
            FGUIManagerComponent.Instance.CloseUIForms(type);
        }

        /// <summary>
        /// 清空ui
        /// </summary>
        public static void ClearUI()
        {
            Game.Scene.RemoveComponent<FGUIManagerComponent>();
            Game.Scene.AddComponent<FGUIManagerComponent>();
        }

        /// <summary>
        /// 提示
        /// </summary>
        /// <param name="content"></param>
        private static GComponent tip;

        public static void Tip(string content)
        {
            if (tip == null)
            {
#if UNITY_EDITOR
                UIPackage.AddPackage("UI/Common");
#endif
                tip = UIPackage.CreateObject("Common", "Tip").asCom;
            }

            tip.GetChild("textContent").asTextField.text = content;
            GRoot.inst.ShowPopup(tip);
            tip.Center();
        }
    }
}