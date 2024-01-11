using System;
using YIUIFramework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ET.Client
{
    [FriendOf(typeof(GMParamItemComponent))]
    public static partial class GMParamItemComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this GMParamItemComponent self)
        {
        }
        
        [EntitySystem]
        private static void Awake(this GMParamItemComponent self)
        {
        }
        
        [EntitySystem]
        private static void Destroy(this GMParamItemComponent self)
        {
        }
        
        public static void ResetItem(this GMParamItemComponent self, GMParamInfo info)
        {
            self.ParamInfo       = info;
            self.u_DataParamDesc.SetValue(info.Desc);
            self.u_ComInputField.text = info.Value;
            self.u_DataTypeIsBool.SetValue(info.ParamType == EGMParamType.Bool);
            switch (info.ParamType)
            {
                case EGMParamType.String:
                    self.u_ComInputField.contentType = TMP_InputField.ContentType.Alphanumeric;
                    break;
                case EGMParamType.Bool:
                    self.u_ComToggle.isOn = !string.IsNullOrEmpty(info.Value) && info.Value != "0";
                    break;
                case EGMParamType.Float:
                    self.u_ComInputField.contentType = TMP_InputField.ContentType.DecimalNumber;
                    break;
                case EGMParamType.Int:
                case EGMParamType.Long:
                    self.u_ComInputField.contentType = TMP_InputField.ContentType.IntegerNumber;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        
        #region YIUIEvent开始
        
        private static void OnEventInputAction(this GMParamItemComponent self, string p1)
        {
            self.ParamInfo.Value = p1;
        }
        
        private static void OnEventToggleAction(this GMParamItemComponent self, bool p1)
        {
            self.ParamInfo.Value = p1? "1" : "0";
        }
        #endregion YIUIEvent结束
    }
}