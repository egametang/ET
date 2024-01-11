using System.Collections.Generic;
using YIUIFramework;

namespace ET.Client
{
    //GM命令接口
    public interface IGMCommand
    {
        //返回这个命令所需参数列表
        List<GMParamInfo> GetParams();
        //执行命令方法  返回值=执行完毕后是否关闭GM面板
        ETTask<bool>      Run(Scene clientScene, ParamVo paramVo);
    }
}