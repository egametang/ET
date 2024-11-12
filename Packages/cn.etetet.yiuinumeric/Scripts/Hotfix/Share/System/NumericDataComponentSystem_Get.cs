using System.Collections.Generic;

namespace ET
{
    [FriendOf(typeof(NumericDataComponent))]
    public static partial class NumericDataComponentSystem
    {
        #region Get

        public static bool GetAsBool(this NumericDataComponent self, int numericType)
        {
            return self.NumericData.GetAsBool(numericType);
        }

        public static float GetAsFloat(this NumericDataComponent self, int numericType)
        {
            return self.NumericData.GetAsFloat(numericType);
        }

        public static int GetAsInt(this NumericDataComponent self, int numericType)
        {
            return self.NumericData.GetAsInt(numericType);
        }

        public static long GetAsLong(this NumericDataComponent self, int numericType)
        {
            return self.NumericData.GetAsLong(numericType);
        }

        public static bool GetAsBool(this NumericDataComponent self, ENumericType numericType)
        {
            return self.NumericData.GetAsBool(numericType);
        }

        public static float GetAsFloat(this NumericDataComponent self, ENumericType numericType)
        {
            return self.NumericData.GetAsFloat(numericType);
        }

        public static int GetAsInt(this NumericDataComponent self, ENumericType numericType)
        {
            return self.NumericData.GetAsInt(numericType);
        }

        public static long GetAsLong(this NumericDataComponent self, ENumericType numericType)
        {
            return self.NumericData.GetAsLong(numericType);
        }

        #region Other

        public static object GetObjectValue(this NumericDataComponent self, ENumericType numericType)
        {
            return self.NumericData.GetObjectValue(numericType);
        }

        public static Dictionary<int, long> GetNumericDic(this NumericDataComponent self)
        {
            return self.NumericData.GetNumericDic();
        }

        public static void GetNumericDic(this NumericDataComponent self, ref Dictionary<int, long> refDic)
        {
            self.NumericData.GetNumericDic(refDic);
        }

        #endregion

        #endregion
    }
}