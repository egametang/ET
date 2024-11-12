namespace ET
{
    public static class NumericDemoConfig_Extend
    {
        public static NumericData GetNumericData(this NumericDemoConfig config)
        {
            return config.__NumericDataConfig ??= config.Numeric.CreateNumericData();
        }
    }
}
