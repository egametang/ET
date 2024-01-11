using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace YIUIFramework
{
    [HideLabel]
    [HideReferenceObjectPicker]
    [Serializable]
    public class UIDataValueVector3 : UIDataValueBase<Vector3>, IEquatable<UIDataValueVector3>
    {
        public override string ToString()
        {
            var value = GetValue();
            return $"X:{value.x} Y:{value.y} Z:{value.z}";
        }

        public override EUIBindDataType UIBindDataType => EUIBindDataType.Vector3;

        public override Type UIDataValueType => typeof(Vector3);

        #region 对比函数

        public bool Equals(UIDataValueVector3 other)
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

        protected override bool EqualsValue(Vector3 value)
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

            return GetValue() == (Vector3)obj;
        }

        public override int GetHashCode()
        {
            return GetValue().GetHashCode();
        }

        public static bool operator ==(UIDataValueVector3 lhs, UIDataValueVector3 rhs)
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

        public static bool operator !=(UIDataValueVector3 lhs, UIDataValueVector3 rhs)
        {
            return !(lhs == rhs);
        }

        public static bool operator >(UIDataValueVector3 lhs, UIDataValueVector3 rhs)
        {
            return false;
        }

        public static bool operator <(UIDataValueVector3 lhs, UIDataValueVector3 rhs)
        {
            return false;
        }

        public static bool operator >=(UIDataValueVector3 lhs, UIDataValueVector3 rhs)
        {
            return false;
        }

        public static bool operator <=(UIDataValueVector3 lhs, UIDataValueVector3 rhs)
        {
            return false;
        }

        #endregion
    }
}