using System.Collections.Generic;

namespace ET.Client
{
    [Condition(EConditionType.Demo, true,typeof(ConditionCheckLevel))]
    public class Condition_Demo : ICondition
    {
        public async ETTask<(bool result, string errorTips)> Check(Scene scene, ConditionConfig conditionConfig, ConditionCheckValue checkValue)
        {
            await ETTask.CompletedTask;
            var result     = false;
            var checkLevel = ((ConditionCheckLevel)checkValue).Level;

            var demoComponent = scene?.GetComponent<CondititonDemoComponent>();
            if (demoComponent != null)
            {
                var demoValue = demoComponent.DemoValue;

                switch (conditionConfig.CompareType)
                {
                    case ECompareType.Equal:
                        result = demoValue == checkLevel;
                        break;
                    case ECompareType.NotEqual:
                        result = demoValue != checkLevel;
                        break;
                    case ECompareType.Less:
                        result = demoValue < checkLevel;
                        break;
                    case ECompareType.LessEqual:
                        result = demoValue <= checkLevel;
                        break;
                    case ECompareType.Greater:
                        result = demoValue > checkLevel;
                        break;
                    case ECompareType.GreaterEqual:
                        result = demoValue >= checkLevel;
                        break;
                    default:
                        result = false;
                        Log.Error($"没有这个比较类型：{conditionConfig.CompareType}");
                        break;
                }
            }

            return result ? (true, "") : (false, string.Format(conditionConfig.Tips, checkLevel));
        }
    }
}
