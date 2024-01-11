using System;
using Sirenix.OdinInspector;

namespace YIUIFramework
{
    [HideLabel]
    [HideReferenceObjectPicker]
    [Serializable]
    public class UIDataValueString : UIDataValueBase<string>, IEquatable<UIDataValueString>
    {
        public override string ToString()
        {
            return GetValue() ?? "";
        }

        public override EUIBindDataType UIBindDataType => EUIBindDataType.String;

        public override Type UIDataValueType => typeof(string);

        #region 对比函数

        public bool Equals(UIDataValueString other)
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

        protected override bool EqualsValue(string value)
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

            return GetValue() == (string)obj;
        }

        public override int GetHashCode()
        {
            return GetValue().GetHashCode();
        }

        public static bool operator ==(UIDataValueString lhs, UIDataValueString rhs)
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

        public static bool operator !=(UIDataValueString lhs, UIDataValueString rhs)
        {
            return !(lhs == rhs);
        }

        public static bool operator >(UIDataValueString lhs, UIDataValueString rhs)
        {
            return false;
        }

        public static bool operator <(UIDataValueString lhs, UIDataValueString rhs)
        {
            return false;
        }

        public static bool operator >=(UIDataValueString lhs, UIDataValueString rhs)
        {
            return false;
        }

        public static bool operator <=(UIDataValueString lhs, UIDataValueString rhs)
        {
            return false;
        }

        #endregion
    }
}