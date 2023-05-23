using System;
using ET.Client;
using UnityEngine;

namespace ET
{
    [FriendOf(typeof(GlobalComponent))]
    public static class EUIRootHelper
    {
        public static void Init()
        {
          
        }
        
        public static Transform GetTargetRoot(UIWindowType type)
        {
            if (type == UIWindowType.Normal)
            {
                return GlobalComponent.Instance.NormalRoot;
            }
            else if (type == UIWindowType.Fixed)
            {
                return GlobalComponent.Instance.FixedRoot;
            }
            else if (type == UIWindowType.PopUp)
            {
                return GlobalComponent.Instance.PopUpRoot;
            }
            else if (type == UIWindowType.Other)
            {
                return GlobalComponent.Instance.OtherRoot;
            }

            Log.Error("uiroot type is error: " + type.ToString());
            return null;
        }
    }
}