using System;
using UnityEngine;

namespace ET.PackageManager.Editor
{
    /// 用于本地小数据量记录的playerPre的包装，用于方便把单个设置项直接当做变量使用
    /// 使用这个，可以避免读取时每次都调用PlayerPrefs
    /// 另外集成了以groupKey(比如userid)来分组数据
    public struct Prefs<T>
    {
        private T      m_value;
        private string m_key;
        private string m_valueKey;
        private RwFlag m_rwFlag;

        private int            m_groupKeyVer;
        private IGroupKey      m_groupKey;
        private IPrefsAccessor m_accessor;

        private T m_defValue;

        public Prefs(string key, IGroupKey groupKey, T defValue, IPrefsAccessor accessor)
        {
            m_defValue    = defValue;
            m_value       = defValue;
            m_groupKey    = groupKey;
            m_accessor    = accessor;
            m_groupKeyVer = int.MinValue;
            m_rwFlag      = RwFlag.None;
            m_valueKey    = key;
            m_key         = key;
            UpdateKey();
        }

        private void UpdateKey(bool notWrite = false)
        {
            if (m_groupKey == null)
            {
                return;
            }

            var curVer = m_groupKey.Version;
            if (curVer != m_groupKeyVer)
            {
                m_groupKeyVer = curVer;
                var oldKey = m_key;
                m_key = StrUtil.Concat(m_groupKey.Key, "|", m_valueKey);
                if (m_rwFlag.Has(RwFlag.Write))
                {
                    if (oldKey != m_key)
                    {
                        //如果之前已经有写入，则需要删除旧的，写入新的
                        PlayerPrefs.DeleteKey(oldKey);
                        if (!notWrite)
                        {
                            m_accessor.Set(m_key, m_value);
                        }
                    }
                }

                //清除读取标记
                m_rwFlag.UnmarkSelf(RwFlag.Read);
            }
        }

        public T Value
        {
            get
            {
                UpdateKey();

                //如果即没有读过，也没有写过
                if (!m_rwFlag.Overlaps(RwFlag.Full))
                {
                    if (m_accessor.HasKey(m_key))
                    {
                        m_value = m_accessor.Get(m_key);
                    }
                    else if (m_groupKey != null && m_accessor.HasKey(m_valueKey))
                    {
                        //第一次时，如果定向的值拿不到，就去尝试拿全局的值
                        m_value = m_accessor.Get(m_valueKey);
                    }
                    else
                    {
                        m_value = m_defValue;
                    }

                    m_rwFlag.MarkSelf(RwFlag.Read);
                }

                return m_value;
            }

            set
            {
                UpdateKey();
                if (m_rwFlag.Overlaps(RwFlag.Full) && value.Equals(m_value))
                {
                    return;
                }

                m_value  =  value;
                m_rwFlag |= RwFlag.Write;
                m_accessor.Set(m_key, value);
            }
        }

        public void Delete()
        {
            UpdateKey(true);
            m_groupKeyVer = int.MinValue;
            m_value       = default;
            m_rwFlag.MarkSelf(RwFlag.Write);
            PlayerPrefs.DeleteKey(m_key);
        }

        public interface IPrefsAccessor
        {
            bool HasKey(string key);
            T    Get(string    key);
            void Set(string    key, T value);
        }
    }

    public struct IntPrefs
    {
        private Prefs<int> m_value;

        public int Value
        {
            get => m_value.Value;
            set => m_value.Value = value;
        }

        public IntPrefs(string key, IGroupKey groupKey = null, int defValue = 0)
        {
            m_value = new Prefs<int>(key, groupKey, defValue, IntAccessor.Inst);
        }

        public void Delete()
        {
            m_value.Delete();
        }

        public static implicit operator int(IntPrefs value)
        {
            return value.Value;
        }
    }

    public struct FloatPrefs
    {
        private Prefs<float> m_value;

        public float Value
        {
            get => m_value.Value;
            set => m_value.Value = value;
        }

        public FloatPrefs(string key, IGroupKey groupKey = null, float defValue = 0f)
        {
            m_value = new Prefs<float>(key, groupKey, defValue, FloatAccessor.Inst);
        }

        public void Delete()
        {
            m_value.Delete();
        }

        public static implicit operator float(FloatPrefs value)
        {
            return value.Value;
        }
    }

    public struct StringPrefs
    {
        private Prefs<string> m_value;

        public string Value
        {
            get => m_value.Value;
            set => m_value.Value = value;
        }

        public StringPrefs(string key, IGroupKey groupKey = null, string defValue = null)
        {
            m_value = new Prefs<string>(key, groupKey, defValue, StrAccessor.Inst);
        }

