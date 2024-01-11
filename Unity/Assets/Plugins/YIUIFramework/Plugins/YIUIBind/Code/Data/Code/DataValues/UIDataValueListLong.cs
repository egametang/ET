using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;

namespace YIUIFramework
{
    [HideLabel]
    [HideReferenceObjectPicker]
    [Serializable]
    public class UIDataValueListLong : UIDataValueBase<List<long>>, IEquatable<UIDataValueListLong>
    {
        public override string ToString()
        {
            return GetValue().ToString();
        }

        public override EUIBindDataType UIBindDataType => EUIBindDataType.List_Long;

        public UIDataValueListLong()
        {
            if (GetValue() == null)
            {
                SetValue(new List<long>());
            }
        }

        public override Type UIDataValueType => typeof(List<long>);

        protected override void SetValueFrom(List<long> value)
        {
            if (GetValue() == null)
            {
                base.SetValueFrom(value);
                return;
            }
            GetValue().Clear();
            GetValue().AddRange(value);
        }

        #region 对比函数

        private bool EqualsList(List<long> value)
        {
            if (GetValue()?.Count != value.Count)
            {
                return false;
            }

            return !GetValue().Where((t, i) => t != value[i]).Any();
        }

        public bool Equals(UIDataValueListLong other)
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

            return EqualsList(other.GetValue());
        }

        protected override bool EqualsValue(List<long> value)
        {
            return EqualsList(value);
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

            var value = (List<long>)obj;

            return EqualsList(value);
        }

        public override int GetHashCode()
        {
            return GetValue().GetHashCode();
        }

        public static bool operator ==(UIDataValueListLong lhs, UIDataValueListLong rhs)
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

        public static bool operator !=(UIDataValueListLong lhs, UIDataValueListLong rhs)
        {
            return !(lhs == rhs);
        }

        public static bool operator >(UIDataValueListLong lhs, UIDataValueListLong rhs)
        {
            return lhs.GetValue().Count > rhs.GetValue().Count;
        }

        public static bool operator <(UIDataValueListLong lhs, UIDataValueListLong rhs)
        {
            return lhs.GetValue().Count < rhs.GetValue().Count;
        }

        public static bool operator >=(UIDataValueListLong lhs, UIDataValueListLong rhs)
        {
            return lhs.GetValue().Count >= rhs.GetValue().Count;
        }

        public static bool operator <=(UIDataValueListLong lhs, UIDataValueListLong rhs)
        {
            return lhs.GetValue().Count <= rhs.GetValue().Count;
        }

        #endregion
    }
}