using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace YIUIFramework
{
    [HideLabel]
    [HideReferenceObjectPicker]
    [Serializable]
    public class UIDataValueVector2 : UIDataValueBase<Vector2>, IEquatable<UIDataValueVector2>
    {
        public override string ToString()
        {
            var value = GetValue();
            return $"X:{value.x} Y:{value.y}";
        }

        public override EUIBindDataType UIBindDataType => EUIBindDataType.Vector2;

        public override Type UIDataValueType => typeof(Vector2);

        #region 对比函数

        public bool Equals(UIDataValueVector2 other)
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

        protected override bool EqualsValue(Vector2 value)
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

            return GetValue() == (Vector2)obj;
        }

        public override int GetHashCode()
        {
            return GetValue().GetHashCode();
        }

        public static bool operator ==(UIDataValueVector2 lhs, UIDataValueVector2 rhs)
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

        public static bool operator !=(UIDataValueVector2 lhs, UIDataValueVector2 rhs)
        {
            return !(lhs == rhs);
        }

        public static bool operator >(UIDataValueVector2 lhs, UIDataValueVector2 rhs)
        {
            return false;
        }

        public static bool operator <(UIDataValueVector2 lhs, UIDataValueVector2 rhs)
        {
            return false;
        }

        public static bool operator >=(UIDataValueVector2 lhs, UIDataValueVector2 rhs)
        {
            return false;
        }

        public static bool operator <=(UIDataValueVector2 lhs, UIDataValueVector2 rhs)
        {
            return false;
        }

        #endregion
    }
}