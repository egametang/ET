using System;
using Sirenix.OdinInspector;

namespace YIUIFramework
{
    [HideLabel]
    [HideReferenceObjectPicker]
    [Serializable]
    public class UIDataValueInt : UIDataValueBase<int>, IEquatable<UIDataValueInt>
    {
        public override string ToString()
        {
            return GetValue().ToString();
        }

        public override Type UIDataValueType => typeof(int);

        public override EUIBindDataType UIBindDataType => EUIBindDataType.Int;

        #region 对比函数

        public bool Equals(UIDataValueInt other)
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

        protected override bool EqualsValue(int value)
        {
            return GetValue() == value;
        }

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

            return GetValue() == (int)obj;
        }

        public override int GetHashCode()
        {
            return GetValue().GetHashCode();
        }

        public static bool operator ==(UIDataValueInt lhs, UIDataValueInt rhs)
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

        public static bool operator !=(UIDataValueInt lhs, UIDataValueInt rhs)
        {
            return !(lhs == rhs);
        }

        public static bool operator >(UIDataValueInt lhs, UIDataValueInt rhs)
        {
            return lhs.GetValue() > rhs.GetValue();
        }

        public static bool operator <(UIDataValueInt lhs, UIDataValueInt rhs)
        {
            return lhs.GetValue() < rhs.GetValue();
        }

        public static bool operator >=(UIDataValueInt lhs, UIDataValueInt rhs)
        {
            return lhs.GetValue() >= rhs.GetValue();
        }

        public static bool operator <=(UIDataValueInt lhs, UIDataValueInt rhs)
        {
            return lhs.GetValue() <= rhs.GetValue();
        }

        #endregion
    }
}