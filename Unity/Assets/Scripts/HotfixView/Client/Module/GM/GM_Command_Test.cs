using System.Collections.Generic;
using UnityEngine;
using YIUIFramework;

namespace ET.Client
{
    //1 所在页签
    //2 命令等级 当执行人等级不足时无法使用
    //3 命令名称 页面上显示的名字
    //4 命令描述 有描述时显示描述 没有时不显示
    [GM(EGMType.Test, 1, "测试案列", "测试案列描述.....")]
    public class GM_Test: IGMCommand
    {
        public List<GMParamInfo> GetParams()
        {
            return new()
            {
                //目前有5种参数类型可选 长度无限
                //根据需求这里设定对应参数类型与描述 将在页面上显示
                new GMParamInfo(EGMParamType.String, "参数1 字符串"),
                new GMParamInfo(EGMParamType.Bool, "参数2 布尔"),
                new GMParamInfo(EGMParamType.Float, "参数3 小数"),
                new GMParamInfo(EGMParamType.Int, "参数4 整数"),
                new GMParamInfo(EGMParamType.Long, "参数5 64整数"),
            };
        }

        public async ETTask<bool> Run(Scene clientScene, ParamVo paramVo)
        {
            //通过paramvo 可以取出所有参数
            var paramString = paramVo.Get<string>(0);
            var paramBool   = paramVo.Get<bool>(1);
            var paramFloat  = paramVo.Get<float>(2);
            var paramInt    = paramVo.Get<int>(3);
            var paramLong   = paramVo.Get<long>(4);

            //最后根据参数自行处理命令
            Debug.LogError(paramString);
            Debug.LogError(paramBool);
            Debug.LogError(paramFloat);
            Debug.LogError(paramInt);
            Debug.LogError(paramLong);

            await ETTask.CompletedTask;

            return true;
        }
    }
}