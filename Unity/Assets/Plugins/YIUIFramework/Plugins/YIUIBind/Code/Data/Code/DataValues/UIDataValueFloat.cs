using System;
using Sirenix.OdinInspector;

namespace YIUIFramework
{
    [HideLabel]
    [HideReferenceObjectPicker]
    [Serializable]
    public class UIDataValueFloat : UIDataValueBase<float>, IEquatable<UIDataValueFloat>
    {
        public override string ToString()
        {
            return GetValue().ToString();
        }

        public override EUIBindDataType UIBindDataType => EUIBindDataType.Float;

        public override Type UIDataValueType => typeof(float);

        #region 对比函数

        public bool Equals(UIDataValueFloat other)
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

            return Math.Abs(GetValue() - other.GetValue()) < 0.0001f;
        }

        protected override bool EqualsValue(float value)
        {
            return Math.Abs(GetValue() - value) < 0.0001f;
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

            return Math.Abs(GetValue() - (float)obj) < 0.0001f;
        }

        public override int GetHashCode()
        {
            return GetValue().GetHashCode();
        }

        public static bool operator ==(UIDataValueFloat lhs, UIDataValueFloat rhs)
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

        public static bool operator !=(UIDataValueFloat lhs, UIDataValueFloat rhs)
        {
            return !(lhs == rhs);
        }

        public static bool operator >(UIDataValueFloat lhs, UIDataValueFloat rhs)
        {
            return lhs.GetValue() > rhs.GetValue();
        }

        public static bool operator <(UIDataValueFloat lhs, UIDataValueFloat rhs)
        {
            return lhs.GetValue() < rhs.GetValue();
        }

        public static bool operator >=(UIDataValueFloat lhs, UIDataValueFloat rhs)
        {
            return lhs.GetValue() >= rhs.GetValue();
        }

        public static bool operator <=(UIDataValueFloat lhs, UIDataValueFloat rhs)
        {
            return lhs.GetValue() <= rhs.GetValue();
        }

        #endregion
    }
}