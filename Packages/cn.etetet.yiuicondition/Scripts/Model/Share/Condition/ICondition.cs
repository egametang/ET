using System.Collections.Generic;

namespace ET
{
    //条件接口
    public interface ICondition
    {
        ETTask<(bool result, string errorTips)> Check(Scene clientScene, ConditionConfig conditionConfig, ConditionCheckValue checkValue);
    }
}
