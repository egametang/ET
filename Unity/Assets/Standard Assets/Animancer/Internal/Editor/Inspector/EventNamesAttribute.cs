// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

using Animancer.Editor;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Animancer
{
    /// <summary>[Editor-Conditional] 
    /// Specifies a set of acceptable names for <see cref="AnimancerEvent"/>s so they can be displayed using a dropdown
    /// menu instead of a text field.
    /// </summary>
    /// 
    /// <remarks>
    /// Placing this attribute on a type applies it to all fields in that type.
    /// <para></para>
    /// Note that values selected using the dropdown menu are still stored as strings. Modifying the names in the
    /// script will NOT automatically update any values previously set in the Inspector.
    /// <para></para>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/events/animancer#event-names">Event Names</see>
    /// </remarks>
    /// 
    /// <example><code>
    /// [EventNames(...)]// Apply to all fields in this class.
    /// public class AttackState
    /// {
    ///     [SerializeField]
    ///     [EventNames(...)]// Apply to only this field.
    ///     private ClipState.Transition _Action;
    /// }
    /// </code>
    /// See the constructors for examples of their usage.
    /// </example>
    /// 
    /// https://kybernetik.com.au/animancer/api/Animancer/EventNamesAttribute
    /// 
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
    [System.Diagnostics.Conditional(Strings.UnityEditor)]
    public sealed class EventNamesAttribute : Attribute
    {
        /************************************************************************************************************************/

#if UNITY_EDITOR
        /// <summary>[Editor-Only] The names that can be used for events in the attributed field.</summary>
        public readonly string[] Names;
#endif

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="EventNamesAttribute"/> containing the specified `names`.</summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException">`names` contains no elements.</exception>
        /// <example><code>
        /// public class AttackState
        /// {
        ///     [SerializeField]
        ///     [EventNames("Hit Start", "Hit End")]
        ///     private ClipState.Transition _Animation;
        /// 
        ///     private void Awake()
        ///     {
        ///         _Animation.Events.SetCallback("Hit Start", OnHitStart);
        ///         _Animation.Events.SetCallback("Hit End", OnHitEnd);
        ///     }
        /// 
        ///     private void OnHitStart() { }
        ///     private void OnHitEnd() { }
        /// }
        /// </code></example>
        public EventNamesAttribute(params string[] names)
        {
#if UNITY_EDITOR
            if (names == null)
                throw new ArgumentNullException(nameof(names));
            else if (names.Length == 0)
                throw new ArgumentOutOfRangeException(nameof(names), "Array must not be empty");

            Names = AddSpecialItems(names);
#endif
        }

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="EventNamesAttribute"/> with <see cref="Names"/> from the `type`.</summary>
        /// 
        /// <remarks>
        /// If the `type` is an enum, all of its values will be used.
        /// <para></para>
        /// Otherwise the values of all static <see cref="string"/> fields (including constants) will be used.
        /// </remarks>
        /// <exception cref="ArgumentNullException"/>
        /// 
        /// <example><code>
        /// public class AttackState
        /// {
        ///     public static class Events
        ///     {
        ///         public const string HitStart = "Hit Start";
        ///         public const string HitEnd = "Hit End";
        ///     }
        /// 
        ///     [SerializeField]
        ///     [EventNames(typeof(Events))]// Use all string fields in the Events class.
        ///     private ClipState.Transition _Animation;
        /// 
        ///     private void Awake()
        ///     {
        ///         _Animation.Events.SetCallback(Events.HitStart, OnHitStart);
        ///         _Animation.Events.SetCallback(Events.HitEnd, OnHitEnd);
        ///     }
        /// 
        ///     private void OnHitStart() { }
        ///     private void OnHitEnd() { }
        /// }
        /// </code></example>
        public EventNamesAttribute(Type type)
        {
#if UNITY_EDITOR
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (type.IsEnum)
            {
                Names = Enum.GetNames(type);
            }
            else
            {
                Names = GatherNamesFromStaticFields(type);
            }

            Names = AddSpecialItems(Names);
#endif
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Creates a new <see cref="EventNamesAttribute"/> with <see cref="Names"/> from a member in the `type`
        /// with the specified `name`.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException">No member with the specified `name` exists in the `type`.</exception>
        /// 
        /// <remarks>
        /// The specified member must be static and can be a Field, Property, or Method.
        /// <para></para>
        /// The member type can be anything implementing <see cref="IEnumerable"/> (including arrays, lists, and
        /// coroutines).
        /// </remarks>
        /// 
        /// <example><code>
        /// public class AttackState
        /// {
        ///     public static readonly string[] Events = { "Hit Start", "Hit End" };
        /// 
        ///     [SerializeField]
        ///     [EventNames(typeof(AttackState), nameof(Events))]// Get the names from AttackState.Events.
        ///     private ClipState.Transition _Animation;
        /// 
        ///     private void Awake()
        ///     {
        ///         _Animation.Events.SetCallback(Events[0], OnHitStart);
        ///         _Animation.Events.SetCallback(Events[1], OnHitEnd);
        ///     }
        /// 
        ///     private void OnHitStart() { }
        ///     private void OnHitEnd() { }
        /// }
        /// </code></example>
        public EventNamesAttribute(Type type, string name)
        {
#if UNITY_EDITOR
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            object obj;

            var field = type.GetField(name, AnimancerEditorUtilities.StaticBindings);
            if (field != null)
            {
                obj = field.GetValue(null) as IEnumerable;
                goto GotCollection;
            }

            var property = type.GetProperty(name, AnimancerEditorUtilities.StaticBindings);
            if (property != null)
            {
                obj = property.GetValue(null, null) as IEnumerable;
                goto GotCollection;
            }

            var method = type.GetMethod(name, AnimancerEditorUtilities.StaticBindings, null, Type.EmptyTypes, null);
            if (method != null)
            {
                obj = method.Invoke(null, null) as IEnumerable;
                goto GotCollection;
            }

            throw new ArgumentException($"{type.FullName} does not contain a member named '{name}'");

            GotCollection:
            if (obj == null)
                throw new ArgumentException($"The collection retrieved from {type.FullName}.{name} is null");
            if (!(obj is IEnumerable collection))
                throw new ArgumentException($"The object retrieved from {type.FullName}.{name} is not an {nameof(IEnumerable)}");

            using (ObjectPool.Disposable.AcquireList<string>(out var names))
            {
                names.Add(NoName);
                foreach (var item in collection)
                {
                    if (item == null)
                        continue;

                    var itemName = item.ToString();
                    if (string.IsNullOrEmpty(itemName))
                        continue;

                    names.Add(itemName);
                }

                if (names.Count == 1)
                    throw new ArgumentException($"The collection retrieved from {type.FullName}.{name} is empty");

                Names = names.ToArray();
            }
#endif
        }

        /************************************************************************************************************************/
#if UNITY_EDITOR
        /************************************************************************************************************************/

        /// <summary>The entry used for the menu function to clear the name (U+202F Narrow No-Break Space).</summary>
        public const string NoName = " ";

        /************************************************************************************************************************/

        private static string[] AddSpecialItems(string[] names)
        {
            if (names == null)
                return null;

            var newNames = new string[names.Length + 1];
            newNames[0] = NoName;
            Array.Copy(names, 0, newNames, 1, names.Length);
            return newNames;
        }

        /************************************************************************************************************************/

        private static string[] GatherNamesFromStaticFields(Type type)
        {
            using (ObjectPool.Disposable.AcquireList<string>(out var names))
            {
                var fields = type.GetFields(AnimancerEditorUtilities.StaticBindings);
                for (int i = 0; i < fields.Length; i++)
                {
                    var field = fields[i];
                    if (field.FieldType == typeof(string))
                    {
                        var name = (string)field.GetValue(null);
                        if (name != null && !names.Contains(name))
                            names.Add(name);
                    }
                }

                if (names.Count > 0)
                    return names.ToArray();
                else
                    return null;
            }
        }

        /************************************************************************************************************************/

        /// <summary>[Editor-Only] Get the event names associated with the specified `property` from its fields or declaring types.</summary>
        public static string[] GetNames(SerializedProperty property)
        {
            var accessor = property.GetAccessor();
            while (accessor != null)
            {
                var names = GetNames(accessor.Field);
                if (names != null && names.Length > 0)
                    return names;

                accessor = accessor.Parent;
            }

            // If none of the fields of types they are declared in have names, try the actual type of the target.
            {
                var names = GetNames(property.serializedObject.targetObject.GetType());
                if (names != null && names.Length > 0)
                    return names;
            }

            return null;
        }

        /************************************************************************************************************************/

        private static readonly Dictionary<MemberInfo, string[]>
            MemberToNames = new Dictionary<MemberInfo, string[]>();

        /// <summary>[Editor-Only] Get the event names associated with the specified `field` or its declaring type.</summary>
        public static string[] GetNames(FieldInfo field)
        {
            if (!MemberToNames.TryGetValue(field, out var names))
            {
                if (!field.IsDefined(typeof(EventNamesAttribute), true))
                    return GetNames(field.DeclaringType);

                try
                {
                    var attributes = field.GetCustomAttributes(typeof(EventNamesAttribute), true);
                    names = ((EventNamesAttribute)attributes[0]).Names;
                }
                catch (Exception exception)
                {
                    Debug.LogError(GetError($"{field.DeclaringType.FullName}.{field.Name}", exception));
                }

                MemberToNames.Add(field, names);
            }

            return names;
        }

        /// <summary>[Editor-Only] Get the event names associated with the specified `type`.</summary>
        public static string[] GetNames(Type type)
        {
            if (!MemberToNames.TryGetValue(type, out var names))
            {
                try
                {
                    if (type.IsDefined(typeof(EventNamesAttribute), true))
                    {
                        var attributes = type.GetCustomAttributes(typeof(EventNamesAttribute), true);
                        names = ((EventNamesAttribute)attributes[0]).Names;
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogError(GetError(type.FullName, exception));
                }

                MemberToNames.Add(type, names);
            }

            return names;
        }

        private static string GetError(string memberName, Exception exception)
            => $"{exception.GetType()} thrown by [EventNames] {memberName}: {exception.Message}";

        /************************************************************************************************************************/
#endif
        /************************************************************************************************************************/
    }
}

