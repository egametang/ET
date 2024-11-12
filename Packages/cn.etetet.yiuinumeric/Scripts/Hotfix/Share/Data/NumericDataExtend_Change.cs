namespace ET
{
    /// <summary>
    /// 额外数值数据扩展
    /// 改变时
    /// </summary>
    [FriendOf(typeof(NumericDataComponent))]
    public static partial class NumericDataExtend
    {
        #region Change

        /*
         * Change 对应 += -= 传的值的正负对应
         */

        public static void Change(this NumericData self, int numericType, bool value)
        {
            self.ChangeByKey(numericType, value ? 1 : 0, true, false);
        }

        public static void Change(this NumericData self, int numericType, float value)
        {
            self.ChangeByKey(numericType, (long)(value * NumericConst.FloatRate));
        }

        public static void Change(this NumericData self, int numericType, long value)
        {
            self.ChangeByKey(numericType, value);
        }

        public static void Change(this NumericData self, int numericType, int value)
        {
            self.ChangeByKey(numericType, value);
        }

        public static void Change(this NumericData self, ENumericType numericType, bool value)
        {
            self.ChangeByKey((int)numericType, value ? 1 : 0, true, false);
        }

        public static void Change(this NumericData self, ENumericType numericType, float value)
        {
            self.ChangeByKey((int)numericType, (long)(value * NumericConst.FloatRate));
        }

        public static void Change(this NumericData self, ENumericType numericType, int value)
        {
            self.ChangeByKey((int)numericType, value);
        }

        public static void Change(this NumericData self, ENumericType numericType, long value)
        {
            self.ChangeByKey((int)numericType, value);
        }

        //跳过检查 非必要禁止使用
        public static void ChangeUnCheck(this NumericData self, ENumericType numericType, long value)
        {
            self.ChangeByKey((int)numericType, value, true, true, false);
        }

        #region ChangeNoEvent

        public static void ChangeNoEvent(this NumericData self, int numericType, bool value)
        {
            self.ChangeByKey(numericType, value ? 1 : 0, false, false);
        }

        public static void ChangeNoEvent(this NumericData self, int numericType, float value)
        {
            self.ChangeByKey(numericType, (long)(value * NumericConst.FloatRate), false);
        }

        public static void ChangeNoEvent(this NumericData self, int numericType, long value)
        {
            self.ChangeByKey(numericType, value, false);
        }

        public static void ChangeNoEvent(this NumericData self, ENumericType numericType, bool value)
        {
            self.ChangeByKey((int)numericType, value ? 1 : 0, false, false);
        }

        public static void ChangeNoEvent(this NumericData self, ENumericType numericType, float value)
        {
            self.ChangeByKey((int)numericType, (long)(value * NumericConst.FloatRate), false);
        }

        public static void ChangeNoEvent(this NumericData self, ENumericType numericType, long value)
        {
            self.ChangeByKey((int)numericType, value, false);
        }

        //跳过检查 非必要禁止使用
        public static void ChangeNoEventUnCheck(this NumericData self, ENumericType numericType, long value)
        {
            self.ChangeByKey((int)numericType, value, false, true, false);
        }

        #endregion

        #endregion
    }
}