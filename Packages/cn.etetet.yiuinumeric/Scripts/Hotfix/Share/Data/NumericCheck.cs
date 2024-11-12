namespace ET
{
    /// <summary>
    /// 数值公式 数值类型对应关系
    /// 扩展对外API
    /// </summary>
    public static class NumericCheck
    {
        /// <summary>
        /// 检查修改数值的合理性
        /// </summary>
        public static bool CheckChangeNumeric(this ENumericType numericType)
        {
            var numericId = (int)numericType;

            //[Min - Max] = 这个数值的最终值 是不允许直接修改的
            if (numericId is >= NumericConst.Min and <= NumericConst.Max)
            {
                //最新版 0可以修改 前提是非成长
                //最新版非成长 只有0这个一个数据了  没有0之后的任何数据
                if (numericType.IsNotGrowNumeric())
                {
                    return true;
                }

                Log.Error($"不允许直接修改最终数据 {numericType}");
                return false;
            }

            //只能修改[min*10+1 - max*10+6] 这个范围的值
            if (numericId is < NumericConst.ChangeMin or > NumericConst.ChangeMax)
            {
                Log.Error($"只能修改[{NumericConst.ChangeMin} - {NumericConst.ChangeMax}] 这个范围的值 当前={numericId}");
                return false;
            }

            //且个位数必须是 1-RangeMax; RangeMax 最高=9;
            var mod = numericId % 10;
            if (mod is <= 0 or > NumericConst.RangeMax)
            {
                Log.Error($"不合法的ID 个位数必须是=( 1 - {mod > NumericConst.RangeMax}) 当前={numericId}");
                return false;
            }

            //如果检查到这个值是非成长的 说明你不能修改他
            if (numericType.IsNotGrowNumeric())
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 检查修改数值的合理性
        /// </summary>
        public static bool CheckChangeNumeric(this int numericType)
        {
            return CheckChangeNumeric((ENumericType)numericType);
        }

        /// <summary>
        /// 检查当前目标数值是否是目标类型
        /// </summary>
        /// <param name="numericType>被检查数值</param>
        /// <param name="valueType">输入的类型</param>
        /// <returns>是否与登记时匹配</returns>
        public static bool CheckGetNumeric(this ENumericType numericType, ENumericValueType valueType)
        {
            /*
             * 由于数值系统特性 3 5 一定是float类型  因为他们都是结果增加百分比
             * 其他值 = 设定的数值类型
             * 这里已经不需要进行判断到底使用什么类型 将会由自动生成代码自动对应
             */

            var checkCfg = numericType.GetCheckConfig();
            if (checkCfg == null) return false;

            //与配置中的类型是否一致
            if (checkCfg.Check == valueType) return true;

            Log.Error($"请注意目标 [{numericType}] 是[{checkCfg.Check}]类型  !!!但是使用的是 [{valueType}]类型");
            return false;
        }

        /// <summary>
        /// 根据数值类型 获得值的类型
        /// </summary>
        /// <param name="numericType></param>
        /// <returns></returns>
        public static ENumericValueType GetNumericValueType(this ENumericType numericType)
        {
            var checkCfg = numericType.GetCheckConfig();
            if (checkCfg == null)
            {
                Log.Error($"请注意目标 [{numericType}] 没有找到Check配置 无法判断 ValueType ");
                return ENumericValueType.None;
            }

            return checkCfg.Check;
        }

        /// <summary>
        /// 检查当前目标数值是否是目标类型
        /// </summary>
        public static bool CheckGetNumeric(this int numericType, ENumericValueType valueType)
        {
            return CheckGetNumeric((ENumericType)numericType, valueType);
        }

        //获取目标数值的 检查配置档
        public static NumericValueCheckConfig GetCheckConfig(this ENumericType numericType)
        {
            if (NumericValueCheckConfigCategory.Instance == null) return null;
            if (NumericValueCheckConfigCategory.Instance.DataMap.TryGetValue(numericType, out var checkCfg))
                return checkCfg;
            Log.Error($"请注意目标 [{numericType}] 是非成长类型  !!!由公式计算得出 你不应该操作这个数据!!!");
            return null;
        }

        //判断当前数值是不是非成长数值
        public static bool IsNotGrowNumeric(this ENumericType numericType)
        {
            var checkCfg = numericType.GetCheckConfig();

            //1 没有配置说明他是非成长  且这个值永=0
            //2 配置判断是非成长
            return checkCfg == null || checkCfg.NotGrow;
        }

        public static bool IsNotGrowNumeric(this int numericType)
        {
            return IsNotGrowNumeric((ENumericType)numericType);
        }

        /// <summary>
        /// 根据这个ID 来获取他的最终值枚举
        /// </summary>
        public static ENumericType GetNumericFinalEnum(this ENumericType numericType)
        {
            var numericId = (int)numericType;

            if (numericId is >= NumericConst.Min and <= NumericConst.Max)
            {
                return numericType;
            }

            if (numericId is >= NumericConst.ChangeMin and <= NumericConst.ChangeMax)
            {
                var final = numericId / 10;
                return (ENumericType)final;
            }

            //因为枚举是自动化生成的 所以不太可能是非法的 但是万一呢...
            Log.Error($"这是一个非法数值 请检查 {numericType}");
            return numericType;
        }

        /// <summary>
        /// 根据这个ID 来获取他的最终值枚举
        /// </summary>
        public static ENumericType GetNumericFinalEnum(this int numericType)
        {
            return GetNumericFinalEnum((ENumericType)numericType);
        }

        /// <summary>
        /// 根据ID 获取数值定义类型
        /// </summary>
        public static ENumericDefinitionType GetNumericDefinitionType(this ENumericType numericType)
        {
            return GetNumericDefinitionType((int)numericType);
        }

        /// <summary>
        /// 根据ID 获取数值定义类型
        /// </summary>
        public static ENumericDefinitionType GetNumericDefinitionType(this int numericId)
        {
            if (numericId is >= NumericConst.Min and <= NumericConst.Max)
            {
                return ENumericDefinitionType.Result;
            }

            if (numericId is >= NumericConst.ChangeMin and <= NumericConst.ChangeMax)
            {
                var mod = numericId % 10;
                if (mod is <= 0 or > NumericConst.RangeMax)
                {
                    Log.Error($"不合法的ID 个位数必须是=( 1 - {mod > NumericConst.RangeMax}) 当前={numericId}");
                    return ENumericDefinitionType.None;
                }

                return (ENumericDefinitionType)mod;
            }

            //因为枚举是自动化生成的 所以不太可能是非法的 但是万一呢...
            Log.Error($"这是一个非法数值 请检查 {numericId}");

            return ENumericDefinitionType.None;
        }
    }
}