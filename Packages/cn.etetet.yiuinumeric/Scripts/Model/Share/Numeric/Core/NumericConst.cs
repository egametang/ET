namespace ET
{
    public static class NumericConst
    {
        // 数值系统内部精度 float与long转换倍率
        public const float FloatRate = 10000f;
        public const int IntRate = 10000;
        //设计中N的范围 = 1 - 6; RangeMax 最高=9;
        public const int RangeMax = 6;
        //最小ID 因为倍率是10 如果低于这个ID 那倍率后的ID 会比Max小 会计算错误
        public const int Min = 100000;
        //最大基础ID 倍率影响 才可限制
        public const int Max = 1000000;
        //[Min - Max] = 这个数值的最终值 是不允许直接修改的
        //只能修改[min*10+1 - max*10+6] 这个范围的值
        //且个位数必须是 1-RangeMax; RangeMax 最高=9;
        public const int ChangeMin = Min * 10 + 1;
        public const int ChangeMax = Max * 10 + RangeMax;
    }
}