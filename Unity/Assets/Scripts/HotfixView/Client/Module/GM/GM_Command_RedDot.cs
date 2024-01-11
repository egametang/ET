using System;
using System.Collections.Generic;
using UnityEngine;
using YIUIFramework;

namespace ET.Client
{
    [GM(EGMType.Common, 1, "打开红点调试界面")]
    public class GM_OpenReddotPanel: IGMCommand
    {
        public List<GMParamInfo> GetParams()
        {
            return new();
        }

        public async ETTask<bool> Run(Scene clientScene, ParamVo paramVo)
        {
            await YIUIMgrComponent.Inst.OpenPanelAsync<RedDotPanelComponent>();
            return true;
        }
    }
}