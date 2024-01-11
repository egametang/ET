using System.Collections.Generic;
using UnityEngine;
using YIUIFramework;

namespace ET.Client
{
    [GM(EGMType.Test, 1, "弹窗测试-消息弹窗")]
    public class GM_TipsTest1: IGMCommand
    {
        public List<GMParamInfo> GetParams()
        {
            return new()
            {
                new GMParamInfo(EGMParamType.String, "消息内容"),
            };
        }

        public async ETTask<bool> Run(Scene clientScene, ParamVo paramVo)
        {
            TipsHelper.OpenSync<MessageTipsViewComponent>(paramVo);
            await ETTask.CompletedTask;
            return true;
        }
    }
    
    [GM(EGMType.Test, 1, "弹窗测试-消息回调弹窗")]
    public class GM_TipsTest2: IGMCommand
    {
        public List<GMParamInfo> GetParams()
        {
            return new();
        }

        public async ETTask<bool> Run(Scene clientScene, ParamVo paramVo)
        {
            TipsHelper.Open<MessageTipsViewComponent>("回调测试",new MessageTipsExtraData()
            {
                ConfirmCallBack = () =>
                {
                    Debug.LogError($"回调测试, 确定按钮");
                },
                CancelCallBack = () =>
                {
                    Debug.LogError($"回调测试, 取消按钮");
                }
            }).Coroutine();
            await ETTask.CompletedTask;
            return true;
        }
    }
    
    [GM(EGMType.Test, 1, "弹窗测试-消息回调弹窗 只有确定")]
    public class GM_TipsTest3: IGMCommand
    {
        public List<GMParamInfo> GetParams()
        {
            return new();
        }

        public async ETTask<bool> Run(Scene clientScene, ParamVo paramVo)
        {
            TipsHelper.Open<MessageTipsViewComponent>("只有确定 回调测试",new MessageTipsExtraData()
            {
                ConfirmCallBack = () =>
                {
                    Debug.LogError($"回调测试, 确定按钮");
                }
            }).Coroutine();
            await ETTask.CompletedTask;
            return true;
        }
    }
    
    [GM(EGMType.Test, 1, "弹窗测试-消息回调弹窗 修改按钮名称")]
    public class GM_TipsTest4: IGMCommand
    {
        public List<GMParamInfo> GetParams()
        {
            return new();
        }

        public async ETTask<bool> Run(Scene clientScene, ParamVo paramVo)
        {
            TipsHelper.Open<MessageTipsViewComponent>("回调测试 修改按钮名称",new MessageTipsExtraData()
            {
                ConfirmCallBack = () =>
                {
                    Debug.LogError($"回调测试, 确定按钮");
                },
                CancelCallBack = () =>
                {
                    Debug.LogError($"回调测试, 取消按钮");
                },
                CloseCallBack = () =>
                {
                    Debug.LogError($"回调测试, 关闭按钮");
                },
                ConfirmName = "Confirm",
                CancelName = "Cancel"
            }).Coroutine();
            await ETTask.CompletedTask;
            return true;
        }
    }
    
    [GM(EGMType.Test, 1, "弹窗测试-飘字消息")]
    public class GM_TipsTest5: IGMCommand
    {
        public List<GMParamInfo> GetParams()
        {
            return new()
            {
                new GMParamInfo(EGMParamType.String, "消息内容"),
            };
        }

        public async ETTask<bool> Run(Scene clientScene, ParamVo paramVo)
        {
            TipsHelper.OpenSync<TextTipsViewComponent>(paramVo);
            await ETTask.CompletedTask;
            return true;
        }
    }
}