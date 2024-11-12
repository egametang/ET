namespace ET
{
    public sealed partial class NumericDemoConfig
    {
        //不允许直接获取这个属性  应该使用扩展方法获取 NumericDemoConfig.GetNumericData();
        public NumericData __NumericDataConfig { get; set; }
    }
}