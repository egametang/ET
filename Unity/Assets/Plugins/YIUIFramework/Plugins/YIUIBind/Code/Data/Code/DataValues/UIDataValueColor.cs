using System;
using Sirenix.OdinInspector;
using UnityEngine;
using Logger = YIUIFramework.Logger;

namespace YIUIFramework
{
    [HideLabel]
    [HideReferenceObjectPicker]
    [Serializable]
    public class UIDataValueColor : UIDataValueBase<Color>, IEquatable<UIDataValueColor>
    {
        public override string ToString()
        {
            return GetValue().ToString();
        }

        public override EUIBindDataType UIBindDataType => EUIBindDataType.Color;

        public UIDataValueColor()
        {
            SetValue(Color.white);
        }

        public override Type UIDataValueType => typeof(Color);

        #region 对比函数

        public bool Equals(UIDataValueColor other)
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

        protected override bool EqualsValue(Color value)
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

            return GetValue() == (Color)obj;
        }

        public override int GetHashCode()
        {
            return GetValue().GetHashCode();
        }

        public static bool operator ==(UIDataValueColor lhs, UIDataValueColor rhs)
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

        public static bool operator !=(UIDataValueColor lhs, UIDataValueColor rhs)
        {
            return !(lhs == rhs);
        }

        public static bool operator >(UIDataValueColor lhs, UIDataValueColor rhs)
        {
            Logger.LogError($"请不要比较这个类型 {EUIBindDataType.Color}");
            return false;
        }

        public static bool operator <(UIDataValueColor lhs, UIDataValueColor rhs)
        {
            Logger.LogError($"请不要比较这个类型 {EUIBindDataType.Color}");
            return false;
        }

        public static bool operator >=(UIDataValueColor lhs, UIDataValueColor rhs)
        {
            Logger.LogError($"请不要比较这个类型 {EUIBindDataType.Color}");
            return false;
        }

        public static bool operator <=(UIDataValueColor lhs, UIDataValueColor rhs)
        {
            Logger.LogError($"请不要比较这个类型 {EUIBindDataType.Color}");
            return false;
        }

        #endregion
    }
}