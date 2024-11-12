namespace ET
{
    [FriendOf(typeof(NumericDataComponent))]
    public static partial class NumericDataComponentSystem
    {
        #region Change

        /*
         * Change 对应 += -= 传的值的正负对应
         */

        public static void Change(this NumericDataComponent self, int numericType, bool value)
        {
            self.NumericData.Change(numericType, value);
        }

        public static void Change(this NumericDataComponent self, int numericType, float value)
        {
            self.NumericData.Change(numericType, value);
        }

        public static void Change(this NumericDataComponent self, int numericType, long value)
        {
            self.NumericData.Change(numericType, value);
        }

        public static void Change(this NumericDataComponent self, int numericType, int value)
        {
            self.NumericData.Change(numericType, value);
        }

        public static void Change(this NumericDataComponent self, ENumericType numericType, bool value)
        {
            self.NumericData.Change(numericType, value);
        }

        public static void Change(this NumericDataComponent self, ENumericType numericType, float value)
        {
            self.NumericData.Change(numericType, value);
        }

        public static void Change(this NumericDataComponent self, ENumericType numericType, int value)
        {
            self.NumericData.Change(numericType, value);
        }

        public static void Change(this NumericDataComponent self, ENumericType numericType, long value)
        {
            self.NumericData.Change(numericType, value);
        }

        //跳过检查 非必要禁止使用
        public static void ChangeUnCheck(this NumericDataComponent self, ENumericType numericType, long value)
        {
            self.NumericData.ChangeUnCheck(numericType, value);
        }

        #region ChangeNoEvent

        public static void ChangeNoEvent(this NumericDataComponent self, int numericType, bool value)
        {
            self.NumericData.ChangeNoEvent(numericType, value);
        }

        public static void ChangeNoEvent(this NumericDataComponent self, int numericType, float value)
        {
            self.NumericData.ChangeNoEvent(numericType, value);
        }

        public static void ChangeNoEvent(this NumericDataComponent self, int numericType, long value)
        {
            self.NumericData.ChangeNoEvent(numericType, value);
        }

        public static void ChangeNoEvent(this NumericDataComponent self, ENumericType numericType, bool value)
        {
            self.NumericData.ChangeNoEvent(numericType, value);
        }

        public static void ChangeNoEvent(this NumericDataComponent self, ENumericType numericType, float value)
        {
            self.NumericData.ChangeNoEvent(numericType, value);
        }

        public static void ChangeNoEvent(this NumericDataComponent self, ENumericType numericType, long value)
        {
            self.NumericData.ChangeNoEvent(numericType, value);
        }

        //跳过检查 非必要禁止使用
        public static void ChangeNoEventUnCheck(this NumericDataComponent self, ENumericType numericType, long value)
        {
            self.NumericData.ChangeNoEventUnCheck(numericType, value);
        }

        #endregion

        #endregion
    }
}