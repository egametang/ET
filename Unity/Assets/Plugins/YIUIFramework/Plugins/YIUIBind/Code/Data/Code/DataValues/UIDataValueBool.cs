using System;
using Sirenix.OdinInspector;
using Logger = YIUIFramework.Logger;

namespace YIUIFramework
{
    [HideLabel]
    [HideReferenceObjectPicker]
    [Serializable]
    public class UIDataValueBool : UIDataValueBase<bool>, IEquatable<UIDataValueBool>
    {
        public override string ToString()
        {
            return GetValue() ? "True" : "False";
        }

        public override Type UIDataValueType => typeof(bool);

        public override EUIBindDataType UIBindDataType => EUIBindDataType.Bool;

        #region 对比函数

        //类对比
        public bool Equals(UIDataValueBool other)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (GetType() != other.GetType())
            {
                return false;
            }

            return GetValue() == other.GetValue();
        }

        //值对比
        protected override bool EqualsValue(bool value)
        {
            return GetValue() == value;
        }

        //值对比 少用 这个有装箱拆箱的性能开销
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != UIDataValueType)
            {
                return false;
            }

            return GetValue() == (bool)obj;
        }

        public override int GetHashCode()
        {
            return GetValue().GetHashCode();
        }

        public static bool operator ==(UIDataValueBool lhs, UIDataValueBool rhs)
        {
            if (ReferenceEquals(lhs, null))
            {
                if (ReferenceEquals(rhs, null))
                {
                    return true;
                }

                return false;
            }

            return lhs.Equals(rhs);
        }

        public static bool operator !=(UIDataValueBool lhs, UIDataValueBool rhs)
        {
            return !(lhs == rhs);
        }

        public static bool operator >(UIDataValueBool lhs, UIDataValueBool rhs)
        {
            Logger.LogError($"注意你正在对比2个bool的 大小 >");
            return false;
        }

        public static bool operator <(UIDataValueBool lhs, UIDataValueBool rhs)
        {
            Logger.LogError($"注意你正在对比2个bool的 大小 <");
            return false;
        }

        public static bool operator >=(UIDataValueBool lhs, UIDataValueBool rhs)
        {
            Logger.LogError($"注意你正在对比2个bool的 大小 >=");
            return false;
        }

        public static bool operator <=(UIDataValueBool lhs, UIDataValueBool rhs)
        {
            Logger.LogError($"注意你正在对比2个bool的 大小 <=");
            return false;
        }

        #endregion
    }
}