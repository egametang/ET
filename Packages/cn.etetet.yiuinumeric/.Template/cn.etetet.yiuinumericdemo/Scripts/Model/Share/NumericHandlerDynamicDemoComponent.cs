namespace ET
{
    // 动态数字监视器Demo组件
    [ComponentOf(typeof(Unit))]
    public class NumericHandlerDynamicDemoComponent : Entity, IAwake, INumericHandlerDynamic<Unit, NumericChange>
    {
        public string TestValue;
    }
}