        public void Delete()
        {
            m_value.Delete();
        }

        public static implicit operator string(StringPrefs value)
        {
            return value.Value;
        }
    }

    public struct BoolPrefs
    {
        private Prefs<int> m_value;

        public bool Value
        {
            get => m_value.Value == 1;
            set => m_value.Value = value ? 1 : 0;
        }

        public BoolPrefs(string key, IGroupKey groupKey = null, bool defValue = false)
        {
            m_value = new Prefs<int>(key, groupKey, defValue ? 1 : 0, IntAccessor.Inst);
        }

        public void Delete()
        {
            m_value.Delete();
        }

        public static implicit operator bool(BoolPrefs value)
        {
            return value.Value;
        }
    }

    public struct ArrPrefs<T> where T : IComparable
    {
        private Prefs<string> m_value;

        public ArrPrefs(string key, IGroupKey groupKey = null)
        {
            m_value = new Prefs<string>(key, groupKey, "", StrAccessor.Inst);
        }

        public T[] Get()
        {
            string value = m_value.Value;
            if (string.IsNullOrEmpty(value))
            {
                return Array.Empty<T>();
            }

            return StrConv.ToArr<T>(value, StrConv.ArrSplitLv1);
        }

        public void Set(T[] value)
        {
            if (value == null || value.Length < 1)
            {
                m_value.Delete();
                return;
            }

            var sb = SbPool.Get();
            sb.Append(value[0].ToString());
            for (int i = 1; i < value.Length; i++)
            {
                sb.Append(StrConv.ChrArrSplitLv1).Append(value[i].ToString());
            }

            m_value.Value = SbPool.PutAndToStr(sb);
        }

        public void Delete()
        {
            m_value.Delete();
        }
    }

    public struct EnumPrefs<T> where T : Enum
    {
        private Prefs<int> m_value;

        public T Value
        {
            get => (T)Enum.ToObject(typeof(T), m_value.Value);
            set => m_value.Value = Convert.ToInt32(value);
        }

        public EnumPrefs(string key, IGroupKey groupKey = null, T defValue = default)
        {
            m_value = new Prefs<int>(key, groupKey, Convert.ToInt32(defValue), IntAccessor.Inst);
        }

        public void Delete()
        {
            m_value.Delete();
        }

        public static implicit operator T(EnumPrefs<T> value)
        {
            return value.Value;
        }
    }

    public class IntAccessor : Prefs<int>.IPrefsAccessor
    {
        public static readonly IntAccessor Inst = new IntAccessor();

        public bool HasKey(string key)
        {
            return PlayerPrefs.HasKey(key);
        }

        public int Get(string key)
        {
            return PlayerPrefs.GetInt(key);
        }

        public void Set(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
        }
    }

    public class FloatAccessor : Prefs<float>.IPrefsAccessor
    {
        public static readonly FloatAccessor Inst = new FloatAccessor();

        public bool HasKey(string key)
        {
            return PlayerPrefs.HasKey(key);
        }

        public float Get(string key)
        {
            return PlayerPrefs.GetFloat(key);
        }

        public void Set(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
        }
    }

    public class StrAccessor : Prefs<string>.IPrefsAccessor
    {
        public static readonly StrAccessor Inst = new StrAccessor();

        public bool HasKey(string key)
        {
            return PlayerPrefs.HasKey(key);
        }

        public string Get(string key)
        {
            return PlayerPrefs.GetString(key);
        }

        public void Set(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
        }
    }

    /// <summary>
    /// 分组KEY
    /// </summary>
    public interface IGroupKey
    {
        int    Version { get; }
        string Key     { get; }
    }

    [Flags]
    public enum RwFlag
    {
        None  = 0,
        Write = 1,
        Read  = 1 << 1,
        Full  = Write | Read
    }

    public static class RwFlagExt
    {
        public static RwFlag Mark(this RwFlag owner, RwFlag flags)
        {
            return owner | flags;
        }

        public static RwFlag Unmark(this RwFlag owner, RwFlag flags)
        {
            return owner & (~flags);
        }

        public static void MarkSelf(this ref RwFlag owner, RwFlag flags)
        {
            owner |= flags;
        }

        public static void UnmarkSelf(this ref RwFlag owner, RwFlag flags)
        {
            owner &= (~flags);
        }

        public static bool Has(this RwFlag owner, RwFlag flags)
        {
            return (owner & flags) == flags;
        }

        public static bool Overlaps(this RwFlag owner, RwFlag flags)
        {
            return (owner & flags) != 0;
        }
    }
}
