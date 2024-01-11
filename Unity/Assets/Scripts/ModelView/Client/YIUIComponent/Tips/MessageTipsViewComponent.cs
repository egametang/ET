using System;
using YIUIFramework;

namespace ET.Client
{
    public partial class MessageTipsViewComponent: Entity, IYIUIOpen<ParamVo>, IYIUIOpenTween, IYIUICloseTween
    {
        public MessageTipsExtraData ExtraData;
    }

    //额外参数
    public class MessageTipsExtraData
    {
        public string ConfirmName;     //确定按钮换名字
        public Action ConfirmCallBack; //确定方法
        public string CancelName;      //取消按钮换名字
        public Action CancelCallBack;  //取消方法
        public Action CloseCallBack;   //关闭方法
    }
}