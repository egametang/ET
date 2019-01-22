#if SERVER
namespace UnityEngine
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    /// <summary>
    ///   <para>Store a collection of Keyframes that can be evaluated over time.</para>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public sealed class AnimationCurve
    {
        internal IntPtr m_Ptr;
        private List<Keyframe> m_Keys = new List<Keyframe>();

        /// <summary>
        ///   <para>All keys defined in the animation curve.</para>
        /// </summary>
        public Keyframe[] keys
        {
            get
            {
                return GetKeys();
            }
            set
            {
                SetKeys(value);
            }
        }

        public Keyframe this[int index]
        {
            get
            {
                return GetKey(index);
            }
        }

        /// <summary>
        ///   <para>The number of keys in the curve. (Read Only)</para>
        /// </summary>
        public int length
        {
            get;
        }

        /// <summary>
        ///   <para>The behaviour of the animation before the first keyframe.</para>
        /// </summary>
        public WrapMode preWrapMode
        {
            get;
            set;
        }

        /// <summary>
        ///   <para>The behaviour of the animation after the last keyframe.</para>
        /// </summary>
        public WrapMode postWrapMode
        {
            get;
            set;
        }

        /// <summary>
        ///   <para>Creates an animation curve from an arbitrary number of keyframes.</para>
        /// </summary>
        /// <param name="keys">An array of Keyframes used to define the curve.</param>
        public AnimationCurve(params Keyframe[] keys)
        {
            Init(keys);
        }

        /// <summary>
        ///   <para>Creates an empty animation curve.</para>
        /// </summary>
        public AnimationCurve()
        {
            Init(null);
        }

        /// <summary>
        ///   <para>Add a new key to the curve.</para>
        /// </summary>
        /// <param name="time">The time at which to add the key (horizontal axis in the curve graph).</param>
        /// <param name="value">The value for the key (vertical axis in the curve graph).</param>
        /// <returns>
        ///   <para>The index of the added key, or -1 if the key could not be added.</para>
        /// </returns>
        public int AddKey(float time, float value)
        {
            return AddKey(new Keyframe(time, value));
        }

        /// <summary>
        ///   <para>Add a new key to the curve.</para>
        /// </summary>
        /// <param name="key">The key to add to the curve.</param>
        /// <returns>
        ///   <para>The index of the added key, or -1 if the key could not be added.</para>
        /// </returns>
        public int AddKey(Keyframe key)
        {
            m_Keys.Add(key);
            return m_Keys.IndexOf(key);
        }

        /// <summary>
        ///   <para>Removes a key.</para>
        /// </summary>
        /// <param name="index">The index of the key to remove.</param>
        public void RemoveKey(int index)
        {
            if(m_Keys != null)
            {
                m_Keys.RemoveAt(index);
            }
        }

        private Keyframe GetKey(int index)
        {
            if (m_Keys.Count > index && index >= 0)
            {
                return m_Keys[index];
            }

            return default;
        }

        private void SetKeys(Keyframe[] keys)
        {
            m_Keys.Clear();
            AddKeys(keys);
        }

        private void AddKeys(Keyframe[] keys)
        {
            for (int i = 0; keys != null && keys.Length > i; i++)
            {
                m_Keys.Add(keys[i]);
            }
        }

        private Keyframe[] GetKeys()
        {
            return m_Keys.ToArray();
        }

        /// <summary>
        ///   <para>Creates a constant "curve" starting at timeStart, ending at timeEnd and with the value value.</para>
        /// </summary>
        /// <param name="timeStart">The start time for the constant curve.</param>
        /// <param name="timeEnd">The start time for the constant curve.</param>
        /// <param name="value">The value for the constant curve.</param>
        /// <returns>
        ///   <para>The constant curve created from the specified values.</para>
        /// </returns>
        public static AnimationCurve Constant(float timeStart, float timeEnd, float value)
        {
            return Linear(timeStart, value, timeEnd, value);
        }

        /// <summary>
        ///   <para>A straight Line starting at timeStart, valueStart and ending at timeEnd, valueEnd.</para>
        /// </summary>
        /// <param name="timeStart">The start time for the linear curve.</param>
        /// <param name="valueStart">The start value for the linear curve.</param>
        /// <param name="timeEnd">The end time for the linear curve.</param>
        /// <param name="valueEnd">The end value for the linear curve.</param>
        /// <returns>
        ///   <para>The linear curve created from the specified values.</para>
        /// </returns>
        public static AnimationCurve Linear(float timeStart, float valueStart, float timeEnd, float valueEnd)
        {
            float num = (valueEnd - valueStart) / (timeEnd - timeStart);
            Keyframe[] keys = new Keyframe[2]
            {
            new Keyframe(timeStart, valueStart, 0f, num),
            new Keyframe(timeEnd, valueEnd, num, 0f)
            };
            return new AnimationCurve(keys);
        }

        /// <summary>
        ///   <para>Creates an ease-in and out curve starting at timeStart, valueStart and ending at timeEnd, valueEnd.</para>
        /// </summary>
        /// <param name="timeStart">The start time for the ease curve.</param>
        /// <param name="valueStart">The start value for the ease curve.</param>
        /// <param name="timeEnd">The end time for the ease curve.</param>
        /// <param name="valueEnd">The end value for the ease curve.</param>
        /// <returns>
        ///   <para>The ease-in and out curve generated from the specified values.</para>
        /// </returns>
        public static AnimationCurve EaseInOut(float timeStart, float valueStart, float timeEnd, float valueEnd)
        {
            Keyframe[] keys = new Keyframe[2]
            {
            new Keyframe(timeStart, valueStart, 0f, 0f),
            new Keyframe(timeEnd, valueEnd, 0f, 0f)
            };
            return new AnimationCurve(keys);
        }

        private void Init(Keyframe[] keys)
        {
            AddKeys(keys);
        }
    }

}
#endif