// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only]
    /// A simple wrapper around <see cref="EditorPrefs"/> to get and set a bool.
    /// <para></para>
    /// If you are interested in a more comprehensive pref wrapper that supports more types, you should check out
    /// <see href="https://kybernetik.com.au/inspector-gadgets">Inspector Gadgets</see>.
    /// </summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/BoolPref
    /// 
    public sealed class BoolPref
    {
        /************************************************************************************************************************/

        /// <summary>The prefix which is automatically added before the <see cref="Key"/>.</summary>
        public const string KeyPrefix = nameof(Animancer) + "/";

        /// <summary>The identifier with which this pref will be saved.</summary>
        public readonly string Key;

        /// <summary>The label to use when adding a function to toggle this pref to a menu.</summary>
        public readonly string MenuItem;

        /// <summary>The starting value to use for this pref if none was previously saved.</summary>
        public readonly bool DefaultValue;

        /************************************************************************************************************************/

        private bool _HasValue;
        private bool _Value;

        /// <summary>The current value of this pref.</summary>
        public bool Value
        {
            get
            {
                if (!_HasValue)
                {
                    _HasValue = true;
                    _Value = EditorPrefs.GetBool(Key, DefaultValue);
                }

                return _Value;
            }
            set
            {
                if (_Value == value &&
                    _HasValue)
                    return;

                _Value = value;
                _HasValue = true;
                EditorPrefs.SetBool(Key, value);
            }
        }

        /// <summary>Returns the current value of the `pref`.</summary>
        public static implicit operator bool(BoolPref pref) => pref.Value;

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="BoolPref"/>.</summary>
        public BoolPref(string menuItem, bool defaultValue)
            : this(null, menuItem, defaultValue) { }

        /// <summary>Creates a new <see cref="BoolPref"/>.</summary>
        public BoolPref(string keyPrefix, string menuItem, bool defaultValue)
        {
            MenuItem = menuItem + " ?";
            Key = KeyPrefix + keyPrefix + menuItem;
            DefaultValue = defaultValue;
        }

        /************************************************************************************************************************/

        /// <summary>Adds a menu function to toggle the <see cref="Value"/> of this pref.</summary>
        public void AddToggleFunction(GenericMenu menu)
        {
            menu.AddItem(new GUIContent(MenuItem), _Value, () =>
            {
                Value = !Value;
            });
        }

        /************************************************************************************************************************/

        /// <summary>Returns a string containing the <see cref="Key"/> and <see cref="Value"/>.</summary>
        public override string ToString() => $"{nameof(BoolPref)} ({nameof(Key)} = '{Key}', {nameof(Value)} = {Value})";

        /************************************************************************************************************************/
    }
}

#endif

